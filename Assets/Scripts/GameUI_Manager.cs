using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI_Manager : MonoBehaviour
{
    public GameManager gm;
    public TMPro.TextMeshProUGUI coinText;
    public Slider healthSlider;

    public GameObject pauseScreen;
    public GameObject overScreen;
    public GameObject finishScreen;

    public enum screenState
    {
        game,
        pause,
        over,
        finish
    }
    public screenState currentState;

    private void Start()
    {
        StateManager(screenState.game);
    }
    private void Update()
    {
        healthSlider.value = gm.character.GetComponent<Health>().currentHealthPercentage;
        coinText.text = gm.character.coin.ToString();
    }

    private void StateManager(screenState newState)
    {
        pauseScreen.SetActive(false);
        overScreen.SetActive(false);
        finishScreen.SetActive(false);

        Time.timeScale = 1f;

        switch (newState)
        {
            case screenState.game:
                break;
            case screenState.pause:
                Time.timeScale = 0f;
                pauseScreen.SetActive(true); 
                break;
            case screenState.over:
                overScreen.SetActive(true);
                break;
            case screenState.finish:
                finishScreen.SetActive(true);
                break;
        }

        currentState = newState;
    }

    public void TogglePauseUI()
    {
        if(currentState == screenState.game)
        {
            StateManager(screenState.pause);
        }
        else if (currentState == screenState.pause)
        {
            StateManager(screenState.game);
        }
    }

    public void ButtonMainMenu()
    {
        Time.timeScale = 1f;
        gm.ReturnMainMenu();
    }

    public void ButtonRestart()
    {
        gm.RestartGame();
    }

    public void OpenGameOverScreen()
    {
        StateManager(screenState.over);
    }

    public void OpenGameFinishedScreen()
    {
        StateManager(screenState.finish);
    }
}
