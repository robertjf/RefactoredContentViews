using System;
using System.Web;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using umbraco.interfaces;
using umbraco.presentation;
using Refactored.Dynamics;

namespace Refactored.UmbracoViewCounter.EditorControls
{
    public class ViewCounter : PlaceHolder, IDataEditor
    {
        private IData _data;

        private readonly bool _hideCounter;
        private readonly bool _enableHistory;
        private readonly bool _disableReset;
        private readonly string _category;

        private readonly int _nodeId;
        private int _currentViewCount;

        private HiddenField _pageCountH;
        public HyperLink _resetCmd;

        internal const string DataNodeName = "viewCounter";

        public ViewCounter(IData data, Dictionary<string, string> preValues)
        {
            _data = data;

            if (preValues != null)
            {
                if (preValues.ContainsKey("hideCounter"))
                    _hideCounter = (preValues["hideCounter"] == "1" ? true : false);

                if (preValues.ContainsKey("enableHistory"))
                    _enableHistory = (preValues["enableHistory"] == "1" ? true : false);

                if (preValues.ContainsKey("disableReset"))
                    _disableReset = (preValues["disableReset"] == "1" ? true : false);

                if (preValues.ContainsKey("category"))
                    _category = preValues["category"];
                else
                    _category = string.Empty;
            }

            int.TryParse(HttpContext.Current.Request["id"], out _nodeId);

            _currentViewCount = ViewCount.GetViewCount(_nodeId, _category);

        }

        public System.Web.UI.Control Editor
        {
            get { return this; }
        }

        /// <summary>
        /// Creates and saves a xml format of the selected options and other prevalues.
        /// </summary>
        public void Save()
        {
            dynamic details = new DynamicDictionary();
            details.hideCounter = _hideCounter ? "1" : "0";
            details.enableHistory = _enableHistory ? "1" : "0";
            details.category = _category;

            if (!_disableReset)
            {
                int tmpPageCount = 0;
                if (int.TryParse(_pageCountH.Value, out tmpPageCount) && tmpPageCount == -1)
                    // Reset the page view count to 0.
                    ViewCount.Reset(_nodeId, _category);
            }
            _data.Value = DynamicDictionary.DynamicDictionaryToXML(DataNodeName, details).ToString();

            // Save to config table as well for this node.
            ViewCount.UpdateConfig(_nodeId, _category, _hideCounter, _enableHistory);
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Controls.Add(new Literal { Text = string.Format("{0:N0} Views&nbsp;&nbsp;", _currentViewCount) });
            if (!_disableReset)
            {
                _pageCountH = new HiddenField { ID = "pageCount" + base.ID };
                _resetCmd = new HyperLink { Text = "Reset..." };
                Controls.Add(_pageCountH);
                Controls.Add(_resetCmd);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!_disableReset)
                _resetCmd.NavigateUrl = String.Format("javascript:document.getElementById('{0}').value = '-1';alert('View Count will be reset when you Save');", _pageCountH.ClientID);
            base.OnPreRender(e);
        }
    }
}
