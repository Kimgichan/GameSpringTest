using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour, IPointerClickHandler
{
    //private UnityAction<PointerEventData> onClickEvent;

    // IPointer,Drag 기타 등등의 헨들러는 UI gameObject쪽에서만 함수를 호출한다는 것을 몰랐음.
    // 헨들러가 graphicRaycaster 이용해서 이벤트 관리하는 모양(추측: 확실한 확인을 하지 않은 상태).

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
