using UnityEditor;

namespace StealWithAttribution.Editor
{
    [System.Serializable]
    public class FolderReference
    {
        public string guid;
        public string Path => AssetDatabase.GUIDToAssetPath(guid);
        public string AbsolutePath => Utils.ResolveAssetPath(Path);
    }
}