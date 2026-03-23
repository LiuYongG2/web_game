using UnityEngine;

public class TeamDeathmatch : GameModeBase
{
    void Awake()
    {
        modeName = "团队竞技";
        matchDuration = 300f;
        scoreToWin = 30;
    }

    public override void OnKill(AITeam killerTeam, bool headshot)
    {
        base.OnKill(killerTeam, headshot);
        if (headshot) OnAnnouncement?.Invoke("爆头击杀！");
    }
}
