using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    /// <summary>
    /// From https://msdn.microsoft.com/magazine/dn630647.aspx
    /// From https://github.com/StephenCleary
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class AsyncCommand<TResult> : AsyncCommandBase, INotifyPropertyChanged {
        readonly Func<CancellationToken, Task<TResult>> _command;
        readonly CancelAsyncCommand _cancelCommand;
        INotifyTaskCompletion<TResult> _execution;
        IObservable<bool> _canExecute;
        bool _latestCanExecute;

        public AsyncCommand(Func<CancellationToken, Task<TResult>> command, IObservable<bool> canExecute = null) {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _command = command;
            _canExecute = canExecute ?? Observable.Return<bool>(true);
            _latestCanExecute = true;
            _canExecute
                .StartWith(true)
                .DistinctUntilChanged()
                .Subscribe(i => {
                    _latestCanExecute = i;
                    RaiseCanExecuteChanged();
                });
            _cancelCommand = new CancelAsyncCommand();
        }

        public override bool CanExecute(object parameter) => _latestCanExecute && (Execution == null || Execution.IsCompleted);

        public override async Task ExecuteAsync(object parameter) {
            _cancelCommand.NotifyCommandStarting();
            Execution = NotifyTaskCompletion.Create<TResult>(_command(_cancelCommand.Token));
            RaiseCanExecuteChanged();
            await Execution.TaskCompleted;
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }

        public ICommand CancelCommand => _cancelCommand;

        public INotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            private set
            {
                _execution = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private sealed class CancelAsyncCommand : ICommand {
            private CancellationTokenSource _cts = new CancellationTokenSource();
            private bool _commandExecuting;

            public CancellationToken Token => _cts.Token;

            public void NotifyCommandStarting() {
                _commandExecuting = true;
                if (!_cts.IsCancellationRequested)
                    return;
                _cts = new CancellationTokenSource();
                RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished() {
                _commandExecuting = false;
                RaiseCanExecuteChanged();
            }

            bool ICommand.CanExecute(object parameter) => _commandExecuting && !_cts.IsCancellationRequested;

            void ICommand.Execute(object parameter) {
                _cts.Cancel();
                RaiseCanExecuteChanged();
            }

            public event EventHandler CanExecuteChanged;

            private void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public static class AsyncCommand {
        public static AsyncCommand<object> Create(Func<Task> command) => 
            new AsyncCommand<object>(async _ => { await command(); return null; });

        public static AsyncCommand<TResult> Create<TResult>(Func<Task<TResult>> command) => 
            new AsyncCommand<TResult>(_ => command());

        public static AsyncCommand<object> Create(Func<CancellationToken, Task> command) => 
            new AsyncCommand<object>(async token => { await command(token); return null; });

        public static AsyncCommand<TResult> Create<TResult>(Func<CancellationToken, Task<TResult>> command) => 
            new AsyncCommand<TResult>(command);
    }
}
