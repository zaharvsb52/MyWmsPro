using System;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomTextEditSettings : TextEditSettings, IDisposable
    {
        #region .  Fields  .

        public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register("Converter", typeof(IValueConverter), typeof(CustomTextEditSettings));

        #endregion

        #region .  Properties  .

        private bool _isFocus;
        private bool _isDirty;
      
        public IValueConverter Converter
        {
            get { return (IValueConverter) GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }
      
        #endregion

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~CustomTextEditSettings()
        {
            if (_disposed)
                return;

           
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            GC.SuppressFinalize(this);
            _disposed = true;
        }

      
        #endregion
        
        #region .  Methods  .

        protected override void AssignToEditCore(IBaseEdit edit)
        {
            base.AssignToEditCore(edit);

            if (Converter == null)
                return;

            var te = edit as TextEdit;
            if (te == null)
                return;
            
            te.EditValueChanged += TEOnEditValueChanged;
            te.GotFocus += OnGotFocus;
            te.LostFocus += OnLostFocus;

            //var binding = new Binding("Text")
            //{
            //    Mode = BindingMode.OneWay,
            //    RelativeSource = new RelativeSource(RelativeSourceMode.Self),
            //    Converter = Converter,
            //    ConverterParameter = te.EditMode,
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            //};
            //te.SetBinding(BaseEdit.EditValueProperty, binding);
        }
       
        private void TEOnEditValueChanged(object sender, EditValueChangedEventArgs editValueChangedEventArgs)
        {
            if (Converter == null)
                return;

            var te = sender as TextEdit;
            if (te == null)
                return;

            if (_isDirty || _isFocus)
            {
                te.EditValue = editValueChangedEventArgs.NewValue;
                return;
            }

            te.EditValue = Converter.ConvertBack(editValueChangedEventArgs.NewValue, null, null, null);
            _isDirty = true;
        }


        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Converter == null || _isFocus)
                return;

            var te = sender as TextEdit;
            if (te == null)
                return;

            _isFocus = true;
            if (!_isDirty)
                te.EditValue = Converter.ConvertBack(te.EditValue, null, null, null);
            else
                _isDirty = false;
            _isFocus = false;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Converter == null || _isFocus)
                return;
         
            var te = sender as TextEdit;
            if (te == null)
                return;

            _isFocus = true;
            te.EditValue = Converter.Convert(te.EditValue, null, null, null);
            _isFocus = false;
        }
        #endregion
    }
}