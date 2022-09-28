using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enums
{
    public enum CardSuitKind
    {
        Spade,
        Heart,
        Diamond,
        Club,
    }

    public enum CardState
    {
        Selected,
        Open,
        Close,
        Out, // ���� ���忡�� ���ܵ� ����
    }

    public enum GameState
    {
        Play,
        Ready,
    }

    public enum SaveLoadKind
    {
        Ranking,
    }

    public enum GameResult 
    {
        None, // ��� ����� ���� ������ ����
        Success, 
        Fail,
        Renewal, // �������� �Ӹ� �ƴ϶� ����� ���ŵ��� ���
    }
}
