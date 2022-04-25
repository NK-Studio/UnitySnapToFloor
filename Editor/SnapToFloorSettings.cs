using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class SnapToFloorEditor : EditorWindow
{
    private const string KShowOnStartupPreference = "NKStudio.SnapToFloor.ShowAtStartup";

    private static bool ShowOnStartup
    {
        get => EditorPrefs.GetBool(KShowOnStartupPreference, true);
        set
        {
            if (value != ShowOnStartup) EditorPrefs.SetBool(KShowOnStartupPreference, value);
        }
    }

    [MenuItem("Window/SnapToFloor")]
    public static void Title()
    {
        SnapToFloorEditor wnd = GetWindow<SnapToFloorEditor>();
        wnd.titleContent = new GUIContent("SnapToFloorSetting");
        wnd.minSize = new Vector2(280, 300);
        wnd.maxSize = new Vector2(400, 360);
    }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        bool autoShowOnStartup = EditorPrefs.GetInt($"SnapToFloor-show-{Application.productName}", 0) == 0;

        if (ShowOnStartup && autoShowOnStartup)
            EditorApplication.update += ShowAtStartup;
    }

    private void OnDestroy()
    {
        EditorApplication.update -= ShowAtStartup;
    }

    static void ShowAtStartup()
    {
        if (!Application.isPlaying)
            Title();

        EditorApplication.update -= ShowAtStartup;
    }

    public void CreateGUI()
    {
        #region 기본 설정

        // 각 편집기 창에는 루트 VisualElement 개체가 포함되어 있습니다.
        VisualElement root = rootVisualElement;

        //UXML 경로
        string path = AssetDatabase.GUIDToAssetPath("5e2b8bced1177ba4fb631ef5aa5fe2eb");

        //UXML 가져오기
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        VisualElement container = visualTree.Instantiate();
        root.Add(container);

        #endregion

        #region 초기화

        //현재 유니티 모드가 무엇인지 가져옵니다.
        EditorBehaviorMode behaviorMode = EditorSettings.defaultBehaviorMode;

        //드롭다운 데이터 가져오기
        EnumField modeDropDown = root.Q<EnumField>("unity-drop");
        EnumField showDropDown = root.Q<EnumField>("unity-show");
        EnumField languageDropDown = root.Q<EnumField>("unity-language");
        ObjectField settingField = container.Q<ObjectField>("unity-setting-filed");
        Button applyButton = root.Q<Button>("unity-apply");
        Button createButton = root.Q<Button>("create-settings");
        Label htuLabel = root.Q<Label>("unity-htu");
        Label inHtuLabel = root.Q<Label>("unity-inhtu");

        //타입 지정
        settingField.objectType = typeof(SnapToFloorSettings);
        settingField.allowSceneObjects = false;

        #endregion

        //settings파일의 ID를 가져옵니다.
        int id = EditorPrefs.GetInt("SettingsInstanceID", 0);
        string settingsPath = AssetDatabase.GetAssetPath(id);

        //값을 재대로 가져왔다면,
        if (!settingsPath.Equals(""))
        {
            //에셋 데이터를 가져옵니다.
            settingField.value = AssetDatabase.LoadAssetAtPath<SnapToFloorSettings>(settingsPath);

            //필드에 대입
            //settingField.value = settings;

            //에디터를 세팅합니다.
            EditorSetUp((SnapToFloorSettings) settingField.value);
        }

        //현재 선택된 빌트 타겟에 처리합니다.
        var defines = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

        //모드 변경
        modeDropDown.RegisterValueChangedCallback(evt =>
        {
            Assert.IsNotNull(settingField.value, "Settings 필드가 Null입니다.");
            SnapToFloorSettings.SnapMode getSnapMode = (SnapToFloorSettings.SnapMode) evt.newValue;
            SnapToFloorSettings settings = (SnapToFloorSettings) settingField.value;

            switch (getSnapMode)
            {
                case SnapToFloorSettings.SnapMode._3D:
                    settings.Mode = SnapToFloorSettings.SnapMode._3D;
                    break;
                case SnapToFloorSettings.SnapMode._2D:
                    settings.Mode = SnapToFloorSettings.SnapMode._2D;
                    break;
                default:
                    settings.Mode = SnapToFloorSettings.SnapMode.Both;
                    break;
            }
        });

        showDropDown.RegisterValueChangedCallback(evt =>
        {
            Assert.IsNotNull(settingField.value, "Settings 필드가 Null입니다.");

            SnapToFloorSettings.StarUp getShowUp = (SnapToFloorSettings.StarUp) evt.newValue;
            SnapToFloorSettings settings = (SnapToFloorSettings) settingField.value;

            switch (getShowUp)
            {
                case SnapToFloorSettings.StarUp.Always:
                    settings.ShowUp = SnapToFloorSettings.StarUp.Always;
                    break;
                case SnapToFloorSettings.StarUp.Never:
                    settings.ShowUp = SnapToFloorSettings.StarUp.Never;
                    break;
            }
        });

        languageDropDown.RegisterValueChangedCallback(evt =>
        {
            Assert.IsNotNull(settingField.value, "Settings 필드가 Null입니다.");

            SnapToFloorSettings.ELanguage getLanguage = (SnapToFloorSettings.ELanguage) evt.newValue;
            SnapToFloorSettings settings = (SnapToFloorSettings) settingField.value;

            ChangeLanguage(getLanguage, ref settings);
        });

        void ChangeLanguage(SnapToFloorSettings.ELanguage value, ref SnapToFloorSettings settings)
        {
            switch (value)
            {
                case SnapToFloorSettings.ELanguage.English:
                    settings.Language = SnapToFloorSettings.ELanguage.English;
                    applyButton.text = "Apply";
                    modeDropDown.label = "Use Mode ?";
                    htuLabel.text = "How to use ?";
                    inHtuLabel.text = "Check whether the Unity mode is 2D or 3D.";
                    languageDropDown.label = "Language";
                    showDropDown.label = "Show at StartUp";
                    break;

                case SnapToFloorSettings.ELanguage.한국어:
                    settings.Language = SnapToFloorSettings.ELanguage.한국어;
                    applyButton.text = "적용";
                    modeDropDown.label = "사용하는 모드 ?";
                    htuLabel.text = "사용법 ?";
                    inHtuLabel.text = "자신이 사용하는 유니티 모드가 2D/3D에 맞춰 선택하고 적용을 누르세요.";
                    languageDropDown.label = "언어";
                    showDropDown.label = "시작 시 표시";
                    break;
            }
        }

        applyButton.RegisterCallback<MouseUpEvent>(evt =>
        {
            Assert.IsNotNull(settingField.value, "Settings 필드가 Null입니다.");
            SnapToFloorSettings settings = (SnapToFloorSettings) settingField.value;

            EditorCoroutineUtility.StartCoroutineOwnerless(RefreshDefine(settings.Mode, defines));
        });

        createButton.RegisterCallback<MouseUpEvent>(evt =>
        {
            bool hasSettingsFile = File.Exists("Assets/Settings/STFAsset.asset");

            if (hasSettingsFile)
            {
                Debug.LogWarning("이미 Settings 폴더에 STFAsseet파일이 있습니다.");
                return;
            }

            bool hasSettingsDir = Directory.Exists($"{Application.dataPath}/Settings");

            if (!hasSettingsDir)
                Directory.CreateDirectory($"{Application.dataPath}/Settings");

            #region 실제 에셋 생성

            SnapToFloorSettings settings = CreateInstance<SnapToFloorSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Settings/STFAsset.asset");

            #endregion

            #region 값 초기화

            settings.Mode = (SnapToFloorSettings.SnapMode) behaviorMode;
            settings.Language = 0;
            settings.ShowUp = SnapToFloorSettings.StarUp.Always;

            #endregion

            settingField.value = settings;
            EditorSetUp(settings);
            EditorPrefs.SetInt("SettingsInstanceID", settings.GetInstanceID());
            AssetDatabase.Refresh();
            Debug.Log("STFAsseet파일이 생성 됨");
        });

        //오브젝트가 변함을 체크
        settingField.RegisterCallback<ChangeEvent<Object>>((evt) =>
        {
            //따로 빼버렸다면 ID저장을 뺀다
            if (evt.newValue == null)
            {
                EditorPrefs.DeleteKey("SettingsInstanceID");
                settingField.value = null;
                return;
            }

            //세팅 값을 넣습니다.
            settingField.value = (SnapToFloorSettings) evt.newValue;
            
            //인스턴스ID를 저장합니다.
            EditorPrefs.SetInt("SettingsInstanceID", settingField.value.GetInstanceID());

            //에디터를 세팅합니다.
            EditorSetUp((SnapToFloorSettings) settingField.value);
        });

        void EditorSetUp(SnapToFloorSettings _snapToFloorSettings)
        {
            modeDropDown.Init(_snapToFloorSettings.Mode);
            showDropDown.Init(_snapToFloorSettings.ShowUp);
            languageDropDown.Init(_snapToFloorSettings.Language);

            //언어 체인지
            ChangeLanguage(_snapToFloorSettings.Language, ref _snapToFloorSettings);
        }
    }


    private IEnumerator RefreshDefine(SnapToFloorSettings.SnapMode modeIndex, List<string> defines)
    {
        switch (modeIndex)
        {
            case SnapToFloorSettings.SnapMode._3D:
                defines.Add("SNAP2FLOOR_3D");
                defines.Remove("SNAP2FLOOR_2D");
                defines.Remove("SNAP2FLOOR_BOTH");
                break;
            case SnapToFloorSettings.SnapMode._2D:
                defines.Add("SNAP2FLOOR_2D");
                defines.Remove("SNAP2FLOOR_3D");
                defines.Remove("SNAP2FLOOR_BOTH");
                break;
            case SnapToFloorSettings.SnapMode.Both:
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

        yield return null;
    }
}