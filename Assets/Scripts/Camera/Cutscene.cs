using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public Image timelinefader;
    public KeyCode skipKey;
    private float fadeDuration = 1f;
    private float timelineDuration;


    void Awake()
    {
        timelineDuration = (float)playableDirector.playableAsset.duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(skipKey))
        {
            SkipTimeline();
        }
    }

    public void SkipTimeline()
    {
        playableDirector.time = timelineDuration;
        TimelineEnd();
    }

    public void TimelineEnd()
    {
        SceneManager.LoadScene("Level1_Blockout");
        timelinefader.DOFade(0, fadeDuration);
    }
}
