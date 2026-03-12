using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    private UIDocument pauseDocument; 

    // References to the panels and buttons inside your UXML
    private VisualElement root;
    private VisualElement pausePanel;

    private bool isPaused = false;

    void Awake()
    {
        pauseDocument = GetComponent<UIDocument>();
        if (pauseDocument == null)
        {
            Debug.LogError("Pause UIDocument is not assigned in the Inspector!");
            return;
        }

        root = pauseDocument.rootVisualElement;

        pausePanel = root.Q<VisualElement>("PauseOverlay");

        // Hide everything at start
        if (pausePanel != null) 
        {
            pausePanel.style.display = DisplayStyle.None;
        }

        // Wire up all buttons (names must match your UXML button names)
        if (pausePanel != null)
        {
            pausePanel.Q<Button>("ResumeBtn").clicked     += OnResumeButton;
            pausePanel.Q<Button>("SettingsBtn").clicked   += OpenSettings;
            pausePanel.Q<Button>("QuitMenuBtn").clicked  += QuitToMainMenu;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
            PauseGame();
        else
            ResumeGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.Flex;

        pauseDocument.enabled = true; 
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)    pausePanel.style.display    = DisplayStyle.None;

        isPaused = false;
    }

    // Button callbacks
    public void OnResumeButton() => ResumeGame(); 

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.style.display = DisplayStyle.None;
    }

    public void CloseSettings()
    {
        if (pausePanel != null) pausePanel.style.display = DisplayStyle.Flex;
    }

    public void QuitToMainMenu()
    {
        ResumeGame(); // reset time scale before loading
        SceneManager.LoadScene("MainMenu"); 
    }

    public void QuitGame()
    {
        ResumeGame();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public bool IsPaused => isPaused;
}