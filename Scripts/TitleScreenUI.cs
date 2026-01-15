using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pressAnyButtonPanel;
    public GameObject mainMenuPanel;
    public GameObject loadGamePanel;

    [Header("Buttons")]
    public Button continueButton;
    public Button newGameButton;
    public Button loadGameButton; // Optional
    public Button quitButton;

    private void Start()
    {
        // Ensure correct initial state
        if (pressAnyButtonPanel != null) pressAnyButtonPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);

        // Setup Listeners
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        // Check Save Data (Simplified integration)
        bool hasSave = false;
        if (TitleScreenManager.instance != null) hasSave = TitleScreenManager.instance.hasSaveData;

        if (continueButton != null) continueButton.interactable = hasSave;
    }

    // Called by PressAnyButton.cs
    public void OpenMainMenu()
    {
        if (pressAnyButtonPanel != null) pressAnyButtonPanel.SetActive(false);
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            // Force Layout Rebuild (Fixes layout glitches when enabling objects)
            LayoutRebuilder.ForceRebuildLayoutImmediate(mainMenuPanel.GetComponent<RectTransform>());

            if (newGameButton != null) newGameButton.Select();
        }
    }

    public void OnNewGameClicked()
    {
        if (TitleScreenManager.instance != null)
            TitleScreenManager.instance.StartNewGame();
    }

    public void OnContinueClicked()
    {
        if (TitleScreenManager.instance != null)
            TitleScreenManager.instance.ContinueGame();
    }

    public void OnQuitClicked()
    {
        if (TitleScreenManager.instance != null)
            TitleScreenManager.instance.QuitGame();
    }
}