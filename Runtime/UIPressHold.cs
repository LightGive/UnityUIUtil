using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LightGive.UIUtil
{
    public class UIPressHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent OnPressDownEvemt = new UnityEvent();
        public UnityEvent OnPressHoldEvent = new UnityEvent();
        public UnityEvent OnPressUpEvent = new UnityEvent();
        public bool IsButtonDown { get; private set; } = false;

        private void Update()
        {
            if (!IsButtonDown) { return; }
            OnPressHoldEvent?.Invoke();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            IsButtonDown = true;
            OnPressDownEvemt?.Invoke();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            IsButtonDown = false;
            OnPressUpEvent?.Invoke();
        }
    }
}