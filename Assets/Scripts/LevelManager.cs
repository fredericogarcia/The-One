using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Load the main game scene
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_0");
    }
    // Load the main menu scene
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    // Load the game over scene
    public void LoadGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }
    // Load the victory scene
    public void LoadVictory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Victory");
    }
    // Quit the application
    public void Quit()
    {
        Application.Quit();
    }
    
}