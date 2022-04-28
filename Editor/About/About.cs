using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NKStudio.UnitySnapToFloor
{
    public class About : EditorWindow
    {
        private const string KResourcePath = "Assets/Plugins/UnitySnapToFloor/Editor/About/About.uxml";
    
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