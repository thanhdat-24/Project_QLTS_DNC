using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Project_QLTS_DNC.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteFunc;

        public RelayCommand(Action executeAction, Func<bool> canExecuteFunc = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteFunc = canExecuteFunc;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc == null || _canExecuteFunc();
        }

        public void Execute(object parameter)
        {
            _executeAction();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _executeAction;
        private readonly Predicate<T> _canExecutePredicate;

        public RelayCommand(Action<T> executeAction, Predicate<T> canExecutePredicate = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecutePredicate = canExecutePredicate;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecutePredicate == null ||
                   (parameter is T typedParam && _canExecutePredicate(typedParam));
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
            {
                _executeAction(typedParam);
            }
        }

      

    }
}