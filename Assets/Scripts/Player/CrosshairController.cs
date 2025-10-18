using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CrosshairController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform crosshair;   // Asigna el objeto UI de tu crosshair
    public Canvas canvas;             // El Canvas que contiene el crosshair

    [Header("Opcional")]
    public bool lockCursor = false;   // Si quieres ocultar y bloquear el cursor
    public bool clampToScreen = true; // Limitar dentro de la pantalla

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!crosshair)
        {
            var img = GetComponentInChildren<Image>(true);
            if (img) crosshair = img.rectTransform;
        }
    }

    void OnEnable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnDisable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update()
    {
        if (!crosshair || Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (clampToScreen)
        {
            mousePos.x = Mathf.Clamp(mousePos.x, 0f, Screen.width);
            mousePos.y = Mathf.Clamp(mousePos.y, 0f, Screen.height);
        }

        // Canvas en Screen Space Overlay: posición de pantalla = posición local
        crosshair.position = mousePos;
    }
}
