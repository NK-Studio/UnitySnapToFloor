using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class SnapToFloorEditor : EditorWindow
{
    private const string KResourcePath = "Packages/com.nkstudio.snap-to-floor/Editor/Window/SnapToFloor.uxml";

    private const string KSettingsFileInstallPath = "Assets/Settings/STFAsset.asset";

    private SnapToFloorSettings _settings;

    [MenuItem("Window/SnapToFloor/settings")]
    public static void Title()
    {
        SnapToFloorEditor wnd = GetWindow<SnapToFloorEditor>();
        wnd.titleContent = new GUIContent("SnapToFloorSetting");
        wnd.minSize = new Vector2(280, 300);
        wnd.maxSize = new Vector2(400, 360);
    }

#if false
        [MenuItem("Window/SnapToFloor/UnInstall")]
        public static void UnInstall()
        {
            bool check;

            if (Application.systemLanguage == SystemLanguage.Korean)
            {
                check = EditorUtility.DisplayDialog("삭제 마법사", "진짜로 SnapToFloor를 유니티에서 삭제하겠습니까?",
                    "삭제", "취소");
            }
            else
            {
                check = EditorUtility.DisplayDialog("Uninstall",
                    "Are you sure you want to delete SnapToFloor from Unity?",
                    "Uninstall", "Cancel");
            }

            if (!check) return;

            //settings파일의 ID를 가져옵니다.
            int id = EditorPrefs.GetInt("SettingsInstanceID", -1);
            string settingsPath = AssetDatabase.GetAssetPath(id);
            bool hasSettingsFile = !string.IsNullOrEmpty(settingsPath);

            string defines =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings
                    .selectedBuildTargetGroup);

            //파일이 있을 경우 파일 데이터 반영
            if (hasSettingsFile)
            {
                SnapToFloorSettings instance = AssetDatabase.LoadAssetAtPath<SnapToFloorSettings>(settingsPath);

                string removed =
                    defines.Replace(
                        instance.Mode == SnapToFloorSettings.KSnapMode.Mode3D ? "SNAP2FLOOR_3D" : "SNAP2FLOOR_2D", "");

                //파일 삭제
                File.Delete(KSettingsFileInstallPath);

                //디파인 삭제
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    removed);
            }

            //에디터에서 삭제
            UnityEditor.PackageManager.Client.Remove("com.nkstudio.snap-to-floor");
        }
#endif
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        //settings파일의 ID를 가져옵니다.
        int id = EditorPrefs.GetInt("SettingsInstanceID", -1);
        string settingsPath = AssetDatabase.GetAssetPath(id);
        bool hasSettingsFile = !string.IsNullOrEmpty(settingsPath);

        //파일이 있을 경우 파일 데이터 반영
        if (hasSettingsFile)
        {
            SnapToFloorSettings instance = AssetDatabase.LoadAssetAtPath<SnapToFloorSettings>(settingsPath);
            bool showOnStartup = instance.StartAtShow == SnapToFloorSettings.KStartAtShow.Always;

            if (showOnStartup)
                EditorApplication.update += ShowAtStartup;
        }
        else
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

        // 각 편집기 창에는 루트 VisualElement 개체가 포함되어 있습니다.
        VisualElement root = rootVisualElement;

        //UXML 가져오기
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(KResourcePath);
        VisualElement container = visualTree.Instantiate();
        root.Add(container);

        #endregion

        #region DropdownField

        DropdownField modeDropDown = root.Q<DropdownField>("mode-DropDown");
        DropdownField languageDropDown = root.Q<DropdownField>("language-DropDown");
        DropdownField startAtShowDropDown = root.Q<DropdownField>("startAtShow-DropDown");

        #endregion

        #region ObjectField

        ObjectField settingField = container.Q<ObjectField>("setting-Filed");
        settingField.objectType = typeof(SnapToFloorSettings);
        settingField.allowSceneObjects = false;

        #endregion

        #region Button

        Button applyButton = root.Q<Button>("apply-Button");
        Button createButton = root.Q<Button>("createSettings-Button");

        #endregion

        #region Label

        Label howToUseLabel = root.Q<Label>("howToUse-Text");
        Label descriptionLabel = root.Q<Label>("description-Text");

        #endregion

        InitGUI();

        //세팅 파일 생성
        createButton.RegisterCallback<MouseUpEvent>(_ => CreateSettingsFile());

        //모드 변경
        modeDropDown.RegisterValueChangedCallback(_ => ChangeMode());

        startAtShowDropDown.RegisterValueChangedCallback(_ => ChangeStartAtShow());

        languageDropDown.RegisterValueChangedCallback(_ => ChangeLanguage());

        applyButton.RegisterCallback<MouseUpEvent>(_ => Apply());

        //오브젝트가 변함을 체크
        settingField.RegisterCallback<ChangeEvent<Object>>(SettingsFieldChangeListener);

        void InitGUI()
        {
            //ID를 가져와서 경로를 구합니다.
            int id = EditorPrefs.GetInt("SettingsInstanceID", -1);
            string assetPath = AssetDatabase.GetAssetPath(id);
            bool hasSettingsFile = !string.IsNullOrEmpty(assetPath);

            //드롭다운에 내용을 추가합니다.
            languageDropDown.choices = Enum.GetNames(typeof(SnapToFloorSettings.KLanguage)).ToList();
            modeDropDown.choices = Enum.GetNames(typeof(SnapToFloorSettings.KSnapMode)).ToList();

            //세팅파일이 없다면 시스템 언어로 진행하고, 있으면 세팅파일의 언어 설정으로 진행한다.
            startAtShowDropDown.choices = _settings != null
                ? SnapToFloorSettings.StartAtShowText[_settings.Language]
                : SnapToFloorSettings.StartAtShowText[(SnapToFloorSettings.KLanguage) GetSystemLanguageEnOrKr()];

            //잘 가져왔다면,
            if (hasSettingsFile)
            {
                _settings = AssetDatabase.LoadAssetAtPath<SnapToFloorSettings>(assetPath);
                settingField.value = _settings;

                languageDropDown.index = (int) _settings.Language;
                startAtShowDropDown.index = (int) _settings.StartAtShow;
                modeDropDown.index = (int) _settings.Mode;

                languageDropDown.SetEnabled(true);
                startAtShowDropDown.SetEnabled(true);
                modeDropDown.SetEnabled(true);
                applyButton.SetEnabled(true);
            }
            else
            {
                languageDropDown.index = GetSystemLanguageEnOrKr();
                modeDropDown.index = GetEditorMode();
                startAtShowDropDown.index = 0;

                languageDropDown.SetEnabled(false);
                startAtShowDropDown.SetEnabled(false);
                modeDropDown.SetEnabled(false);
                applyButton.SetEnabled(false);
            }

            //언어 체인지를 합니다.
            SetLanguage(true);
        }

        void EditorSetUp()
        {
            //세팅 데이터를 가지고 있는지 가져옵니다.
            bool hasSettingsData = settingField.value != null && _settings != null;

            //파일이 없으면 에러를 표시합니다.
            Assert.IsTrue(hasSettingsData, GetSystemLanguageEnOrKr() == 1
                ? "EditorSetUp Error : 데이터가 없습니다."
                : "EditorSetUp Error : no data");

            //드롭다운에 내용을 추가합니다.
            languageDropDown.choices = Enum.GetNames(typeof(SnapToFloorSettings.KLanguage)).ToList();
            modeDropDown.choices = Enum.GetNames(typeof(SnapToFloorSettings.KSnapMode)).ToList();

            //세팅파일이 없다면 시스템 언어로 진행하고, 있으면 세팅파일의 언어 설정으로 진행한다.
            startAtShowDropDown.choices = _settings != null
                ? SnapToFloorSettings.StartAtShowText[_settings.Language]
                : SnapToFloorSettings.StartAtShowText[(SnapToFloorSettings.KLanguage) GetSystemLanguageEnOrKr()];

            //인덱스를 적용합니다.
            if (_settings != null)
            {
                languageDropDown.index = (int) _settings.Language;
                startAtShowDropDown.index = (int) _settings.StartAtShow;
                modeDropDown.index = (int) _settings.Mode;
            }

            //클릭할 수 있도록 활성화
            languageDropDown.SetEnabled(true);
            startAtShowDropDown.SetEnabled(true);
            modeDropDown.SetEnabled(true);

            //저장된 세팅파일에 맞춰서 언어를 변경합니다.
            SetLanguage(true);
        }

        void CreateSettingsFile()
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsFile = File.Exists(KSettingsFileInstallPath);

            if (hasSettingsFile)
            {
                //파일을 가지고 있으면 에러를 표시합니다.
                string message = GetSystemLanguageEnOrKr() == 1
                    ? "CreateSettingsFile Error : Settings 파일을 가지고 있습니다."
                    : "CreateSettingsFile Error : I have a settings file in my project";

                //경고
                Debug.LogWarning(message);
                return;
            }

            bool hasSettingsDir = Directory.Exists($"{Application.dataPath}/Settings");

            if (!hasSettingsDir)
                Directory.CreateDirectory($"{Application.dataPath}/Settings");

            //인스턴스 생성
            _settings = CreateInstance<SnapToFloorSettings>();
            settingField.value = _settings;

            //값 설정
            _settings.Mode = (SnapToFloorSettings.KSnapMode) GetEditorMode();
            _settings.Language = (SnapToFloorSettings.KLanguage) GetSystemLanguageEnOrKr();
            _settings.StartAtShow = SnapToFloorSettings.KStartAtShow.Always;

            //에셋 생성
            AssetDatabase.CreateAsset(_settings, "Assets/Settings/STFAsset.asset");

            //인스턴스 ID 생성 및 새로고침
            EditorPrefs.SetInt("SettingsInstanceID", _settings.GetInstanceID());
            AssetDatabase.Refresh();

            //언어 맞춰서 생성 표시
            Debug.Log(GetSystemLanguageEnOrKr() == (int) SnapToFloorSettings.KLanguage.한국어
                ? "STFAsset 파일이 생성 됨"
                : "STFAsset file is created");

            //새로 고침
            AssetDatabase.Refresh();

            //파일을 적용함
            EditorSetUp();
        }

        void ChangeMode()
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsData = settingField.value != null && _settings != null;

            //파일을 가지고 있으면 에러를 표시합니다.
            if (!hasSettingsData) return;

            //모드를 변경합니다.
            _settings.Mode = (SnapToFloorSettings.KSnapMode) modeDropDown.index;
        }

        void ChangeLanguage()
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsData = settingField.value != null && _settings != null;

            if (!hasSettingsData) return;

            _settings.Language = (SnapToFloorSettings.KLanguage) languageDropDown.index;
            SetLanguage(true);
        }

        void SetLanguage(bool changeStartAtShow = false)
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsData = settingField.value != null && _settings != null;

            //초기 선언
            SnapToFloorSettings.KLanguage language;

            //시스템 언어를 가져옵니다.
            int systemLanguageEnOrKr = GetSystemLanguageEnOrKr();

            if (hasSettingsData)
            {
                //세팅파일이 있다면 세팅 파일의 언어로 진행합니다.
                language = _settings.Language;
            }
            else
                //세팅파일이 없다면 시스템 언어로 진행하고, 있으면 세팅파일의 언어 설정으로 진행한다. 
                language = (SnapToFloorSettings.KLanguage) systemLanguageEnOrKr;

            //세팅파일이 없다면 시스템 언어로 진행하고, 있으면 세팅파일의 언어 설정으로 진행한다.
            if (changeStartAtShow)
            {
                startAtShowDropDown.choices = language == SnapToFloorSettings.KLanguage.English
                    ? SnapToFloorSettings.StartAtShowText[SnapToFloorSettings.KLanguage.English]
                    : SnapToFloorSettings.StartAtShowText[SnapToFloorSettings.KLanguage.한국어];

                if (hasSettingsData)
                    startAtShowDropDown.index = (int) _settings.StartAtShow;
                else
                    startAtShowDropDown.index = 0;
            }

            switch (language)
            {
                case SnapToFloorSettings.KLanguage.English:
                    applyButton.text = "Apply";
                    modeDropDown.label = "Use Mode ?";
                    howToUseLabel.text = "How to use ?";
                    descriptionLabel.text = "Check whether the Unity mode is 2D or 3D.";
                    languageDropDown.label = "Language";
                    startAtShowDropDown.label = "Show at StartUp";
                    createButton.text = "Create";

                    modeDropDown.tooltip = "Choose a mode according to your project." +
                                           "If you're making 2.5D, if you choose 3D in a 2D project, the character will use the sprite renderer and the terrain will use the 3D object.";
                    languageDropDown.tooltip = "You can change the language in the settings window of SnapToFloor.";
                    startAtShowDropDown.tooltip =
                        "After setting, change this option to 'Never' so that it will not be created after compilation.";
                    break;

                case SnapToFloorSettings.KLanguage.한국어:
                    applyButton.text = "적용";
                    modeDropDown.label = "사용하는 모드 ?";
                    howToUseLabel.text = "사용법 ?";
                    descriptionLabel.text = "자신이 사용하는 유니티 모드가 2D/3D에 맞춰 선택하고 적용을 누르세요.";
                    languageDropDown.label = "언어";
                    startAtShowDropDown.label = "시작 시 표시";
                    createButton.text = "생성";

                    modeDropDown.tooltip = "프로젝트에 맞춰서 모드를 선택합니다." +
                                           "2.5D를 만드는 경우, 2D프로젝트에서 3D를 선택하면 캐릭터는 스프라이트 렌더러를 사용하고 지형은 3D오브젝트를 사용할 수 있습니다.";
                    languageDropDown.tooltip = "SnapToFloor의 세팅창에 언어를 변경할 수 있습니다.";
                    startAtShowDropDown.tooltip = "설정을 완료하면 이 옵션을 '끄기'로 변경하여 컴파일 이후 설정창이 생성되는 것을 끌 수 있습니다.";
                    break;
            }
        }

        void ChangeStartAtShow()
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsData = settingField.value != null && _settings != null;

            if (!hasSettingsData) return;

            //값을 적용
            _settings.StartAtShow = (SnapToFloorSettings.KStartAtShow) startAtShowDropDown.index;
        }

        void SettingsFieldChangeListener(ChangeEvent<Object> changeEvent)
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsFile = changeEvent.newValue != null;

            //따로 빼버렸다면 ID저장을 뺀다
            if (hasSettingsFile)
            {
                //세팅 값을 넣습니다.
                _settings = (SnapToFloorSettings) changeEvent.newValue;
                settingField.value = _settings;

                //Settings의 id입니다.
                int id = _settings.GetInstanceID();

                //인스턴스ID를 저장합니다.
                EditorPrefs.SetInt("SettingsInstanceID", id);
            }
            else
            {
                //인스턴스 ID를 제거합니다.
                EditorPrefs.DeleteKey("SettingsInstanceID");
                settingField.value = null;
            }

            //초기화 합니다.
            InitGUI();

            if (!hasSettingsFile)
                RemoveDefine();
        }

        void Apply()
        {
            //파일을 가지고 있는지 체크
            bool hasSettingsData = settingField.value != null && _settings != null;

            //파일을 가지고 있으면 에러를 표시합니다.
            Assert.IsTrue(hasSettingsData, GetSystemLanguageEnOrKr() == 1
                ? "Apply Error : Settings 필드가 Null입니다."
                : "Apply Error : the settings field is null"
            );

            if (_settings != null)
                AddDefine(_settings.Mode, _settings.Language);
        }
    }

    private static void AddDefine(SnapToFloorSettings.KSnapMode modeIndex, SnapToFloorSettings.KLanguage language)
    {
        //현재 선택된 빌트 타겟에 처리합니다.
        var defines = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';')
            .ToList();

        switch (modeIndex)
        {
            case SnapToFloorSettings.KSnapMode.Mode3D:

                if (defines.Contains("SNAP2FLOOR_3D"))
                {
                    Debug.LogWarning(language == SnapToFloorSettings.KLanguage.English
                        ? "It has already been applied."
                        : "이미 적용이 완료되었습니다.");
                    return;
                }

                defines.Add("SNAP2FLOOR_3D");
                defines.Remove("SNAP2FLOOR_2D");
                break;
            case SnapToFloorSettings.KSnapMode.Mode2D:
                if (defines.Contains("SNAP2FLOOR_2D"))
                {
                    Debug.LogWarning(language == SnapToFloorSettings.KLanguage.English
                        ? "It has already been applied."
                        : "이미 적용이 완료되었습니다.");
                    return;
                }

                defines.Add("SNAP2FLOOR_2D");
                defines.Remove("SNAP2FLOOR_3D");
                break;
        }

        //디파인 중복 제거
        defines = defines.Distinct().ToList();

        //문자열 다시 합친후 심볼(디파인) 적용 
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", defines.ToArray()));
    }

    private static void RemoveDefine()
    {
        //현재 선택된 빌트 타겟에 처리합니다.
        var defines = PlayerSettings
            .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';')
            .ToList();

        //디파인에 없었다면,
        if (!(defines.Contains("SNAP2FLOOR_2D") || defines.Contains("SNAP2FLOOR_3D")))
            return;

        //제거
        defines.Remove("SNAP2FLOOR_2D");
        defines.Remove("SNAP2FLOOR_3D");

        //디파인 중복 제거
        defines = defines.Distinct().ToList();

        //문자열 다시 합친후 심볼(디파인) 적용 
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", defines.ToArray()));
    }

    /// <summary>
    /// 시스템 언어가 한국어이면 1, 아니면 0을 반환한다.
    /// </summary>
    /// <returns></returns>
    private static int GetSystemLanguageEnOrKr() =>
        Application.systemLanguage == SystemLanguage.Korean ? 1 : 0;

    /// <summary>
    /// 3D모드이면 0, 2D이면 1을 반환합니다.
    /// </summary>
    /// <returns></returns>
    private static int GetEditorMode() => (int) EditorSettings.defaultBehaviorMode;
}