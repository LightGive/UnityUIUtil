using System.Collections;
using UnityEngine;

namespace LightGive.UIUtil
{
    [RequireComponent(typeof(Animation))]
    public class UINodeAnimation : UINode
    {
        [SerializeField] AnimationClip _showClip = null;
        [SerializeField] AnimationClip _hideClip = null;
        [SerializeField] Animation _anim = null;

        private void Reset() => _anim = gameObject.GetComponent<Animation>();

        protected override IEnumerator ShowBeforeCoroutine(UINode baseNode)
        {
            if (_anim == null || _showClip == null) { yield break; }
            if (baseNode != this && !IsShow)
            {
                //スキップ
                _anim.Play(_showClip.name, PlayMode.StopAll);
                _anim[_showClip.name].normalizedTime = 1.0f;
                yield break;
            }
            else
            {
                _anim[_showClip.name].time = 0.0f;
                _anim.Play(_showClip.name, PlayMode.StopAll);
            }
        }

        protected override IEnumerator ShowAfterCoroutine(UINode baseNode)
        {
            if (_anim == null || !_anim.isPlaying) { yield break; }
            yield return AnimationEndCheck(_showClip.name);
        }

        protected override IEnumerator HideBeforeCoroutine(UINode baseNode)
        {
            if (_anim == null || _hideClip == null) { yield break; }
            _anim.Play(_hideClip.name);
            if (baseNode != this && IsShow)
            {
                //スキップ
                _anim.Play(_hideClip.name, PlayMode.StopAll);
                _anim[_hideClip.name].normalizedTime = 1.0f;
                yield break;
            }
            else
            {
                _anim[_hideClip.name].time = 0.0f;
                _anim.Play(_hideClip.name, PlayMode.StopAll);
            }
            yield return AnimationEndCheck(_hideClip.name);
        }

        /// <summary>
        /// アニメーションが終了するまで待つ
        /// </summary>
        /// <param name="animName">アニメーションの名前</param>
        /// <returns></returns>
        IEnumerator AnimationEndCheck(string animName)
        {
            while (_anim.IsPlaying(animName)) { yield return null; }
        }
    }
}
