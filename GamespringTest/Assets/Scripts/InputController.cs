using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour, IPointerClickHandler
{
    //private UnityAction<PointerEventData> onClickEvent;

    // IPointer,Drag ��Ÿ ����� ��鷯�� UI gameObject�ʿ����� �Լ��� ȣ���Ѵٴ� ���� ������.
    // ��鷯�� graphicRaycaster �̿��ؼ� �̺�Ʈ �����ϴ� ���(����: Ȯ���� Ȯ���� ���� ���� ����).

    public void OnPointerClick(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
        //if(onClickEvent != null)
        //{
        //    onClickEvent(eventData);
        //}

        if(GameManager.Instance != null && GameManager.Instance.GameController != null)
        {
            var GC = GameManager.Instance.GameController;
            if (GC.GameState != Enums.GameState.Play) return;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(eventData.position), out RaycastHit hit))
            {
                var card = hit.collider.gameObject.GetComponent<Card>();
                if(card != null)
                {
                    GC.SelectCard(card);
                }
            }
        }
    }
}
