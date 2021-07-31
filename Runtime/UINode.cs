using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		protected List<UINode> Children { get; private set; }
		private UINode Parent { get; set; }

		public int SetID(int preID)
		{
			preID++;
			ID = preID;
			foreach (var c in Children)
			{
				preID = c.SetID(preID);
			}
			return preID;
		}

		/// <summary>
		/// 親の階層が表示しているかチェック
		/// </summary>
		/// <returns></returns>
		public bool IsShowParent()
		{
			if (IsTopNode)
			{
				return true;
			}

			return IsShow ? Parent.IsShowParent() : false;
		}

		/// <summary>
        /// このUIを含め小階層のUIをリストに追加する
        /// </summary>
        /// <param name="nodeList"></param>
        /// <param name="parent"></param>
		public void SetList(List<UINode> nodeList, UINode parent)
		{
			nodeList.Add(this);
			Parent = parent;
			foreach (var c in Children)
			{
				c.SetList(nodeList, this);
			}
		}

		/// <summary>
		/// 初期化
		/// </summary>
		public void Init()
		{
			Children = new List<UINode>();

			for (var i = 0; i < transform.childCount; i++)
			{
				if (!transform.GetChild(i).TryGetComponent(out UINode node))
				{
					continue;
				}
				Children.Add(node);
			}

			gameObject.SetActive(false);
			OnInit();

			foreach (var c in Children)
			{
				c.Init();
			}
		}

		/// <summary>
        /// 小階層のUIを全て表示させる
        /// </summary>
		public void ShowAllChild()
		{
			Children.ForEach(x => x.Show());
		}

		/// <summary>
		/// UIを表示する。
		/// 親階層のUINodeが閉じている時は表示されない
		/// </summary>
		/// <param name="isParentOpen">親階層のUIを同時に開くかどうか</param>
		/// <param name="isChildrenOpen">子階層のUIを同時に開くかどうか</param>
		public virtual void Show(bool isParentOpen = false, bool isChildrenOpen = false)
		{
			if (Parent != this && isParentOpen)
			{
				Parent.Show(true);
			}

			if (isChildrenOpen)
			{
				foreach (var c in Children)
				{
					c.Show(false, false);
				}
			}
			IsShow = true;
			gameObject.SetActive(true);
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public virtual void Hide()
		{
			IsShow = false;
			gameObject.SetActive(false);
		}

		/// <summary>
		/// 初期化された時のコールバック(Awake時に呼ばれる)
		/// </summary>
		protected virtual void OnInit() { }
		/// <summary>
        /// UIが表示された時のコールバック
        /// </summary>
		protected virtual void OnShow() { }
		/// <summary>
        /// UIが非表示になったときのコールバック
        /// </summary>
		protected virtual void OnHide() { }
	}
}