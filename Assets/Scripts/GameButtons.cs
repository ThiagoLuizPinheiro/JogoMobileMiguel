using UnityEngine;
using UnityEngine.SceneManagement;

public class GameButtons : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelMenu;
    [SerializeField] private GameObject painelMorte;

    [Header("Player")]
    [SerializeField] private PlayerController player;

    public static bool iniciarAutomaticamente;

    public void Restart()
    {
        Time.timeScale = 1f;

        iniciarAutomaticamente = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;

        player.ResetarJogo();

        painelMorte.SetActive(false);
        painelMenu.SetActive(true);
    }
}