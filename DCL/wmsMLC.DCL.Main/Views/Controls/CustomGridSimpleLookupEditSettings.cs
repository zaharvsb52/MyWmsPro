using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Grid;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Views.Controls
{
    /// <summary>
    /// Settings для Lookup-ов, отображающихся в grid-ах. Реализует упрощенную модель работы
    /// </summary>
    internal class CustomGridSimpleLookupEditSettings : CustomBaseLookupEditSetting
    {
        #region .  Fields  .
        private List<CustomComboBoxItem> _autoFilterItemsCache;
        private bool _autoFilterItemsCacheLoaded;
        #endregion .  Fields  .

        #region .  Ctors  .

        static CustomGridSimpleLookupEditSettings()
        {
            //EditorSettingsProvider.Default.RegisterUserEditor(typeof(CustomGridSimpleLookupEdit), typeof(CustomGridSimpleLookupEditSettings),
            //    () => new CustomGridSimpleLookupEdit(),
            //    () => new CustomGridSimpleLookupEditSettings());

            EditorSettingsProvider.Default.RegisterUserEditor2(typeof (CustomGridSimpleLookupEdit),
                typeof (CustomGridSimpleLookupEditSettings),
                optimized => optimized ? (IBaseEdit) new InplaceBaseEdit() : new CustomGridSimpleLookupEdit(),
                () => new CustomGridSimpleLookupEditSettings());
        }

        public CustomGridSimpleLookupEditSettings()
        {
            AllowAddNewValue = true;
        }
        #endregion .  Ctors  .

        #region .  Properties  .

        public bool AllowEditAutofilter
        {
            get
            {
                if (LookUpInfo == null)
                    return true;

                return LookUpInfo.DisplayMember == LookUpInfo.ValueMember;
            }
        }

        #endregion .  Properties  .

        #region .  Methods  .

        protected override void AssignToEditCore(IBaseEdit edit)
        {
            var editor = edit as CustomComboBoxEdit;
            if (editor != null)
                editor.IsSettings = true;

            base.AssignToEditCore(edit);
        }

        public override string GetDisplayText(object editValue, bool applyFormatting)
        {
            var res = base.GetDisplayText(editValue, applyFormatting);
            if (editValue != null && string.IsNullOrEmpty(res))
                return editValue.ToString();
            return res;
        }

        //protected override void FilterConditionChanged(FilterCondition? filterCondition)
        //{
        //    base.FilterConditionChanged(filterCondition);
        //}

        //protected override void FilterCriteriaChanged(CriteriaOperator criteriaOperator)
        //{
        //    base.FilterCriteriaChanged(criteriaOperator);
        //}

        public override string GetDisplayTextFromEditor(object editValue)
        {
            // ищем в подгруженном списке
            var item = GetDisplayValueByEditValue(editValue);

            // если нет - пытаемся получить правильный текст для отображения
            if (editValue != null && item == null)
                return base.GetDisplayTextFromEditor(editValue);

            // если есть - возвращаем
            return item == null ? string.Empty : item.ToString();
        }

        protected override void OnLookUpCodeEditorPropertyChanged()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                return;

            if (LookUpInfo == null)
                LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);

            var column = Parent as ColumnBase;
            if (column != null && !AllowEditAutofilter)
            {
                var newStyle = new System.Windows.Style(typeof(FilterCellContentPresenter), null);
                newStyle.Setters.Add(new System.Windows.Setter(Control.BackgroundProperty, System.Windows.Media.Brushes.Wheat));
                column.AutoFilterRowCellStyle = newStyle;

                // такие поля должны работать только по значению - иначе получаются проблемы на границе отбора и отображения
                // в шаблоне уже выставлено, но чтобы не перетералось из настроек - задаем ручками
                column.ColumnFilterMode = ColumnFilterMode.Value;
            }

            // Если DisplayMember == ValueMember, то ничего получать не нужно - будем отображать ValueMember
            if (LookUpInfo.DisplayMember == LookUpInfo.ValueMember)
                return;

            base.OnLookUpCodeEditorPropertyChanged();

            RefreshData();
        }

        public object GetDisplayValueByEditValue(object editValue)
        {
            var result = ItemsProvider.GetDisplayValueByEditValue(editValue, null);
            return result;
        }

        protected override string GetAttrEntity()
        {
            return LookupHelper.GetAttrEntity(LookUpInfo);
        }

        public List<CustomComboBoxItem> GetAutoFilterItems(string fieldName)
        {
            if (_autoFilterItemsCacheLoaded)
                return _autoFilterItemsCache;

            _autoFilterItemsCache = CreateAutoFilterItems(fieldName);
            _autoFilterItemsCacheLoaded = true;
            return _autoFilterItemsCache;
        }

        private List<CustomComboBoxItem> CreateAutoFilterItems(string fieldName)
        {
            var filterItems = new List<CustomComboBoxItem>
            {
                new CustomComboBoxItem
                {
                    DisplayValue = DCL.Resources.StringResources.CustomGridSimpleLookupEditSettingsDisplayValueAll,
                    EditValue = new CustomComboBoxItem()
                },
                new CustomComboBoxItem
                {
                    DisplayValue = DCL.Resources.StringResources.CustomGridSimpleLookupEditSettingsDisplayValueEmpty,
                    EditValue = new CustomComboBoxItem
                        {
                            EditValue = new FunctionOperator(FunctionOperatorType.IsNullOrEmpty, new OperandProperty(fieldName))
                        }
                },
                new CustomComboBoxItem
                {
                    DisplayValue = DCL.Resources.StringResources.CustomGridSimpleLookupEditSettingsDisplayValueNotEmpty,
                    EditValue = new CustomComboBoxItem
                        {
                            EditValue = new NotOperator(new FunctionOperator(FunctionOperatorType.IsNullOrEmpty, new OperandProperty(fieldName)))
                        }
                }
            };

            var items = ItemsSource as IEnumerable<object>;
            if (items == null)
                return null;

            var properties = TypeDescriptor.GetProperties(LookUpInfo.ItemType);
            var valueMemberProperty = properties[ValueMember];
            var diplayMemberProperty = properties[DisplayMember];

            foreach (var item in items)
            {
                var editValue = valueMemberProperty.GetValue(item);
                if (filterItems.Any(i => Equals(i.EditValue, editValue)))
                    continue;

                var displayValue = DisplayMember == ValueMember ? editValue : diplayMemberProperty.GetValue(item);
                filterItems.Add(new CustomComboBoxItem
                {
                    DisplayValue = displayValue,
                    EditValue = editValue
                });
            }
            return filterItems;
        }

        #endregion .  Methods  .
    }
}
