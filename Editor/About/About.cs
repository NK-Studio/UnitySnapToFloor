using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnitySnapToFloor.Editor.About
{
    public class About : EditorWindow
    {
#if false
        private const string KResourcePath = "Packages/com.nkstudio.snap-to-floor/Editor/About/About.uxml";
#else
        private const string KResourcePath = "Assets/UnitySnapToFloor/Editor/About/About.uxml";
#endif
        [MenuItem("Window/SnapToFloor/About")]
        public static void ShowExample()
        {
            About wnd = GetWindow<About>();
            wnd.titleContent = new GUIContent("About");
            wnd.minSize = new Vector2(350, 120);
            wnd.maxSize = new Vector2(350, 120);
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(KResourcePath);
            VisualElement container = visualTree.Instantiate();
            root.Add(container);
        }
    }
}