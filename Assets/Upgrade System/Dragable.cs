using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dragable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private UpgradePiece parent;

    private void Start()
    {
        parent = transform.parent.gameObject.GetComponent<UpgradePiece>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parent.dragBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        parent.dragging(eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("End Drag");
        parent.dragDrop();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Clicked on");
    }
}
