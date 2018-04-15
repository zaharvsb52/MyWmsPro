using System;
using System.Windows;
using System.Windows.Controls;

namespace wmsMLC.General.PL.WPF.Views
{
    public class CustomUserControl : UserControl, IDisposable
    {
        protected bool IsDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                // спрашиваем ViewModel
                var disposable = DataContext as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                IsDisposed = true;
            }
        }
    }

    /// <summary>
    /// Контрол, обобщающий логику работы вложенных в форму детализации контролов
    /// </summary>
    public class CustomSubControl : CustomUserControl
    {
        public static double CalcMaxHeight()
        {
            // возвращаем 70% от величины экрана. основная идея - чтобы грид целиком помещался на экране (с учетом меню)
            return 0.7 * SystemParameters.PrimaryScreenHeight;
        }

        public bool IsCustomization { get; set; }
    }
}
