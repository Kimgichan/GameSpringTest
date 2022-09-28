using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class RankingTableController : MonoBehaviour
{
    [SerializeField] private Transform contentTr;

    public void RankingUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.SaveLoadDatabase == null) return;

        var SL = GameManager.Instance.SaveLoadDatabase;

        var slotCount = contentTr.childCount;
        var rankingCount = SL.RankingCount;
        SL.RankingSizeUpdate();

        //���� �������� ��ŷ ī��Ʈ�� ���� ���
        for(int i = 0, icount = rankingCount - slotCount; i<icount; i++)
        {
            Instantiate(contentTr.GetChild(0).gameObject, contentTr);
        }

        //���� �������� ��ŷ ī��Ʈ�� ���� ���
        for(int start = rankingCount, end = slotCount; start<end; start++)
        {
            contentTr.GetChild(start).gameObject.SetActive(false);
        }

        for(int i = 0, icount = rankingCount; i<icount; i++)
        {
            var slotTxt = contentTr.GetChild(i).GetChild(0).GetComponent<Text>();

            if(slotTxt == null)
            {
                Debug.LogWarning(slotTxt);
                return;
            }

            contentTr.GetChild(i).gameObject.SetActive(true);
            slotTxt.text = $"{i + 1}��   {SL.GetRanking(i)}";
        }
    }
}
