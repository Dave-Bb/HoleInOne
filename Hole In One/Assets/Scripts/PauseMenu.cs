using System;
using System.Collections;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button backButton;
    
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;
    
    [SerializeField] private GameController gameController;

    private bool unpauseNextFrame;
    
    private enum PauseState
    {
        Playing,
        Paused
    }

    private PauseState pauseState;

    private void Awake()
    {
        UnpauseNextFrame();
        pauseButton.onClick.AddListener(OnPauseClicked);
        resumeButton.onClick.AddListener(OnPauseClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPauseClicked();
        }

        if (unpauseNextFrame)
        {
            UnpauseGame();
        }
    }
    
    private void OnDestroy()
    {
        pauseButton.onClick.RemoveListener(OnPauseClicked);
        resumeButton.onClick.RemoveListener(OnPauseClicked);
        restartButton.onClick.RemoveListener(OnRestartClicked);
        optionsButton.onClick.RemoveListener(OnOptionsClicked);
        backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        optionsMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    private void OnOptionsClicked()
    {
        optionsMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }

    private void OnRestartClicked()
    {
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    private void OnPauseClicked()
    {
        switch (pauseState)
        {
            case PauseState.Playing:
                PauseGame();
                break;
            case PauseState.Paused:
                UnpauseNextFrame();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UnpauseNextFrame()
    {
        unpauseNextFrame = true;
    }

    void UnpauseGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1;
        pauseState = PauseState.Playing;
        gameController.SetPause(false);
        unpauseNextFrame = false;
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        optionsMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        pauseState = PauseState.Paused;
        gameController.SetPause(true);
    }
}
