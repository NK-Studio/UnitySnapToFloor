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

    private static bool ShowOnStartup => EditorPrefs.GetBool(KShowOnStartupPreference, true);

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
        //settings파일의 ID를 가져옵니다.
        int id = EditorPrefs.GetInt("SettingsInstanceID", 0);
        string settingsPath = AssetDatabase.GetAssetPath(id);

        //항상 켜지도록 처리
        bool autoShowOnStartup = true;

        //파일이 있을 경우 파일 데이터 반영
        if (!settingsPath.Equals(""))
        {
            //세팅 데이터를 가져옵니다.
            SnapToFloorSettings settings = AssetDatabase.LoadAssetAtPath<SnapToFloorSettings>(settingsPath);

            //항상 켜기 일 경우
            autoShowOnStartup = settings.ShowUp == SnapToFloorSettings.StarUp.Always;
        }

        //켜지게할지 말지 처리
        if (ShowOnStartup && autoShowOnStartup)
            EditorApplication.update += ShowAtStartup;
    }

    private void OnDestroy() => EditorApplication.update -= ShowAtStartup;

    static void ShowAtStartup()
    {
        if (!Application.isPlaying)
            Title();

        EditorApplication.update -= ShowAtStartup;
    }

    public void CreateGUI()
    {
        #region 기본 설정

        //사용중인 시스템 언어 가져오기
        SystemLanguage language = Application.systemLanguage;

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

        //ID를 가져와서 경로를 구합니다.
        int id = EditorPrefs.GetInt("SettingsInstanceID", 0);
        string settingsPath = AssetDatabase.GetAssetPath(id);

        //값을 재대로 가져왔다면,
        if (!settingsPath.Equals(""))
        {
            //에셋 데이터를 가져옵니다.
            settingField.value = AssetDatabase.LoadAssetAtPath<SnapToFloorSettings>(settingsPath);

            //에디터를 세팅합니다.
            EditorSetUp((SnapToFloorSettings) settingField.value);
        }
        else
        {
            //기초 설정
            SnapToFloorSettings settings = CreateInstance<SnapToFloorSettings>();
            settings.Mode = (SnapToFloorSettings.SnapMode) behaviorMode;
            settings.Language = language == SystemLanguage.Korean
                ? SnapToFloorSettings.ELanguage.한국어
                : SnapToFloorSettings.ELanguage.English;
            settings.ShowUp = SnapToFloorSettings.StarUp.Always;
            EditorSetUp(settings);
        }

        //현재 선택된 빌트 타겟에 처리합니다.
        var defines = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

        //모드 변경
        modeDropDown.RegisterValueChangedCallback(evt =>
        {
            Assert.IsNotNull(settingField.value,
                language == SystemLanguage.Korean
                    ? "Mode Error : Settings 필드가 Null입니다."
                    : "Mode Error : the settings field is null");

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
            }
        });

        showDropDown.RegisterValueChangedCallback(evt =>
        {
            Assert.IsNotNull(settingField.value,
                language == SystemLanguage.Korean
                    ? "Start at show Error : Settings 필드가 Null입니다."
                    : "Start at show Error : the settings field is null");

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
            Assert.IsNotNull(settingField.value,
                language == SystemLanguage.Korean
                    ? "Language Error : Settings 필드가 Null입니다."
                    : "Language Error : the settings field is null");
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

        applyButton.RegisterCallback<MouseUpEvent>(_ =>
        {
            Assert.IsNotNull(settingField.value,
                language == SystemLanguage.Korean
                    ? "Apply Error : Settings 필드가 Null입니다."
                    : "Apply Error : the settings field is null");

            SnapToFloorSettings settings = (SnapToFloorSettings) settingField.value;

            EditorCoroutineUtility.StartCoroutineOwnerless(RefreshDefine(settings.Mode, defines));
        });

        createButton.RegisterCallback<MouseUpEvent>(_ =>
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsFile = File.Exists("Assets/Settings/STFAsset.asset");

            //세팅스 프로필
            SnapToFloorSettings settings;

            //가지고 있다면 에러 표시
            if (hasSettingsFile)
            {
                settings = (SnapToFloorSettings) settingField.value;
                string warningMsg = settings.Language == SnapToFloorSettings.ELanguage.English
                    ? "There is already a STFAasset file in the Settings folder."
                    : "이미 Settings 폴더에 STFAsseet파일이 있습니다.";
                Debug.LogWarning(warningMsg);

                return;
            }

            bool hasSettingsDir = Directory.Exists($"{Application.dataPath}/Settings");

            if (!hasSettingsDir)
                Directory.CreateDirectory($"{Application.dataPath}/Settings");

            #region 실제 에셋 생성

            settings = CreateInstance<SnapToFloorSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Settings/STFAsset.asset");

            #endregion

            #region 값 초기화

            settings.Mode = (SnapToFloorSettings.SnapMode) behaviorMode;
            settings.Language = language == SystemLanguage.Korean
                ? SnapToFloorSettings.ELanguage.한국어
                : SnapToFloorSettings.ELanguage.English;
            settings.ShowUp = SnapToFloorSettings.StarUp.Always;

            #endregion

            settingField.value = settings;
            EditorSetUp(settings);
            EditorPrefs.SetInt("SettingsInstanceID", settings.GetInstanceID());
            AssetDatabase.Refresh();

            Debug.Log(language == SystemLanguage.Korean ? "STFAsseet파일이 생성 됨" : "stfaasset file is created");
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

        void EditorSetUp(SnapToFloorSettings snapToFloorSettings)
        {
            modeDropDown.Init(snapToFloorSettings.Mode);
            showDropDown.Init(snapToFloorSettings.ShowUp);
            languageDropDown.Init(snapToFloorSettings.Language);

            //언어 체인지
            ChangeLanguage(snapToFloorSettings.Language, ref snapToFloorSettings);
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
        }

        //디파인 중복 제거
        defines = defines.Distinct().ToList();

        //문자열 다시 합친후 심볼(디파인) 적용 
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", defines.ToArray()));

        yield return null;
    }
}