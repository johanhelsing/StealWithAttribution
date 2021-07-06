using Cysharp.Threading.Tasks;
using StealWithAttribution.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StealWithAttribution.Sketchfab.Editor
{
    public class SketchfabBrowser : EditorWindow
    {
        private TextField userField, passField, importUrlField;
        private GltfImportSettings settings;

        private const string UserKey = "stealwithattribution_sketchfab_user";
        private const string PassKey = "stealwithattribution_sketchfab_pass";

        public static void Open(GltfImportSettings settings)
        {
            var window = GetWindow(typeof(SketchfabBrowser)) as SketchfabBrowser;
            Debug.Assert(window != null);
            window.settings = settings;
        }

        private string User
        {
            get => EditorPrefs.GetString(UserKey);
            set => EditorPrefs.SetString(UserKey, value);
        }

        private string Pass
        {
            get => SessionState.GetString(PassKey, "");
            set => SessionState.SetString(PassKey, value);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Sketchfab browser");
        }

        // ReSharper disable once UnusedMember.Local // event function
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(userField = new TextField
            {
                label = "Username", value = User
            });
            root.Add(passField = new TextField
            {
                label = "Password", value = Pass, isPasswordField = true
            });
            root.Add(new Button(() =>
            {
                User = userField.value;
                Pass = passField.value;
                Debug.Log($"Logging in user {User}");
                SketchfabApi.Login(User, Pass).Forget();
            })
            {
                text = "Login"
            });

            root.Add(importUrlField = new TextField
            {
                label = "Url"
            });
            root.Add(new Button(() => DownloadAndImportModel(importUrlField.text).Forget())
            {
                text = "Import"
            });
        }

        private async UniTask DownloadAndImportModel(string url)
        {
            var uid = SketchfabApi.GetUidFromUrl(url);
            var downloadUrl = await SketchfabApi.GetDownloadUrl(uid);
            var folderName = Utils.FileNameFromUrl(downloadUrl).Replace(".zip", "");
            await Utils.DownloadAndUnzip(
                downloadUrl,
                $"{settings.outputDirectory.AbsolutePath}/{folderName}"
            );
            AssetDatabase.Refresh();
        }
    }
}