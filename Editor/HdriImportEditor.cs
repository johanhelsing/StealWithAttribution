using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace StealWithAttribution.Editor
{
    [CustomEditor(typeof(HdriImportSettings))]
    public class HdriImportEditor : UnityEditor.Editor
    {
        private HdriImportSettings settings;
        private string assetUrl = "https://hdrihaven.com/hdri/?h=wasteland_clouds";
        private Texture lastImportedTexture;
        private Material lastImportedMaterial;

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(loading))
            {
                base.OnInspectorGUI();
                settings = (HdriImportSettings) target;
                EditorGUILayout.Space();

                ImportFromUrl();
                ImportSummary();

                HdriHavenBrowser();
            }
        }

        private const int MaxPreviewPerPage = 200;
        private int previewsPerPage = 20;
        private readonly Texture[] previewTextures = new Texture[MaxPreviewPerPage];
        private readonly string[] previewIds = new string[MaxPreviewPerPage];
        private bool loading = false;
        private readonly AnimBool browserExpanded = new AnimBool(false);
        private void HdriHavenBrowser()
        {
            browserExpanded.target = EditorGUILayout.Foldout(browserExpanded.target, "HDRI Haven");

            using (var group = new EditorGUILayout.FadeGroupScope(browserExpanded.faded))
            using (new EditorGUI.IndentLevelScope(1))
            {
                if (!group.visible) return;

                var oldPreviewsPerPage = previewsPerPage;
                previewsPerPage = EditorGUILayout.IntField("Per page", previewsPerPage);
                if (previewsPerPage != oldPreviewsPerPage)
                {
                    previewsPerPage = Mathf.Min(previewsPerPage, MaxPreviewPerPage);
                    previewsPerPage += previewsPerPage % 2; // Make even
                    for (var i = 0; i < previewsPerPage; ++i) previewTextures[i] = null;
                }

                if (previewTextures.Take(previewsPerPage).All(texture => texture == null))
                {
                    if (!loading) LoadPreviews().Forget();
                    return;
                }

                for (var i = 0; i < previewsPerPage; ++i)
                {
                    var texture = previewTextures[i];
                    var id = previewIds[i];

                    if (i % 2 == 0) EditorGUILayout.BeginHorizontal();
                    if (texture != null)
                    {
                        EditorGUILayout.BeginVertical();
                        if (GUILayout.Button(texture, GUILayout.MinWidth(100), GUILayout.MinHeight(40),
                            GUILayout.MaxHeight(EditorGUIUtility.currentViewWidth * texture.height / texture.width / 2.0f),
                            GUILayout.ExpandWidth(true)))
                        {
                            DownloadAndImportHdriHavenTexture(id).Forget();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    else GUILayout.Button(previewIds[i]);
                    if (i % 2 != 0) EditorGUILayout.EndHorizontal();
                }
            }
        }

        private async UniTask LoadPreviews()
        {
            loading = true;
            var metaData = await HdriHavenApi.GetMetaDataAsync();
            var previews = metaData.assets
                .Select(asset => asset.Key)
                .Select(id => (id: id, download: HdriHavenApi.GetAssetPreviewTexture(id)))
                .Take(previewsPerPage)
                .ToArray();

            var textures = await UniTask.WhenAll(previews.Select(p => p.download));

            for (var i = 0; i < previewsPerPage; ++i)
            {
                previewTextures[i] = textures[i];
                previewIds[i] = previews[i].id;
            }
            loading = false;
        }

        private void ImportFromUrl()
        {
            EditorGUILayout.LabelField("Import asset");
            assetUrl = EditorGUILayout.TextField("Url", assetUrl);
            if (GUILayout.Button("Import")) Import(assetUrl).Forget();
        }

        private void ImportSummary()
        {
            if (lastImportedTexture != null)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Downloaded texture", lastImportedTexture, typeof(Texture), false);
                GUI.enabled = true;
            }

            if (lastImportedMaterial != null)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Generated material", lastImportedMaterial, typeof(Material), false);
                GUI.enabled = true;
            }
        }

        private async UniTask Import(string browserUrl)
        {
            lastImportedTexture = null;
            lastImportedMaterial = null;
            Debug.Assert(!loading);
            loading = true;

            // TODO: support other sources
            Debug.Log($"Importing asset: {browserUrl}");
            var id = HdriHavenApi.GetIdFromUrl(browserUrl);
            var textureUrl = await HdriHavenApi.GetTextureUrlAsync(id, settings.resolution);
            await DownloadAndImportTexture(textureUrl, id);
        }

        private async UniTask DownloadAndImportHdriHavenTexture(string id)
        {
            var textureUrl = await HdriHavenApi.GetTextureUrlAsync(id, settings.resolution);
            await DownloadAndImportTexture(textureUrl, id);
        }

        private async UniTask DownloadAndImportTexture(string textureUrl, string id)
        {
            loading = true;
            var filePath = await Utils.DownloadAndSaveFile(
                textureUrl, $"{settings.outputDirectory.AbsolutePath}/{Utils.FileNameFromUrl(textureUrl)}"
                // , Progress.Create<float>(x => Debug.Log($"{x*100:0}%"))
            );

            AssetDatabase.Refresh();

            var assetPath = Utils.AssetDatabasePathFromAbsolute(filePath);

            if (settings.useCubemap)
            {
                var importer = (TextureImporter) AssetImporter.GetAtPath(assetPath);
                importer.textureShape = TextureImporterShape.TextureCube;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh(); // TODO: needed?
            var importedTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(assetPath);

            if (settings.createSkyboxMaterial)
            {
                var shader = Shader.Find(settings.useCubemap ? "Skybox/Cubemap" : "Skybox/Panoramic");
                var skyboxMaterial = new Material(shader) {mainTexture = importedTexture};
                if (settings.useCubemap)
                {
                    skyboxMaterial.SetTexture("_Tex", importedTexture);
                    skyboxMaterial.SetTexture("_Cube", importedTexture);
                }

                AssetDatabase.CreateAsset(skyboxMaterial, $"{settings.outputDirectory.Path}/{id}.mat");
                AssetDatabase.Refresh();
                if (settings.assignAsCurrentSkybox) RenderSettings.skybox = skyboxMaterial;
                lastImportedMaterial = skyboxMaterial;
            }

            lastImportedTexture = importedTexture;
            loading = false;
            Debug.Log("Done with import");
        }
    }
}