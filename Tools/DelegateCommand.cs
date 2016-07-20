using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Tools {
    public class DelegateCommand : ICommand, IDisposable {
        Action _action;
        bool _canExecute;
        CompositeDisposable _disposables;
        bool _isDisposed;

        public DelegateCommand(Action action, bool initialCanExecute = true, IObservable<bool> canExecute = null) {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _disposables = new CompositeDisposable();
            _action = action;
            _canExecute = initialCanExecute;
            if (canExecute != null) {
                canExecute
                    .DistinctUntilChanged() // ObserveOn?
                    .Subscribe(i => {
                        _canExecute = i;
                        OnCanExecuteChanged(EventArgs.Empty);
                    })
                    .AddTo(_disposables);
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute;

        public void Execute(object parameter) => _action();

        protected virtual void OnCanExecuteChanged(EventArgs args) => CanExecuteChanged?.Invoke(this, args);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_isDisposed)
                return;
            if (disposing) {
                _disposables.Dispose();
            }
            _isDisposed = true;
        }
    }
}
