using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void OnPlayClicked()
    {
        if (MainMenuMusic.Instance != null)
        {
            MainMenuMusic.Instance.StopMusic();
        }

        SoulUI.TotalSouls = 0;
        PassiveItemManager.ActivePerks.Clear();
        PlayerHealth.IsNewGame = true;

        if (PlayerHealth.Instance != null)
        {
            Destroy(PlayerHealth.Instance.gameObject);
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }
}
