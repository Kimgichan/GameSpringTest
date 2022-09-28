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
    [SerializeField] private Transform cardDeckTr;
    [SerializeField] private TableController tableController;
    [SerializeField] private int maxRound;
    [SerializeField] private int shuffleGap;
    [SerializeField] private float cardTurnSpeed;
    [SerializeField] private float cardMoveSpeed;

    [ReadOnly] [SerializeField] private List<Card> cards; // 인스턴스된 카드들
    [ReadOnly] [SerializeField] private List<int> cardDeck; // 카드의 순서
    [ReadOnly] [SerializeField] private int round;

    private UnityAction<GameController> gameEnd;
    private UnityAction<GameController> onDestroy;

    #endregion


    #region 프로퍼티
    public TableController TableController => tableController;
    public float CardTurnSpeed => cardTurnSpeed;
    public float CardMoveSpeed => cardMoveSpeed;
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
        if (tableController == null)
        {
            Debug.LogWarning(tableController);
            return;
        }

        if(!GetRowColumn(out int row, out int column))
        {
            Debug.LogWarning(round);
            return;
        }

        tableController.Setting(row, column);

        //카드 배치
        StartCoroutine(CardSettingCor());
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
    /// 카드 배치 중 중단했을 경우 정리 작업 아직 구현 안돼있음.
    /// </summary>
    private IEnumerator CardSettingCor()
    {
        var wait = new WaitForSeconds(0.25f);
        var row = tableController.Row;
        var column = tableController.Column;

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

        // 람다식에서 캡처할 변수
        // card가 목적지에 도달했는지 체크용
        var arriveCount = 0;

        for(int col = 0; col < column; col++)
        {
            for(int r = 0; r < row; r++)
            {
                var i = col * row + r;

                var cardPrefab = GameManager.Instance.CardDB.GetCardPrefab(cardDeck[cardSeats[i] % cardKind]);
                var newCard = Instantiate(cardPrefab, tableController.transform);
                newCard.transform.localScale = Vector3.one * 0.05f;
                newCard.transform.position = cardDeckTr.position;
                newCard.SetRotation(new Vector3(-90f, 180f, 0f));

                cards.Add(newCard);

                #region 주석 (생성 때문에 배치 도중 렉같은게 걸리는 것 같아서 배치 순서를 뒤쪽으로 변경)
                //newCard.transform.localPosition = cardPos;
                //newCard.SetRotation(new Vector3(-90f, 0f, 0f));

                //var worldCardPos = tableController.transform.TransformPoint(cardPos);
                //newCard.CardMove(worldCardPos);
                //newCard.AddMoveEvent((card) =>
                //{
                //    arriveCount += 1;
                //    if(arriveCount >= TableController.Row * TableController.Column)
                //    {
                //        // 카드가 각 위치에 도달하면 Ready이벤트 수행    
                //        Debug.Log("도착 완료");
                //    }
                //    card.RemoveAllMoveEvent();
                //});

                //yield return wait;
                #endregion
            }
        }

        yield return wait;
        for (int col = 0; col < column; col++)
        {
            for (int r = 0; r < row; r++)
            {
                var i = col * row + r;

                var cardPos = TableController.GetPos(r, col);
                var worldCardPos = tableController.transform.TransformPoint(cardPos);
                cards[i].CardMove(worldCardPos);
                cards[i].AddMoveEvent((card) =>
                {
                    arriveCount += 1;
                    if (arriveCount >= TableController.Row * TableController.Column)
                    {
                        // 카드가 각 위치에 도달하면 Ready이벤트 수행    
                        Debug.Log("도착 완료");
                    }
                    card.RemoveAllMoveEvent();
                });

                yield return wait;
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
