using UnityEngine;
using UnityEngine.Events;

public abstract class GameModeBase : MonoBehaviour
{
    [Header("Mode Settings")]
    public string modeName;
    public float matchDuration = 120f;
    public int scoreToWin = 50;

    [Header("Events")]
    public UnityEvent<string, int> OnScoreUpdate;
    public UnityEvent<string> OnMatchEnd;
    public UnityEvent<string> OnAnnouncement;

    protected float timeRemaining;
    protected int redScore;
    protected int blueScore;
    protected bool matchActive;

    public virtual void StartMatch()
    {
        timeRemaining = matchDuration;
        redScore = 0;
        blueScore = 0;
        matchActive = true;
        OnAnnouncement?.Invoke("比赛开始！");
    }

    protected virtual void Update()
    {
        if (!matchActive) return;
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0) EndMatch();
    }

    public virtual void OnKill(AITeam killerTeam, bool headshot)
    {
        int points = headshot ? 2 : 1;
        if (killerTeam == AITeam.Red) redScore += points;
        else blueScore += points;
        OnScoreUpdate?.Invoke($"红 {redScore} - {blueScore} 蓝", 0);
        if (redScore >= scoreToWin || blueScore >= scoreToWin) EndMatch();
    }

    protected virtual void EndMatch()
    {
        matchActive = false;
        string winner = redScore > blueScore ? "红方胜利！" : blueScore > redScore ? "蓝方胜利！" : "平局！";
        OnMatchEnd?.Invoke(winner);
    }

    public float TimeRemaining => timeRemaining;
    public bool IsActive => matchActive;
}
