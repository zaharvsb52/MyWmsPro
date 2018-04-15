using System;
using System.Windows.Input;

namespace wmsMLC.DCL.General
{
    /// <summary>
    /// Базовый класс реализации комманд.
    /// <remarks>"Позаимствовал" из примеров devexpress</remarks>
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region .  Fields  .
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion

        #region .  Constructors  .
        public RelayCommand(Action<object> execute) : this(execute, null) { }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }

    public class RelayCommand<T> : ICommand
    {
        #region .  Fields  .
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;
        #endregion

        #region .  Constructors  .
        public RelayCommand(Action<T> execute) : this(execute, null) { }
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        public bool CanExecute(T parameter)
        {
            return ((ICommand)this).CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { ((ICommand)this).CanExecuteChanged += value; }
            remove { ((ICommand)this).CanExecuteChanged -= value; }
        }

        public void Execute(T parameter)
        {
            ((ICommand)this).Execute(parameter);
        }

        #region ICommand Members

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        void ICommand.Execute(object parameter)
        {
            _execute((T) parameter);
        }

        #endregion // ICommand Members
    }
}