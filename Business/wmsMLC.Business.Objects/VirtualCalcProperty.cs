using System;
using System.ComponentModel;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    /// <summary> 
    /// Виртуальное вычисляемое поле 
    /// </summary> 
    public class VirtualCalcProperty : CustomProperty
    {
        #region .  Properties  .
        /// <summary> 
        /// Выражение, по которому происходит вычисление 
        /// </summary> 
        public string Expression { get; set; }

        /// <summary> 
        /// Родительский объект свойства 
        /// </summary> 
        public object Obj { get; set; }
        #endregion

        #region .  Constructors  .
        public VirtualCalcProperty(string name) : base(name) { }
        public VirtualCalcProperty(string name, Type type, object defaultValue) : base(name, type, defaultValue) { }
        public VirtualCalcProperty(string name, Type type, object defaultValue, string expression, object obj)
            : base(name, type, defaultValue)
        {
            Expression = expression;
            Obj = obj;
            var npch = Obj as INotifyPropertyChanged;
            if (npch != null)
                npch.PropertyChanged += ObjectPropertyChanged;
        }
        #endregion

        #region .  Methods  .
        private void ObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Expression) || !Expression.Contains(e.PropertyName))
                return;

            var rpch = Obj as IRaisePropertyChanged;
            if (rpch != null)
                rpch.RaisePropertyChanged(Name);
        }

        private static readonly object _lock = new object();
        private static readonly Lazy<CalcEngine.CalcEngine> _calcEngine = new Lazy<CalcEngine.CalcEngine>();
        private static object Evaluate(string expression, object obj, object defaultValue)
        {
            if (string.IsNullOrEmpty(expression))
                return defaultValue;

            try
            {
                lock (_lock)
                {
                    _calcEngine.Value.DataContext = obj;
                    var res = _calcEngine.Value.Evaluate(expression);
                    _calcEngine.Value.DataContext = null;
                    return res;
                }
            }
            finally
            {
                _calcEngine.Value.DataContext = null;
            }
        }

        protected override object GetValue()
        {
            return Evaluate(Expression, Obj, GetDefaultValue());
        }

        protected override void SetValue(object value)
        {
            // ничего не делаем. виртуальные поля не должын выставляться извне 
            //throw new DeveloperException("Virtual property can't be set"); 
        }

        #endregion
    } 
}