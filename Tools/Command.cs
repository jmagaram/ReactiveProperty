using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Threading;

namespace Tools {
    public class Command<TParameter, TResult> : ICommand, IObservable<TResult>, IDisposable {
        IObservable<bool> _canExecute;
        IDisposable _canExecuteSubscription;
        Subject<Exception> _exceptions;
        Subject<ExecutionProgress> _progress;
        ISubject<ExecutionProgress, ExecutionProgress> _progressSynchronized;
        IObservable<bool> _isExecuting;
        IObservable<TResult> _results;
        Func<TParameter, Task<TResult>> _execute;
        bool _disposed = false;

        public Command(Func<TParameter, Task<TResult>> execute, IObservable<bool> canExecute) {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));
            _exceptions = new Subject<Exception>();
            _execute = execute;
            _progress = new Subject<ExecutionProgress>();
            _progressSynchronized = Subject.Synchronize(_progress);
            _isExecuting =
                _progressSynchronized
                .Select(i => i.State == ExecutionState.Started)
                .StartWith(false)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
            _canExecute =
                canExecute
                .Catch<bool, Exception>((ex) => {
                    _exceptions.OnNext(ex);
                    return Observable.Return(false);
                })
                .StartWith(false)
                .CombineLatest(_isExecuting, (canEx, isEx) => canEx && !isEx)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
            _canExecuteSubscription =
                _canExecute
                //.ObserveOn(DefaultScheduler.Instance) // UI hangs without this, maybe just if task uses background thread, like Task.Delay?
                .Subscribe((i) => OnCanExecuteChanged(EventArgs.Empty));
            _results =
                _progressSynchronized
                .Where(i => i.State == ExecutionState.GeneratedResult)
                .Select(i => i.Result);
        }

        public IObservable<TResult> Execute(TParameter parameter = default(TParameter)) {
            return Observable
                .Defer(() => {
                    _progressSynchronized.OnNext(ExecutionProgress.Started());
                    return Observable.FromAsync(() => _execute(parameter));
                })
                .Do(onNext: result => _progressSynchronized.OnNext(ExecutionProgress.CreatedResult(result)))
                .Catch<TResult, Exception>(
                    ex => {
                        _exceptions.OnNext(ex);
                        return Observable.Throw<TResult>(ex);
                    })
                .Finally(() => { _progressSynchronized.OnNext(ExecutionProgress.Finished()); })
                .Publish()
                .RefCount();
        }

        public event EventHandler CanExecuteChanged;

        public IDisposable Subscribe(IObserver<TResult> observer) => _results.Subscribe(observer);

        bool ICommand.CanExecute(object parameter) => CanExecute.FirstAsync().Wait();

        void ICommand.Execute(object parameter) {
            // For value types, ensure that null is coerced to a sensible default
            if (parameter == null) {
                parameter = default(TParameter);
            }
            if (parameter != null && !(parameter is TParameter)) {
                throw new ArgumentException(
                    paramName: nameof(parameter),
                    message: $"Must be of type {typeof(TParameter).FullName} but received type {parameter.GetType().FullName}.");
            }
            Execute((TParameter)parameter)
                .Catch(Observable.Empty<TResult>())
                .Subscribe();
        }

        public IObservable<bool> CanExecute => _canExecute;
        public IObservable<bool> IsExecuting => _isExecuting;
        public IObservable<Exception> Exceptions => _exceptions.AsObservable();

        protected virtual void OnCanExecuteChanged(EventArgs args) => CanExecuteChanged?.Invoke(this, args);

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;
            if (disposing) {
                _exceptions.Dispose();
                _progress.Dispose();
                _canExecuteSubscription.Dispose();
            }
            _disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        enum ExecutionState {
            Started,
            GeneratedResult,
            Finished
        }

        class ExecutionProgress {
            static ExecutionProgress _started = new ExecutionProgress(ExecutionState.Started, default(TResult));
            static ExecutionProgress _finished = new ExecutionProgress(ExecutionState.Finished, default(TResult));

            static public ExecutionProgress CreatedResult(TResult result) => new ExecutionProgress(ExecutionState.GeneratedResult, result);
            static public ExecutionProgress Started() => _started;
            static public ExecutionProgress Finished() => _finished;

            ExecutionProgress(ExecutionState state, TResult result) {
                State = state;
                Result = result;
            }

            public ExecutionState State { get; }
            public TResult Result { get; }
        }
    }
}
