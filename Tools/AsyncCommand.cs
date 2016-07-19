using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    public class AsyncCommand<TResult> : ICommand, IObservable<IObservable<TResult>>, IDisposable {
        bool _isDisposed;
        bool _initialCanExecute;
        Func<CancellationToken, Task<TResult>> _execute;
        BehaviorSubject<bool> _isExecuting;
        IDisposable _isExecutingSubscription;
        Subject<IObservable<TResult>> _results;

        public AsyncCommand(Func<CancellationToken, Task<TResult>> execute, IObservable<bool> canExecute = null, bool initialCanExecute = true) {
            _isDisposed = false;
            _isExecuting = new BehaviorSubject<bool>(false);
            _results = new Subject<IObservable<TResult>>();
            _initialCanExecute = initialCanExecute;
            _execute = execute;
            _isExecutingSubscription = 
                _isExecuting
                .DistinctUntilChanged()
                .Subscribe(i => { OnCanExecuteChanged(EventArgs.Empty); });
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            _isExecuting
            .MostRecent(_initialCanExecute)
            .Select(i => !i)
            .First();

        protected virtual void OnCanExecuteChanged(EventArgs args) => CanExecuteChanged?.Invoke(this, args);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IObservable<bool> IsExecuting => _isExecuting.AsObservable();

        public IObservable<TResult> Execute(object parameter) {
            var result = Observable
                .Return(default(TResult))
                .Do((_) => _isExecuting.OnNext(true))
                .Concat(Observable.FromAsync(_execute))
                .Skip(1)
                .Finally(() => _isExecuting.OnNext(false));
            _results.OnNext(result);
            return result;
        }

        void ICommand.Execute(object parameter) => Execute(null);

        protected virtual void Dispose(bool disposing) {
            if (_isDisposed)
                return;
            if (disposing) {
                _isExecutingSubscription.Dispose();
                _isExecuting.Dispose();
                _results.Dispose();
            }
            _isDisposed = true;
        }

        public IDisposable Subscribe(IObserver<IObservable<TResult>> observer) => _results.Subscribe(observer);
    }
}
