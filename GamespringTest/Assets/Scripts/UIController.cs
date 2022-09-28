using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    #region 변수
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button backBtn;

    [Space(20)]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button replayBtn;
    [SerializeField] private RankingTableController rankingTableController;

    #region ShowPanel
    [Space(20)]
    [SerializeField] private GameObject showPanel;
    [SerializeField] private Text point;
    [SerializeField] private Text round;
    [SerializeField] private Text timer;
    [SerializeField] private Text panelty;
    #endregion


    private Stack<UnityAction> backEvents;
    private IEnumerator paneltyCor;
    #endregion


    #region 프로퍼티
    public int Point
    {
        set
        {
            if (value < 0) value = 0;
            point.text = $"{value}";
        }
    }

    public int Round
    {
        set
        {
            if(value <= 0 || value > GameManager.Instance.GameController.MaxRound)
            {
                round.text = $"Stage";
                return;
            }

            round.text = $"Stage {value}";
        }
    }

    public float Timer
    {
        set
        {
            if(value <= 0f)
            {
                timer.text = $"Timer: 0.000(s)";
            }
            timer.text = $"Timer: {value.ToString("F3")}(s)";
        }
    }

    public RankingTableController RankingTableController => rankingTableController;
    #endregion


    #region 함수
    #region 공개
    public void Init()
    {
        Point = 0;
        Round = 0;
        Timer = 0f;
    }

    public void Back()
    {
        if (backEvents == null || backEvents.Count == 0)
        {
            Debug.LogWarning($"{backEvents}");
            return;
        }

        if (backEvents.Count > 1)
        {
            backEvents.Pop()();
        }
        else backEvents.Peek()();
    }

    public void ShowPanelty()
    {
        if (paneltyCor != null) StopCoroutine(paneltyCor);
        paneltyCor = PaneltyCor();
        StartCoroutine(paneltyCor);
    }
    #endregion

    #region 비공개
    private void Start()
    {
        backEvents = new Stack<UnityAction>();

        backBtn.onClick.AddListener(Back);
        startBtn.onClick.AddListener(GameStart);
        replayBtn.onClick.AddListener(GameReplay);

        panelty.enabled = false;

        backEvents.Push(() =>
        {
            GameManager.Instance.SetTimeScales(new Nodes.TimeScales(0f, 1f));

            gamePanel.SetActive(false);
            showPanel.SetActive(false);
            LobbyEnter();
        });

        Back();
    }

    private void LobbyEnter()
    {
        lobbyPanel.SetActive(true);
        if (GameManager.Instance == null || GameManager.Instance.GameController == null || GameManager.Instance.GameController.Round <= 0)
        {
            replayBtn.gameObject.SetActive(false);

            if (GameManager.Instance == null || GameManager.Instance.GameController == null)
            {
                RankingTableController.gameObject.SetActive(false);
            }
            else
            {
                RankingTableController.gameObject.SetActive(true);
                RankingTableController.RankingUpdate();
            }
        }
        else
        {
            replayBtn.gameObject.SetActive(true);

            RankingTableController.gameObject.SetActive(true);
            RankingTableController.RankingUpdate();
        }
    }

    private void GameStart()
    {
        if(GameManager.Instance != null && GameManager.Instance.GameController != null)
        {
            var GC = GameManager.Instance.GameController;
            GC.StartRound();

            GameReplay();
        }
    }

    private void GameReplay()
    {
        GameManager.Instance.SetTimeScales(new Nodes.TimeScales(1f, 1f));

        gamePanel.SetActive(true);
        showPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    private IEnumerator PaneltyCor()
    {
        panelty.enabled = true;
        yield return StartCoroutine(GameManager.Instance.WaitCor_GameTimeScale(0.5f));

        panelty.enabled = false;
        paneltyCor = null;
    }
    #endregion
    #endregion
}
