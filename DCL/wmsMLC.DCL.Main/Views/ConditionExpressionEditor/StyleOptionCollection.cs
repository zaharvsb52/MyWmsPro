using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    public class StyleOptionCollection : ObservableCollection<StyleOption>, ICloneable
    {
        private const string RegexEv = "(?<!\\\\)\\\\(?!\\\\)";
        private const string ReplaceEv = "\\\\";

        public IDataColumnInfo ColumnInfo { get; set; }

        public Func<string, string> ValidateHandler { get; set; }

        public string ConvertToCaptions(string expression)
        {
            var regex = new Regex(RegexEv);
            if (regex.IsMatch(expression))
                expression = regex.Replace(expression, ReplaceEv);
            return new CriteriaLexerTokenHelper(expression).ConvertProperties(true, ConvertToCaptionsHelper);
        }

        private string ConvertToCaptionsHelper(string listName, string name)
        {
            if (ColumnInfo == null || ColumnInfo.Columns == null)
                return name;
            var col = ColumnInfo.Columns.FirstOrDefault(p => p.FieldName == name);
            return col == null ? name : col.Caption;
        }

        public string ConvertToFields(string expression)
        {
            var regex = new Regex(RegexEv);
            if (regex.IsMatch(expression))
                expression = regex.Replace(expression, ReplaceEv);
            return new CriteriaLexerTokenHelper(expression).ConvertProperties(true, ConvertToFieldsHelper);
        }

        private string ConvertToFieldsHelper(string listName, string name)
        {
            if (ColumnInfo == null || ColumnInfo.Columns == null)
                return name;
            var col = ColumnInfo.Columns.FirstOrDefault(p => p.Caption == name);
            return col == null ? name : col.FieldName;
        }

        public string ValidateExpression(string expression)
        {
            if (ValidateHandler != null)
            {
                var error = ValidateHandler(expression);
                if (!string.IsNullOrEmpty(error))
                    return error;
            }

            expression = ConvertToFields(expression);
            if (ColumnInfo.Controller == null)
                return null;

            try
            {
                ColumnInfo.Controller.ValidateExpression(CriteriaOperator.Parse(expression, null));
            }
            catch (Exception ex)
            {
                return string.Format(Resources.StringResources.StyleOptionConditionError, ex.Message);
            }
            return null;
        }

        protected override void InsertItem(int index, StyleOption item)
        {
            if (item != null)
                item.Parent = this;
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, StyleOption item)
        {
            if (item != null)
                item.Parent = this;
            base.SetItem(index, item);
        }

        public StyleOptionCollection Clone()
        {
            var result = new StyleOptionCollection { ColumnInfo = ColumnInfo };
            for (var i = 0; i < Count; i++)
            {
                var item = this[i].Clone();
                item.Parent = result;
                result.Add(item);
            }
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
