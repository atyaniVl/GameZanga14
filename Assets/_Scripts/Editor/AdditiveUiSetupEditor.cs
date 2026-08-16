#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AdditiveUiSetupEditor
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string PauseScenePath = "Assets/Scenes/PauseMenu.unity";
    private const string SettingsScenePath = "Assets/Scenes/Settings.unity";

    [MenuItem("Tools/Zanga UI/Rebuild Additive UI Scenes")]
    public static void RebuildAdditiveUiScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        DeleteAssetIfExists(PauseScenePath);
        DeleteAssetIfExists(SettingsScenePath);
        DeleteAssetIfExists(MainMenuScenePath);

        SetupAdditiveUiScenes();
    }

    [MenuItem("Tools/Zanga UI/Setup Additive UI Scenes")]
    public static void SetupAdditiveUiScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder("Assets/Scenes");

        var startingScene = SceneManager.GetActiveScene();

        try
        {
            SetupMainMenuScene();
            SetupPauseScene();
            SetupSettingsScene();
            AddScenesToBuildSettings();
            SetupManagersInAllBuildScenes();
            ValidateAdditiveUiSetupInternal(logSummary: true, autoFix: false);

            if (startingScene.IsValid() && !string.IsNullOrWhiteSpace(startingScene.path))
                EditorSceneManager.OpenScene(startingScene.path, OpenSceneMode.Single);

            Debug.Log("[AdditiveUiSetupEditor] Additive UI setup complete.");
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Tools/Zanga UI/Validate Additive UI Setup")]
    public static void ValidateAdditiveUiSetup()
    {
        ValidateAdditiveUiSetupInternal(logSummary: true, autoFix: false);
    }

    [MenuItem("Tools/Zanga UI/Validate And Auto-Fix Additive UI Setup")]
    public static void ValidateAndAutoFixAdditiveUiSetup()
    {
        ValidateAdditiveUiSetupInternal(logSummary: true, autoFix: true);
    }

    private static void SetupPauseScene()
    {
        var scene = OpenOrCreateScene(PauseScenePath);
        SceneManager.SetActiveScene(scene);

        EnsureEventSystem();

        var canvas = EnsureCanvas("PauseMenuCanvas");
        var panel = EnsurePanel(canvas.transform, "PausePanel", new Color(0f, 0f, 0f, 0.72f));

        var buttonRoot = EnsureVerticalRoot(panel.transform, "PauseButtons");

        var resumeButton = EnsureButton(buttonRoot, "ResumeButton", "Resume");
        var settingsButton = EnsureButton(buttonRoot, "SettingsButton", "Settings");
        var mainMenuButton = EnsureButton(buttonRoot, "MainMenuButton", "Main Menu");
        var quitButton = EnsureButton(buttonRoot, "QuitButton", "Quit");

        var managerGo = GetOrCreateRoot("PauseMenu Manager");
        var pauseManager = managerGo.GetComponent<PauseMenuManager>();
        if (pauseManager == null)
            pauseManager = managerGo.AddComponent<PauseMenuManager>();

        WirePauseButtons(pauseManager, resumeButton, settingsButton, mainMenuButton, quitButton);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupMainMenuScene()
    {
        var scene = OpenOrCreateScene(MainMenuScenePath);
        SceneManager.SetActiveScene(scene);

        EnsureEventSystem();

        var canvas = EnsureCanvas("MainMenuCanvas");
        var panel = EnsurePanel(canvas.transform, "MainMenuPanel", new Color(0f, 0f, 0f, 0.62f));
        var buttonRoot = EnsureVerticalRoot(panel.transform, "MainMenuButtons");

        var playButton = EnsureButton(buttonRoot, "PlayButton", "Play");
        var storyButton = EnsureButton(buttonRoot, "StoryButton", "Story");
        var settingsButton = EnsureButton(buttonRoot, "SettingsButton", "Settings");
        var quitButton = EnsureButton(buttonRoot, "QuitButton", "Quit");

        var managerGo = GetOrCreateRoot("MainMenu Manager");
        var manager = managerGo.GetComponent<MainMenuManager>();
        if (manager == null)
            manager = managerGo.AddComponent<MainMenuManager>();

        var so = new SerializedObject(manager);
        so.FindProperty("gameplayScene").stringValue = "Core Game";
        so.FindProperty("settingsScene").stringValue = "Settings";
        so.FindProperty("gameplayLoadsSingle").boolValue = true;
        so.FindProperty("storyLoadsSingle").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        WireMainMenuButtons(manager, playButton, storyButton, settingsButton, quitButton);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupSettingsScene()
    {
        var scene = OpenOrCreateScene(SettingsScenePath);
        SceneManager.SetActiveScene(scene);

        EnsureEventSystem();

        var canvas = EnsureCanvas("SettingsCanvas");
        var panel = EnsurePanel(canvas.transform, "SettingsPanel", new Color(0f, 0f, 0f, 0.72f));
        var controlsRoot = EnsureVerticalRoot(panel.transform, "SettingsControls");

        var masterSlider = EnsureSlider(controlsRoot, "MasterVolumeSlider");
        var sfxSlider = EnsureSlider(controlsRoot, "SfxVolumeSlider");
        var musicSlider = EnsureSlider(controlsRoot, "MusicVolumeSlider");

        var masterToggle = EnsureToggle(controlsRoot, "MasterEnabledToggle", "Master Enabled");
        var sfxToggle = EnsureToggle(controlsRoot, "SfxEnabledToggle", "SFX Enabled");
        var musicToggle = EnsureToggle(controlsRoot, "MusicEnabledToggle", "Music Enabled");

        var closeButton = EnsureButton(controlsRoot, "CloseButton", "Close");

        var managerGo = GetOrCreateRoot("SettingsMenu Manager");
        var settingsManager = managerGo.GetComponent<SettingsMenuManager>();
        if (settingsManager == null)
            settingsManager = managerGo.AddComponent<SettingsMenuManager>();

        AssignSettingsReferences(settingsManager, masterSlider, sfxSlider, musicSlider, masterToggle, sfxToggle, musicToggle);
        WireSettingsControls(settingsManager, masterSlider, sfxSlider, musicSlider, masterToggle, sfxToggle, musicToggle, closeButton);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupManagersInAllBuildScenes()
    {
        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled || string.IsNullOrWhiteSpace(buildScene.path))
                continue;

            if (buildScene.path == PauseScenePath || buildScene.path == SettingsScenePath)
                continue;

            var scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            var root = GetOrCreateRoot("UI Managers");
            var manager = root.GetComponent<AdditiveUiSceneManager>();
            if (manager == null)
                manager = root.AddComponent<AdditiveUiSceneManager>();

            var so = new SerializedObject(manager);
            ApplyDefaultManagerFields(so);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static Scene OpenOrCreateScene(string scenePath)
    {
        if (System.IO.File.Exists(scenePath))
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);
        return scene;
    }

    private static void AddScenesToBuildSettings()
    {
        var entries = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        AddBuildSceneIfMissing(entries, MainMenuScenePath);
        AddBuildSceneIfMissing(entries, PauseScenePath);
        AddBuildSceneIfMissing(entries, SettingsScenePath);

        EditorBuildSettings.scenes = entries.ToArray();
    }

    private static void AddBuildSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
    {
        foreach (var item in scenes)
        {
            if (item.path == path)
                return;
        }

        scenes.Add(new EditorBuildSettingsScene(path, true));
    }

    private static void ValidateAdditiveUiSetupInternal(bool logSummary, bool autoFix)
    {
        var startingScene = SceneManager.GetActiveScene();
        int issueCount = 0;

        try
        {
            if (!System.IO.File.Exists(PauseScenePath))
            {
                issueCount++;
                Debug.LogWarning("[AdditiveUiSetupEditor] Missing scene: " + PauseScenePath);

                if (autoFix)
                    SetupPauseScene();
            }

            if (!System.IO.File.Exists(SettingsScenePath))
            {
                issueCount++;
                Debug.LogWarning("[AdditiveUiSetupEditor] Missing scene: " + SettingsScenePath);

                if (autoFix)
                    SetupSettingsScene();
            }

            if (!System.IO.File.Exists(MainMenuScenePath))
            {
                issueCount++;
                Debug.LogWarning("[AdditiveUiSetupEditor] Missing scene: " + MainMenuScenePath);

                if (autoFix)
                    SetupMainMenuScene();
            }

            bool pauseInBuild = false;
            bool settingsInBuild = false;
            bool mainMenuInBuild = false;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                if (scene.path == PauseScenePath)
                    pauseInBuild = true;

                if (scene.path == SettingsScenePath)
                    settingsInBuild = true;

                if (scene.path == MainMenuScenePath)
                    mainMenuInBuild = true;
            }

            if (!pauseInBuild)
            {
                issueCount++;
                Debug.LogWarning("[AdditiveUiSetupEditor] Pause scene is not enabled in Build Settings.");

                if (autoFix)
                    AddScenesToBuildSettings();
            }

            if (!settingsInBuild)
            {
                issueCount++;
                Debug.LogWarning("[AdditiveUiSetupEditor] Settings scene is not enabled in Build Settings.");

                if (autoFix)
                    AddScenesToBuildSettings();
            }

            if (!mainMenuInBuild)
            {
                issueCount++;
                Debug.LogWarning("[AdditiveUiSetupEditor] MainMenu scene is not enabled in Build Settings.");

                if (autoFix)
                    AddScenesToBuildSettings();
            }

            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled || string.IsNullOrWhiteSpace(buildScene.path))
                    continue;

                if (buildScene.path == MainMenuScenePath || buildScene.path == PauseScenePath || buildScene.path == SettingsScenePath)
                    continue;

                if (!System.IO.File.Exists(buildScene.path))
                    continue;

                var scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                SceneManager.SetActiveScene(scene);

                var manager = Object.FindFirstObjectByType<AdditiveUiSceneManager>();
                if (manager == null)
                {
                    issueCount++;
                    Debug.LogWarning("[AdditiveUiSetupEditor] Missing AdditiveUiSceneManager in scene: " + buildScene.path);

                    if (autoFix)
                    {
                        var root = GetOrCreateRoot("UI Managers");
                        manager = root.AddComponent<AdditiveUiSceneManager>();
                        var managerSo = new SerializedObject(manager);
                        ApplyDefaultManagerFields(managerSo);
                        managerSo.ApplyModifiedPropertiesWithoutUndo();
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                    else
                    {
                        continue;
                    }
                }

                var so = new SerializedObject(manager);
                var pauseName = so.FindProperty("pauseSceneName").stringValue;
                var settingsName = so.FindProperty("settingsSceneName").stringValue;

                if (string.IsNullOrWhiteSpace(pauseName))
                {
                    issueCount++;
                    Debug.LogWarning("[AdditiveUiSetupEditor] Empty pause scene name in: " + buildScene.path);
                }

                if (string.IsNullOrWhiteSpace(settingsName))
                {
                    issueCount++;
                    Debug.LogWarning("[AdditiveUiSetupEditor] Empty settings scene name in: " + buildScene.path);
                }

                if (autoFix && (string.IsNullOrWhiteSpace(pauseName) || string.IsNullOrWhiteSpace(settingsName)))
                {
                    ApplyDefaultManagerFields(so);
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (logSummary)
            {
                if (issueCount == 0)
                    Debug.Log("[AdditiveUiSetupEditor] Validation passed with no issues.");
                else
                    Debug.LogWarning("[AdditiveUiSetupEditor] Validation found " + issueCount + " issue(s)." + (autoFix ? " Auto-fix applied where possible." : string.Empty));
            }
        }
        finally
        {
            if (startingScene.IsValid() && !string.IsNullOrWhiteSpace(startingScene.path) && System.IO.File.Exists(startingScene.path))
                EditorSceneManager.OpenScene(startingScene.path, OpenSceneMode.Single);
        }
    }

    private static void ApplyDefaultManagerFields(SerializedObject so)
    {
        so.FindProperty("pauseSceneName").stringValue = "PauseMenu";
        so.FindProperty("settingsSceneName").stringValue = "Settings";
        so.FindProperty("pauseGameplayWithPauseScene").boolValue = true;
        so.FindProperty("allowEscapeToggle").boolValue = true;
        so.FindProperty("pauseToggleKey").enumValueIndex = (int)KeyCode.Escape;
    }

    private static void DeleteAssetIfExists(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) == null)
            return;

        AssetDatabase.DeleteAsset(assetPath);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        var parts = folderPath.Split('/');
        var current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private static GameObject GetOrCreateRoot(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
            return go;

        return new GameObject(name);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
    }

    private static Canvas EnsureCanvas(string name)
    {
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null)
            return existing;

        var canvasGo = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
        return canvas;
    }

    private static GameObject EnsurePanel(Transform parent, string name, Color color)
    {
        var panel = FindChild(parent, name);
        if (panel == null)
        {
            panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
        }

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = panel.GetComponent<Image>();
        image.color = color;

        return panel;
    }

    private static Transform EnsureVerticalRoot(Transform parent, string name)
    {
        var root = FindChild(parent, name);
        if (root == null)
        {
            root = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(parent, false);
        }

        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(460f, 0f);

        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(18, 18, 18, 18);

        var fitter = root.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return root.transform;
    }

    private static Button EnsureButton(Transform parent, string name, string label)
    {
        var go = FindChild(parent, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
        }

        var image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.14f);

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = 58f;

        var textGo = FindChild(go.transform, "Label");
        if (textGo == null)
        {
            textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
        }

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 20;
        text.resizeTextMaxSize = 32;

        return go.GetComponent<Button>();
    }

    private static Slider EnsureSlider(Transform parent, string name)
    {
        var go = FindChild(parent, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
        }

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = 40f;

        var slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        // Ensure a basic target graphic to avoid missing-reference warnings.
        var background = EnsureImageChild(go.transform, "Background", new Color(1f, 1f, 1f, 0.18f));
        var fillArea = EnsureRectChild(go.transform, "Fill Area");
        var fill = EnsureImageChild(fillArea.transform, "Fill", new Color(0.4f, 0.86f, 0.98f, 1f));
        var handleArea = EnsureRectChild(go.transform, "Handle Slide Area");
        var handle = EnsureImageChild(handleArea.transform, "Handle", Color.white);

        var rootRect = go.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(420f, 40f);

        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(8f, 0f);
        fillAreaRect.offsetMax = new Vector2(-8f, 0f);

        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        var handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        var handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 34f);

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static Toggle EnsureToggle(Transform parent, string name, string label)
    {
        var go = FindChild(parent, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
        }

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = 36f;

        var background = EnsureImageChild(go.transform, "Background", new Color(1f, 1f, 1f, 0.14f));
        var checkmark = EnsureImageChild(background.transform, "Checkmark", new Color(0.45f, 0.91f, 0.52f, 1f));
        var labelGo = FindChild(go.transform, "Label");
        if (labelGo == null)
        {
            labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
        }

        var rootRect = go.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(420f, 36f);

        var bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.5f);
        bgRect.anchorMax = new Vector2(0f, 0.5f);
        bgRect.pivot = new Vector2(0f, 0.5f);
        bgRect.sizeDelta = new Vector2(28f, 28f);
        bgRect.anchoredPosition = new Vector2(0f, 0f);

        var checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(40f, 0f);
        labelRect.offsetMax = Vector2.zero;

        var text = labelGo.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 16;
        text.resizeTextMaxSize = 28;

        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = background.GetComponent<Image>();
        toggle.graphic = checkmark.GetComponent<Image>();

        return toggle;
    }

    private static GameObject EnsureImageChild(Transform parent, string name, Color color)
    {
        var go = FindChild(parent, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
        }

        go.GetComponent<Image>().color = color;
        return go;
    }

    private static GameObject EnsureRectChild(Transform parent, string name)
    {
        var go = FindChild(parent, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
        }

        return go;
    }

    private static GameObject FindChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        return child != null ? child.gameObject : null;
    }

    private static void WirePauseButtons(PauseMenuManager manager, Button resume, Button settings, Button mainMenu, Button quit)
    {
        ResetButton(resume);
        ResetButton(settings);
        ResetButton(mainMenu);
        ResetButton(quit);

        UnityEventTools.AddPersistentListener(resume.onClick, manager.OnResumeClicked);
        UnityEventTools.AddPersistentListener(settings.onClick, manager.OnSettingsClicked);
        UnityEventTools.AddPersistentListener(mainMenu.onClick, manager.OnBackToMainMenuClicked);
        UnityEventTools.AddPersistentListener(quit.onClick, manager.OnQuitGameClicked);
    }

    private static void WireMainMenuButtons(MainMenuManager manager, Button play, Button story, Button settings, Button quit)
    {
        ResetButton(play);
        ResetButton(story);
        ResetButton(settings);
        ResetButton(quit);

        UnityEventTools.AddPersistentListener(play.onClick, manager.OnPlayClicked);
        UnityEventTools.AddPersistentListener(story.onClick, manager.OnStoryClicked);
        UnityEventTools.AddPersistentListener(settings.onClick, manager.OnSettingsClicked);
        UnityEventTools.AddPersistentListener(quit.onClick, manager.OnQuitClicked);
    }

    private static void WireSettingsControls(
        SettingsMenuManager manager,
        Slider masterSlider,
        Slider sfxSlider,
        Slider musicSlider,
        Toggle masterToggle,
        Toggle sfxToggle,
        Toggle musicToggle,
        Button closeButton)
    {
        masterSlider.onValueChanged = new Slider.SliderEvent();
        sfxSlider.onValueChanged = new Slider.SliderEvent();
        musicSlider.onValueChanged = new Slider.SliderEvent();

        masterToggle.onValueChanged = new Toggle.ToggleEvent();
        sfxToggle.onValueChanged = new Toggle.ToggleEvent();
        musicToggle.onValueChanged = new Toggle.ToggleEvent();

        ResetButton(closeButton);

        UnityEventTools.AddPersistentListener(masterSlider.onValueChanged, manager.OnMasterVolumeChanged);
        UnityEventTools.AddPersistentListener(sfxSlider.onValueChanged, manager.OnSfxVolumeChanged);
        UnityEventTools.AddPersistentListener(musicSlider.onValueChanged, manager.OnMusicVolumeChanged);

        UnityEventTools.AddPersistentListener(masterToggle.onValueChanged, manager.OnMasterEnabledChanged);
        UnityEventTools.AddPersistentListener(sfxToggle.onValueChanged, manager.OnSfxEnabledChanged);
        UnityEventTools.AddPersistentListener(musicToggle.onValueChanged, manager.OnMusicEnabledChanged);

        UnityEventTools.AddPersistentListener(closeButton.onClick, manager.OnCloseClicked);
    }

    private static void AssignSettingsReferences(
        SettingsMenuManager manager,
        Slider master,
        Slider sfx,
        Slider music,
        Toggle masterToggle,
        Toggle sfxToggle,
        Toggle musicToggle)
    {
        var so = new SerializedObject(manager);
        so.FindProperty("masterSlider").objectReferenceValue = master;
        so.FindProperty("sfxSlider").objectReferenceValue = sfx;
        so.FindProperty("musicSlider").objectReferenceValue = music;
        so.FindProperty("masterToggle").objectReferenceValue = masterToggle;
        so.FindProperty("sfxToggle").objectReferenceValue = sfxToggle;
        so.FindProperty("musicToggle").objectReferenceValue = musicToggle;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ResetButton(Button button)
    {
        button.onClick = new Button.ButtonClickedEvent();
    }
}
#endif