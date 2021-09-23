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
		public virtual bool IsShow { get; protected set; } = false;
		/// <summary>
		/// 最上層のノードか
		/// </summary>
		public bool IsTopNode => UITreeView.IsTopNode(this);
		/// <summary>
        /// UIのID
        /// </summary>
		public UIIDType ID = null;
		public UITreeView UITreeView { get; set; }

		List<UINode> _childrenList;
		UINode _parent;
		Coroutine _showCoroutine = null;
		Coroutine _hideCoroutine = null;

		/// <summary>
		/// 親の階層が表示しているかチェック
		/// </summary>
		/// <returns></returns>
		public bool IsShowParent()
		{
			if (IsTopNode) { return true; }
			return IsShow ? _parent.IsShowParent() : false;
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
		/// 小階層のUIを全て表示させる
		/// </summary>
		public void ShowAllChild()
		{
			_childrenList.ForEach(x => x.Show());
		}

		/// <summary>
		/// UIを表示する。
		/// 親階層のUINodeが閉じている時は表示されない
		/// </summary>
		public void Show()
		{
			_showCoroutine = UITreeView.StartCoroutine(ShowCoroutine(this));
		}

		IEnumerator ShowCoroutine(UINode baseNode)
		{
			if(baseNode != this && IsShow)
            {
				yield break;
            }
			if (_parent != this)
			{
				yield return _parent.ShowCoroutine(baseNode);
			}
			yield return ShowBeforeCoroutine(baseNode);
			OnShowBefore();
			IsShow = true;
			gameObject.SetActive(true);
			yield return ShowAfterCoroutine(baseNode);
			OnShowAfter();
			_showCoroutine = null;
		}
		protected virtual IEnumerator ShowBeforeCoroutine(UINode baseNode) { yield break; }
		protected virtual IEnumerator ShowAfterCoroutine(UINode baseNode) { yield break; }

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Hide()
		{
			_hideCoroutine = UITreeView.StartCoroutine(HideCoroutine(this));
		}

		IEnumerator HideCoroutine(UINode baseNode)
		{
			yield return HideBeforeCoroutine(baseNode);
			OnHideBefore();
			IsShow = false;
			gameObject.SetActive(false);
			foreach (var node in _childrenList)
			{
				yield return node.HideCoroutine(baseNode);
			}
			yield return HideAfterCoroutine(baseNode);
			OnHideAfter();
			_hideCoroutine = null;
		}

		protected virtual IEnumerator HideBeforeCoroutine(UINode baseNode) { yield break; }
		protected virtual IEnumerator HideAfterCoroutine(UINode baseNode) { yield break; }

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