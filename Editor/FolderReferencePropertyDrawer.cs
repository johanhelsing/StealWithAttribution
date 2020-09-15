using System.IO;
using UnityEditor;
using UnityEngine;

namespace StealWithAttribution.Editor
{
    [CustomPropertyDrawer(typeof(FolderReference))]
    public class FolderReferencePropertyDrawer : PropertyDrawer
    {
        private bool initialized;
        private SerializedProperty guid;
        private Object obj;

        private void Init(SerializedProperty property)
        {
            initialized = true;
            guid = property.FindPropertyRelative(nameof(FolderReference.guid));
            obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid.stringValue));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!initialized) Init(property);

            var guiContent = EditorGUIUtility.ObjectContent(obj, typeof(DefaultAsset));

            var r = EditorGUI.PrefixLabel(position, label);

            var textFieldRect = r;
            textFieldRect.width -= 19f;

            var textFieldStyle = new GUIStyle("TextField")
            {
                imagePosition = obj ? ImagePosition.ImageLeft : ImagePosition.TextOnly
            };

            if (GUI.Button(textFieldRect, guiContent, textFieldStyle) && obj)
                EditorGUIUtility.PingObject(obj);

            if (textFieldRect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                    {
                        var reference = DragAndDrop.objectReferences[0];
                        var path = AssetDatabase.GetAssetPath(reference);
                        DragAndDrop.visualMode = Directory.Exists(path) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                        Event.current.Use();
                        break;
                    }
                    case EventType.DragPerform:
                    {
                        var reference = DragAndDrop.objectReferences[0];
                        var path = AssetDatabase.GetAssetPath(reference);
                        if (Directory.Exists(path))
                        {
                            obj = reference;
                            guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                        }
                        Event.current.Use();
                        break;
                    }
                }
            }

            var objectFieldRect = r;
            objectFieldRect.x = textFieldRect.xMax + 1f;
            objectFieldRect.width = 19f;

            if (GUI.Button(objectFieldRect, "", GUI.skin.GetStyle("IN ObjectField")))
            {
                var path = EditorUtility.OpenFolderPanel("Select a folder", "Assets", "");
                if (path.Contains(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    obj = AssetDatabase.LoadAssetAtPath(path, typeof(DefaultAsset));
                    guid.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
                }
                else Debug.LogError("The path must be in the Assets folder");
            }
        }
    }
}