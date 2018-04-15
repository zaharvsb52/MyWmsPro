using System;
using System.Collections.Generic;
using DevExpress.Data;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    internal class ExpressionColumnInfo : IDataColumnInfo
    {
        private readonly IDataColumnInfo _columnInfo;

        public ExpressionColumnInfo(IDataColumnInfo column)
        {
            _columnInfo = column;
            FillColumnsList();
        }

        private void FillColumnsList()
        {
            Columns = new List<IDataColumnInfo>();
            foreach (var col in _columnInfo.Columns)
            {
                Columns.Add(col);
            }
            Columns.Add(_columnInfo);
        }

        public string Caption
        {
            get { return _columnInfo.Caption; }
        }

        public List<IDataColumnInfo> Columns { get; private set; }

        public DataControllerBase Controller
        {
            get { return _columnInfo.Controller; }
        }

        public string FieldName
        {
            get { return _columnInfo.FieldName; }
        }

        public Type FieldType
        {
            get { return _columnInfo.FieldType; }
        }

        public string Name
        {
            get { return _columnInfo.Name; }
        }

        public string UnboundExpression
        {
            get { return String.Empty; }
        }
    }
}
