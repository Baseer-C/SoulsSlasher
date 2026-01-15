using UnityEngine;
using TMPro; // REQUIRED for TextMeshProUGUI
using System.Collections;

public class PressAnyButton : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI pressAnyButtonText;
    public GameObject pressButtonPanel;
    public GameObject mainMenuPanel;

    [Header("Animation Settings")]
    public float blinkSpeed = 2.0f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1.0f;

    private bool hasPressedButton = false;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Auto-add CanvasGroup if missing
        canvasGroup = pressButtonPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = pressButtonPanel.AddComponent<CanvasGroup>();

        mainMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (hasPressedButton) return;

        // 1. Blinking Effect
        if (pressAnyButtonText != null)
        {
            // PingPong oscillates time between 0 and 1
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1));
            pressAnyButtonText.alpha = alpha;
        }

        // 2. Input Detection
        if (Input.anyKeyDown)
        {
            hasPressedButton = true;
            StartCoroutine(TransitionToMenu());
        }
    }

    IEnumerator TransitionToMenu()
    {
        float duration = 0.5f;
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;

        // Fade out
        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        pressButtonPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}