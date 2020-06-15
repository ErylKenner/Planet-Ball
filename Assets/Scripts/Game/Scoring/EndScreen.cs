using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{

    public Image EndScreenImage;
    public Text EndScreenText;
    public Text FinalScoreText;

    public bool Ended = false;

    private void Awake()
    {
        Score.OnEndGame += EndGame;
    }

    public void EndGame(int teamNumber)
    {
        Time.timeScale = 0;
        Ended = true;
        EndScreenImage.gameObject.SetActive(true);
        EndScreenImage.color = Score.GetColor(teamNumber);
        EndScreenText.text = "Player " + teamNumber + " wins!";
        FinalScoreText.text = Score.GetScoresTextLong();
    }

    private void Update()
    {
        if (Ended)
        {
            Player[] players = InputAssign.players;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].ControllerInput == null)
                {
                    continue;
                }

                if (Input.GetButtonDown(players[i].ControllerInput.Button("Start")))
                {
                    Time.timeScale = 1;
                    InputAssign.currentPlayer = 1;
                    Score.ResetScores();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}
