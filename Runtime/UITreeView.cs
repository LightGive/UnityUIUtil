using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightGive.UIUtil
{
    public class UITreeView : MonoBehaviour
    {
        [SerializeField]
        private UINode topNode;

        public List<UINode> AllNodeList { get; set; }

        private void Awake()
        {
            AllNodeList = new List<UINode>();

            topNode.IsTopNode = true;
            topNode.Init();
            topNode.SetList(AllNodeList, topNode);
            AllNodeList.ForEach(x => x.UITreeView = this);
            var count = topNode.SetID(0);
            Debug.Log("全てのUIノード数" + count.ToString());
        }
        /// <summary>
        /// ノードを継承したUIを取得する
        /// Start以下で使用するように
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetNode<T>()
        {
            return AllNodeList
                .Select(x => x.GetComponent<T>())
                .Where(x => x != null)
                .First();
        }
    }
}