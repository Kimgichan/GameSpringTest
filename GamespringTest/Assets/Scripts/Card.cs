using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

public class Card : MonoBehaviour
{
    #region 변수

    [SerializeField] private CardSuitKind suitKind;
    [SerializeField] private int number;

    #endregion

    #region 프로퍼티
    public CardSuitKind SuitKind => suitKind;
    public int Number => number;
    public string CardKind => $"{suitKind}_{GameManager.Instance.CardDB.IntToCardNumber(number)}";
    #endregion


    #region 함수

    #region 에디터(에서만) 작동 구문
#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서만 호출할 함수
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


    #region 에디터 + 빌드 작동 구문

    public bool CardEquals(Card card)
    {
        return (SuitKind == card.SuitKind && Number == card.Number);
    }

    #endregion
    #endregion
}
