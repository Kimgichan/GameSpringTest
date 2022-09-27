using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Nodes;

public class GameManager : MonoBehaviour
{
    #region 변수
    private static GameManager instance;

    [SerializeField] private CardDatabase cardDB;

    /// <summary>
    /// 유니티에서 제공하는 timeScale은 모든 time(real 제외)의 영향을 주므로 분리.
    /// </summary>
    [SerializeField] private TimeScales timeScales;

    [SerializeField] private GameController gameController;

    #endregion

    #region 프로퍼티
    public static GameManager Instance => instance;
    public CardDatabase CardDB => cardDB;

    public float GameTime => timeScales.gameScale * Time.deltaTime;
    public float UITime => timeScales.uiScale * Time.deltaTime;

    public GameController GameController => gameController;
    #endregion


    #region 함수

    #region 공개
    public void SetTimeScales(TimeScales timeScales)
    {
        this.timeScales = timeScales;
    }

    public void EnterGameController(GameController gameController)
    {
        if (gameController == null && this.gameController != null)
        {
            // 전 게임 컨트롤러가 실행 중 교체될 경우에 정리해야될 부분이 생기면 기재.

            this.gameController = null;
            return;
        }

        if (this.gameController == gameController) return;

        gameController.AddEndEvent(GameResult);
        gameController.AddDestroyEvent((destroyController) =>
        {
            if (this.gameController == destroyController)
            {
                this.gameController = null;
            }
        });

        this.gameController = gameController;
    }

    #endregion

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SetTimeScales(TimeScales.One);
    }

 
    private void GameResult(GameController gameController)
    {

    }

    #endregion
}
