using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "ScriptableObject/LevelDatabase", order = int.MaxValue)]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private List<float> roundTimers;


    public float GetRoundTimer(int round) => roundTimers[round];
}
