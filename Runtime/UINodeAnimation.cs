using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LightGive.UIUtil
{
    [RequireComponent(typeof(Animation))]
    public class UINodeAnimation : UINode
    {
        [SerializeField] AnimationClip _showClip;
        [SerializeField] AnimationClip _hideClip;
        [SerializeField] Animation _anim;

        bool _isEndAnimHide = false;

        public UnityEvent OnShowAnimationEnd { get; set; } = new UnityEvent();
        public UnityEvent OnHideAnimationEnd { get; set; } = new UnityEvent();

        private void Reset() => _anim = gameObject.GetComponent<Animation>();  

        protected override void OnShowBefore()
        {
            base.OnShowBefore();
            _anim.Play(_showClip.name);
        }

        protected override void OnShowAfter()
        {
            base.OnShowAfter();
            UITreeView.StartCoroutine(AnimationEndCheck(_showClip.name, () => OnShowAnimationEnd?.Invoke()));
        }

        protected override bool HideConditions() => _isEndAnimHide;

        protected override void OnHideBefore()
        {
            base.OnHideBefore();
            _isEndAnimHide = false;
            _anim.Play(_hideClip.name);
            UITreeView.StartCoroutine(AnimationEndCheck(_hideClip.name, () =>
            {
                _isEndAnimHide = true;
                OnHideAnimationEnd?.Invoke();
            }));
        }

        /// <summary>
        /// アニメーションが終了するまで待つ
        /// </summary>
        /// <param name="animName">アニメーションの名前</param>
        /// <param name="act">アニメーション終了時のコールバック</param>
        /// <returns></returns>
        IEnumerator AnimationEndCheck(string animName, UnityAction act = null)
        {
            while (_anim.IsPlaying(animName)) { yield return null; }
            act?.Invoke();
        }
    }
}
