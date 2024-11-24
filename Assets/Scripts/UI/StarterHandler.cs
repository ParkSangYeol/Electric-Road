using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stage.UI
{
    public class StarterHandler : MonoBehaviour
    {
        private Vector3 defaultPos;
        [SerializeField] private Transform thresholdTransform;
        [SerializeField] private Transform activeTransform;

        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private GraphicRaycaster raycaster;
        [SerializeField] private Camera mainCamera;

        [SerializeField] private Image filledImage;

        [SerializeField] private float handleAutoMoveSpeed;
        
        [ShowInInspector]
        private HandleState state;
        private float offsetY;
        private float diffY;

        public UnityEvent onActivate;
        private bool isActivate;
        
        void Awake()
        {
            state = HandleState.IDLE;
            defaultPos = transform.position;
            diffY = activeTransform.position.y - defaultPos.y;
            isActivate = false;
        }

        void Update()
        {
            if (!isActivate)
            {
                // 핸들 이동 처리
                ControlHandle();
                // 핸들 자동 올라감 처리
                AutoMoveHandle();
            }
            // 배경 자동 채움 처리
            UpdateFilledImage();
        }

        private bool CheckClick(Vector3 position)
        {

            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = position;

            List<RaycastResult> results = new List<RaycastResult>();
            LayerMask layerMask = LayerMask.NameToLayer("StarterHandle");
            raycaster.Raycast(pointerEventData, results);
            foreach (var result in results)
            {
                if (result.gameObject.layer == layerMask)
                {
                    return true;
                }       
            }

            return false;
        }

        private void ControlHandle()
        {
            switch (state)
            {
                case HandleState.IDLE:
                    // 입력이 존재하는지 확인 후 처리
#if UNITY_STANDALONE_WIN
                    if (Input.GetMouseButtonDown(0) && CheckClick(Input.mousePosition))
                    {
                        state = HandleState.MOVEING;
                        offsetY = mainCamera.ScreenToWorldPoint(Input.mousePosition).y - transform.position.y;
                    }
#elif UNITY_ANDROID
                    
#endif
                    break;
                case HandleState.MOVEING:
                    {
                        Vector3 newPos = new Vector3(
                            transform.position.x, 
                            Mathf.Clamp(
                                mainCamera.ScreenToWorldPoint(Input.mousePosition).y - offsetY,
                                defaultPos.y, 
                                activeTransform.position.y), 
                            transform.position.z);
                        transform.position = newPos;
                        
                        if (Input.GetMouseButtonUp(0))
                        {
                            state = newPos.y > thresholdTransform.position.y
                                ? HandleState.NOT_INTERACTABLE
                                : HandleState.IDLE;
                        }
                    }
                    break;
                case HandleState.NOT_INTERACTABLE:
                    return;
                    break;
            }
        }

        private void AutoMoveHandle()
        {
            switch (state)
            {
                case HandleState.IDLE:
                    if (transform.position.y > defaultPos.y)
                    {
                        transform.position -= new Vector3(0, handleAutoMoveSpeed, 0);
                    }
                    else
                    {
                        transform.position = defaultPos;
                    }
                    break;
                case HandleState.NOT_INTERACTABLE:
                    if (transform.position.y < activeTransform.position.y)
                    {
                        transform.position += new Vector3(0,handleAutoMoveSpeed, 0);
                    }
                    else
                    {
                        transform.position = activeTransform.position;
                        if (!isActivate)
                        {
                            isActivate = true;
                            onActivate.Invoke();
                        }
                    }
                    break;
            }
        }
        private void UpdateFilledImage()
        {
            filledImage.fillAmount = (transform.position.y - defaultPos.y) / diffY;
        }
        
#if UNITY_EDITOR
        [Button]
        public void TestReset()
        {
            StartCoroutine(ResetHandler());
        }
#endif
        public IEnumerator ResetHandler()
        {
            while (transform.position.y >= thresholdTransform.position.y)
            {
                transform.position -= new Vector3(0, handleAutoMoveSpeed, 0);
                yield return null;
            }

            state = HandleState.IDLE;
            isActivate = false;
        }
    }

    public enum HandleState
    {
        IDLE,
        MOVEING,
        NOT_INTERACTABLE
    }
}