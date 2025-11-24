using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script que asegura que SoundManager exista en escenas de gameplay.
/// Agrega este script a cualquier GameObject en cutscene o Level1_Blockout.
/// </summary>
public class SoundManagerInitializer : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Prefab del SoundManager (opcional, se puede crear vacío si no existe)")]
    public GameObject soundManagerPrefab;

    void Awake()
    {
        // Verificar si ya existe un SoundManager
        if (SoundManager.Instance == null)
        {
            // Si hay un prefab asignado, instanciarlo
            if (soundManagerPrefab != null)
            {
                Instantiate(soundManagerPrefab);
                Debug.Log("[SoundManagerInitializer] SoundManager creado desde prefab");
            }
            else
            {
                // Crear un GameObject vacío con SoundManager
                GameObject soundManagerGO = new GameObject("SoundManager");
                soundManagerGO.AddComponent<SoundManager>();
                Debug.Log("[SoundManagerInitializer] SoundManager creado automáticamente");
            }
        }
    }
}
