using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action OnGameStateChanged;

    public static GameManager Instance { get; private set; }

    private bool hasWon;

    private enum GameState
    {
        GamePlaying,
        GameOver
    }

    private GameState gameState;

    public bool IsGamePlaying() => gameState == GameState.GamePlaying;

    public bool IsGameOver() => gameState == GameState.GameOver;

    public bool HasWon() => hasWon;

    private void Awake()
    {
        Instance = this;                
    }

    private void Start()
    {
        Time.timeScale = 1f;
        gameState = GameState.GamePlaying;
        OnGameStateChanged?.Invoke();
    }

    public void GameOver(bool hasWon)
    {
        gameState = GameState.GameOver;

        this.hasWon = hasWon;

        OnGameStateChanged?.Invoke();

        Time.timeScale = 0f;
    }
}
