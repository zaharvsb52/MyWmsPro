using System.Collections.Generic;
using System.Windows.Controls;
using DevExpress.Xpf.Editors;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Views.Controls
{
    class CustomFilterMemoEditControl : MemoEdit
    {
        #region .  Fields  .
        private readonly IEnumerable<DataField> _fields;
        private string _filter;
        #endregion

        #region .  Methods  .
        public CustomFilterMemoEditControl(IEnumerable<DataField> vmFields,string filterExp)
        {
            _fields = vmFields;
            _filter = filterExp;
            ShowIcon = false;
            AllowDefaultButton = false;
            EditValueChanged += CustomFilterMemoEditControl_EditValueChanged;

            ICustomCommand openFilterWindowCommand = new DelegateCustomCommand(OnShowFilterView);
            Buttons.Add(new ButtonInfo()
            {
                GlyphKind = GlyphKind.Regular,
                ButtonKind = ButtonKind.Simple,
                Command = openFilterWindowCommand
            });
        }

        private void OnShowFilterView()
        {
            // показываем фильтр пользователю
            var viewService = GetViewService();
            var filterModel = new FilterViewModel { IsFilterMode = false, FilterExpression = _filter, MaxRowCount = null};

            if (_fields != null)
            {
                foreach (var field in _fields)
                {
                    filterModel.Fields.Add(field);
                }
            }

            var res = viewService.ShowDialogWindow(filterModel, true, false, "40%", "50%");

            if (!res.HasValue||!res.Value) return;
            filterModel.MaxRowCount = null;
            filterModel.AcceptChanges();

            var retFilter = filterModel.GetSqlExpression();
            EditValue = string.IsNullOrEmpty(retFilter) ? EditValue : retFilter;
            _filter = EditValue.ToString();
        }
        protected static IViewService GetViewService()
        {
            return wmsMLC.General.IoC.Instance.Resolve<IViewService>();
        }

        void CustomFilterMemoEditControl_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (EditValue != null)
            _filter = EditValue.ToString();
        }
 
        

        #endregion
    }
}
