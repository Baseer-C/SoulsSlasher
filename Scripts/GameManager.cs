using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winScreen;
    public GameObject loseScreen;

    void Start()
    {
        if (winScreen) winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);
    }

    public void PlayerDied()
    {
        Debug.Log("Game Over");
        if (loseScreen) loseScreen.SetActive(true);
        // Invoke restart logic here if you want
    }

    public void BossDied()
    {
        Debug.Log("Victory");
        if (winScreen) winScreen.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}