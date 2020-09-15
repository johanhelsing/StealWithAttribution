using UnityEditor;
using UnityEngine.UIElements;

namespace StealWithAttribution.Editor
{
    // [CustomEditor(typeof(TextureSettings))]
    public class TextureImporterEditorUIElements : UnityEditor.Editor
    {
        private HdriImportSettings settings;

        public override VisualElement CreateInspectorGUI()
        {
            // settings = (TextureSettings) target;
            var root = new VisualElement();
            var label = new Label("Hello World! From C#");
            root.Add(label);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("AssetGet/Editor/TextureImporterEditor.uxml");
            var labelFromUXML = visualTree.CloneTree();
            root.Add(labelFromUXML);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/AssetGet/Editor/TextureImporterEditor.uss");
            root.styleSheets.Add(styleSheet);
            return root;
        }
    }
}