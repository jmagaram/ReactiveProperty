using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tools {
    public class PropertyBase<T> : Model, IReadOnlyProperty<T> {
        BehaviorSubject<T> _subject;

        public PropertyBase(T value = default(T), IObservable<T> values = null) {
            _subject = new BehaviorSubject<T>(value);
            _subject.AddTo(Disposables);
            _subject
                .Subscribe(i => { OnPropertyChanged(nameof(Value)); })
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
            set
            {
                if (!Equals(value, _subject.Value)) {
                    _subject.OnNext(value);
                }
            }
        }

        protected void ResubmitCurrentValue() => _subject.OnNext(Value);

        protected IObservable<T> Values => _subject.AsObservable();

        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public override string ToString() => Value.ToString();

        public static implicit operator T(PropertyBase<T> p) => p.Value;
    }
}
