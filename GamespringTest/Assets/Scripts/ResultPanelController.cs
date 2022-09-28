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
                totalPoint.text = $"���� :  0";
            }
            else
            {
                totalPoint.text = $"���� :  {value}";
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
                        resultContent.text = "������ �����մϴ�!";
                        break;
                    }
                case GameResult.Fail:
                    {
                        resultContent.text = "�ƽ��� ����Դϴ�...";
                        break;
                    }
                case GameResult.Renewal:
                    {
                        resultContent.text = "��� ����, ����Ͻʴϴ�!!";
                        break;
                    }
            }
        }
    }
}
