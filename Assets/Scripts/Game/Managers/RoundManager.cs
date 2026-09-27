
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [SerializeField] private AudioSource RatScoreSound;
    [SerializeField] private AudioSource MonsterScoreSound;
    [SerializeField] private AudioSource RatWinSound;
    [SerializeField] private AudioSource MonsterWinSound;

    [SerializeField] private AudioSource MainMusic;
    [SerializeField] private AudioSource TenseMusic;
    
    [SerializeField] private AudioSource TimerTickSnd;
    
    private void Start()
    {
        instance = this;
        UpdateRoundUI();
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

        int lastInt = Mathf.CeilToInt(RoundTimeLeft);
        RoundTimeLeft -= Time.deltaTime;
        if (RoundTimeLeft <= 0)
        {
            
            MainMusic.volume = 0;
            TenseMusic.volume = 0;
            if (!roundOver)
            {
                EndRound();
            }

            if (RoundTimeLeft <= -5)
            {
                SceneManager.LoadScene("Menu");
            }
            return;
        }
        
        if (RoundTimeLeft < 10)
        {
            MainMusic.volume = 0;
            TenseMusic.volume = 0.2f;
            
            if (lastInt != Mathf.CeilToInt(RoundTimeLeft))
            {
                TimerTickSnd.Play();
            }
        }
        
        UpdateRoundUI();
    }

    private void UpdateRoundUI()
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

        if (GetWinningTeam() == GameTeam.Demon)
        {
            MonsterWinSound.Play();
        } else if (GetWinningTeam() == GameTeam.Rat)
        {
            RatWinSound.Play();
        }
        
        TimeLeftText.gameObject.SetActive(false);
    }

    public void AddToScore(GameTeam team, int amount)
    {
        if (roundOver || !roundStarted)
            return;
        if (team == GameTeam.Demon)
        {
            demonScore += amount;
            MonsterScoreSound.Play();
        }
        else
        {
            ratScore += amount;
            RatScoreSound.Play();
        }
    }
}
