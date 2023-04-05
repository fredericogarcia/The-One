using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_0");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
    
    public void Quit()
    {
        Application.Quit();
    }
    
}