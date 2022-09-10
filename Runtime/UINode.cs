using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LightGive.UIUtil
{
    /// <summary>
    /// ツリー構造のUI
    /// 親オブジェクトには必ずUINodeを入れて下さい。
    /// </summary>
    public class UINode : MonoBehaviour
	{
		/// <summary>
		/// 表示しているか
		/// </summary>
		public virtual bool IsShow { get; protected set; } = false;
		/// <summary>
		/// 最上層のノードか
		/// </summary>
		public bool IsTopNode => _parent == null;

		/// <summary>
        /// 表示中かどうか
        /// </summary>
		public bool IsShowing => _showCoroutine != null;
		/// <summary>
        /// 非表示中かどうか
        /// </summary>
		public bool IsHiding => _hideCoroutine != null;

		/// <summary>
        /// UIのID
        /// </summary>
		public UIIDType ID = null;
		public UITreeView UITreeView { get; set; }

		List<UINode> _childrenList;
		/// <summary>
        /// Rootにはnullが入る
        /// </summary>
		UINode _parent;
		Coroutine _showCoroutine = null;
		Coroutine _hideCoroutine = null;

		/// <summary>
		/// このUIを含め小階層のUIをリストに追加する
		/// </summary>
		/// <param name="nodeList"></param>
		/// <param name="parent"></param>
		public void SetList(List<UINode> nodeList, UINode parent)
		{
			nodeList.Add(this);
			_parent = parent;
			_childrenList.ForEach(c => c.SetList(nodeList, this));
		}

		/// <summary>
		/// 初期化
		/// </summary>
		public void Init()
		{
			ID = new UIIDType();
			_childrenList = new List<UINode>();
			for (var i = 0; i < transform.childCount; i++)
			{
				if (!transform.GetChild(i).TryGetComponent(out UINode node)) { continue; }
				_childrenList.Add(node);
			}
			gameObject.SetActive(false);
			OnInit();
			_childrenList.ForEach(c => c.Init());
		}

		void CancelForceShow()
        {
			if (!IsShowing) { return; }
			UITreeView.StopCoroutine(_showCoroutine);
			_showCoroutine = null;
			ShowForce();
			IsShow = true;
		}

		void CancelForceHide()
        {
            if (!IsHiding) { return; }
			UITreeView.StopCoroutine(_hideCoroutine);
			_hideCoroutine = null;
			HideForce();
			IsShow = false;
		}

		/// <summary>
		/// 親階層側が表示可能か
		/// ・親階層で表示中だった時は強制的に表示させる
		/// ・親階層で非表示中だった時は表示不可とする（いつ非表示になるか分からないため）
		/// </summary>
		/// <returns></returns>
		bool CanShowHideParent()
		{
			if (IsTopNode) { return true; }
			if (!_parent.CanShowHideParent()) { return false; }

			//非表示中の時は処理しない
			if (IsHiding)
			{
				Debug.LogError($"{name}が非表示中。親階層で非表示中のUINodeがあると子階層で表示・非表示にする事が出来ない");
				return false;
			}

			//表示中の処理を一旦止めて強制的に表示状態にする
			CancelForceShow();
			return true;
		}

		/// <summary>
		/// 子階層が表示・非表示中だった時はスキップする
		/// </summary>
		void ChildShowHideForce()
		{
			//子階層側から先に処理
			//親階層から処理すると子階層に影響が出るため
			foreach (var child in _childrenList)
			{
				child.ChildShowHideForce();
			}
			CancelForceShow();
			CancelForceHide();
		}

		/// <summary>
		/// 表示する
		/// </summary>
		/// <param name="canReshow">再表示を許可するか</param>
		/// <param name="onShowBefore">表示前のコールバック
		/// OnShowBeforeよりは後に呼ばれる</param>
		/// <param name="onShowAfter">表示後のコールバック
		/// OnShowAfterより後に呼ばれる</param>
		public void Show(bool? canReshow = null, UnityAction onShowBefore = null, UnityAction onShowAfter = null)
		{
			if (canReshow == null) { canReshow = UITreeView.DefaultReShowHide; }
			//親階層のチェック
			if (_parent != null && !_parent.CanShowHideParent()) { return; }
			if (!canReshow.Value && (IsShow || IsShowing || IsHiding)) { return; }
			CancelForceShow();
			CancelForceHide();
			if (IsShow)
			{
				gameObject.SetActive(false);
				IsShow = false;
			}
			//子階層の表示・非表示中の処理をスキップ
			ChildShowHideForce();
			_showCoroutine = UITreeView.StartCoroutine(ShowCoroutine());
		}

		/// <summary>
		/// 表示処理
		/// </summary>
		/// <param name="onShowBefore"></param>
		/// <param name="onShowAfter"></param>
		/// <returns></returns>
		IEnumerator ShowCoroutine(UnityAction onShowBefore = null, UnityAction onShowAfter = null)
		{
			if (!IsTopNode)
			{
				//親のUIが表示されていない時に強制的に表示する
				_parent.ShowForceRecursively();
			}
			yield return UITreeView.StartCoroutine(ShowBeforeCoroutine());
			OnShowBefore();
			onShowBefore?.Invoke();
			gameObject.SetActive(true);
			yield return UITreeView.StartCoroutine(ShowAfterCoroutine());
			OnShowAfter();
			onShowAfter?.Invoke();
			_showCoroutine = null;
			IsShow = true;
		}

		/// <summary>
		/// 表示処理（再帰用）
		/// </summary>
		/// <returns></returns>
		void ShowForceRecursively()
		{
			if (!IsTopNode)
			{
				_parent.ShowForceRecursively();
			}
			if (IsShow)
			{
				return;
			}
			ShowForce();
			IsShow = true;
		}
		/// <summary>
		/// 強制的に表示する
		/// </summary>
		protected virtual void ShowForce()
		{
			gameObject.SetActive(true);
		}
		protected virtual IEnumerator ShowBeforeCoroutine() { yield break; }
		protected virtual IEnumerator ShowAfterCoroutine() { yield break; }

		/// <summary>
		/// 閉じる
		/// </summary>
		/// <param name="canRehide">再度非表示に出来るようにするか</param>
		/// <param name="onHideBefore">表示前のコールバック
		/// OnHideBeforeよりは後に呼ばれる</param>
		/// <param name="onHideAfter">表示後のコールバック
		/// OnHideAfterより後に呼ばれる</param>
		public void Hide(bool? canRehide = null, UnityAction onHideBefore = null, UnityAction onHideAfter = null)
		{
			if (canRehide == null) { canRehide = UITreeView.DefaultReShowHide; }
			if (_parent != null && !_parent.CanShowHideParent()) { return; }
			//再非表示不可の場合、表示中、非表示中は処理しない
			if (!canRehide.Value && (!IsShow || IsShowing || IsHiding)) { return; }
			CancelForceShow();
			CancelForceHide();
			if (!IsShow)
			{
				gameObject.SetActive(true);
				IsShow = true;
			}
			//子階層の表示・非表示中の処理をスキップ
			ChildShowHideForce();
			_hideCoroutine = UITreeView.StartCoroutine(HideCoroutine());
		}

		/// <summary>
		/// 非表示処理
		/// </summary>
		/// <param name="onHideBefore"></param>
		/// <param name="onHideAfter"></param>
		/// <returns></returns>
		IEnumerator HideCoroutine(UnityAction onHideBefore = null, UnityAction onHideAfter = null)
		{
			yield return UITreeView.StartCoroutine(HideBeforeCoroutine());
			OnHideBefore();
			onHideBefore?.Invoke();
			gameObject.SetActive(false);
			yield return UITreeView.StartCoroutine(HideAfterCoroutine());
			OnHideAfter();
			onHideAfter?.Invoke();
			_hideCoroutine = null;
			IsShow = false;
		}

		protected virtual IEnumerator HideBeforeCoroutine() { yield break; }
		protected virtual IEnumerator HideAfterCoroutine() { yield break; }

		/// <summary>
		/// 強制的に非表示にする
		/// </summary>
		protected virtual void HideForce()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// 初期化された時のコールバック(Awake時に呼ばれる)
		/// </summary>
		protected virtual void OnInit() { }
		/// <summary>
		/// UIが表示される前のコールバック
		/// </summary>
		protected virtual void OnShowBefore() { }
		/// <summary>
		/// UIが表示された後のコールバック
		/// </summary>
		protected virtual void OnShowAfter() { }
		/// <summary>
        /// UIが非表示になる前のコールバック
        /// </summary>
		protected virtual void OnHideBefore() { }
		/// <summary>
		/// UIが非表示になった後のコールバック
		/// </summary>
		protected virtual void OnHideAfter() { }
	}
}
