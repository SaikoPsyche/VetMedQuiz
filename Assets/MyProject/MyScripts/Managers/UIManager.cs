// ----------------------------------------------------------------------------
// Added 2026-09-18 by Claude Code.
//
// Owns the UI Toolkit side of the game. The three Canvas scenes were replaced by
// five UIDocuments living together in Quiz.unity, so screen changes are now a
// display toggle rather than a scene load.
//
// Screens are hidden with style.display rather than by deactivating the
// GameObject: a UIDocument rebuilds its visual tree whenever it is re-enabled,
// which would invalidate every cached element and drop every callback registered
// here. Keeping the tree alive and just hiding it avoids that entirely.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public enum Screen { Welcome, Level, Quiz, End }

    // The visual tree asset names the documents are matched against, so no
    // inspector wiring is needed. Renaming a .uxml means updating these.
    private const string WelcomeTree = "WelcomeScreen";
    private const string LevelTree = "LevelScreen";
    private const string QuizTree = "QuizScreen";
    private const string SettingsTree = "SettingsScreen";
    private const string EndTree = "End Document";

    private const int AnswerButtonCount = 4;
    private const string PlayerAssetName = "Player";

    private readonly Dictionary<Screen, UIDocument> _screens = new Dictionary<Screen, UIDocument>();
    private readonly List<Button> _answerButtons = new List<Button>();

    private UIDocument _settingsDocument;
    private bool _settingsOpen;

    private Label _questionLabel;
    private Label _timerLabel;
    private TextField _nameField;

    private QuizManager _quizManager;
    private PlayerData _player;

    private void Awake()
    {
        _quizManager = GetComponent<QuizManager>();

        if (_quizManager == null)
            Debug.LogError("UIManager expects a QuizManager on the same GameObject.");

        _player = Resources.Load<PlayerData>(PlayerAssetName);

        if (_player == null)
            Debug.LogError($"No PlayerData asset named '{PlayerAssetName}' under a Resources folder.");

        CollectDocuments();
    }

    private void OnEnable()
    {
        EventManager.OnToggleSettings += ToggleSettings;
    }

    private void OnDisable()
    {
        EventManager.OnToggleSettings -= ToggleSettings;
    }

    private void Start()
    {
        BindWelcome();
        BindLevel();
        BindQuiz();
        BindEnd();
        BindSettings();

        SetSettingsVisible(false);
        Show(Screen.Welcome);
    }

    // ---------------------------------------------------------------- documents

    private void CollectDocuments()
    {
        // Every document has to be active for its tree to exist and be queryable.
        // The welcome document in particular ships disabled in the scene.
        UIDocument[] documents = FindObjectsByType<UIDocument>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (UIDocument document in documents)
        {
            if (!document.gameObject.activeSelf) document.gameObject.SetActive(true);

            string treeName = document.visualTreeAsset != null ? document.visualTreeAsset.name : null;

            switch (treeName)
            {
                case WelcomeTree: _screens[Screen.Welcome] = document; break;
                case LevelTree: _screens[Screen.Level] = document; break;
                case QuizTree: _screens[Screen.Quiz] = document; break;
                case EndTree: _screens[Screen.End] = document; break;
                case SettingsTree: _settingsDocument = document; break;
            }
        }

        foreach (Screen screen in System.Enum.GetValues(typeof(Screen)))
        {
            if (!_screens.ContainsKey(screen))
                Debug.LogError($"No UIDocument found for the {screen} screen.");
        }
    }

    private VisualElement Root(Screen screen)
    {
        return _screens.TryGetValue(screen, out UIDocument document) && document != null
            ? document.rootVisualElement
            : null;
    }

    public void Show(Screen screen)
    {
        foreach (KeyValuePair<Screen, UIDocument> entry in _screens)
        {
            VisualElement root = entry.Value != null ? entry.Value.rootVisualElement : null;

            if (root != null)
                root.style.display = entry.Key == screen ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // ----------------------------------------------------------------- binding

    private void BindWelcome()
    {
        Root(Screen.Welcome)?.Q<Button>("Start")?.RegisterCallback<ClickEvent>(_ =>
        {
            EventManager.PlayClick();
            Show(Screen.Level);
        });
    }

    private void BindLevel()
    {
        VisualElement root = Root(Screen.Level);

        if (root == null) return;

        _nameField = root.Q<TextField>("Name");

        root.Q<Button>("Level1")?.RegisterCallback<ClickEvent>(_ => ChooseLevel(0));
        root.Q<Button>("Level2")?.RegisterCallback<ClickEvent>(_ => ChooseLevel(1));
    }

    private void BindQuiz()
    {
        VisualElement root = Root(Screen.Quiz);

        if (root == null) return;

        _questionLabel = root.Q<Label>("Question");
        _timerLabel = root.Q<Label>("Timer");

        _answerButtons.Clear();

        for (int i = 0; i < AnswerButtonCount; i++)
        {
            Button button = root.Q<Button>($"Ans{i}");

            _answerButtons.Add(button);

            if (button == null)
            {
                Debug.LogError($"QuizScreen.uxml has no button named 'Ans{i}'.");
                continue;
            }

            int index = i; // captured per iteration, not shared across the loop
            button.RegisterCallback<ClickEvent>(_ => AnswerChosen(index));
        }
    }

    private void BindEnd()
    {
        VisualElement root = Root(Screen.End);

        if (root == null) return;

        root.Q<Button>("Restart")?.RegisterCallback<ClickEvent>(_ =>
        {
            EventManager.PlayClick();
            _quizManager.BeginQuiz(_player != null ? _player.PlayerLevel : 0);
        });

        root.Q<Button>("NextQuiz")?.RegisterCallback<ClickEvent>(_ =>
        {
            EventManager.PlayClick();
            Show(Screen.Level);
        });

        root.Q<Button>("Home")?.RegisterCallback<ClickEvent>(_ =>
        {
            EventManager.PlayClick();
            Show(Screen.Welcome);
        });
    }

    private void BindSettings()
    {
        VisualElement root = _settingsDocument != null ? _settingsDocument.rootVisualElement : null;

        if (root == null) return;

        root.Q<Button>("MusicOn")?.RegisterCallback<ClickEvent>(_ => EventManager.SetMusicMuted(false));
        root.Q<Button>("MusicOff")?.RegisterCallback<ClickEvent>(_ => EventManager.SetMusicMuted(true));
        root.Q<Button>("SoundOn")?.RegisterCallback<ClickEvent>(_ => EventManager.SetClickMuted(false));
        root.Q<Button>("SoundOff")?.RegisterCallback<ClickEvent>(_ => EventManager.SetClickMuted(true));
        root.Q<Button>("Settings")?.RegisterCallback<ClickEvent>(_ => ToggleSettings());
    }

    // ------------------------------------------------------------------- flow

    private void ChooseLevel(int difficulty)
    {
        EventManager.PlayClick();

        string testerName = TypedName();

        if (_player != null)
        {
            _player.PlayerName = testerName;
            _player.PlayerLevel = difficulty;
            _player.PlayerScore = 0;
        }

        // Records the attempt on disk so scores survive a restart; the
        // ScriptableObject above only carries the run that is in progress.
        EventManager.AddTester(testerName, difficulty);

        Show(Screen.Quiz);
        _quizManager.BeginQuiz(difficulty);
    }

    // The UXML binds PlayerName to the field's placeholderText, which is display
    // only, so the typed value has to be read back from the field itself.
    private string TypedName()
    {
        string typed = _nameField != null ? _nameField.value : null;

        return string.IsNullOrWhiteSpace(typed) ? string.Empty : typed.Trim();
    }

    private void AnswerChosen(int index)
    {
        _quizManager.CheckAnswer(index);
    }

    // ------------------------------------------------------------- quiz output

    public void ShowQuestion(string question, IReadOnlyList<string> options)
    {
        if (_questionLabel != null) _questionLabel.text = question;

        for (int i = 0; i < _answerButtons.Count; i++)
        {
            Button button = _answerButtons[i];

            if (button == null) continue;

            bool hasOption = options != null && i < options.Count && !string.IsNullOrEmpty(options[i]);

            // True/False questions carry only two options, so the spare buttons are
            // taken out of the layout rather than left blank and still clickable.
            button.style.display = hasOption ? DisplayStyle.Flex : DisplayStyle.None;

            if (hasOption) button.text = options[i];
        }
    }

    public void ShowTime(string time)
    {
        if (_timerLabel != null) _timerLabel.text = time;
    }

    public void ShowEndScreen()
    {
        Show(Screen.End);
    }

    // --------------------------------------------------------------- settings

    private void ToggleSettings()
    {
        SetSettingsVisible(!_settingsOpen);
    }

    private void SetSettingsVisible(bool visible)
    {
        _settingsOpen = visible;

        VisualElement root = _settingsDocument != null ? _settingsDocument.rootVisualElement : null;

        if (root == null) return;

        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        // The settings document sits below the others in the panel's sort order, so
        // it has to be lifted while open or it renders behind the active screen.
        if (_settingsDocument != null)
            _settingsDocument.sortingOrder = visible ? 100f : 1f;
    }
}
