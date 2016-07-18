using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools {
    public static class IObservableExtensions {
        public static IObservable<TResult> CombineWithPrevious<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector) {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (previous, current) => Tuple.Create(previous.Item2, current))
                .Select(t => resultSelector(t.Item1, t.Item2));
        }


        private static int _observableIndex = 0;

        public static IObservable<T> Log<T>(this IObservable<T> source, ILogger logger, string name) {
            return Observable.Create<T>(o => {
                var index = Interlocked.Increment(ref _observableIndex);
                var label = $"{index:0000}{name}";
                logger.Log($"{label}.Subscribe()");
                var disposed = Disposable.Create(() => logger.Log($"{label}.Dispose()"));
                var subscription = source
                    .Do(
                        x => logger.Log($"{label}.OnNext({x?.ToString() ?? "null"})"),
                        ex => logger.Log($"{label}.OnError({ex})"),
                        () => logger.Log($"{label}.OnCompleted()")
                    )
                    .Subscribe(o);
                return new CompositeDisposable(subscription, disposed);
            });
        }
    }
}
