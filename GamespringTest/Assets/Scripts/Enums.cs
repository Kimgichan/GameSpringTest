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

    public enum GameResult 
    {
        None, // 경기 결과가 아직 나오지 않음
        Success, 
        Fail,
        Renewal, // 성공했을 뿐만 아니라 기록이 갱신됐을 경우
    }
}
