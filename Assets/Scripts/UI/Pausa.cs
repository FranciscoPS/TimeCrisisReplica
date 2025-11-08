using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Pausa : MonoBehaviour
{
    public KeyCode pausaKey;
    public CanvasGroup canvasPausa;

    private bool gamePaused;
    private const float TWEEN_TIME = 0.3f;
    private Tween pauseTween;

    void Start()
    {
        gamePaused = false;
        canvasPausa.alpha = 0;
    }


    void Update()
    {
        if (Input.GetKeyDown(pausaKey))
        {
            TogglePause(!gamePaused);
        }
    }

    public void TogglePause(bool pausa)
    {
        Time.timeScale = pausa ? 0 : 1;

        float canvasAlpha = pausa ? 1 : 0;

        pauseTween?.Kill();
        canvasPausa.interactable = pausa;
        canvasPausa.blocksRaycasts = pausa;

        pauseTween = canvasPausa.DOFade(canvasAlpha, TWEEN_TIME).SetUpdate(true);
        gamePaused = pausa;
    }
}
