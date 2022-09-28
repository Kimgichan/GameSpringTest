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
        Out, // 게임 라운드에서 제외된 상태
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
}
