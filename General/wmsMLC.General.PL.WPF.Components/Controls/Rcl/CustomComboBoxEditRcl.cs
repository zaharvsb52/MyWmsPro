using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.EditStrategy;
using log4net;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomComboBoxEditRcl : CustomComboBoxEdit
    {
        #region .  Fields  .
        private readonly ILog _log = LogManager.GetLogger(typeof(CustomComboBoxEditRcl));
        #endregion .  Fields  .

        public CustomComboBoxEditRcl()
        {
            ShowEditorButtons = false;
            ShowWindowCommand = new DelegateCustomCommand(OnShowWindow, CanShowWindow);
        }

        #region .  Properties  .
        public bool HasFocus { get; private set; }
        public ICommand ShowWindowCommand { get; set; }
        public int? MaxRowsOnPage { get; set; }
        public bool UseFunctionKeys { get; set; }
        public bool ParentKeyPreview { get; set; }
        public Key HotKey { get; set; }
        public string CustomDisplayMember { get; set; }
        public RclLookupType LookupType { get; set; }
        public string LookupTitle { get; set; }
        public bool IsWfDesignMode { get; set; }
        public string LayoutValue { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .
        private bool CanShowWindow()
        {
            return !IsReadOnly;
        }

        private void OnShowWindow()
        {
            if (!CanShowWindow())
                return;

            using (var dlgmodel = (IDisposable) GetViewModel())
            {
                var viewService = IoC.Instance.Resolve<IViewService>();
                if (viewService.ShowDialogWindow(viewModel: (IViewModel) dlgmodel, isRestoredLayout: false) == true)
                {
                    var model = (IRclListViewModel)dlgmodel;
                    if (model.SelectedItem != null)
                        EditValue = model.SelectedItem;
                    if (IsWfDesignMode)
                    {
                        model.GetLayoutFromView();
                        LayoutValue = model.LayoutValue;
                    }
                }
            }
        }

        public void OpenPopup()
        {
            OnShowWindow();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            HasFocus = true;
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            HasFocus = false;
            base.OnLostFocus(e);
        }

        protected override EditStrategyBase CreateEditStrategy()
        {
            return new CustomComboBoxEditStrategy(this);
        }

        protected override void OnIsPopupOpenChanged()
        {
        }

        private IViewModel GetViewModel()
        {
            var panelCaption = LookupTitle;
            var itemsSourceType = GetItemsSourceType();
            if (string.IsNullOrEmpty(panelCaption) && itemsSourceType != null)
                panelCaption = GetCaption(itemsSourceType);

            switch (LookupType)
            {
                case RclLookupType.Default:
                    var itemsSource = new List<CustomSelectControlBase.SelectListItem>();
                    var data = ItemsSource as IList;
                    if (data != null && data.Count > 0)
                    {
                        PropertyDescriptorCollection prdesc = null;
                        PropertyDescriptor idProp = null;
                        PropertyDescriptor displayMemberProp = null;

                        CalcEngine.CalcEngine engine = null;
                        if (!string.IsNullOrEmpty(CustomDisplayMember))
                            engine = new CalcEngine.CalcEngine();

                        var id = 0;
                        foreach (var d in data)
                        {
                            if (prdesc == null)
                                prdesc = TypeDescriptor.GetProperties(d);
                            if (idProp == null)
                                idProp = prdesc.Find(ValueMember, true);
                            if (displayMemberProp == null)
                                displayMemberProp = prdesc.Find(DisplayMember, true);

                            var item = new CustomSelectControlBase.SelectViewItem {Id = idProp.GetValue(d)};

                            if (engine != null)
                            {
                                try
                                {
                                    engine.DataContext = d;
                                    item.Name = engine.Evaluate(CustomDisplayMember).To<string>();
                                }
                                catch (Exception ex)
                                {
                                    _log.WarnFormat(
                                        "При попытке получить описание по CustomDisplayMember='{0}' возникла ошибка: {1}",
                                        CustomDisplayMember, ExceptionHelper.ExceptionToString(ex));
                                    _log.Debug(ex);
                                    // прописываем текст из DisplayMember
                                    item.Name = displayMemberProp.GetValue(d).To<string>();
                                }
                            }
                            else
                                item.Name = displayMemberProp.GetValue(d).To<string>();

                            var listItem = new CustomSelectControlBase.SelectListItem {Id = id++, Value = item};
                            itemsSource.Add(listItem);
                        }
                    }

                    var model = new CustomComboBoxEditRclContentViewModel
                    {
                        ItemsSource = itemsSource,
                        SelectedItem = EditValue,
                        FontSize = FontSize,
                        UseFunctionKeys = UseFunctionKeys,
                        ParentKeyPreview = ParentKeyPreview,
                        PanelCaption = panelCaption
                    };

                    if (MaxRowsOnPage.HasValue)
                        model.MaxRowsOnPage = MaxRowsOnPage.Value;

                    return model;

                case RclLookupType.DefaultGrid:
                    if (itemsSourceType == null)
                        return null;

                    var destType = typeof(RclLookupViewModel<>).MakeGenericType(itemsSourceType);
                    var rclListViewModel = (IRclListViewModel) Activator.CreateInstance(destType);

                    rclListViewModel.PanelCaption = panelCaption;
                    rclListViewModel.FontSize = FontSize;
                    rclListViewModel.SetItemsSource(ItemsSource);
                    rclListViewModel.LayoutValue = LayoutValue;
                    rclListViewModel.IsWfDesignMode = IsWfDesignMode;
                    rclListViewModel.SelectedItem = EditValue;
                    return rclListViewModel;
            }

            return null;
        }

        private Type GetItemsSourceType()
        {
            if (ItemsSource == null)
                return null;

            var typeItemsSource = ItemsSource.GetType();
            if (typeItemsSource.IsGenericType)
                return typeItemsSource.GetGenericArguments().First();
            return typeItemsSource;
        }

        private string GetCaption(Type type)
        {
            if (type == null)
                return null;

            if (type.IsGenericType)
                type = type.GetGenericArguments().First();

            var attributes = TypeDescriptor.GetAttributes(type);
            var att = attributes[typeof(ListViewCaptionAttribute)] as ListViewCaptionAttribute;
            return att == null
                ? string.Format(Properties.Resources.ObjectListViewModelBasePanelCaptionFormatDefault, type.Name)
                : att.Caption;
        }

        public bool PreviewHotKey(Key hotKey)
        {
            if (Visibility == Visibility.Visible && IsEnabled && !IsReadOnly && hotKey == HotKey)
            {
                Focus();
                OnShowWindow();
                return true;
            }
            return false;
        }

        public bool PreviewHotKey(Key hotKey, Key hotKey2)
        {
            var result = PreviewHotKey(hotKey);
            return result || PreviewHotKey(hotKey2);
        }

        public bool PreviewHotKey(KeyEventArgs e)
        {
            if (e.Handled)
                return true;

            e.Handled = PreviewHotKey(e.Key);
            return e.Handled;
        }
        #endregion .  Methods  .
    }

    public class CustomComboBoxEditStrategy : ComboBoxEditStrategy
    {
        public CustomComboBoxEditStrategy(ComboBoxEdit editor)
            : base(editor)
        {
        }

        protected override bool AllowSpin
        {
            get { return false; }
        }

        protected override void ProcessPreviewKeyDownInternal(KeyEventArgs e)
        {
            if (!e.Handled && !Editor.IsReadOnly)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        SpinUp();
                        e.Handled = true;
                        break;
                    case Key.Right:
                        SpinDown();
                        e.Handled = true;
                        break;
                }
            }

            base.ProcessPreviewKeyDownInternal(e);
        }
    }
}