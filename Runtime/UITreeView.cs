using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightGive.UIUtil
{
    public class UITreeView : MonoBehaviour
    {
        [SerializeField] UINode _topNode = null;
        public List<UINode> AllNodeList { get; set; } = null;

        private void Awake()
        {
            AllNodeList = new List<UINode>();
            _topNode.IsTopNode = true;
            _topNode.Init();
            _topNode.SetList(AllNodeList, _topNode);
            AllNodeList.ForEach(x => x.UITreeView = this);
            var count = _topNode.SetID(0);
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