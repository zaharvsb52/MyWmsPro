using System.Collections.ObjectModel;
using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.LookUp;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.Configurator.Views.Controls
{
    public class ConfiguratorLookUpEdit : LookUpEdit
    {
        public ConfiguratorLookUpEdit()
        {
            AutoComplete = true;
            AllowSpinOnMouseWheel = false;
            ImmediatePopup = true;
            NullValue = null;
            AllowNullInput = true; // Gets or sets whether end-users can set the editor's value to a null reference by pressing the CTRL+DEL or CTRL+0 key combinations.
        }

        /// <summary> Источник данных для построения колонок (DependencyProperty). </summary>
        public ObservableCollection<DataField> LookUpColumnsSource
        {
            get { return (ObservableCollection<DataField>)GetValue(LookUpColumnsSourceProperty); }
            set { SetValue(LookUpColumnsSourceProperty, value); }
        }

        public static readonly DependencyProperty LookUpColumnsSourceProperty = DependencyProperty.Register("LookUpColumnsSource", typeof(ObservableCollection<DataField>), typeof(ConfiguratorLookUpEdit));

        public CriteriaOperator LookupFilterCriteria
        {
            get { return (CriteriaOperator) GetValue(LookupFilterCriteriaProperty); }
            set { SetValue(LookupFilterCriteriaProperty, value); }
        }

        public static readonly DependencyProperty LookupFilterCriteriaProperty = DependencyProperty.Register("LookupFilterCriteria", typeof(CriteriaOperator), typeof(ConfiguratorLookUpEdit));

        protected override void OnPopupOpened()
        {
            base.OnPopupOpened();
            var grid = GetGridControl() as CustomGridControl;
            if (grid != null)
            {
                grid.View.ShowFilterPanelMode = ShowFilterPanelMode.Never;
                grid.FilterCriteria = LookupFilterCriteria;
            }
        }
    }
}
