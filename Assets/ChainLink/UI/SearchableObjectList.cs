using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChainLink.UI
{
    public class SearchableObjectList<T> : ObjectList<T> where T : ISearchable
    {
        public List<T> FilteredData;
        [SerializeField]
        private TMP_InputField inputField;

        [ReadOnly]
        public string SearchTerm;

        public override void Display(List<T> dataList)
        {
            Data = dataList;
            FilteredData = GetFilteredData(SearchTerm);
            base.Display(dataList);
        }

        public List<T> GetFilteredData(string term)
        {
            if (string.IsNullOrEmpty(SearchTerm))
                return Data;
            List<T> returnList = new List<T>();
            if (Data == null)
                return null;
            foreach (T item in Data) {
                bool show = false;
                if (item.PrimarySearchField.ToLower().Contains(term.ToLower()))
                    show = true;
                if (!show) {
                    if (item.SecondarySearchFields != null) {
                        foreach (string field in item.SecondarySearchFields) {
                            if (field.ToLower().Contains(term.ToLower())) {
                                show = true;
                                break;
                            }
                        }
                    }
                }
                if (show)
                    returnList.Add(item);
            }
            return returnList;
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (FilteredData == null)
                return 0;
            return FilteredData.Count;
        }


        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            ChainLinkListItem<T> cellView = scroller.GetCellView(buttonPrefab) as ChainLinkListItem<T>;

            cellView.name = "Cell " + dataIndex.ToString();

            cellView.SetItem(FilteredData[dataIndex]);
            cellView.AddAction(OnItemSelected);

            return cellView;
        }

        protected override void Awake()
        {
            base.Awake();
            if (inputField != null) {
                inputField.onValueChanged.AddListener((text) => {
                    SearchTerm = text;
                    Display(Data);
                });
            }
        }
    }
}