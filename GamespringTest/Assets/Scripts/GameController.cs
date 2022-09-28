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
    #region ����
    [SerializeField] private Transform cardDeckTr;
    [SerializeField] private TableController tableController;
    [SerializeField] private UIController uiController;
    [SerializeField] private int maxRound;
    [SerializeField] private int shuffleGap;
    [SerializeField] private float cardTurnSpeed;
    [SerializeField] private float cardMoveSpeed;

    [ReadOnly] [SerializeField] private List<Card> cards; // �ν��Ͻ��� ī���
    [ReadOnly] [SerializeField] private List<int> cardDeck; // ī���� ����

    [ReadOnly] [SerializeField] private GameState gameState;

    [ReadOnly] [SerializeField] private float currentRoundTime;
    [ReadOnly] [SerializeField] private int currentRound;
    [ReadOnly] [SerializeField] private int currentPoint;

    /// <summary>
    /// SelectedCard ������Ƽ�� ����� �� (����)
    /// </summary>
    [ReadOnly] [SerializeField] private Card selectedCard;

    private UnityAction<GameController> gameEnd;
    private UnityAction<GameController> onDestroy;

    private IEnumerator cardSettingCor;
    private IEnumerator readyCor;
    private IEnumerator timerCor;

    #endregion


    #region ������Ƽ
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
    //            // ���� ȹ��
    //        }
    //        else
    //        {

    //            // ���� �г�Ƽ
    //        }
    //    }
    //}
    #endregion


    #region �Լ�

    #region ����
    [Button("���� ����(ī��Ʈ �� ����)")]
    public void StartRoundNotUpdate()
    {
        StartRound(false);
    }
    [Button("���� ����(ī��Ʈ ����)")]
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
            Debug.LogWarning("���� ���� �߿��� Ŭ�����ּ���.");
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

    #region �̺�Ʈ
    /// <summary>
    /// ���尡 ��� ����� ���, ������ ����� ������ ���
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
    /// GameController�� �ı��� ���
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
            // ���� �г�Ƽ
            var beforeSelectedCard = selectedCard;

            UnityAction<Card> failEvent = (thisCard) =>
            {
                thisCard.CardAction(CardState.Close);
                beforeSelectedCard.CardAction(CardState.Close);
                AddPanelty();
            };
            failEvent += (Card thisCard) =>
            {
                //Debug.Log("����");
                thisCard.RemoveOpenEvent(failEvent);
            };
            card.AddOpenEvent(failEvent);

            selectedCard = null;
        }
    }

    #endregion

    #endregion

    #region �����

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

        //ī�� ��ġ

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
    /// ī�� ��ġ 
    /// </summary>
    private IEnumerator CardSettingCor()
    {
        var wait = 0.25f;
        var row = tableController.Row;
        var column = tableController.Column;

        //�� ������ ���� ī�� ����. 
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

        // ���ٽĿ��� ĸó�� ����
        // ��ġ�� ��� card�� �������� �����ߴ��� üũ��
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

                #region �ּ� (���� ������ ��ġ ���� �������� �ɸ��� �� ���Ƽ� ��ġ ������ �������� ����)
                //newCard.transform.localPosition = cardPos;
                //newCard.SetRotation(new Vector3(-90f, 0f, 0f));

                //var worldCardPos = tableController.transform.TransformPoint(cardPos);
                //newCard.CardMove(worldCardPos);
                //newCard.AddMoveEvent((card) =>
                //{
                //    arriveCount += 1;
                //    if(arriveCount >= TableController.Row * TableController.Column)
                //    {
                //        // ī�尡 �� ��ġ�� �����ϸ� Ready�̺�Ʈ ����    
                //        Debug.Log("���� �Ϸ�");
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
                        // ī�尡 �� ��ġ�� �����ϸ� Ready�̺�Ʈ ����    
                        //Debug.Log("���� �Ϸ�");

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

        // ���ٽĿ��� ĸó�� ����
        // ��ġ�� ��� ī�尡 turn�� �����ߴ��� üũ��
        var turnCount = 0;

        //Step 1 : �̵�
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
            //Step 2 : �밢�� ����
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
                                    // ���� ���� �̺�Ʈ
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

        // ������ �и� Ȯ���� �� �ִ� ������ �̺�Ʈ
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
    /// ī�带 ������ ����� ���� ����. ���� �ð��� ���� �Ϳ� ���� �߰� ������ GameRoundClear�Լ����� ����
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
    /// �� �Լ��� ������ ���Ḧ �˸��� �Ͱ� �Բ� '���� or �ʱ�ȭ' �۾��� ������.
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
