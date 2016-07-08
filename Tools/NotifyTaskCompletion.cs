using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Tools {
    public interface INotifyTaskCompletion : INotifyPropertyChanged {
        Task Task { get; }
        Task TaskCompleted { get; }
        TaskStatus Status { get; }
        bool IsCompleted { get; }
        bool IsNotCompleted { get; }
        bool IsSuccessfullyCompleted { get; }
        bool IsCanceled { get; }
        bool IsFaulted { get; }
        AggregateException Exception { get; }
        Exception InnerException { get; }
        string ErrorMessage { get; }
    }

    public interface INotifyTaskCompletion<TResult> : INotifyTaskCompletion {
        new Task<TResult> Task { get; }
        TResult Result { get; }
    }

    public static class NotifyTaskCompletion {
        public static INotifyTaskCompletion Create(Task task) => new NotifyTaskCompletionImplementation(task);

        public static INotifyTaskCompletion<TResult> Create<TResult>(Task<TResult> task) => new NotifyTaskCompletionImplementation<TResult>(task);

        public static INotifyTaskCompletion Create(Func<Task> asyncAction) => Create(asyncAction());

        public static INotifyTaskCompletion<TResult> Create<TResult>(Func<Task<TResult>> asyncAction) => Create(asyncAction());

        private sealed class NotifyTaskCompletionImplementation : INotifyTaskCompletion {
            public NotifyTaskCompletionImplementation(Task task) {
                Task = task;
                if (task.IsCompleted) {
                    TaskCompleted = System.Threading.Tasks.Task.FromResult(true);
                    return;
                }
                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                TaskCompleted = task.ContinueWith(t => {
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(IsNotCompleted));
                    if (t.IsCanceled) {
                        OnPropertyChanged(nameof(IsCanceled));
                    }
                    else if (t.IsFaulted) {
                        OnPropertyChanged(nameof(IsFaulted));
                        OnPropertyChanged(nameof(Exception));
                        OnPropertyChanged(nameof(InnerException));
                        OnPropertyChanged(nameof(ErrorMessage));
                    }
                    else {
                        OnPropertyChanged(nameof(IsSuccessfullyCompleted));
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
            }

            public Task Task { get; }
            public Task TaskCompleted { get; }
            public TaskStatus Status => Task.Status;
            public bool IsCompleted => Task.IsCompleted;
            public bool IsNotCompleted => !Task.IsCompleted;
            public bool IsSuccessfullyCompleted => Task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion;
            public bool IsCanceled => Task.IsCanceled;
            public bool IsFaulted => Task.IsFaulted;
            public AggregateException Exception => Task.Exception;
            public Exception InnerException => (Exception == null) ? null : Exception.InnerException;
            public string ErrorMessage => (InnerException == null) ? null : InnerException.Message;

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class NotifyTaskCompletionImplementation<TResult> : INotifyTaskCompletion<TResult> {
            public NotifyTaskCompletionImplementation(Task<TResult> task) {
                Task = task;
                if (task.IsCompleted) {
                    TaskCompleted = System.Threading.Tasks.Task.FromResult(true);
                    return;
                }
                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                TaskCompleted = task.ContinueWith(t => {
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(IsNotCompleted));
                    if (t.IsCanceled) {
                        OnPropertyChanged(nameof(IsCanceled));
                    }
                    else if (t.IsFaulted) {
                        OnPropertyChanged(nameof(IsFaulted));
                        OnPropertyChanged(nameof(Exception));
                        OnPropertyChanged(nameof(InnerException));
                        OnPropertyChanged(nameof(ErrorMessage));
                    }
                    else {
                        OnPropertyChanged(nameof(IsSuccessfullyCompleted));
                        OnPropertyChanged(nameof(Result));
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
            }

            public Task<TResult> Task { get; }
            Task INotifyTaskCompletion.Task { get { return Task; } }
            public Task TaskCompleted { get; }
            public TResult Result => (Task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion) ? Task.Result : default(TResult);
            public TaskStatus Status => Task.Status;
            public bool IsCompleted => Task.IsCompleted;
            public bool IsNotCompleted => !Task.IsCompleted;
            public bool IsSuccessfullyCompleted => Task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion;
            public bool IsCanceled => Task.IsCanceled;
            public bool IsFaulted => Task.IsFaulted;
            public AggregateException Exception => Task.Exception;
            public Exception InnerException => (Exception == null) ? null : Exception.InnerException;
            public string ErrorMessage => (InnerException == null) ? null : InnerException.Message;

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
