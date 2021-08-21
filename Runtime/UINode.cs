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
		public bool IsShow { get; private set; } = false;
		/// <summary>
		/// 最上層のノードか
		/// </summary>
		public bool IsTopNode { get; set; } = false;
		public int ID { get; private set; }
		public UITreeView UITreeView { get; set; }

		List<UINode> _childrenList;
		UINode _parent;

		public int SetID(int preID)
		{
			preID++;
			ID = preID;
			_childrenList.ForEach(c => c.SetID(preID));
			return preID;
		}

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
		public void ShowAllChild() => _childrenList.ForEach(x => x.Show());

		/// <summary>
        /// UnityEventコールバック用
        /// </summary>
		public void Show() => Show(true, false);

		/// <summary>
		/// UIを表示する。
		/// 親階層のUINodeが閉じている時は表示されない
		/// </summary>
		/// <param name="isParentOpen">親階層のUIを同時に開くかどうか</param>
		/// <param name="isChildrenOpen">子階層のUIを同時に開くかどうか</param>
		public void Show(bool isParentOpen, bool isChildrenOpen) => UITreeView.StartCoroutine(ShowCoroutine(isParentOpen, isChildrenOpen));

		IEnumerator ShowCoroutine(bool isParentOpen, bool isChildrenOpen)
		{
			if (_parent != this && isParentOpen) { _parent.Show(true, false); }
			if (isChildrenOpen) { _childrenList.ForEach(c => c.Show(false, false)); }
			if (IsShow) { yield break; }
			IsShow = true;
			OnShowBefore();
			yield return new WaitUntil(ShowConditions);
			gameObject.SetActive(true);
			OnShowAfter();
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Hide() => UITreeView.StartCoroutine(HideCoroutine());

		IEnumerator HideCoroutine()
        {
			if (!IsShow) { yield break; }
			IsShow = false;
			OnHideBefore();
			yield return new WaitUntil(HideConditions);
			gameObject.SetActive(false);
			OnHideAfter();
		}

		/// <summary>
		/// 初期化された時のコールバック(Awake時に呼ばれる)
		/// </summary>
		protected virtual void OnInit() { }

		/// <summary>
        /// 表示する条件
        /// </summary>
        /// <returns></returns>
		protected virtual bool ShowConditions() => true;
		/// <summary>
		/// UIが表示される前のコールバック
		/// </summary>
		protected virtual void OnShowBefore() { }
		/// <summary>
		/// UIが表示された後のコールバック
		/// </summary>
		protected virtual void OnShowAfter() { }

		/// <summary>
        /// 非表示にする条件
        /// </summary>
        /// <returns></returns>
		protected virtual bool HideConditions() => true;
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