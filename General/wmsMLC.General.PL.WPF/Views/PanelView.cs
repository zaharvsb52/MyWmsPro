using System;
using System.Windows;
using System.Windows.Media;

namespace wmsMLC.General.PL.WPF.Views
{
    public class PanelView : CustomUserControl, IPanelView
    {
        #region .  Fields  .
        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PanelView));
        private DateTime _startLoadTime = DateTime.Now;
        #endregion .  Fields  .

        #region .  Dependency Properties declaration  .
        public static readonly DependencyProperty PanelCaptionProperty = DependencyProperty.Register("PanelCaption", typeof(string), typeof(PanelView));
        public static readonly DependencyProperty PanelCaptionImageProperty = DependencyProperty.Register("PanelCaptionImage", typeof(ImageSource), typeof(PanelView));
        public static readonly DependencyProperty AllowClosePanelProperty = DependencyProperty.Register("AllowClosePanel", typeof(bool), typeof(PanelView));
        #endregion .  Dependency Properties declaration  .

        public PanelView()
        {
            // по умолчанию считаем, что в источнике св-ва называются также
            SetBinding(PanelCaptionProperty, PanelCaptionProperty.Name);
            SetBinding(PanelCaptionImageProperty, PanelCaptionImageProperty.Name);
            SetBinding(AllowClosePanelProperty, AllowClosePanelProperty.Name);
            Loaded += OnLoaded;
        }

        #region .  Properties  .
        /// <summary>
        /// Признак того, что панель не может быть закрыта
        /// </summary>
        public bool AllowClosePanel
        {
            get { return (bool)GetValue(AllowClosePanelProperty); }
            set { SetValue(AllowClosePanelProperty, value); }
        }

        /// <summary>
        /// Иконка панели
        /// </summary>
        public ImageSource PanelCaptionImage
        {
            get { return (ImageSource)GetValue(PanelCaptionImageProperty); }
            set { SetValue(PanelCaptionImageProperty, value); }
        }

        /// <summary>
        /// Заголовок панели
        /// </summary>
        public string PanelCaption
        {
            get { return (string)GetValue(PanelCaptionProperty); }
            set { SetValue(PanelCaptionProperty, value); }
        }
        #endregion .  Properties  .

        #region .  Methods  .
        public virtual bool CanClose()
        {
            // спрашиваем ViewModel
            var closable = DataContext as IClosable;
            if (closable != null && closable != this)
                return closable.CanClose();

            return true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _log.DebugFormat("View for '{0}' has been loaded in {1}", PanelCaption, DateTime.Now - _startLoadTime);
        }

        protected override void Dispose(bool disposing)
        {
            _log = null;
            base.Dispose(disposing);
        }

        public void SetStartLoadTime()
        {
            _startLoadTime = DateTime.Now;
        }
        #endregion .  Methods  .
    }
}