using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    public abstract class AsyncCommandBase : IAsyncCommand {
        public abstract bool CanExecute(object parameter);

        public abstract Task ExecuteAsync(object parameter);

        async void ICommand.Execute(object parameter) => await ExecuteAsync(parameter);

        public event EventHandler CanExecuteChanged;

        protected void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}