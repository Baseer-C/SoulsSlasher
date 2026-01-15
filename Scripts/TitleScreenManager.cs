using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager instance;

    [Header("Scene Management")]
    public string worldSceneName = "WorldScene"; // Name of your main game scene
    public int worldSceneIndex = 1;

    [Header("Save Data")]
    // We will hook into WorldSaveGameManager later, for now we setup the flow
    public bool hasSaveData = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewGame()
    {
        // In a full implementation, this would open Character Creation
        // For now, we will load directly into the game world
        StartCoroutine(LoadWorldScene());
    }

    public void ContinueGame()
    {
        if (hasSaveData)
        {
            // Load save data logic here
            StartCoroutine(LoadWorldScene());
        }
        else
        {
            Debug.Log("No Save Data Found");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadWorldScene()
    {
        // Optional: Trigger a fade out effect here
        yield return new WaitForSeconds(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(worldSceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}