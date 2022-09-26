using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;


[CreateAssetMenu(fileName = "CardDatabase", menuName = "ScriptableObject/CardDatabase", order = int.MaxValue)]
public class CardDatabase : ScriptableObject
{
    #region 변수

    [SerializeField] private List<string> keys;
    [SerializeField] private List<Sprite> values;

    private Dictionary<string, Sprite> cardDict;

    #endregion


    #region 함수

    private void OnEnable()
    {
        if (keys == null || values == null) return;

        if (keys.Count < 4 * 13)
        {
            keys.Clear();
            for (int currentSuit = 0, suitCount = 4; currentSuit < suitCount; currentSuit++)
            {
                for(int currentCardNumber = 1, maxNumber = 13; currentCardNumber <= maxNumber; currentCardNumber++)
                {
                    string replaceCardNumber = IntToCardNumber(currentCardNumber);
                    keys.Add($"{(CardSuitKind)currentSuit}_{replaceCardNumber}");
                }
            }
        }

        cardDict = new Dictionary<string, Sprite>();

        var valueCount = values.Count;
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
    }


    /// <summary>
    /// suitKind(하트, 클로버, 다이아몬트, 스페이스)<br/>
    /// cardNumber 1=A ~ 11=J, 12=Q, 13=K 
    /// </summary>
    /// <param name="suitKind"></param>
    /// <param name="cardNumber"></param>
    /// <returns></returns>
    public Sprite GetCardSprite(CardSuitKind suitKind, int cardNumber)
    {
        string replaceCardNumber = IntToCardNumber(cardNumber);

        if (cardDict.TryGetValue($"{suitKind}_{replaceCardNumber}", out Sprite card)) return card;
        return null;
    }


    /// <summary>
    /// number 숫자의 범위는 1~13.<br/> 
    /// 범위 안 값이 아닐 경우 null값 리턴
    /// </summary>
    private string IntToCardNumber(int number)
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
