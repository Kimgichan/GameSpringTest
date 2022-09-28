using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nodes
{
    [System.Serializable]
    public class Vector3Array 
    {
        public List<Vector3> array;

        public Vector3Array()
        {
            array = new List<Vector3>();
        }
    }

    [System.Serializable]
    public struct TimeScales
    {
        public float gameScale; //게임쪽 time scale
        public float uiScale; //UI쪽 time scale

        public static TimeScales One => new TimeScales(1f, 1f);


        public TimeScales(float gameScale, float uiScale)
        {
            this.gameScale = gameScale;
            this.uiScale = uiScale;
        }
    }

    [System.Serializable]
    public struct Int2
    {
        public int x;
        public int y;

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Int2 operator +(Int2 op1, Int2 op2)
        {
            return new Int2(op1.x + op2.x, op1.y + op2.y);
        }

        public static Int2 operator -(Int2 op1, Int2 op2)
        {
            return new Int2(op1.x - op2.x, op1.y - op2.y);
        }
    }

    [System.Serializable]
    public struct GameData
    {
        public int point;
        public float clearTime;
    }
}

