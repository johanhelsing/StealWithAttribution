using StealWithAttribution.Sketchfab.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StealWithAttribution.Editor
{
    [CreateAssetMenu(menuName = "Steal With Attribution/GLTF importer")]
    public class GltfImportSettings : ScriptableObject
    {
        [SerializeField] public FolderReference outputDirectory;
    }

    [CustomEditor(typeof(GltfImportSettings))]
    public class GltfImportSettingsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var settings = target as GltfImportSettings;
            var root = new VisualElement();

            var serializedObject = new SerializedObject(settings);
            root.Add(new PropertyField(serializedObject.FindProperty(nameof(GltfImportSettings.outputDirectory))));
            // var normalFields = base.CreateInspectorGUI();
            // root.Add(normalFields);
            root.Add(new Button(() => SketchfabBrowser.Open(settings))
            {
                text = "Sketchfab browser"
            });
            return root;
        }
    }
}