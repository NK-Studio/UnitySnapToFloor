using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SnapToFloorEditor : EditorWindow
{
    
    
    [MenuItem("Window/SnapToFloor")]
    public static void Title()
    {
        SnapToFloorEditor wnd = GetWindow<SnapToFloorEditor>();
        wnd.titleContent = new GUIContent("SnapToFloorSetting");
    }

    public void CreateGUI()
    {
        #region 기본 설정

        // 각 편집기 창에는 루트 VisualElement 개체가 포함되어 있습니다.
        VisualElement root = rootVisualElement;

        //UXML 경로
        string path = AssetDatabase.GUIDToAssetPath("5e2b8bced1177ba4fb631ef5aa5fe2eb");

        //UXML 가져오기
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        VisualElement container = visualTree.Instantiate();
        root.Add(container);

        #endregion

        //현재 유니티 모드가 무엇인지 가져옵니다.
        var behaviorMode = EditorSettings.defaultBehaviorMode;

        //드롭다운 데이터 가져오기
        DropdownField dropdownField = root.Q<DropdownField>("unity-drop");
        Button button = root.Q<Button>("unity-apply");


        //드롭다운 인덱스 가져오기
        var dropdownIndex = EditorPrefs.GetInt("SnapToFloor-Mode", (int) behaviorMode);
        dropdownField.index = dropdownIndex;

        //현재 선택된 빌트 타겟에 처리합니다.
        List<string> defines = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

        int modeIndex = dropdownIndex;
        dropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
        {
            string mode = evt.newValue;

            switch (mode)
            {
                case "3D":
                    modeIndex = 0;
                    break;
                case "2D":
                    modeIndex = 1;
                    break;
                default:
                    modeIndex = 2;
                    break;
            }
        });


        
        button.RegisterCallback<MouseUpEvent>(evt =>
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(RefreshDefine(modeIndex,defines));
        });
    }

    private IEnumerator RefreshDefine(int modeIndex, List<string> defines)
    {
        switch (modeIndex)
        {
            case 0:
                defines.Add("SNAP2FLOOR_3D");
                defines.Remove("SNAP2FLOOR_2D");
                defines.Remove("SNAP2FLOOR_BOTH");
                break;
            case 1:
                defines.Add("SNAP2FLOOR_2D");
                defines.Remove("SNAP2FLOOR_3D");
                defines.Remove("SNAP2FLOOR_BOTH");
                break;
            case 2:
                defines.Add("SNAP2FLOOR_BOTH");
                defines.Remove("SNAP2FLOOR_2D");
                defines.Remove("SNAP2FLOOR_3D");
                break;
        }

        //디파인 중복 제거
        defines = defines.Distinct().ToList();
            
        //문자열 다시 합친후 심볼(디파인) 적용 
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", defines.ToArray()));

        EditorPrefs.SetInt("SnapToFloor-Mode", modeIndex);
        
        yield return null;
    }
}