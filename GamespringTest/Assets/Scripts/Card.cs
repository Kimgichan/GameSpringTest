using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using DG.Tweening;

using Enums;
using NaughtyAttributes;



public class Card : MonoBehaviour
{
    #region ����

    [SerializeField] private CardSuitKind suitKind;
    [SerializeField] private int number;

    [ReadOnly] [SerializeField] private CardState cardState;

    private UnityAction<Card> closeEvent;
    private UnityAction<Card> openEvent;
    private UnityAction<Card> moveEvent;

    /// <summary>
    /// EulerAngle�� 0~360�� �����̱� ������ �������� ��������� ġȯ��. <br/>
    /// EulerAngle < 0 || EulerAngle > 360 ġȯ ������ ����ϱⰡ ���������� �ǰ���. <br/>
    /// �׷��� ������ ȸ���� �߰�.
    /// </summary>
    private Vector3 placeAngles;

    private IEnumerator cardActionCor;
    private IEnumerator moveCor;
    #endregion

    #region ������Ƽ
    public CardSuitKind SuitKind => suitKind;
    public int Number => number;
    public string CardKind => $"{suitKind}_{GameManager.Instance.CardDB.IntToCardNumber(number)}";
    public CardState CardState => cardState;
    #endregion


    #region �Լ�

    #region ����

    #region ������(������) �۵� ����
#if UNITY_EDITOR
    /// <summary>
    /// �����Ϳ����� ȣ���� �Լ�
    /// </summary>
    /// <param name="suitKind"></param>
    /// <param name="number"></param>
    public void Setting(CardSuitKind suitKind, int number)
    {
        this.suitKind = suitKind;
        this.number = number;
    }
#endif
    #endregion
    #region ������ + ���� �۵� ����

    #region �̺�Ʈ
    /// <summary>
    /// ī�� �޸鿡 �����ϸ� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="closeEvent"></param>
    public void AddCloseEvent(UnityAction<Card> closeEvent)
    {
        if (closeEvent == null)
        {
            Debug.LogWarning(closeEvent);
            return;
        }

        if (this.closeEvent == null) this.closeEvent = closeEvent;
        else this.closeEvent += closeEvent;
    }

    public void RemoveCloseEvent(UnityAction<Card> closeEvent)
    {
        if (closeEvent == null)
        {
            Debug.LogWarning(closeEvent);
            return;
        }

        if (this.closeEvent != null) this.closeEvent -= closeEvent;
    }

    public void RemoveAllCloseEvent()
    {
        closeEvent = null;
    }

    /// <summary>
    /// ī�� �ո鿡 �����ϸ� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="openEvent"></param>
    public void AddOpenEvent(UnityAction<Card> openEvent)
    {
        if (openEvent == null)
        {
            Debug.LogWarning(openEvent);
            return;
        }

        if (this.openEvent == null) this.openEvent = openEvent;
        else this.openEvent += openEvent;
    }

    public void RemoveOpenEvent(UnityAction<Card> openEvent)
    {
        if (openEvent == null)
        {
            Debug.LogWarning(openEvent);
            return;
        }

        if (this.openEvent != null) this.openEvent -= openEvent;
    }

    public void RemoveAllOpenEvent()
    {
        openEvent = null;
    }

    /// <summary>
    /// ��ǥ ��ġ�� �����ϸ� �̺�Ʈ �߻�
    /// </summary>
    /// <param name="moveEvent"></param>
    public void AddMoveEvent(UnityAction<Card> moveEvent)
    {
        if (moveEvent == null)
        {
            Debug.LogWarning(moveEvent);
            return;
        }

        if (this.moveEvent == null) this.moveEvent = moveEvent;
        else this.moveEvent += moveEvent;
    }

    public void RemoveMoveEvent(UnityAction<Card> moveEvent)
    {
        if (moveEvent == null)
        {
            Debug.LogWarning(moveEvent);
            return;
        }

        if (this.moveEvent != null) this.moveEvent -= moveEvent;
    }

    public void RemoveAllMoveEvent()
    {
        moveEvent = null;
    }
    #endregion

    public bool CardEquals(Card card)
    {
        return (SuitKind == card.SuitKind && Number == card.Number);
    }

    public void CardAction(CardState cardState)
    {
        //transform.DOKill();

        if (this.cardState != cardState)
        {
            if (cardActionCor != null)
            {
                StopCoroutine(cardActionCor);
            }
            switch (cardState)
            {
                case CardState.Selected:
                    {
                        //dotween�� multi timescale(�ΰ��Ӱ� UI timeScale �и��ϰ� ����) �����ϴ� ����� ã�� ���ؼ� ��� ����
                        //GameManager.Instance.GameTime or UITime ����
                        //transform.DOLocalRotate(new Vector3(0f, -45f, 0f), 4f).time;
                        cardActionCor = SelectedCor();
                        break;
                    }
                case CardState.Open:
                    {
                        cardActionCor = OpenCor();
                        break;
                    }
                case CardState.Close:
                    {
                        cardActionCor = CloseCor();
                        break;
                    }
            }

            this.cardState = cardState;
        }

        if (cardActionCor != null)
            StartCoroutine(cardActionCor);
    }

    public void SetRotation(Vector3 angles)
    {
        placeAngles = angles;
        transform.localEulerAngles = placeAngles;
    }

    /// <summary>
    /// destPos�� ���� ������ ��
    /// </summary>
    /// <param name="destPos"></param>
    public void CardMove(Vector3 destPos)
    {
        if(moveCor != null)
        {
            StopCoroutine(moveCor);
        }

        moveCor = MoveCor(destPos);
        StartCoroutine(moveCor);
    }

    #endregion

    #endregion

    #region �����
    private IEnumerator SelectedCor()
    {
        var GM = GameManager.Instance;

        while ( placeAngles.y > -45f)
        {
            yield return null;
            placeAngles.y -= GM.GameTime * GM.GameController.CardTurnSpeed;
            transform.localEulerAngles = placeAngles;
        }

        placeAngles.y = -45f;
        transform.localEulerAngles = placeAngles;

        cardActionCor = null;
    }
    private IEnumerator OpenCor()
    {
        var GM = GameManager.Instance;

        while (placeAngles.y > -180f)
        {
            yield return null;
            placeAngles.y -= GM.GameTime * GM.GameController.CardTurnSpeed;
            transform.localEulerAngles = placeAngles;
        }

        placeAngles.y = -180f;
        transform.localEulerAngles = placeAngles;

        if (openEvent != null)
        {
            openEvent(this);
        }

        cardActionCor = null;
    }
    private IEnumerator CloseCor()
    {
        var GM = GameManager.Instance;

        while (placeAngles.y < 0f)
        {
            yield return null;
            placeAngles.y += GM.GameTime * GM.GameController.CardTurnSpeed;
            transform.localEulerAngles = placeAngles;
        }

        placeAngles.y = 0f;
        transform.localEulerAngles = placeAngles;

        if (closeEvent != null)
        {
            closeEvent(this);
        }

        cardActionCor = null;
    }

    private IEnumerator MoveCor(Vector3 destPos)
    {
        float progress = 0f;
        Vector3 startPos = transform.position;

        //����� ũ�� ��� �����ϰ� �־ �������� �̷��� ����
        Vector3 force = destPos - startPos;
        var GM = GameManager.Instance;
        
        while(progress < 1f)
        {
            yield return null;

            progress += GM.GameTime * GM.GameController.CardMoveSpeed;
            transform.position = startPos + force * progress;
        }

        transform.position = destPos;

        if (moveEvent != null) moveEvent(this);

        moveCor = null;
    }
    #endregion

    #endregion
}
