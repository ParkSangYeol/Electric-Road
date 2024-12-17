using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Stage.UI
{
    public class StageDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] 
        private GameObject scrollArea;

        public float leftEndX;
        public float rightEndX;
        public float speed;
        private float beginX;

        public void OnBeginDrag(PointerEventData eventData)
        {
            beginX = eventData.position.x;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float newX = scrollArea.transform.position.x + eventData.delta.x * speed;
            Debug.Log(newX);
            if (newX < leftEndX || newX > rightEndX)
            {
                return;
            }

            Vector3 newPos = scrollArea.transform.position;
            newPos.x = newX;
            scrollArea.transform.position = newPos;
        }
    }
}
