using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameUI_Manager uiManager;

    public Character character;
    public bool gameIsOver;

    private void Awake()
    {
        character = GameObject.FindWithTag("Player").GetComponent<Character>();
    }

    public void GameOver()
    {
        uiManager.OpenGameOverScreen();
    }

    public void GameFinished()
    {
        uiManager.OpenGameFinishedScreen();
    }


    private void Update()
    {
        if (gameIsOver)
            return;
        if(character.currentState == Character.characterState.deathState)
        {
            gameIsOver = true;
            GameOver();
        }
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.TogglePauseUI();
        }

    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
