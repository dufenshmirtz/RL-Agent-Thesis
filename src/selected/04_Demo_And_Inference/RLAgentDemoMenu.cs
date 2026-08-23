using TMPro;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RLAgentDemoMenu : MonoBehaviour
{
    private const string GameplaySceneName = "GamePlayScene";
    private const string DefaultStage = "Stage 1";

    private static readonly string[] DifficultyLabels =
    {
        "AgentSmith2.1",
        "AgentSmith2.2",
        "AgentSmith3.0"
    };

    private static readonly string[] CharacterNames =
    {
        "Steelager",
        "Vander",
        "Rager",
        "Skipler",
        "Fin",
        "Lazy Bigus",
        "Lithra",
        "Chiback",
        "Lupen",
        "Visvia"
    };

    [SerializeField] private PvEDifficulty defaultDifficulty = PvEDifficulty.Hard;

    [Header("Difficulty Models")]
    [SerializeField] private NNModel beginnerModel;
    [SerializeField] private NNModel intermediateModel;
    [SerializeField] private NNModel expertModel;

    [Header("Menu Text")]
    [SerializeField] private string titleText = "RL Agent Thesis Demo";
    [SerializeField] private string infoText = "This demo allows the user to play against the final reinforcement learning agent developed for the thesis. The model was trained with PPO using Unity ML-Agents.";
    [SerializeField] private string agent1ModelLabel = "Agent 1 Level: ";
    [SerializeField] private string agent2ModelLabel = "Agent 2 Level: ";
    [SerializeField] private string player1CharacterLabel = "Player 1 Character: ";
    [SerializeField] private string agentCharacterLabel = "Agent Character: ";
    [SerializeField] private string playVsAgentButtonText = "Play vs Final RL Agent";
    [SerializeField] private string agentVsAgentButtonText = "Watch Agent vs Agent";
    [SerializeField] private string controlsButtonText = "Controls";
    [SerializeField] private string quitButtonText = "Quit";
    [SerializeField] private string footerText = "Player 1 is human. Player 2 is ML-Agent inference only. No Python, mlagents-learn, or training mode is used.";

    [Header("Controls Text")]
    [SerializeField] private string controlsTitleText = "Controls";
    [SerializeField] private string controlsBodyText = "Move: A / D\nJump: W    Drop: S\nQuick Attack: U    Heavy Attack: I\nBlock: O    Special: P    Charge: J\n\nWin by reducing the agent to 0 HP. The demo uses the default 100 HP, one-round ruleset and the normal arena.\n\nDuring a match, press Enter to restart quickly after or during gameplay.\nPress Backspace during a demo match to return to this demo menu.";
    [SerializeField] private string controlsBackButtonText = "Back";

    private PvEDifficulty selectedAgent1Difficulty;
    private PvEDifficulty selectedAgent2Difficulty;
    private int selectedPlayer1CharacterIndex = 3;
    private int selectedAgentCharacterIndex = 4;
    private TMP_Text agent1ModelText;
    private TMP_Text agent2ModelText;
    private TMP_Text player1CharacterText;
    private TMP_Text agentCharacterText;
    private GameObject controlsPanel;
    private Button playButton;

    private void Awake()
    {
        selectedAgent1Difficulty = defaultDifficulty;
        selectedAgent2Difficulty = defaultDifficulty;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        EnsureEventSystem();
        BuildMenu();
    }

    private void Update()
    {
        if (controlsPanel != null && controlsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            controlsPanel.SetActive(false);
            SelectPlayButton();
        }
    }

    private void BuildMenu()
    {
        Canvas canvas = CreateCanvas();
        RectTransform root = canvas.GetComponent<RectTransform>();

        Image background = CreateImage(root, "Background", new Color(0.035f, 0.02f, 0.025f, 1f));
        Stretch(background.rectTransform);

        CreateText(
            root,
            "Title",
            titleText,
            54,
            new Color(0.92f, 0.83f, 0.62f, 1f),
            new Vector2(0f, 235f),
            new Vector2(1000f, 90f),
            FontStyles.Bold);

        CreateText(
            root,
            "Info",
            infoText,
            25,
            new Color(0.83f, 0.78f, 0.68f, 1f),
            new Vector2(0f, 160f),
            new Vector2(960f, 80f),
            FontStyles.Normal);

        agent1ModelText = CreateText(
            root,
            "Agent1ModelText",
            "",
            24,
            new Color(0.9f, 0.84f, 0.7f, 1f),
            new Vector2(-430f, 88f),
            new Vector2(460f, 42f),
            FontStyles.Bold);

        CreateButton(root, "PreviousAgent1ModelButton", "<", new Vector2(-720f, 88f), new Vector2(56f, 40f), () => ChangeAgent1Difficulty(-1));
        CreateButton(root, "NextAgent1ModelButton", ">", new Vector2(-140f, 88f), new Vector2(56f, 40f), () => ChangeAgent1Difficulty(1));

        agent2ModelText = CreateText(
            root,
            "Agent2ModelText",
            "",
            24,
            new Color(0.9f, 0.84f, 0.7f, 1f),
            new Vector2(430f, 88f),
            new Vector2(460f, 42f),
            FontStyles.Bold);

        CreateButton(root, "PreviousAgent2ModelButton", "<", new Vector2(140f, 88f), new Vector2(56f, 40f), () => ChangeAgent2Difficulty(-1));
        CreateButton(root, "NextAgent2ModelButton", ">", new Vector2(720f, 88f), new Vector2(56f, 40f), () => ChangeAgent2Difficulty(1));
        RefreshAgentModelTexts();

        player1CharacterText = CreateText(
            root,
            "Player1CharacterText",
            "",
            24,
            new Color(0.9f, 0.84f, 0.7f, 1f),
            new Vector2(-430f, 18f),
            new Vector2(460f, 42f),
            FontStyles.Bold);

        CreateButton(root, "PreviousPlayer1CharacterButton", "<", new Vector2(-720f, 18f), new Vector2(56f, 40f), () => ChangePlayer1Character(-1));
        CreateButton(root, "NextPlayer1CharacterButton", ">", new Vector2(-140f, 18f), new Vector2(56f, 40f), () => ChangePlayer1Character(1));

        agentCharacterText = CreateText(
            root,
            "AgentCharacterText",
            "",
            24,
            new Color(0.9f, 0.84f, 0.7f, 1f),
            new Vector2(430f, 18f),
            new Vector2(460f, 42f),
            FontStyles.Bold);

        CreateButton(root, "PreviousAgentCharacterButton", "<", new Vector2(140f, 18f), new Vector2(56f, 40f), () => ChangeAgentCharacter(-1));
        CreateButton(root, "NextAgentCharacterButton", ">", new Vector2(720f, 18f), new Vector2(56f, 40f), () => ChangeAgentCharacter(1));
        RefreshCharacterTexts();

        playButton = CreateButton(root, "PlayButton", playVsAgentButtonText, new Vector2(0f, -78f), new Vector2(430f, 56f), StartDemo);
        CreateButton(root, "AgentVsAgentButton", agentVsAgentButtonText, new Vector2(0f, -146f), new Vector2(430f, 56f), StartAgentVsAgentDemo);
        CreateButton(root, "ControlsButton", controlsButtonText, new Vector2(0f, -212f), new Vector2(430f, 52f), ToggleControls);
        CreateButton(root, "QuitButton", quitButtonText, new Vector2(0f, -274f), new Vector2(430f, 52f), QuitDemo);

        CreateText(
            root,
            "Footer",
            footerText,
            22,
            new Color(0.68f, 0.62f, 0.54f, 1f),
            new Vector2(0f, -360f),
            new Vector2(1000f, 48f),
            FontStyles.Normal);

        controlsPanel = CreateControlsPanel(root);
        controlsPanel.SetActive(false);

        SelectPlayButton();
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("RLAgentDemoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private GameObject CreateControlsPanel(RectTransform root)
    {
        Image panel = CreateImage(root, "ControlsPanel", new Color(0.07f, 0.035f, 0.045f, 0.96f));
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(840f, 540f);

        CreateText(
            panelRect,
            "ControlsTitle",
            controlsTitleText,
            42,
            new Color(0.95f, 0.86f, 0.64f, 1f),
            new Vector2(0f, 195f),
            new Vector2(760f, 64f),
            FontStyles.Bold);

        CreateText(
            panelRect,
            "ControlsBody",
            controlsBodyText,
            27,
            new Color(0.86f, 0.82f, 0.72f, 1f),
            new Vector2(0f, -15f),
            new Vector2(760f, 320f),
            FontStyles.Normal);

        CreateButton(panelRect, "CloseControlsButton", controlsBackButtonText, new Vector2(0f, -222f), new Vector2(260f, 54f), () =>
        {
            controlsPanel.SetActive(false);
            SelectPlayButton();
        });

        return panel.gameObject;
    }

    private Button CreateButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.34f, 0.05f, 0.06f, 0.98f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.34f, 0.05f, 0.06f, 0.98f);
        colors.highlightedColor = new Color(0.52f, 0.08f, 0.08f, 1f);
        colors.pressedColor = new Color(0.18f, 0.025f, 0.025f, 1f);
        colors.selectedColor = new Color(0.52f, 0.08f, 0.08f, 1f);
        colors.disabledColor = new Color(0.16f, 0.13f, 0.13f, 0.8f);
        button.colors = colors;

        TMP_Text text = CreateText(
            rect,
            name + "Text",
            label,
            28,
            new Color(0.98f, 0.9f, 0.72f, 1f),
            Vector2.zero,
            size,
            FontStyles.Bold);
        text.raycastTarget = false;

        return button;
    }

    private TMP_Text CreateText(RectTransform parent, string name, string value, int fontSize, Color color, Vector2 anchoredPosition, Vector2 size, FontStyles style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.fontStyle = style;

        return text;
    }

    private Image CreateImage(RectTransform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ChangeAgent1Difficulty(int direction)
    {
        int current = (int)selectedAgent1Difficulty;
        current = (current + direction + DifficultyLabels.Length) % DifficultyLabels.Length;
        selectedAgent1Difficulty = (PvEDifficulty)current;
        RefreshAgentModelTexts();
    }

    private void ChangeAgent2Difficulty(int direction)
    {
        int current = (int)selectedAgent2Difficulty;
        current = (current + direction + DifficultyLabels.Length) % DifficultyLabels.Length;
        selectedAgent2Difficulty = (PvEDifficulty)current;
        RefreshAgentModelTexts();
    }

    private void RefreshAgentModelTexts()
    {
        if (agent1ModelText != null)
        {
            agent1ModelText.text = agent1ModelLabel + DifficultyLabels[(int)selectedAgent1Difficulty];
        }

        if (agent2ModelText != null)
        {
            agent2ModelText.text = agent2ModelLabel + DifficultyLabels[(int)selectedAgent2Difficulty];
        }
    }

    private void ChangePlayer1Character(int direction)
    {
        selectedPlayer1CharacterIndex = WrapIndex(selectedPlayer1CharacterIndex + direction, CharacterNames.Length);
        RefreshCharacterTexts();
    }

    private void ChangeAgentCharacter(int direction)
    {
        selectedAgentCharacterIndex = WrapIndex(selectedAgentCharacterIndex + direction, CharacterNames.Length);
        RefreshCharacterTexts();
    }

    private int WrapIndex(int index, int count)
    {
        return (index + count) % count;
    }

    private void RefreshCharacterTexts()
    {
        if (player1CharacterText != null)
        {
            player1CharacterText.text = player1CharacterLabel + GetCharacterDisplayName(selectedPlayer1CharacterIndex);
        }

        if (agentCharacterText != null)
        {
            agentCharacterText.text = agentCharacterLabel + GetCharacterDisplayName(selectedAgentCharacterIndex);
        }
    }

    private string GetCharacterDisplayName(int characterIndex)
    {
        return "Character " + characterIndex;
    }

    private void StartDemo()
    {
        ApplyDemoState(false);
        SceneManager.LoadScene(GameplaySceneName);
    }

    private void StartAgentVsAgentDemo()
    {
        ApplyDemoState(true);
        SceneManager.LoadScene(GameplaySceneName);
    }

    private void ApplyDemoState(bool agentVsAgent)
    {
        PvESelectionState.IsPvE = true;
        PvESelectionState.IsRLAgentDemo = true;
        PvESelectionState.IsRLAgentDemoAgentVsAgent = agentVsAgent;
        PvESelectionState.SelectedBotType = PvEBotType.MLAgent;
        PvESelectionState.SelectedDifficulty = selectedAgent2Difficulty;
        PvESelectionState.RLAgentDemoAgent1Difficulty = selectedAgent1Difficulty;
        PvESelectionState.RLAgentDemoAgent2Difficulty = selectedAgent2Difficulty;
        PvESelectionState.SelectedBotSide = PvEBotSide.Player2;
        RLAgentDemoModelOverrides.Set(beginnerModel, intermediateModel, expertModel);

        RulesetSelectionState.SelectDefault();

        CustomRuleset ruleset = new CustomRuleset
        {
            slotName = "Default",
            rounds = 1,
            powerupsEnabled = false,
            health = 100,
            hideHealth = false,
            playerSpeed = 4,
            devTools = false,
            portals = 0,
            chanChan = false,
            quickDisabled = false,
            heavyDisabled = false,
            blockDisabled = false,
            specialDisabled = false,
            chargeDisabled = false
        };

        PlayerPrefs.SetString("SelectedRuleset", JsonUtility.ToJson(ruleset));
        PlayerPrefs.SetString("SelectedStage", DefaultStage);
        PlayerPrefs.SetString("Player1Choice", CharacterNames[selectedPlayer1CharacterIndex]);
        PlayerPrefs.SetString("Player2Choice", CharacterNames[selectedAgentCharacterIndex]);
        PlayerPrefs.Save();
    }

    private void ToggleControls()
    {
        if (controlsPanel == null)
        {
            return;
        }

        controlsPanel.SetActive(!controlsPanel.activeSelf);
    }

    private void QuitDemo()
    {
        Application.Quit();
    }

    private void SelectPlayButton()
    {
        if (playButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        EventSystem.current = eventSystem.GetComponent<EventSystem>();
    }
}

public static class RLAgentDemoModelOverrides
{
    private static NNModel beginnerModel;
    private static NNModel intermediateModel;
    private static NNModel expertModel;

    public static void Set(NNModel beginner, NNModel intermediate, NNModel expert)
    {
        beginnerModel = beginner;
        intermediateModel = intermediate;
        expertModel = expert;
    }

    public static void Clear()
    {
        beginnerModel = null;
        intermediateModel = null;
        expertModel = null;
    }

    public static NNModel GetModel(PvEDifficulty difficulty)
    {
        switch (difficulty)
        {
            case PvEDifficulty.Easy:
                return beginnerModel;
            case PvEDifficulty.Medium:
                return intermediateModel;
            case PvEDifficulty.Hard:
                return expertModel;
            default:
                return intermediateModel;
        }
    }
}
