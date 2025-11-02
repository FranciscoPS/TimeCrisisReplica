using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.ShaderData;

public class BotonesPausa : MonoBehaviour
{
    public PauseMenu pausaScript;
    public void RegresarAMenu()
    {
        pausaScript.TogglePause(false);
        SceneManager.LoadScene("Main Menu");
    }

    public void ReiniciarNivel()
    {
        pausaScript.TogglePause(false);
        SceneManager.LoadScene("Level1_Blockout");
    }

    public void RegresarAlJuego()
    {
        pausaScript.TogglePause(false);
    }
}
