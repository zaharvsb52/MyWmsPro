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
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Views
{
    public abstract class ObjectViewBase : DXPanelView, IRestoreLayoutInfo, IHelpHandler
    {
        protected ObjectViewBase()
        {
            DataContextChanged += OnDataContextChanged;
        }

        public bool IsRestored { get; protected set; }
        protected LayoutGroup Group { get; set; }
        protected MenuView MenuView { get; set; }

        #region .  Methods  .

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnDataContextChanged(e);
        }

        protected virtual void OnDataContextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null)
                return;

            UnSubscribeDataContextPropertyChanged(e.OldValue);

            SubscribeOnRefreshView();
            var vm = DataContext as IObjectViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
                if (MenuView != null)
                    MenuView.DataContext = vm;
            }

            RefreshBinding();
            SubscribeDataContextPropertyChanged(e.NewValue);
        }

        private void RefreshViewEvent(object sender, EventArgs e)
        {
            RefreshBinding();
        }

        protected virtual void FillGroup(dynamic obj, LayoutGroup group, object parentData, SettingDisplay displaySetting)
        {
            var inPropertyEditMode = false;

            if (obj != null)
                group.DataContext = obj;

            //TODO: по хорошему нужно писать свой контрол на остнове DataLayout-а (научить правильно биндиться, понимать аттрибуты и т.д.)
            var vm = (IObjectViewModel) DataContext;
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
                        li.ParentViewModel = vm;
                        li.Visibility = field.Visible ? Visibility.Visible : Visibility.Hidden;
                        if (field.IsEnabled.HasValue)
                            li.IsReadOnly = !field.IsEnabled.Value;
                        li.IsMergedProperty = isMergedProperty;
                        li.ApplyProperties();
                    }
                    continue;
                }

                LayoutGroup foundGroup = null;
                var index = 0;

                if (oldLayout != null)
                {
                    foundGroup = SearchGroup(group, (UIElement)oldLayout);
                    if (foundGroup == null)
                        continue;
                    index = foundGroup.Children.IndexOf((UIElement)oldLayout);
                    if (index < 0)
                        continue;
                    foundGroup.Children.Remove((UIElement)oldLayout);
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

                //if (typeof(IList).IsAssignableFrom(field.FieldType))
                //{
                //    layoutItem.LabelPosition = LayoutItemLabelPosition.Top;
                //    layoutItem.Margin = new Thickness(0, 5, 0, 5);
                //    var style = FindResource("LayoutItemLabel") as Style;
                //    layoutItem.LabelStyle = style;
                //}

                var formulaBinding = CreateFormulaBinding(vm as IFormulaHandler, obj as IIsNew, field.FieldName);
                if (formulaBinding != null)
                {
                    layoutItem.FormulaBinding = formulaBinding;
                    BindingOperations.SetBinding(layoutItem, CustomDataLayoutItem.InFormulaModeProperty,
                        new Binding("InFormulaMode") { Source = vm, Mode = BindingMode.TwoWay });
                }

                if (oldLayout != null && index > -1)
                    foundGroup.Children.Insert(index, layoutItem);
                else group.Children.Add(layoutItem);
            }
        }

        protected Binding CreateFormulaBinding(IFormulaHandler formulaHandler, IIsNew newH, string path)
        {
            // только в режиме создания нового объекта поддерживается возможность управлять формулами
            if (formulaHandler == null || newH == null || !newH.IsNew)
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

        protected static StackPanel CreateCustomSuperToolTip(DataField field)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = field.Caption });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = field.Description });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock
            {
                Text = string.Format("[{0}]", field.Name),
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = Brushes.Gray,
                FontSize = 11
            });
            return stack;
        }

        public static LayoutGroup SearchGroup(LayoutGroup gr, UIElement el)
        {
            var index = gr.Children.IndexOf(el);
            if (index > -1)
                return gr;

            var i = 0;
            while (i < gr.Children.Count)
            {
                var lg = gr.Children[i] as LayoutGroup;
                if (lg != null)
                {
                    var newGr = SearchGroup(lg, el);
                    if (newGr != null)
                        return newGr;
                }
                i++;
            }
            return null;
        }

        protected void RefreshBinding()
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

            //Подготавливаем данные
            var groupVisibility = Group.Visibility;
            using (Group.Dispatcher.DisableProcessing())
            //using (Dispatcher.DisableProcessing())
            {
                try
                {
                    Group.Visibility = Visibility.Collapsed;
                    FillGroup(m, Group, parentData, displaySetting);
                    if (!IsRestored) //если вид не загружен - загружаем
                    {
                        var vs = IoC.Instance.Resolve<IViewService>();
                        vs.RestoreLayout(this);
                        IsRestored = true;
                        OnAfterRestoreLayout();
                    }
                }
                finally
                {
                    Group.Visibility = groupVisibility;
                }
            }
        }

        protected virtual void OnAfterRestoreLayout()
        {
        }

        private void UnSubscribeDataContextPropertyChanged(object context)
        {
            var npch = context as INotifyPropertyChanged;
            if (npch != null)
                npch.PropertyChanged -= DataContextPropertyChanged;
        }

        private void DataContextPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == SourceViewModelBase<int>.SourcePropertyName)
                RefreshBinding();
        }

        protected void SubscribeOnFormulaChanged()
        {
            var fvm = DataContext as IFormulaHandler;
            if (fvm == null)
                return;

            UnSubscribeOnFormulaChanged();
            fvm.FormulaStateChanged += OnFormulaStateChanged;
        }

        protected void UnSubscribeOnFormulaChanged()
        {
            var fvm = DataContext as IFormulaHandler;
            if (fvm != null)
                fvm.FormulaStateChanged -= OnFormulaStateChanged;
        }

        private void OnFormulaStateChanged(object sender, FormulaStateEventArgument e)
        {
            OnFormulaStateChanged(e);
        }

        protected virtual void OnFormulaStateChanged(FormulaStateEventArgument e)
        {
            foreach (var child in Group.GetChildren(true))
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

        private void SubscribeDataContextPropertyChanged(object context)
        {
            var npch = context as INotifyPropertyChanged;
            if (npch != null)
                npch.PropertyChanged += DataContextPropertyChanged;
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


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnSubscribeDataContextPropertyChanged(DataContext);
                UnSubscribeOnRefreshView();
                UnSubscribeOnFormulaChanged();
                DataContextChanged -= OnDataContextChanged;

                if (Group != null)
                {
                    foreach (var child in Group.Children)
                    {
                        var disposable = child as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }

                    // прибиваем Layout
                    var groupdisposable = Group as IDisposable;
                    if (groupdisposable != null)
                        groupdisposable.Dispose();
                }
            }

            base.Dispose(disposing);
        }
        #endregion .  Methods  .

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

        #endregion .  IHelpHandler  .
    }
}
