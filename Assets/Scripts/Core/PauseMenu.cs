using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public KeyCode pausaKey;
    public CanvasGroup canvasPausa;

    private bool gamePaused;
    private const float TWEEN_TIME = 0.3f;
    private Tween pausaTween;
    void Start()
    {
        gamePaused = false;
        canvasPausa.alpha = 0f;
    }

    // Update is called once per frame
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

        pausaTween?.Kill();
        canvasPausa.interactable = pausa;
        canvasPausa.blocksRaycasts = pausa;

        pausaTween = canvasPausa.DOFade(canvasAlpha, TWEEN_TIME).SetUpdate(true).OnComplete(() => {

        });
        gamePaused = pausa;
    }
}
