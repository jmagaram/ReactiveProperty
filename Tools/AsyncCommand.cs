using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    public class AsyncCommand<TResult> : ICommand, IObservable<IObservable<TResult>>, IDisposable {
        bool _isDisposed;
        Func<CancellationToken, Task<TResult>> _execute;
        BehaviorSubject<bool> _isExecuting;
        IObservable<bool> _canExecute;
        bool _latestCanExecute = false;
        IDisposable _isExecutingSubscription;
        IDisposable _canExecuteSubscription;
        Subject<IObservable<TResult>> _results;

        public AsyncCommand(Func<CancellationToken, Task<TResult>> execute, IObservable<bool> canExecute = null, bool initialCanExecute = true) {
            var logger = new DelegateLogger(i => Debug.WriteLine(i));
            _isDisposed = false;
            _isExecuting = new BehaviorSubject<bool>(false);
            _canExecute = Observable.CombineLatest(canExecute?.StartWith(initialCanExecute) ?? Observable.Return(true), _isExecuting, (canEx, isEx) => canEx && !isEx).Publish().RefCount();
            _canExecuteSubscription =
                _canExecute
                .DistinctUntilChanged()
                .Subscribe(i => {
                    _latestCanExecute = i;
                    OnCanExecuteChanged(EventArgs.Empty);
                });
            _results = new Subject<IObservable<TResult>>();
            _execute = execute;
            _isExecutingSubscription =
                _isExecuting
                .DistinctUntilChanged()
                .Subscribe(i => { OnCanExecuteChanged(EventArgs.Empty); });
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _latestCanExecute;

        protected virtual void OnCanExecuteChanged(EventArgs args) => CanExecuteChanged?.Invoke(this, args);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IObservable<bool> IsExecuting => _isExecuting.AsObservable();

        public IObservable<TResult> Execute(object parameter) {
            //Func<CancellationToken, object, Task<TResult>> provided = null;
            //Func<CancellationToken, Task<TResult>> needed = (token) => provided(token,parameter);
            // For value types, ensure that null is coerced to a sensible default
            //if (parameter == null) {
            //    parameter = default(TParameter);
            //}
            //if (parameter != null && !(parameter is TParameter)) {
            //    throw new ArgumentException(
            //        paramName: nameof(parameter),
            //        message: $"Must be of type {typeof(TParameter).FullName} but received type {parameter.GetType().FullName}.");
            //}
            //Execute((TParameter)parameter)
            //    .Catch(Observable.Empty<TResult>())
            //    .Subscribe();
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
                _canExecuteSubscription.Dispose();
            }
            _isDisposed = true;
        }

        public IDisposable Subscribe(IObserver<IObservable<TResult>> observer) => _results.Subscribe(observer);
    }
}
