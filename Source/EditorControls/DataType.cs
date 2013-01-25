using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace Refactored.UmbracoViewCounter.EditorControls
{
    public class DataType:BaseDataType, IDataType
    {
        private IDataEditor _editor;
        private IData _baseData;
        private PrevalueEditor _prevalueEditor;

        internal static readonly Guid DataTypeGuid = new Guid("D9C78B8A-33A9-4116-BEA1-C56737DB616A");

        public override IDataEditor DataEditor
        {
            get
            {

                if (_editor == null)
                    _editor = new ViewCounter(Data, ((PrevalueEditor)PrevalueEditor).Prevalues);
                return _editor;
            }
        }

        public override IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new ViewCountData(this);
                return _baseData;
            }
        }

        public override Guid Id
        {
            get { return DataTypeGuid; }
        }

        public override string DataTypeName
        {
            get { return "Content Views Counter"; }
        }

        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueEditor == null)
                    _prevalueEditor = new PrevalueEditor(this);
                return _prevalueEditor;
            }
        }
    }
}