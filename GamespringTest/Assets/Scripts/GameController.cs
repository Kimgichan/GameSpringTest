using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using DG.Tweening;

using Nodes;
using NaughtyAttributes;


public class GameController : MonoBehaviour
{
    #region 변수
    [SerializeField] private SettingController settingController;
    [SerializeField] private int maxRound;
    [SerializeField] private int shuffleGap;

    [ReadOnly] [SerializeField] private List<Card> cards; // 인스턴스된 카드들
    [ReadOnly] [SerializeField] private List<int> cardDeck; // 카드의 순서
    [ReadOnly] [SerializeField] private int round;

    private UnityAction<GameController> gameEnd;
    private UnityAction<GameController> onDestroy;

    #endregion


    #region 프로퍼티
    public SettingController SettingController => settingController;
    #endregion


    #region 함수

    #region 공개
    [Button("라운드 시작(카운트 노 갱신)")]
    public void StartRoundNotUpdate()
    {
        StartRound(false);
    }
    [Button("라운드 시작(카운트 갱신)")]
    public void StartRoundUpdate()
    {
        StartRound();
    }

    public void StartRound(bool roundUpdate = true)
    {
        CardClear();

        if (!Application.isPlaying)
        {
            Debug.LogWarning("게임 실행 중에만 클릭해주세요.");
            return;
        }


        if (roundUpdate)
        {
            if (round >= maxRound)
            {
                if (gameEnd != null)
                {
                    gameEnd(this);
                }

                round = 0;
                return;
            }
            round += 1;
        }

        Shuffle();
        PlaceCard();
    }

    #region 이벤트
    /// <summary>
    /// 라운드가 모두 종료될 경우, 게임의 결과가 정해진 경우
    /// </summary>
    /// <param name="endEvent"></param>
    public void AddEndEvent(UnityAction<GameController> endEvent)
    {
        if (endEvent == null)
        {
            Debug.LogWarning(endEvent);
            return;
        }

        if (gameEnd == null) gameEnd = endEvent;
        else gameEnd += endEvent;
    }

    public void RemoveEndEvent(UnityAction<GameController> endEvent)
    {
        if (endEvent == null)
        {
            Debug.LogWarning(endEvent);
            return;
        }

        if (gameEnd == null) return;

        gameEnd -= endEvent;
    }

    /// <summary>
    /// GameController가 파괴될 경우
    /// </summary>
    /// <param name="destroyEvent"></param>
    public void AddDestroyEvent(UnityAction<GameController> destroyEvent)
    {
        if (destroyEvent == null)
        {
            Debug.LogWarning(destroyEvent);
            return;
        }

        if (onDestroy == null) onDestroy = destroyEvent;
        else onDestroy += destroyEvent;
    }
    public void RemoveDestroyEvent(UnityAction<GameController> destroyEvent)
    {
        if (destroyEvent == null)
        {
            Debug.LogWarning(destroyEvent);
            return;
        }

        if (onDestroy == null) return;

        onDestroy -= destroyEvent;
    }

    public void RemoveAllEndEvent()
    {
        gameEnd = null;
    }

    public void RemoveAllDestroyEvent()
    {
        onDestroy = null;
    }

    #endregion

    #endregion

    #region 비공개

    private IEnumerator Start()
    {
        var wait = new WaitForSeconds(0.5f);
        while(GameManager.Instance == null)
        {
            yield return wait;
        }
        round = 0;

        GameManager.Instance.EnterGameController(this);
    }

    private void OnDestroy()
    {
        if(onDestroy != null)
        {
            onDestroy(this);
        }
    }

    private void Shuffle()
    {
        var cardCount = GameManager.Instance.CardDB.CardCount;
        if(cardDeck == null || cardDeck.Count != cardCount)
        {
            if (cardDeck == null) cardDeck = new List<int>();
            else cardDeck.Clear();

            for(int i = 0, icount = cardCount; i<icount; i++)
            {
                cardDeck.Add(i);
            }
        }

        for(int i = 0, icount = cardDeck.Count; i<icount; i+=shuffleGap)
        {
            int swapIndx = Random.Range(0, icount);
            int swapValue = cardDeck[swapIndx];
            cardDeck[swapIndx] = cardDeck[i];
            cardDeck[i] = swapValue;
        }
    }

    private void PlaceCard()
    {
        if (settingController == null)
        {
            Debug.LogWarning(settingController);
            return;
        }

        if(!GetRowColumn(out int row, out int column))
        {
            Debug.LogWarning(round);
            return;
        }

        settingController.Setting(row, column);

        //카드 배치
        CardSetting();
    }

    private bool GetRowColumn(out int row, out int column)
    {
        if(round < 1 || round > maxRound)
        {
            row = -1;
            column = -1;
            return false;
        }

        if(round == 1)
        {
            row = 2;
            column = 2;
        }
        else if(round == 2)
        {
            row = 4;
            column = 2;
        }
        else
        {
            row = 6;
            column = 3;
        }

        return true;
    }


    /// <summary>
    /// 카드 배치
    /// </summary>
    private void CardSetting()
    {
        var row = settingController.Row;
        var column = settingController.Column;

        //현 라운드의 나올 카드 종류. 
        var cardKind = row * column / 2;

        if (cards == null) cards = new List<Card>();

        var cardSeats = new List<int>();
        for(int i = 0, icount = row * column; i<icount; i++)
        {
            cardSeats.Add(i);
        }


        for (int i = 0, icount = cardSeats.Count; i<icount; i++)
        {
            var swapIndx = Random.Range(0, icount);
            var swapValue = cardSeats[swapIndx];
            cardSeats[swapIndx] = cardSeats[i];
            cardSeats[i] = swapValue;
        }
        
        for(int col = 0; col < column; col++)
        {
            for(int r = 0; r < row; r++)
            {
                var i = col * column + r;
                var cardPos = settingController.GetPos(r, col);


            }
        }
    }

    public void CardClear()
    {
        if (cards == null) return;

        for (int i = 0, icount = cards.Count; i < icount; i++)
        {
            Destroy(cards[i].gameObject);
            cards[i] = null;
        }

        cards.Clear();
    }
    #endregion
    #endregion
}
