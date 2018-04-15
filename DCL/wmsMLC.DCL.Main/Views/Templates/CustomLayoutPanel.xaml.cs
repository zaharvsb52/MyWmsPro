using System;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using DevExpress.Xpf.Docking;
using wmsMLC.General.PL.WPF.Events;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views.Templates
{
    public partial class CustomLayoutPanel : LayoutPanel, IHandle<WaitEvent>, IDisposable
    {
        private IEventAggregator _eventAggregator;
        private readonly DataTemplate _captionTemplateWait;
        private readonly DataTemplate _captionTemplate;

        public CustomLayoutPanel(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _captionTemplateWait = CaptionTemplate;
            _captionTemplate = (DataTemplate) Resources["PanelCaptionTemplate"];
            CaptionTemplate = _captionTemplate;

            KeyDown += CustomLayoutPanel_KeyDown;
        }

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        public bool IsDisposed { get; private set; }

        ~CustomLayoutPanel()
        {
            if (IsDisposed)
                return;

            Dispose(false);
            IsDisposed = true;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">False - если требуется освободить только UnManaged ресурсы, True - если еще и Managed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // events
                KeyDown -= CustomLayoutPanel_KeyDown;

                // managed objects
                if (_eventAggregator != null)
                {
                    _eventAggregator.Unsubscribe(this);
                    _eventAggregator = null;
                }

                var dispContent = Content as IDisposable;
                if (dispContent != null)
                    dispContent.Dispose();
                //this.Content = null;
            }
        }
        #endregion

        public static CustomLayoutPanel CreateWithView(object content)
        {
            var panel = IoC.Get<CustomLayoutPanel>();
            panel.Content = content;
            panel.AllowDock = true;
            panel.AllowClose = true;
            panel.AllowRestore = false;
            panel.ShowCaption = true;
            panel.ShowCaptionImage = false; //Показываем нашу иконку
            panel.ClosingBehavior = ClosingBehavior.ImmediatelyRemove;
            //MICROHACK: на мой взгляд самый правильный способ проверить где брать параметры поведения
            // проблема - передать поведения вложенного контрола родительскому
            if (content is PanelView)
            {
                panel.SetBinding(CaptionProperty, new Binding(PanelView.PanelCaptionProperty.Name) { Source = content });
                panel.SetBinding(CaptionImageProperty, new Binding(PanelView.PanelCaptionImageProperty.Name) { Source = content });
                panel.SetBinding(AllowCloseProperty, new Binding(PanelView.AllowClosePanelProperty.Name) { Source = content });
            }
            else
                panel.Caption = "new page";
            return panel;
        }

        void IHandle<WaitEvent>.Handle(WaitEvent message)
        {
            // отбрасываем чужие сообщения
            //NOTE: позже можно сделать специальный поиск по вложенным элементам
            if (message.Sender != Content &&
                (!(Content is IView) || message.Sender != ((IView)Content).DataContext)) return;

            switch (message.Type)
            {
                case WaitEventType.Start:
                    if (_captionTemplateWait != null && !_captionTemplateWait.Equals(CaptionTemplate))
                        CaptionTemplate = _captionTemplateWait;
                    break;

                case WaitEventType.Stop:
                    //CaptionTemplate = null;
                    if (_captionTemplate != null && !_captionTemplate.Equals(CaptionTemplate))
                        CaptionTemplate = _captionTemplate;
                    break;
            }
        }

        private void CustomLayoutPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Escape)
                return;

            //Если уже установлено true, вкладка не закроется
            if (Closed)
                Closed = false;
            Closed = true;

            e.Handled = true;
        }
    }
}
