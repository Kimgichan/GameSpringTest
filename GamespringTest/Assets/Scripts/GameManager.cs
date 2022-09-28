using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Nodes;

public class GameManager : MonoBehaviour
{
    #region ����
    private static GameManager instance;

    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private LevelDatabase levelDatabase;
    /// <summary>
    /// ����Ƽ���� �����ϴ� timeScale�� ��� time(real ����)�� ������ �ֹǷ� �и�.
    /// </summary>
    [SerializeField] private TimeScales timeScales;

    [SerializeField] private GameController gameController;

    #endregion

    #region ������Ƽ
    public static GameManager Instance => instance;
    public CardDatabase CardDatabase => cardDatabase;
    public LevelDatabase LevelDatabase => levelDatabase;

    public float GameTime => timeScales.gameScale * Time.deltaTime;
    public float UITime => timeScales.uiScale * Time.deltaTime;

    public GameController GameController => gameController;
    #endregion


    #region �Լ�

    #region ����
    public void SetTimeScales(TimeScales timeScales)
    {
        this.timeScales = timeScales;
    }

    public void EnterGameController(GameController gameController)
    {
        if (gameController == null && this.gameController != null)
        {
            // �� ���� ��Ʈ�ѷ��� ���� �� ��ü�� ��쿡 �����ؾߵ� �κ��� ����� ����.

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

    /// <summary>
    /// GameTimeScale�� ������ �޴� Wait �ڷ�ƾ
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public IEnumerator WaitCor_GameTimeScale(float timer)
    {
        while(timer > 0f)
        {
            yield return null;
            timer -= GameTime;
        }
    }

    /// <summary>
    /// UITimeScale�� ������ �޴� Wait �ڷ�ƾ
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public IEnumerator WaitCor_UITimeScale(float timer)
    {
        while(timer > 0f)
        {
            yield return null;
            timer -= UITime;
        }
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
