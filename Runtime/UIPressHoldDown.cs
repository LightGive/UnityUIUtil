using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LightGive.UIUtil
{
    public class UIPressHoldDown : UIPressHold
    {
        public UnityEvent OnPressHoldDownEvent;
        [SerializeField]
        private float pressHoldDownTime;

        public float PressTime
        {
            get => pressHoldDownTime;
            private set => pressHoldDownTime = Mathf.Clamp(value, 0.0f, Mathf.Infinity);
        }

        public Coroutine CoroutineHold { get; set; } = null;
        public bool IsHold => CoroutineHold != null;
        public void CancelCoroutine(MonoBehaviour mono)
        {
            if (CoroutineHold == null) { return; }
            mono.StopCoroutine(CoroutineHold);
            CoroutineHold = null;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            CancelCoroutine(this);
            CoroutineHold = StartCoroutine(TimeForPointerDown(pressHoldDownTime));
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            CancelCoroutine(this);
        }

        private IEnumerator TimeForPointerDown(float pressTime = 0.0f)
        {
            yield return new WaitForSeconds(pressTime);
            OnPressHoldDownEvent?.Invoke();
            CoroutineHold = null;
        }
    }
}