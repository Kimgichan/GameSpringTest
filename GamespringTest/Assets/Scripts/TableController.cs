using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyAttributes;
using Nodes;

public class TableController : MonoBehaviour
{
    #region ����
    [SerializeField] private Vector2 size;
    [SerializeField] private int row;
    [SerializeField] private int column;

    [ReadOnly][SerializeField] private List<Vector3Array> posList;
    #endregion


    #region ������Ƽ
    public int Row => row;
    public int Column => column;
    #endregion


    #region �Լ�

    #region ����
    [Button("ī�� ������ ����")]
    public void Setting()
    {
        Setting(row, column, size);
    }

    public void Setting(int row, int column)
    {
        Setting(row, column, size);
    }

    public void Setting(int row, int column, Vector2 size)
    {
        this.row = row;
        this.column = column;
        this.size = size;

        PositionSetting();
    }

    /// <summary>
    /// ���� ���������� ����� �������� �۾�
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public Vector3 GetPos(int row, int column)
    {
        return posList[column].array[row];
    }
    #endregion

    private void PositionSetting()
    {
        if (posList != null)
            posList.Clear();
        else posList = new List<Vector3Array>();


        Vector3 firstPos = new Vector3(-(float)(row - 1) * size.x * 0.5f, -(float)(column - 1) * size.y * 0.5f, 0f);

        for(int col = 0; col < column; col++)
        {
            var vector3Array = new Vector3Array();
            for(int r = 0; r < row; r++)
            {
                vector3Array.array.Add(firstPos + new Vector3(size.x * r, size.y * col, 0f));
            }

            posList.Add(vector3Array);
        }
    }
    #endregion
}
