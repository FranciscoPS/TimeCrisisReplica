using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MenuVictoria : MonoBehaviour
{
    [Header("Fade Transition")]
    public Image fadePanel;

    public float fadeDuration = 1f;

    void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            fadePanel.DOFade(0f, fadeDuration);
        }
        else
        {

        }
    }

    public void RegresarAMenu()
    {
        if (fadePanel != null)
        {
            SetButtonsInteractable(false);

            fadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene("MainMenu");
            });
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void RegresarANivel()
    {
        if (fadePanel != null)
        {
            SetButtonsInteractable(false);

            fadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene("Level1_Blockout");
            });
        }
        else
        {
            SceneManager.LoadScene("Level1_Blockout");
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.interactable = interactable;
        }

    }
}
