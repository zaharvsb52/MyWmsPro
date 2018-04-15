using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.Content.Views
{
    public partial class CargoIWBPosObjectView :DXPanelView, IHelpHandler
    {
        public static bool IsNeedCargo{get;set;}

        public CargoIWBPosObjectView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        #region .  Methods  .

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnSubscribeDataContextPropertyChanged(e.OldValue);

            SubscribeOnRefreshView();
            var vm = DataContext as IObjectViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }

            RefreshBinding();
            SubscribeDataContextPropertyChanged(e.NewValue);
        }

        private void UnSubscribeDataContextPropertyChanged(object context)
        {
            var npch = context as INotifyPropertyChanged;
            if (npch != null)
                npch.PropertyChanged -= DataContextPropertyChanged;
        }

        private void SubscribeDataContextPropertyChanged(object context)
        {
            var npch = context as INotifyPropertyChanged;
            if (npch != null)
                npch.PropertyChanged += DataContextPropertyChanged;
        }

        private void DataContextPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == SourceViewModelBase<int>.SourcePropertyName)
                RefreshBinding();
        }

        private void FormulaStateChanged(object sender, FormulaStateEventArgument e)
        {
            foreach (var child in objectDataLayout.GetChildren(true))
            {
                var ctrl = child as CustomDataLayoutItem;
                if (ctrl == null)
                    continue;

                if (!ctrl.Name.EqIgnoreCase(e.PropertyName))
                    continue;

                var edit = ctrl.Content as ButtonEdit;
                if (edit == null)
                    continue;

                // если еще не в режиме редактирования формул
                if (!ctrl.InFormulaMode)
                    ctrl.FormulaModeButtonClick(edit, new RoutedEventArgs(ButtonBase.ClickEvent));
                return;
            }
        }
        
        private void RefreshBinding()
        {
            var vm = DataContext as IModelHandler;
            object parentData = null;
            var displaySetting = SettingDisplay.Detail;
            var m = DataContext;
            if (vm != null)
            {
                m = vm.GetSource();
                parentData = vm.ParentViewModelSource;
                displaySetting = vm.DisplaySetting;
            }
            if (m == null)
                throw new DeveloperException("Unknown model.");

            SubscribeOnFormulaChanged();

            FillGroup(m, objectDataLayout, parentData, displaySetting);
        }

        private void RefreshViewEvent(object sender, EventArgs e)
        {
            RefreshBinding();
        }

        private void SubscribeOnRefreshView()
        {
            var vm = DataContext as IModelHandler;
            if (vm == null)
                return;

            UnSubscribeOnRefreshView();
            vm.RefreshViewEvent += RefreshViewEvent;
        }

        private void UnSubscribeOnRefreshView()
        {
            var vm = DataContext as IModelHandler;
            if (vm != null)
                vm.RefreshViewEvent -= RefreshViewEvent;
        }

        private void SubscribeOnFormulaChanged()
        {
            var fvm = DataContext as IFormulaHandler;
            if (fvm == null)
                return;

            UnSubscribeOnFormulaChanged();
            fvm.FormulaStateChanged += FormulaStateChanged;
        }

        private void UnSubscribeOnFormulaChanged()
        {
            var fvm = DataContext as IFormulaHandler;
            if (fvm != null)
                fvm.FormulaStateChanged -= FormulaStateChanged;
        }

        private void FillGroup(dynamic obj, LayoutGroup group, object parentData, SettingDisplay displaySetting)
        {
            bool inPropertyEditMode = false;
            if (obj != null)
                group.DataContext = obj;

            //TODO: по хорошему нужно писать свой контрол на остнове DataLayout-а (научить правильно биндиться, понимать аттрибуты и т.д.)
            var vm = (IObjectViewModel)DataContext;
            var pe = vm as IPropertyEditHandler;
            if (pe != null)
                inPropertyEditMode = pe.InPropertyEditMode;
            var fields = vm.GetDataFields(displaySetting);
            //var sub = DataContext as IObjectMemoryViewModel;
            var read = true;
            if (vm.Mode == ObjectViewModelMode.MemoryObject)
                read = vm.IsEditEnable;
            foreach (var field in fields)
            {
                // если в режиме PropertyEdit то не показываем вложенные коллекции
                if (inPropertyEditMode && typeof(IList).IsAssignableFrom(field.FieldType))
                    continue;
                bool? isMergedProperty = null;
                if (inPropertyEditMode)
                    isMergedProperty = pe.IsMergedPropery(field.Name);
                var oldLayout = FindName(field.Name);
                if (oldLayout != null && !field.IsChangeLookupCode)
                {
                    var li = oldLayout as CustomDataLayoutItem;
                    if (li != null)
                    {
                        li.Visibility = field.Visible ? Visibility.Visible : Visibility.Hidden;
                        if (field.IsEnabled.HasValue)
                            li.IsReadOnly = !field.IsEnabled.Value;
                        li.IsMergedProperty = isMergedProperty;
                        li.ApplyProperties();
                    }
                    continue;
                }

                var index = new int();
                if (oldLayout != null)
                {
                    index = group.Children.IndexOf((UIElement)oldLayout);
                    if (index < 0)
                        continue;
                    group.Children.Remove((UIElement)oldLayout);
                    UnregisterName(((FrameworkElement)oldLayout).Name);
                }

                var layoutItem = new CustomDataLayoutItem(field)
                    {
                        IsVisibilitySetOutside = true,
                        IsDisplayFormatSetOutside = true,
                        ParentViewModelSource = parentData,
                        ToolTipIns = CreateCustomSuperToolTip(field),
                        IsReadOnlyRightDependcy = !read,
                        ParentViewModel = vm,
                        IsMergedProperty = isMergedProperty
                    };

                var formulaBinding = CreateFormulaBinding(obj as IIsNew, field.FieldName);
                if (formulaBinding != null)
                {
                    layoutItem.FormulaBinding = formulaBinding;
                    BindingOperations.SetBinding(layoutItem, CustomDataLayoutItem.InFormulaModeProperty,
                        new Binding("InFormulaMode") {Source = vm, Mode = BindingMode.TwoWay});
                }

                if (oldLayout != null && index > -1)
                    group.Children.Insert(index, layoutItem);
                else group.Children.Add(layoutItem);
            }
            group.UpdateLayout();
        }

        private Binding CreateFormulaBinding(IIsNew newH, string path)
        {
            // только в режиме создания нового объекта поддерживается возможность управлять формулами
            if (newH == null || !newH.IsNew)
                return null;

            // если VM не поддерживает режим работы с формулами - выходим
            var formulaHandler = DataContext as IFormulaHandler;
            if (formulaHandler == null)
                return null;

            // создаем специальный биндинг
            var formulaBinding = new Binding(path)
                {
                    Source = formulaHandler.FormulaValues,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            return formulaBinding;
        }

        private static StackPanel CreateCustomSuperToolTip(DataField field)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = field.Caption });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = field.Description });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = string.Format("[{0}]", field.Name), FontFamily = new FontFamily("Segoe UI"), Foreground = Brushes.Gray, FontSize = 11 });
            return stack;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnSubscribeDataContextPropertyChanged(DataContext);
                UnSubscribeOnRefreshView();
                UnSubscribeOnFormulaChanged();
                DataContextChanged -= OnDataContextChanged;

                foreach (var child in objectDataLayout.Children)
                {
                    var disposable = child as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                // прибиваем Layout
                if (objectDataLayout != null)
                    objectDataLayout.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region .  IHelpHandler  .
        string IHelpHandler.GetHelpLink()
        {
            var dc = DataContext as IHelpHandler;
            if (dc != null)
            {
                var link = dc.GetHelpLink();
                if (link != null)
                    return link;
            }
            return "Object";
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = DataContext as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }
        #endregion
    }
}