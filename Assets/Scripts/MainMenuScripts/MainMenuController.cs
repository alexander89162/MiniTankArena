using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements; 

public class MainMenuController : MonoBehaviour
{
    private UIDocument uiDocument;

    void Start()
    {
        //Allows to get the uidoc
        uiDocument = GetComponent<UIDocument>();

        //Grabs to root visual element to manipulate
        var root = uiDocument.rootVisualElement;

        //Grabs the buttons 
        var playButton   = root.Q<Button>("Play_Btn");
        var settingsButton = root.Q<Button>("Settings_Btn");
        var creditsButton  = root.Q<Button>("Credits_Btn");
        var quitButton     = root.Q<Button>("Quit_Btn");

        //Click events
        if (playButton != null)
            playButton.clicked += OnPlayClicked;

        if (settingsButton != null)
            settingsButton.clicked += OnSettingsClicked;

        if (creditsButton != null)
            creditsButton.clicked += OnCreditsClicked;

        if (quitButton != null)
            quitButton.clicked += OnQuitClicked;
    }

    private void OnPlayClicked()
    {
        //Loads the scence once clicked on play
        SceneManager.LoadScene("SampleScene");       

    }

    //Func to check settings button works
    private void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
       
    }

    //Func to check credits button works
    private void OnCreditsClicked()
    {
        Debug.Log("Credits clicked");
        
    }

    //Function to quit the game
    private void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}