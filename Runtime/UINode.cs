using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		public virtual bool IsShow { get; protected set; }
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
		int frame = 0;

        private void Update()
        {
			frame++;
        }

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

		/// <summary>
		/// 子階層側が表示・非表示中か
		/// </summary>
		/// <returns></returns>
		bool CanShowHideChild()
		{
			if (IsShowing || IsHiding) { return false; }
			foreach (var child in _childrenList)
			{
				if (!child.CanShowHideChild()) { return false; }
			}
			return true;
		}

		/// <summary>
		/// 親階層側が表示・非表示中か
		/// </summary>
		/// <returns></returns>
		bool CanShowHideParent()
		{
			if (IsTopNode) { return true; }
			if (IsShowing || IsHiding) { return false; }
			return _parent.CanShowHideParent();
		}

		/// <summary>
		/// UIを表示する
		/// </summary>
		public void Show()
		{
			//TODO:非表示中の時も表示できるように。
			if (!CanShowHideParent() || !CanShowHideChild() || IsShow)
			{
				Debug.LogError("表示することが出来ない");
				return;
			}
			_showCoroutine = UITreeView.StartCoroutine(ShowCoroutine());
		}

		/// <summary>
        /// 表示処理
        /// </summary>
        /// <returns></returns>
		IEnumerator ShowCoroutine()
		{
			if (!IsTopNode)
			{
				//親のUIが表示されていない時に強制的に表示する
				_parent.ShowForceRecursively();
			}
			yield return UITreeView.StartCoroutine(ShowBeforeCoroutine());
			OnShowBefore();
			gameObject.SetActive(true);
			yield return UITreeView.StartCoroutine(ShowAfterCoroutine());
			OnShowAfter();
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
		public void Hide()
		{
			//TODO:表示中の時も非表示にすることができるように
			if (!CanShowHideParent() || !CanShowHideChild() || !IsShow)
			{
				Debug.LogError("非表示にすることが出来ない");
				return;
			}
			_hideCoroutine = UITreeView.StartCoroutine(HideCoroutine());
		}

		/// <summary>
		/// 非表示にするルーチン
		/// </summary>
		/// <param name="callerNode">呼び出し元のUINode</param>
		/// <returns></returns>
		IEnumerator HideCoroutine()
		{
			foreach(var child in _childrenList)
            {
				child.HideForceRecursively();
            }
			yield return UITreeView.StartCoroutine(HideBeforeCoroutine());
			OnHideBefore();
			gameObject.SetActive(false);
			yield return UITreeView.StartCoroutine(HideAfterCoroutine());
			OnHideAfter();
			_hideCoroutine = null;
			IsShow = false;
		}

		protected virtual IEnumerator HideBeforeCoroutine() { yield break; }
		protected virtual IEnumerator HideAfterCoroutine() { yield break; }

		/// <summary>
		/// 表示処理（再帰用）
		/// </summary>
		/// <returns></returns>
		void HideForceRecursively()
		{
			foreach(var child in _childrenList)
            {
				child.HideForceRecursively();
            }
            if (!IsShow) { return; }
			HideForce();
			IsShow = false;
		}
		/// <summary>
		/// 強制的に表示する
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
