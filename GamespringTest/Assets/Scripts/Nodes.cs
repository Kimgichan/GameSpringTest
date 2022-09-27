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
}

