using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;


[CreateAssetMenu(fileName = "CardDatabase", menuName = "ScriptableObject/CardDatabase", order = int.MaxValue)]
public class CardDatabase : ScriptableObject
{
    #region ����

    [SerializeField] private List<string> keys;
    [SerializeField] private List<Sprite> values;

    private Dictionary<string, Sprite> cardDict;

    #endregion


    #region �Լ�

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
                    string replaceCardNumber;
                    if (currentCardNumber == 1) replaceCardNumber = "A";
                    else if (currentCardNumber == 11) replaceCardNumber = "J";
                    else if (currentCardNumber == 12) replaceCardNumber = "Q";
                    else if (currentCardNumber == 13) replaceCardNumber = "K";
                    else replaceCardNumber = currentCardNumber.ToString();

                    keys.Add($"{(CardSuitKind)currentSuit}_{replaceCardNumber}");
                }
            }
        }

        cardDict = new Dictionary<string, Sprite>();

        var valueCount = values.Count;
        for(int i = 0, icount = keys.Count; i<icount; i++)
        {
            //value ����� key ��Ϻ��� ���� ���
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

    public Sprite GetCardSprite(string key)
    {
        if(cardDict.TryGetValue(key, out Sprite card)) return card;
        return null;
    }

    #endregion
}
