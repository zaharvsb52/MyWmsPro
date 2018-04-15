using System;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using wmsMLC.General.PL.WPF.Events;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public abstract class RclPanelViewModelBase : RclViewModel, IRclPanelViewModel, IClosable
    {
        #region .  Fields & Consts  .
        public const string MenuPropertyName = "Menu";
        public const string SaveLayoutMethodName = "SaveLayout";
        public const string ClearLayoutMethodName = "ClearLayout";
        public const string ShowInNewWindowMethodName = "ShowInNewWindow";
        public const string CustomizationMethodName = "Customization";
        public const string BlockMethodName = "Entity2Block";
        public const string AddBlockMethodName = "AddBlock";

        private string _panelCaption;
        private bool _allowClosePanel;
        private volatile int _waitCallCount;
        #endregion

        protected RclPanelViewModelBase()
        {
            IEventAggregator ea;
            if (IoC.Instance.TryResolve(out ea))
            {
                EventAggregator = ea;
                EventAggregator.Subscribe(this);
            }
        }

        #region .  Properties  .
        public event EventHandler LayoutViewSaved;

        public string PanelCaption
        {
            get { return _panelCaption; }
            set
            {
                if (_panelCaption == value)
                    return;

                _panelCaption = value;
                OnPropertyChanged("PanelCaption");
            }
        }

        private double _fontSize;
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                if (Equals(_fontSize, value))
                    return;

                _fontSize = value;
                OnPropertyChanged("FontSize");
            }
        }

        private string _layoutValue;

        public string LayoutValue
        {
            get { return _layoutValue; }
            set
            {
                if (_layoutValue == value)
                    return;
                _layoutValue = value;
                OnPropertyChanged("LayoutValue");
            }
        }

        public bool IsWfDesignMode { get; set; }

        public bool AllowClosePanel
        {
            get { return _allowClosePanel; }
            set
            {
                if (_allowClosePanel == value)
                    return;

                _allowClosePanel = value;
                OnPropertyChanged("AllowClosePanel");
            }
        }

        private RclMenuViewModel _menu;
        public RclMenuViewModel Menu
        {
            get { return _menu ?? (_menu = new RclMenuViewModel()); }
            protected set
            {
                _menu = value;
                OnPropertyChanged(MenuPropertyName);
            }
        }

        protected IEventAggregator EventAggregator { get; private set; }

        public virtual string PolicyName
        {
            get { return "PL"; }
        }
        #endregion .  Properties  .

        #region .  Methods  .
        protected virtual void ExceptionHandler(Exception ex, string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            if (ex == null)
                return;

            var result = new OperationException(message, ex);
            ExceptionPolicy.Instance.HandleException(result, PolicyName);
        }

        protected static IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }

        protected void WaitStart(bool doEvents = true)
        {
            if (EventAggregator != null)
                EventAggregator.Publish(new WaitEvent(this, WaitEventType.Start));

            _waitCallCount++;
            if (_waitCallCount == 1)
            {
                //DoEvents();
                WaitIndicatorVisible = true;
                if (doEvents)
                    DoEvents();
            }
        }

        protected void WaitStop(bool doEvents = true)
        {
            if (EventAggregator != null)
                EventAggregator.Publish(new WaitEvent(this, WaitEventType.Stop));

            _waitCallCount--;
            if (_waitCallCount == 0)
            {
                //DoEvents();
                WaitIndicatorVisible = false;
                if (doEvents)
                    DoEvents();
            }
            else if (_waitCallCount < 0)
                throw new DeveloperException("Ошибка работы со счетчиками wait запросов. Кол-во start меньше кол-ва stop.");
        }

        /// <summary>
        /// Данный метод позволяет "протолкнуть" исполнение комманды
        /// <remarks>
        /// Пример: Вы взводите флаг необходимости отобразить диалог ожидания, и следующим действием выполняете длительную команду.
        /// В результате, в большинстве случаев диалог так и не появтися, т.к. поток будет занят выполенением следующей комманды.
        /// В этом случаем контролу нужно помочь.
        /// </remarks>
        /// </summary>
        public void DoEvents()
        {
            //копать тут
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(delegate { }));
        }

        /// <summary>
        /// Запрос на закрытие соответсвующего view.
        /// </summary>
        protected void DoCloseRequest()
        {
            EventAggregator.Publish(new CloseRequestEvent(this));
        }

        protected virtual bool CanCloseInternal()
        {
            return true;
        }

        protected virtual void OnError(string message, Exception ex)
        {
            EventAggregator.Publish(new ErrorEvent(message, ex));
        }

        protected override void Dispose(bool disposing)
        {
            EventAggregator.Unsubscribe(this);
            base.Dispose(disposing);
        }

        bool IClosable.CanClose()
        {
            return CanCloseInternal();
        }

        public virtual void SetItemsSource(object source)
        {

        }

        public virtual void GetLayoutFromView()
        {
            OnLayoutViewSaved();
        }

        protected virtual void OnLayoutViewSaved()
        {
            var handler = LayoutViewSaved;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion .  Methods  .
    }
}