using System.Windows.Input;

namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public class CommandMenuItem : CommonMenuItemBase
    {
        #region Fields&Consts
        public const string ParentPropertyName = "Parent";
        public const string CommandPropertyName = "Command";
        public const string CommandParametrPropertyName = "CommandParameter";

        private string _parent;
        private ICommand _command;
        private object _commandparametr;
        #endregion

        #region Properties
        public string Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                    return;

                _parent = value;
                OnPropertyChanged(ParentPropertyName);
            }
        }
        public ICommand Command
        {
            get { return _command; }
            set
            {
                if (_command == value)
                    return;

                _command = value;
                OnPropertyChanged(CommandPropertyName);
            }
        }
        public object CommandParameter
        {
            get { return _commandparametr; }
            set
            {
                if (_commandparametr == value)
                    return;

                _commandparametr = value;
                OnPropertyChanged(CommandParametrPropertyName);
            }
        }
        #endregion
    }
}