using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Enums;

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

    [Space(20)]
    [SerializeField] private ResultPanelController resultPanelController;

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
    //public ResultPanelController ResultPanelController => resultPanelController;
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

    public void EndResult(GameController gameController)
    {
        OpenResultPanel();

        resultPanelController.TotalPoint = gameController.Point;
        resultPanelController.ResultContent = gameController.GameResult;
    }
    #endregion

    #region 비공개
    private IEnumerator Start()
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

        resultPanelController.gameObject.SetActive(false);
        Back();

        while(GameManager.Instance == null || GameManager.Instance.GameController == null)
        {
            yield return null;
        }

        GameManager.Instance.GameController.AddEndEvent(EndResult);
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
            GameReplay();

            GC.GameReset(false);
            GC.StartRound();
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

    private void OpenResultPanel()
    {
        resultPanelController.gameObject.SetActive(true);
        backEvents.Push(CloseResultPanel);
    }

    private void CloseResultPanel()
    {
        resultPanelController.gameObject.SetActive(false);
        Back();
    }

    #endregion
    #endregion
}
