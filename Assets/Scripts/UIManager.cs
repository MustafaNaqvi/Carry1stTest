using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Serialized Variables

    [SerializeField] private GameObject fadedBackgroundPanel;
    [SerializeField] private GameObject gameWonPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text totalMinesText;
    [SerializeField] private TMP_Text remainingMinesText;

    #endregion

    #region Unity Event Functions

    private void OnEnable()
    {
        GameManager.OnGameWon += OnGameWon;
        GameManager.OnGameOver += OnGameOver;
        GameManager.OnMineMarked += OnMineMarked;
        GameManager.OnGameRestarted += OnGameRestarted;
    }

    private void OnDisable()
    {
        GameManager.OnGameWon -= OnGameWon;
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnMineMarked -= OnMineMarked;
        GameManager.OnGameRestarted -= OnGameRestarted;
    }

    private void Start()
    {
        totalMinesText.text = $"Total Mines: {GameManager.TotalMines}";
        remainingMinesText.text = $"Remaining Mines: {GameManager.TotalMines - GameManager.MarkedMines}";
    }

    #endregion

    #region Public Functions

    public void RestartGame()
    {
        GameManager.RestartGame();
        totalMinesText.text = $"Total Mines: {GameManager.TotalMines}";
        remainingMinesText.text = $"Remaining Mines: {GameManager.TotalMines - GameManager.MarkedMines}";
    }

    public void ToggleAutoplay(bool autoplay) => GameManager.ToggleAutoplay(autoplay);

    #endregion

    #region Private Functions

    private void OnGameWon(bool gameWon)
    {
        ToggleFadedBackground(gameWon);
        if (gameWonPanel == null) return;
        gameWonPanel.SetActive(gameWon);
    }

    private void OnGameOver(bool gameOver)
    {
        ToggleFadedBackground(gameOver);
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(gameOver);
    }

    private void OnMineMarked(bool marked)
    {
        remainingMinesText.text = $"Remaining Mines: {GameManager.TotalMines - GameManager.MarkedMines}";
    }

    private void OnGameRestarted()
    {
        OnGameWon(false);
        OnGameOver(false);
    }

    private void ToggleFadedBackground(bool toggleState)
    {
        if (fadedBackgroundPanel == null) return;
        fadedBackgroundPanel.SetActive(toggleState);
    }

    #endregion
}