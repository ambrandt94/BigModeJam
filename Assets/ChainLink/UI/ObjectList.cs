using EnhancedUI.EnhancedScroller;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChainLink.UI
{
    public class ObjectList<T> : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public List<T> Data { get; set; }
        public ChainLinkListItem<T> buttonPrefab;
        [SerializeField]
        private EnhancedScroller scroller;

        //[SerializeField]
        //private GameObject buttonPrefab;
        //[SerializeField]
        //private Transform buttonParent;

        //private List<GameObject> _buttons;

        public virtual void Display(List<T> dataList)
        {
            Data = new List<T>(dataList);
            scroller.ReloadData();
        }

        //protected virtual Button SpawnButton(T data)
        //{
        //    GameObject buttonObject = Instantiate(buttonPrefab, buttonParent);
        //    Button btn = buttonObject.GetComponent<Button>();
        //    _buttons.Add(buttonObject);
        //    IDisplayListItem<T> displayItem = buttonObject.GetComponentInChildren<IDisplayListItem<T>>();
        //    if (displayItem != null) {
        //        displayItem.Set(data);
        //    } else {
        //        TextMeshProUGUI text = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
        //        text.SetText(data.ToString());
        //    }
        //    return btn;
        //}

        //private void ClearItems()
        //{
        //    if (_buttons != null) {
        //        for (int i = _buttons.Count - 1; i >= 0; i--) {
        //            GameObject.Destroy(_buttons[i]);
        //        }
        //    }
        //    _buttons = new List<GameObject>();
        //}

        public virtual void OnItemSelected(T item)
        {
        }
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return (buttonPrefab.transform as RectTransform).rect.height;
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (Data == null)
                return 0;
            // in this example, we just pass the number of our data elements
            return Data.Count;
        }


        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            // first, we get a cell from the scroller by passing a prefab.
            // if the scroller finds one it can recycle it will do so, otherwise
            // it will create a new cell.
            ChainLinkListItem<T> cellView = scroller.GetCellView(buttonPrefab) as ChainLinkListItem<T>;

            // set the name of the game object to the cell's data index.
            // this is optional, but it helps up debug the objects in 
            // the scene hierarchy.
            cellView.name = "Cell " + dataIndex.ToString();

            // in this example, we just pass the data to our cell's view which will update its UI
            cellView.SetItem(Data[dataIndex]);
            cellView.AddAction(OnItemSelected);

            // return the cell to the scroller
            return cellView;
        }

        protected virtual void Awake()
        {
            if (scroller == null)
                scroller = GetComponentInChildren<EnhancedScroller>();
            scroller.Delegate = this;
        }
    }

    public interface IDisplayListItem<T>
    {
        public void Set(T item);
    }

    public interface ISearchable
    {
        public string PrimarySearchField { get; }
        public string[] SecondarySearchFields { get; }
    }

    public class ChainLinkListItem<T> : EnhancedScrollerCellView
    {
        public T Item;

        public virtual void SetItem(T item)
        {
            Item = item;
        }
        public void AddAction(Action<T> action)
        {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(() => { action.Invoke(Item); });
        }
    }
}