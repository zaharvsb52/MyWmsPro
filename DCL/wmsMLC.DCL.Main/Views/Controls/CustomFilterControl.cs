using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.Xpf.Editors.Filtering;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomFilterControl : FilterControl
    {
        public CriteriaOperator SqlFilterCriteria
        {
            get { return (CriteriaOperator) GetValue(SqlFilterCriteriaProperty); }
            set { SetValue(SqlFilterCriteriaProperty, value); }
        }
        public static readonly DependencyProperty SqlFilterCriteriaProperty = DependencyProperty.Register("SqlFilterCriteria", typeof(CriteriaOperator), typeof(CustomFilterControl));

        protected override CriteriaOperator ToCriteria(INode node)
        {
            var result = base.ToCriteria(node);
            if (ReferenceEquals(SqlFilterCriteria, null))
                return result;

            if (ReferenceEquals(result, null))
                return SqlFilterCriteria;

            try
            {
                var coresult = GroupOperator.Combine(GroupOperatorType.And, result, SqlFilterCriteria);
                return coresult;
            }
            catch
            {
                return result;
            }
        }
    }
}