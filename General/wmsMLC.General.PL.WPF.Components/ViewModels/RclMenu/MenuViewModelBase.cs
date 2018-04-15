using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    /// <summary>
    /// Базовый класс для всех элементов меню (панели, кнопки и т.д.)
    /// </summary>
    public abstract class MenuViewModelBase : ViewModelBase
    {
        #region Fields&Consts
        public const string PriorityPropertyName = "Priority";

        private int _priority;
        #endregion

        #region Properties
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
        #endregion
    }
}