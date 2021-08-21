using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LightGive.UIUtil
{
    public class UIPressHoldDown : UIPressHold
    {
        public UnityEvent OnPressHoldDownEvent = null;

        /// <summary>
        /// 長押しの時間
        /// </summary>
        [SerializeField] float _pressHoldDownTime = 1.0f;

        public float PressTime
        {
            get => _pressHoldDownTime;
            private set => _pressHoldDownTime = Mathf.Clamp(value, 0.0f, Mathf.Infinity);
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
            CoroutineHold = StartCoroutine(TimeForPointerDown(_pressHoldDownTime));
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