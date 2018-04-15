using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ProductChangeReservedView))]
    public class ProductChangeReservedViewModel : ExpandoObjectValidateViewModel
    {
        #region .  Fields  .
        private decimal _value;
        private decimal? _minValue;
        private decimal? _maxValue; 
        #endregion

        #region .  Properties  .
        public Decimal Value
        {
            get { return _value; }
            set
            {
                if (_value == value)
                    return;
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public Decimal? MinValue
        {
            get { return _minValue; }
            set
            {
                if (_minValue == value)
                    return;
                _minValue = value;
                OnPropertyChanged("MinValue");
            }
        }

        public Decimal? MaxValue
        {
            get { return _maxValue; }
            set
            {
                if (_maxValue == value)
                    return;
                _maxValue = value;
                OnPropertyChanged("MaxValue");
            }
        }

        public Decimal MaxValueDefault { get; set; }

        public string NewProductPropertyName { get; set; }

        public string SpinEditPropertyName { get; set; } 
        #endregion

        protected override void InitializeSettings()
        {
            base.InitializeSettings();
            IsMenuEnable = true;
            IsCustomizeBarEnabled = true;
        }

        protected override void SourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (!e.PropertyName.EqIgnoreCase(NewProductPropertyName))
                return;

            var productid = this[NewProductPropertyName];
            if (productid == null || Equals(productid, string.Empty))
            {
                MaxValue = MaxValueDefault;
                Value = MaxValueDefault;
                return;
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Product>>())
            {
                var product = mgr.Get(productid, GetModeEnum.Partial);
                var maxValue = Math.Min(product.ProductCountSKU, MaxValueDefault);
                if (maxValue < 0)
                    maxValue = 0;
                MaxValue = maxValue;
                Value = maxValue;
            }

            //if (MinValue.HasValue && Value < MinValue.Value)
            //    Value = MinValue.Value;
            //if (MaxValue.HasValue && Value > MaxValue.Value)
            //    Value = MaxValue.Value;
        }

        public override bool DoAction()
        {
            var result = base.DoAction();
            if (!result)
                return false;

            string message = null;
            var fieldcaption = "New amount";
            if (!string.IsNullOrEmpty(SpinEditPropertyName))
            {
                var field = Fields.FirstOrDefault(p => p.Name.EqIgnoreCase(SpinEditPropertyName));
                if (field != null && !string.IsNullOrEmpty(field.Caption))
                    fieldcaption = field.Caption;
            }
            if (Value <= 0)
            {
                message = string.Format("Значение поля '{0}' должно быть больше 0.", fieldcaption);
            }
            else if (MinValue.HasValue && Value < MinValue.Value)
            {
                message = string.Format("Значение поля '{0}' {1} меньше нижней границы {2}.", fieldcaption, Value, MinValue);
            }
            else if (MaxValue.HasValue && Value > MaxValue.Value)
            {
                message = string.Format("Значение поля '{0}' {1} превышает верхнюю границу {2}.", fieldcaption, Value, MaxValue);
            }

            if (!string.IsNullOrEmpty(message))
            {
                GetViewService().ShowDialog(StringResources.Error
                    ,
                    string.Format("{0}{1}{2}", StringResources.ErrorSave, Environment.NewLine, message)
                    , MessageBoxButton.OK
                    , MessageBoxImage.Error
                    , MessageBoxResult.Yes);
                return false;
            }

            return true;
        }
    }
}
