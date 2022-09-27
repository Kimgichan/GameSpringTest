using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

public class Card : MonoBehaviour
{
    #region ����

    [SerializeField] private CardSuitKind suitKind;
    [SerializeField] private int number;

    #endregion

    #region ������Ƽ
    public CardSuitKind SuitKind => suitKind;
    public int Number => number;
    public string CardKind => $"{suitKind}_{GameManager.Instance.CardDB.IntToCardNumber(number)}";
    #endregion


    #region �Լ�

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

    public bool CardEquals(Card card)
    {
        return (SuitKind == card.SuitKind && Number == card.Number);
    }

    #endregion
    #endregion
}
