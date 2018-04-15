using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels.Menu
{
    /// <summary>
    /// Базовый класс для всех элементов меню (панели, кнопки и т.д.)
    /// </summary>
    public abstract class MenuViewModelBase : ViewModelBase
    {
        #region Fields&Consts
        public const string PriorityPropertyName = "Priority";
        public const string IsEnablePropertyName = "IsEnable";

        private int _priority;
        private bool _isEnable;
        #endregion

        #region Properties
        public virtual string Name { get; set; }

        public bool IsDynamicBarItem { get; set; }

        public int Priority
        {
            get { return _priority; }
            set
            {
                if (_priority == value)
                    return;

                _priority = value;
                OnPropertyChanged(PriorityPropertyName);
            }
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                if (_isEnable == value)
                    return;

                _isEnable = value;
                OnPropertyChanged(IsEnablePropertyName);
            }
        }
        #endregion
    }
}