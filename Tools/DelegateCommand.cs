using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    public class DelegateCommand : ICommand {
        Action _action;
        bool _latestCanExecute;

        public DelegateCommand(Action action, IObservable<bool> canExecute = null) {
            _action = action;
            _latestCanExecute = true;
            if (canExecute != null) {
                canExecute.DistinctUntilChanged().Subscribe((bool i) => {
                    _latestCanExecute = i;
                    OnCanExecuteChanged(EventArgs.Empty);
                });
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _latestCanExecute;

        public void Execute(object parameter) => _action();

        protected virtual void OnCanExecuteChanged(EventArgs args) => CanExecuteChanged?.Invoke(this, args);
    }
}
