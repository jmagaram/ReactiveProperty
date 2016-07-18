using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tools {
    public class PropertyBase<T> : Model, IReadOnlyProperty<T> {
        BehaviorSubject<T> _subject;

        public PropertyBase(T value = default(T), IObservable<T> values = null) {
            _subject = new BehaviorSubject<T>(value);
            _subject.AddTo(Disposables);
            _subject
                .DistinctUntilChanged()
                .Subscribe(i => OnPropertyChanged(nameof(Value))) // ObserveOn?
                .AddTo(Disposables);
            if (values != null) {
                values
                .Subscribe(i => Value = i)
                .AddTo(Disposables);
            }
        }

        public T Value
        {
            get { return _subject.Value; }
            set { _subject.OnNext(value); }
        }

        /// <summary>
        /// Forces error validation to happen again in case it was cancelled or faulted.
        /// </summary>
        protected void ResubmitCurrentValue() => _subject.OnNext(Value);

        protected IObservable<T> Values => _subject.AsObservable();

        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public override string ToString() => Value.ToString();

        public static implicit operator T(PropertyBase<T> p) => p.Value;
    }
}
