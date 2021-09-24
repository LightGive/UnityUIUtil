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

        protected override void OnInit()
        {
            base.OnInit();
            if (_anim != null) { 
            _anim.playAutomatically = false;
        }}

        protected override IEnumerator ShowBeforeCoroutine(UINode callerNode)
        {
            if (_anim == null || _showClip == null || _anim.IsPlaying(_showClip.name))
            {
                yield break;
            }
            if (callerNode == this)
            {
                //呼び出し元のUINodeの処理
                _anim[_showClip.name].time = 0.0f;
                _anim.Play(_showClip.name, PlayMode.StopAll);
                Debug.Log("IsPlaying:" + _anim.IsPlaying(_showClip.name));
            }
            else
            {
                _anim.Play(_showClip.name, PlayMode.StopAll);
                _anim[_showClip.name].normalizedTime = 1.0f;
            }
        }

        protected override IEnumerator ShowAfterCoroutine(UINode callerNode)
        {
            if (!_anim.IsPlaying(_showClip.name) || callerNode != this)
            {
                yield break;
            }
            yield return UITreeView.StartCoroutine(AnimationEndCheck(_showClip.name));
        }

        protected override IEnumerator HideBeforeCoroutine(UINode callerNode)
        {
            if (_anim == null || _hideClip == null || _anim.IsPlaying(_hideClip.name))
            {
                yield break;
            }
            _anim.Play(_hideClip.name);
            if (callerNode == this)
            {
                _anim[_hideClip.name].time = 0.0f;
                _anim.Play(_hideClip.name, PlayMode.StopAll);
                yield return UITreeView.StartCoroutine(AnimationEndCheck(_hideClip.name));
            }
            else
            {
                //スキップ
                _anim.Play(_hideClip.name, PlayMode.StopAll);
                _anim[_hideClip.name].normalizedTime = 1.0f;
                yield break;
            }
        }

        /// <summary>
        /// アニメーションが終了するまで待つ
        /// </summary>
        /// <param name="animName">アニメーションの名前</param>
        /// <returns></returns>
        IEnumerator AnimationEndCheck(string animName)
        {
            while (_anim.IsPlaying(animName))
            {
                Debug.Log($"アニメーションの終了待ち{animName}");
                yield return null;
            }
            yield return new WaitWhile(() => _anim.IsPlaying(animName));
        }
    }
}
