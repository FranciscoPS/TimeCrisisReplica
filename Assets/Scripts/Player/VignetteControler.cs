using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteControler : MonoBehaviour
{
    public KeyCode vignetteKey;
    public Volume volume;
    public Vignette vignette;
    void Awake()
    {
        volume = GetComponent<Volume>();
        if(!volume.profile.TryGet(out vignette))
        {
            Debug.Log("Yay");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(vignetteKey))
        {
            if (vignette != null)
            {
                vignette.intensity.value = 0.5f;
            }
        }
    }
}
