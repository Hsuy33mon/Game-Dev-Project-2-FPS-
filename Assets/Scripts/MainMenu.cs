using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_Text highScoreUI;
    string newGameScene = "SampleScene";

    public AudioClip bg_music;
    public AudioSource main_channel;
    void Start()
    {
        main_channel.PlayOneShot(bg_music);
        int highScore = SaveLoadManager.Instance.LoadHighScore();
        highScoreUI.text = $"Top Wave Survived: {highScore}";
    }

    public void StartNewGame()
    {
        main_channel.Stop();
        SceneManager.LoadScene(newGameScene);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
