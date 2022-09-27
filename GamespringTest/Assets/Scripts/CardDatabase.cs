using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

using Enums;

//using Card = UnityEngine.GameObject;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "ScriptableObject/CardDatabase", order = int.MaxValue)]
public class CardDatabase : ScriptableObject
{
    #region 변수

    [SerializeField] private List<string> keys;
    [SerializeField] private List<Card> values;

    private Dictionary<string, Card> cardDict;
    #endregion

    #region 프로퍼티
    public int CardSuitCount => 4;
    public int CardMaxNumber => 13;
    public int CardCount => keys.Count;
    #endregion


    #region 함수

    private void OnEnable()
    {
        if (keys == null || values == null) return;


        var valueCount = values.Count;

        if (keys.Count < CardSuitCount * CardMaxNumber)
        {
            keys.Clear();
            for (int currentSuit = 0, suitCount = CardSuitCount; currentSuit < suitCount; currentSuit++)
            {
                for(int currentCardNumber = 1, maxNumber = CardMaxNumber; currentCardNumber <= maxNumber; currentCardNumber++)
                {
                    string replaceCardNumber = IntToCardNumber(currentCardNumber);
                    keys.Add($"{(CardSuitKind)currentSuit}_{replaceCardNumber}");
                }
            }
        }

        cardDict = new Dictionary<string, Card>();
        for(int i = 0, icount = keys.Count; i<icount; i++)
        {
            //value 목록이 key 목록보다 작으면
            if(i >= valueCount)
            {
                cardDict.Add(keys[i], null);
            }
            else
            {
                cardDict.Add(keys[i], values[i]);
            }
        }


        #region 에디터(에서만) 작동 구문
#if UNITY_EDITOR
        {
            int i = 0;
            for (int currentSuit = 0, suitCount = CardSuitCount; currentSuit < suitCount; currentSuit++)
            {
                for (int currentCardNumber = 1, maxNumber = CardMaxNumber; currentCardNumber <= maxNumber; currentCardNumber++)
                {
                    if (i >= valueCount) return;

                    if(values[i] != null)
                    {
                        values[i].Setting((CardSuitKind)currentSuit, currentCardNumber);
                    }

                    i += 1;
                }
            }
        }
#endif
        #endregion
    }


    /// <summary>
    /// suitKind(하트, 클로버, 다이아몬트, 스페이스)<br/>
    /// cardNumber 1=A ~ 11=J, 12=Q, 13=K 
    /// </summary>
    /// <param name="suitKind"></param>
    /// <param name="cardNumber"></param>
    /// <returns></returns>
    public Card GetCardPrefab(CardSuitKind suitKind, int cardNumber)
    {
        string replaceCardNumber = IntToCardNumber(cardNumber);

        if (cardDict.TryGetValue($"{suitKind}_{replaceCardNumber}", out Card card)) return card;
        return null;
    }

    public Card GetCardPrefab(int indx) => values[indx];

    public int GetCardPrefabCount() => CardCount;


    /// <summary>
    /// number 숫자의 범위는 1~13.<br/> 
    /// 범위 안 값이 아닐 경우 null값 리턴
    /// </summary>
    public string IntToCardNumber(int number)
    {
        if (number <= 0 || number > 13) return null;


        string replaceCardNumber;
        if (number == 1) replaceCardNumber = "A";
        else if (number == 11) replaceCardNumber = "J";
        else if (number == 12) replaceCardNumber = "Q";
        else if (number == 13) replaceCardNumber = "K";
        else replaceCardNumber = number.ToString();

        return replaceCardNumber;
    }

    #endregion
}
