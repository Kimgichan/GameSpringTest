using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using DG.Tweening;

using Nodes;
using NaughtyAttributes;


public class GameController : MonoBehaviour
{
    #region ����
    [SerializeField] private TableController tableController;
    [SerializeField] private int maxRound;
    [SerializeField] private int shuffleGap;

    [ReadOnly] [SerializeField] private List<Card> cards; // �ν��Ͻ��� ī���
    [ReadOnly] [SerializeField] private List<int> cardDeck; // ī���� ����
    [ReadOnly] [SerializeField] private int round;

    private UnityAction<GameController> gameEnd;
    private UnityAction<GameController> onDestroy;

    #endregion


    #region ������Ƽ
    public TableController TableController => tableController;
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

    public void StartRound(bool roundUpdate = true)
    {
        CardClear();

        if (!Application.isPlaying)
        {
            Debug.LogWarning("���� ���� �߿��� Ŭ�����ּ���.");
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

    #endregion

    #endregion

    #region �����

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

        //ī�� ��ġ
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
    /// ī�� ��ġ
    /// </summary>
    private void CardSetting()
    {
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
        
        for(int col = 0; col < column; col++)
        {
            for(int r = 0; r < row; r++)
            {
                var i = col * column + r;
                var cardPos = tableController.GetPos(r, col);

                var cardPrefab = GameManager.Instance.CardDB.GetCardPrefab(cardDeck[i % cardKind]);
                var newCard = Instantiate(cardPrefab, tableController.transform);
                newCard.transform.localScale = Vector3.one * 0.05f;
                newCard.transform.position = tableController.transform.position;

                cards.Add(newCard);

                newCard.transform.localPosition = cardPos;
                newCard.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
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
