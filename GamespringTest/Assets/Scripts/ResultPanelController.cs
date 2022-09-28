using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Enums;

public class ResultPanelController : MonoBehaviour
{
    [SerializeField] private Text totalPoint;
    [SerializeField] private Text resultContent;

    public int TotalPoint
    {
        set
        {
            if(value <= 0)
            {
                totalPoint.text = $"점수 :  0";
            }
            else
            {
                totalPoint.text = $"점수 :  {value}";
            }
        }
    }

    public GameResult ResultContent
    {
        set
        {
            switch (value)
            {
                case GameResult.Success:
                    {
                        resultContent.text = "성공을 축하합니다!";
                        break;
                    }
                case GameResult.Fail:
                    {
                        resultContent.text = "아쉬운 결과입니다...";
                        break;
                    }
                case GameResult.Renewal:
                    {
                        resultContent.text = "기록 갱신, 대단하십니다!!";
                        break;
                    }
            }
        }
    }
}
