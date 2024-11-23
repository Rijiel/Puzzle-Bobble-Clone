using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject ui;
    [SerializeField] private TextMeshProUGUI gamestateText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.Scene.GameScene));
        quitButton.onClick.AddListener(() => SceneLoader.QuitGame());
    }

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
        Hide();
    }

    private void GameManager_OnGameStateChanged()
    {
        gamestateText.text = (GameManager.Instance.HasWon() ? "YOU WIN" : "YOU LOSE") + "!!!";

        if (GameManager.Instance.IsGameOver())
            Show();
    }

    private void Show() => ui.SetActive(true);
    private void Hide() => ui.SetActive(false);
}
