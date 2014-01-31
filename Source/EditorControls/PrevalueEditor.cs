using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.DataLayer;
using umbraco.interfaces;

namespace Refactored.UmbracoViewCounter.EditorControls
{
    public class PrevalueEditor : PlaceHolder, IDataPrevalue
    {
        private readonly BaseDataType _datatype;
        private Dictionary<string, string> _prevalues;

        private RadioButtonList _hideCounter;
        private RadioButtonList _enableHistory;
        private RadioButtonList _disableReset;
        private TextBox _category;

        public PrevalueEditor(BaseDataType dataType) {
            _datatype = dataType;
            SetupChildControls();
        }

        private void SetupChildControls()
        {
            _hideCounter = new RadioButtonList() { ID = "hideCounter" };

            _hideCounter.Items.AddRange(new ListItem[]{
                new ListItem("Yes","1"),
                new ListItem("No", "0")
            });

            _enableHistory = new RadioButtonList() { ID = "enableHistory" };

            _enableHistory.Items.AddRange(new ListItem[]{
                new ListItem("Yes","1"),
                new ListItem("No", "0")
            });

            _disableReset = new RadioButtonList() { ID = "disableReset" };

            _disableReset.Items.AddRange(new ListItem[]{
                new ListItem("Yes","1"),
                new ListItem("No", "0")
            });

            _hideCounter.SelectedValue = Prevalues["hideCounter"];
            _enableHistory.SelectedValue = Prevalues["enableHistory"];
            _disableReset.SelectedValue = Prevalues["disableReset"];

            Controls.Add(_hideCounter);
            Controls.Add(_enableHistory);
            Controls.Add(_disableReset);

            _category = new TextBox { ID = "category", Text = Prevalues["category"]};
            Controls.Add(_category);

        }

        public Control Editor
        {
            get
            {
                return this;
            }
        }

        public void Save() {
            //clear all datatype data first...
            SqlHelper.ExecuteNonQuery(String.Format("delete from cmsDataTypePrevalues where datatypeNodeId = {0}", _datatype.DataTypeDefinitionId));

            //Save datatype
            _datatype.DBType = DBTypes.Ntext;

            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePrevalues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,@alias)",
                SqlHelper.CreateParameter("@value", _hideCounter.SelectedValue),
                SqlHelper.CreateParameter("@alias", "hideCounter"),
                SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));

            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePrevalues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,@alias)",
                SqlHelper.CreateParameter("@value", _enableHistory.SelectedValue),
                SqlHelper.CreateParameter("@alias", "enableHistory"),
                SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));

            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePrevalues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,@alias)",
                SqlHelper.CreateParameter("@value", _category.Text.Trim()),
                SqlHelper.CreateParameter("@alias", "category"),
                SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));

            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePrevalues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,@alias)",
                SqlHelper.CreateParameter("@value", _disableReset.SelectedValue),
                SqlHelper.CreateParameter("@alias", "disableReset"),
                SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));

            if (_category.Text.Trim() != _prevalues["category"])
            {
                // Update any viewcount records with the old category to the new category name.
                SqlHelper.ExecuteNonQuery("update refViewCount set category = @newCategory where category = @oldCategory",
                    SqlHelper.CreateParameter("@newCategory", _category.Text.Trim()),
                    SqlHelper.CreateParameter("@oldCategory", _prevalues["category"]));
            }

            // Update any config records with the new settings.
            SqlHelper.ExecuteNonQuery(@"update refViewCountConfig 
set category = @newCategory, hideCounter = @hideCounter, enableHistory = @enableHistory 
where category = @oldCategory",
                SqlHelper.CreateParameter("@newCategory", _category.Text.Trim()),
                SqlHelper.CreateParameter("@oldCategory", _prevalues["category"]),
                SqlHelper.CreateParameter("@hideCounter", _prevalues["hideCounter"] == "1"),
                SqlHelper.CreateParameter("@enableHistory", _prevalues["enableHistory"] == "1"));

            _prevalues = null;

        }

        protected static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>Category: </div>");
            _category.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");

            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>Hide View Count:</div>");
            _hideCounter.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");

            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>Enable View History:</div>");
            _enableHistory.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");

            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>Disable Counter Reset:</div>");
            _disableReset.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");
        }

        public Dictionary<string, string> Prevalues
        {
            get
            {
                if (_prevalues == null)
                {
                    _prevalues = new Dictionary<string, string>();
                    IRecordsReader dr = SqlHelper.ExecuteReader(
                        String.Format("Select alias, [value] from cmsDataTypeprevalues where DataTypeNodeId = {0} order by sortorder", 
                            _datatype.DataTypeDefinitionId));

                    while (dr.Read())
                    {
                        if (!_prevalues.ContainsKey(dr.GetString("alias")))
                        {
                            _prevalues.Add(dr.GetString("alias"), dr.GetString("value"));
                        }
                    }

                    dr.Close();

                    if (!_prevalues.ContainsKey("hideCounter"))
                        _prevalues["hideCounter"] = "0";

                    if (!_prevalues.ContainsKey("enableHistory"))
                        _prevalues["enableHistory"] = "0";

                    if (!_prevalues.ContainsKey("disableReset"))
                        _prevalues["disableReset"] = "0";

                    if (!_prevalues.ContainsKey("category"))
                        _prevalues["category"] = string.Empty;

                }
                return _prevalues;
            }
        }
    }
}