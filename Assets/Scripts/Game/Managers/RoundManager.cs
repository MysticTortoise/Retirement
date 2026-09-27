
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameTeam
{
    Demon,
    Rat,
    None
}

public class RoundManager : MonoBehaviour
{

    [SerializeField] public float RoundTimeLeft;

    [NonSerialized] public int demonScore;
    [NonSerialized] public int ratScore;

    [SerializeField] private TextMeshProUGUI DemonScoreText;
    [SerializeField] private TextMeshProUGUI RatScoreText;
    [SerializeField] private TextMeshProUGUI TimeLeftText;

    [SerializeField] private TextMeshProUGUI WinText;

    public static RoundManager instance;

    [NonSerialized] public bool roundStarted = false;
    [NonSerialized] public bool roundOver = false;
    
    private void Start()
    {
        instance = this;
    }

    void StartRound()
    {
        roundStarted = true;
    }

    public GameTeam GetWinningTeam()
    {
        if (ratScore > demonScore)
        {
            return GameTeam.Rat;
        }

        if (demonScore > ratScore)
        {
            return GameTeam.Demon;
        }

        return GameTeam.None;
    }

    private void Update()
    {
        if (!roundStarted)
        {
            return;
        }
        RoundTimeLeft -= Time.deltaTime;
        if (RoundTimeLeft <= 0)
        {
            if (!roundOver)
            {
                EndRound();
            }

            if (RoundTimeLeft <= -5)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }
        
        RoundTick();
    }

    private void RoundTick()
    {
        DemonScoreText.text = demonScore.ToString();
        RatScoreText.text = ratScore.ToString();

        TimeLeftText.text = Mathf.Ceil(RoundTimeLeft).ToString(CultureInfo.CurrentCulture);
    }

    public void EndRound()
    {
        roundOver = true;
        WinText.gameObject.SetActive(true);

        WinText.text = GetWinningTeam() switch
        {
            GameTeam.Demon => "Demon Wins!",
            GameTeam.Rat => "Rats Win!",
            GameTeam.None => "DRAW!",
            _ => "ERROR IDK WHO WON"
        };
        
        TimeLeftText.gameObject.SetActive(false);
    }

    public void AddToScore(GameTeam team, int amount)
    {
        if (roundOver || !roundStarted)
            return;
        if (team == GameTeam.Demon)
        {
            demonScore += amount;
        }
        else
        {
            ratScore += amount;
        }
    }
}
