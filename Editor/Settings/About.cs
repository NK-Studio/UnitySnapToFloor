using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class About : EditorWindow
{
    const string k_ResourcePath = "Packages/UnitySnapToFloor/Editor/Settings/About.uxml";
    
    
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
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_ResourcePath);
        VisualElement container = visualTree.Instantiate();
        root.Add(container);
    }
}