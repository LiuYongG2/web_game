using UnityEngine;
using UnityEngine.Events;

public class BombDefusal : GameModeBase
{
    [Header("Bomb Settings")]
    public float plantTime = 4f;
    public float defuseTime = 5f;
    public float bombTimer = 40f;
    public Transform[] bombSites;

    public UnityEvent OnBombPlanted;
    public UnityEvent OnBombDefused;
    public UnityEvent OnBombExploded;

    private bool bombPlanted;
    private bool bombDefused;
    private float bombCountdown;
    private int currentBombSite;
    private float interactProgress;
    private bool isInteracting;

    void Awake()
    {
        modeName = "拆弹模式";
        matchDuration = 180f;
    }

    public override void StartMatch()
    {
        base.StartMatch();
        bombPlanted = false;
        bombDefused = false;
        interactProgress = 0;
        OnAnnouncement?.Invoke("阻止敌方安装炸弹！");
    }

    protected override void Update()
    {
        base.Update();
        if (!matchActive) return;

        if (bombPlanted && !bombDefused)
        {
            bombCountdown -= Time.deltaTime;
            if (bombCountdown <= 0)
            {
                OnBombExploded?.Invoke();
                OnAnnouncement?.Invoke("💥 炸弹爆炸！进攻方胜利！");
                redScore += 10;
                EndMatch();
            }
        }

        if (isInteracting)
        {
            interactProgress += Time.deltaTime;
            float required = bombPlanted ? defuseTime : plantTime;
            if (interactProgress >= required)
            {
                if (!bombPlanted) PlantBomb();
                else DefuseBomb();
                isInteracting = false;
                interactProgress = 0;
            }
        }
    }

    public void StartInteract()
    {
        isInteracting = true;
        interactProgress = 0;
    }

    public void StopInteract()
    {
        isInteracting = false;
        interactProgress = 0;
    }

    void PlantBomb()
    {
        bombPlanted = true;
        bombCountdown = bombTimer;
        OnBombPlanted?.Invoke();
        OnAnnouncement?.Invoke("💣 炸弹已安装！快去拆除！");
    }

    void DefuseBomb()
    {
        bombDefused = true;
        OnBombDefused?.Invoke();
        OnAnnouncement?.Invoke("✅ 炸弹已拆除！防守方胜利！");
        blueScore += 10;
        EndMatch();
    }

    public bool IsBombPlanted => bombPlanted;
    public float BombCountdown => bombCountdown;
    public float InteractProgress => interactProgress;
    public bool IsInteracting => isInteracting;
}
