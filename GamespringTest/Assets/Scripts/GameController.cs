using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using DG.Tweening;

using Nodes;
using Enums;
using NaughtyAttributes;


public class GameController : MonoBehaviour
{
    #region 변수
    [SerializeField] private Transform cardDeckTr;
    [SerializeField] private TableController tableController;
    [SerializeField] private UIController uiController;
    [SerializeField] private int maxRound;
    [SerializeField] private int shuffleGap;
    [SerializeField] private float cardTurnSpeed;
    [SerializeField] private float cardMoveSpeed;

    [ReadOnly] [SerializeField] private List<Card> cards; // 인스턴스된 카드들
    [ReadOnly] [SerializeField] private List<int> cardDeck; // 카드의 순서

    [ReadOnly] [SerializeField] private GameState gameState;

    [ReadOnly] [SerializeField] private float currentRoundTime;
    [ReadOnly] [SerializeField] private int currentRound;
    [ReadOnly] [SerializeField] private int currentPoint;

    /// <summary>
    /// SelectedCard 프로퍼티를 사용할 것 (삭제)
    /// </summary>
    [ReadOnly] [SerializeField] private Card selectedCard;

    private UnityAction<GameController> gameEnd;
    private UnityAction<GameController> onDestroy;

    private IEnumerator cardSettingCor;
    private IEnumerator readyCor;
    private IEnumerator timerCor;

    #endregion


    #region 프로퍼티
    public TableController TableController => tableController;
    public UIController UIController => uiController;
    public float CardTurnSpeed => cardTurnSpeed;
    public float CardMoveSpeed => cardMoveSpeed;
    public GameState GameState => gameState;

    public float CurrentRoundTime => currentRoundTime;

    public int MaxRound => maxRound;
    public int Round => currentRound;

    public int Point => currentPoint;

    //private Card SelectedCard
    //{
    //    get => selectedCard;
    //    set
    //    {
    //        if (value.CardState == CardState.Open) return;

    //        if(value == null)
    //        {
    //            if (selectedCard != null)
    //            {
    //                selectedCard.CardAction(CardState.Close);
    //                selectedCard = null;
    //            }
    //            return;
    //        }

    //        if(selectedCard == null)
    //        {
    //            selectedCard = value;
    //            selectedCard.CardAction(CardState.Open);
    //            return;
    //        }

    //        if (selectedCard == value) return;

    //        value.CardAction(CardState.Open);
    //        if (selectedCard.CardEquals(value))
    //        {
    //            // 점수 획득
    //        }
    //        else
    //        {

    //            // 실패 패널티
    //        }
    //    }
    //}
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

    public void StartRound(bool currentRoundUpdate = true)
    {
        gameState = GameState.Ready;
        CardClear();

        if (!Application.isPlaying)
        {
            Debug.LogWarning("게임 실행 중에만 클릭해주세요.");
            return;
        }


        if (currentRoundUpdate)
        {
            if (currentRound >= maxRound)
            {
                if (gameEnd != null)
                {
                    gameEnd(this);
                }

                currentRound = 0;
                currentPoint = 0;
                return;
            }
            currentRound += 1;
            this.UIController.Round = Round;
        }

        this.UIController.Timer = GameManager.Instance.LevelDatabase.GetRoundTimer(Round - 1);
        this.UIController.Point = Point;

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

    public void SelectCard(Card card)
    {
        if (card.CardState == CardState.Open) return;

        if (card == null)
        {
            if (selectedCard != null)
            {
                selectedCard.CardAction(CardState.Close);
                selectedCard = null;
            }
            return;
        }

        if (selectedCard == null)
        {
            selectedCard = card;
            selectedCard.CardAction(CardState.Open);
            return;
        }

        if (selectedCard == card) return;

        card.CardAction(CardState.Open);
        if (selectedCard.CardEquals(card))
        {
            var beforeSelectedCard = selectedCard;

            UnityAction<Card> successEvent = (thisCard) =>
            {
                thisCard.CardAction(CardState.Out);
                beforeSelectedCard.CardAction(CardState.Out);

                var isRoundEnd = true;
                for (int i = 0, icount = cards.Count; i < icount; i++)
                {
                    if (cards[i].CardState != CardState.Out)
                    {
                        isRoundEnd = false;
                    }
                }

                if (isRoundEnd)
                {
                    GameRoundClear();
                }
                AddReward();
            };
            successEvent += (Card thisCard) =>
            {
                thisCard.RemoveOpenEvent(successEvent);
            };
            card.AddOpenEvent(successEvent);
            selectedCard = null;
        }
        else
        {
            // 실패 패널티
            var beforeSelectedCard = selectedCard;

            UnityAction<Card> failEvent = (thisCard) =>
            {
                thisCard.CardAction(CardState.Close);
                beforeSelectedCard.CardAction(CardState.Close);
                AddPanelty();
            };
            failEvent += (Card thisCard) =>
            {
                //Debug.Log("실패");
                thisCard.RemoveOpenEvent(failEvent);
            };
            card.AddOpenEvent(failEvent);

            selectedCard = null;
        }
    }

    #endregion

    #endregion

    #region 비공개

    private IEnumerator Start()
    {
        gameState = GameState.Ready;
        var wait = new WaitForSeconds(0.5f);
        while(GameManager.Instance == null)
        {
            yield return wait;
        }

        currentRound = 0;
        this.UIController.Init();
        GameManager.Instance.EnterGameController(this);

        GameReset();
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
        var cardCount = GameManager.Instance.CardDatabase.CardCount;
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
            Debug.LogWarning(currentRound);
            return;
        }

        tableController.Setting(row, column);

        //카드 배치

        if (cardSettingCor != null) CardClear();
        cardSettingCor = CardSettingCor();
        StartCoroutine(cardSettingCor);
    }

    private bool GetRowColumn(out int row, out int column)
    {
        if(currentRound < 1 || currentRound > maxRound)
        {
            row = -1;
            column = -1;
            return false;
        }

        if(currentRound == 1)
        {
            row = 2;
            column = 2;
        }
        else if(currentRound == 2)
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
    private IEnumerator CardSettingCor()
    {
        var wait = 0.25f;
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
        // 배치된 모든 card가 목적지에 도달했는지 체크용
        var arriveCount = 0;

        for(int col = 0; col < column; col++)
        {
            for(int r = 0; r < row; r++)
            {
                var i = col * row + r;

                var cardPrefab = GameManager.Instance.CardDatabase.GetCardPrefab(cardDeck[cardSeats[i] % cardKind]);
                var newCard = Instantiate(cardPrefab, tableController.transform);
                newCard.transform.localScale = Vector3.one * 0.05f;
                newCard.transform.position = cardDeckTr.position;
                newCard.SetRotation(new Vector3(-90f, 0f, 0f));
                newCard.SetCardState(CardState.Close);
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

        yield return StartCoroutine(GameManager.Instance.WaitCor_GameTimeScale(wait));
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
                    if (arriveCount >= cards.Count)
                    {
                        // 카드가 각 위치에 도달하면 Ready이벤트 수행    
                        //Debug.Log("도착 완료");

                        //Ready(CardState.Open);
                        if (readyCor != null) StopCoroutine(readyCor);
                        readyCor = ReadyCor(CardState.Open);
                        StartCoroutine(readyCor);
                    }
                    card.RemoveAllMoveEvent();
                });

                yield return StartCoroutine(GameManager.Instance.WaitCor_GameTimeScale(wait));
            }
        }

        cardSettingCor = null;
    }

    private void CardClear()
    {
        if (cards == null) return;

        if(cardSettingCor != null)
        {
            StopCoroutine(cardSettingCor);
            cardSettingCor = null;
        }

        for (int i = 0, icount = cards.Count; i < icount; i++)
        {
            Destroy(cards[i].gameObject);
            cards[i] = null;
        }

        cards.Clear();
    }

    private IEnumerator ReadyCor(CardState cardState)
    {
        if (cardState == CardState.Selected)
        {
            readyCor = null;
            yield break;
        }

        //var step1Wait = new WaitForSeconds(0.25f);
        var step1Wait = 0.5f;
        yield return null;

        var pivotStep1 = new Int2(0, -1);

        // 람다식에서 캡처할 변수
        // 배치된 모든 카드가 turn을 수행했는지 체크용
        var turnCount = 0;

        //Step 1 : 이동
        for(int i = 0, icount = TableController.Row + TableController.Column - 1; i<icount; i++)
        {
            if(pivotStep1.y < TableController.Column - 1)
            {
                pivotStep1.y += 1;
            }
            else
            {
                pivotStep1.x += 1;
            }

            var pivotStep2 = pivotStep1;
            //Step 2 : 대각선 전파
            while(pivotStep2.y >= 0 && pivotStep2.x < TableController.Row)
            {
                var card = cards[pivotStep2.y * TableController.Row + pivotStep2.x];

                switch (cardState)
                {
                    case CardState.Open:
                        {
                            UnityAction<Card> openEvent = (thisCard) =>
                            {
                                turnCount += 1;

                                if(turnCount >= cards.Count)
                                {
                                    if(readyCor != null)
                                    {
                                        StopCoroutine(readyCor);
                                        readyCor = null;
                                    }

                                    this.StartCoroutine(CheckEventCor());
                                }
                            };
                            openEvent += (Card thisCard) =>
                            {
                                thisCard.RemoveOpenEvent(openEvent);
                            };

                            card.AddOpenEvent(openEvent);
                            card.CardAction(CardState.Open);
                            break;
                        }
                    case CardState.Close:
                        {
                            UnityAction<Card> closeEvent = (thisCard) =>
                            {
                                turnCount += 1;

                                if(turnCount >= cards.Count)
                                {
                                    // 게임 실행 이벤트
                                    GameRun();
                                }
                            };
                            closeEvent += (Card thisCard) =>
                            {
                                thisCard.RemoveCloseEvent(closeEvent);
                            };

                            card.AddCloseEvent(closeEvent);
                            card.CardAction(CardState.Close);
                            break;
                        }
                }

                pivotStep2 += new Int2(1, -1);
            }

            yield return StartCoroutine(GameManager.Instance.WaitCor_GameTimeScale(step1Wait));
        }

        readyCor = null;
    }

    private IEnumerator CheckEventCor()
    {
        //var checkWait = new WaitForSeconds(2f);
        var checkWait = 1.5f;

        // 유저가 패를 확인할 수 있는 딜레이 이벤트
        //yield return checkWait;
        yield return GameManager.Instance.WaitCor_GameTimeScale(checkWait);

        if (readyCor != null) StopCoroutine(readyCor);
        readyCor = ReadyCor(CardState.Close);
        StartCoroutine(readyCor);
    }

    private void GameRoundClear()
    {
        if (timerCor != null)
        {
            StopCoroutine(timerCor);
            timerCor = null;
        }

        var bonus = Mathf.FloorToInt(CurrentRoundTime);
        currentPoint += bonus;
        UIController.Point = Point;

        if (Round >= MaxRound)
        {
            GameSuccess();
        }
        else StartRound();
    }
    private void GameRun()
    {
        gameState = GameState.Play;

        if (timerCor != null) StopCoroutine(timerCor);
        timerCor = TimerCor();
        StartCoroutine(timerCor);
    }
    private IEnumerator TimerCor()
    {
        //currentRoundTime = 0f;
        currentRoundTime = GameManager.Instance.LevelDatabase.GetRoundTimer(Round - 1);
        while (currentRoundTime > 0f)
        {
            UIController.Timer = CurrentRoundTime;
            yield return null;

            currentRoundTime -= GameManager.Instance.GameTime;
        }

        UIController.Timer = 0f;
        GameFail();
    }

    private void GameSuccess()
    {
        var SL = GameManager.Instance.SaveLoadDatabase;
        SL.RankingSizeUpdate();
        SL.RenewalRanking(Point);

        GameReset();
    }
    private void GameFail()
    {

        GameReset();
    }

    /// <summary>
    /// 카드를 맞췄을 경우의 보상만 수행. 여유 시간이 남은 것에 따른 추가 보상은 GameRoundClear함수에서 진행
    /// </summary>
    private void AddReward()
    {
        currentPoint += 1;
        UIController.Point = currentPoint;
    }
    private void AddPanelty()
    {
        currentRoundTime -= 1f;
        UIController.ShowPanelty();
    }

    /// <summary>
    /// 이 함수는 게임의 종료를 알리는 것과 함께 '정리 or 초기화' 작업도 실행함.
    /// </summary>
    private void GameReset()
    {
        if (timerCor != null)
        {
            StopCoroutine(timerCor);
            timerCor = null;
        }

        GameManager.Instance.SaveLoadDatabase.Load(SaveLoadKind.Ranking);

        currentRound = MaxRound;
        StartRound();

        UIController.Back();
    }

    #endregion
    #endregion
}
