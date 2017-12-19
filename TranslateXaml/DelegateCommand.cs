using System;
using System.Windows.Input;

namespace TranslateXaml
{
    /// <summary>
    /// ICommand helper class
    /// </summary>
    public class DelegateCommand : ICommand
    {
        Action execute;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute();
        }

        public DelegateCommand(Action execute)
        {
            this.execute = execute;
        }
    }
}
