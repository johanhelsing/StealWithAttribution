using UnityEngine;

namespace StealWithAttribution.Editor
{
    [CreateAssetMenu(menuName = "Steal With Attribution/HDRI importer")]
    public class HdriImportSettings : ScriptableObject
    {
        [SerializeField] public FolderReference outputDirectory;
        [SerializeField] public bool createSkyboxMaterial = true;
        [SerializeField] public bool useCubemap = true;
        [SerializeField] public bool assignAsCurrentSkybox = true;
        [SerializeField] public HdriHavenApi.Resolution resolution = HdriHavenApi.Resolution._2k;
    }
}
