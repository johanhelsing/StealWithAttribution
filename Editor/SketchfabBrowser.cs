using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StealWithAttribution.Sketchfab.Editor
{
    public class SketchfabBrowser : EditorWindow
    {
        public static void Open() => GetWindow(typeof(SketchfabBrowser));
        private TextField userField, passField;

        private const string UserKey = "stealwithattribution_sketchfab_user";
        private const string PassKey = "stealwithattribution_sketchfab_pass";

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

        public void OnEnable()
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

            root.Add(new Button(() =>
            {
                const string uid = "36d8dcfadeda4c939bd92b30bfad6d1d";
                SketchfabApi.GetDownloadUrl(uid).Forget();
            }));
        }

        // SketchfabRequest tokenRequest = new SketchfabRequest(SketchfabPlugin.Urls.oauth, formData);
    }
}