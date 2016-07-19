using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tools {
    public class PropertyBase<T> : Model, IEditableProperty<T> {
        BehaviorSubject<T> _subject;

        public PropertyBase(T value = default(T)) {
            _subject = new BehaviorSubject<T>(value);
            _subject.AddTo(Disposables);
            _subject
                .DistinctUntilChanged()
                .Subscribe(i => OnPropertyChanged(nameof(Value))) // ObserveOn?
                .AddTo(Disposables);
        }

        public T Value
        {
            get { return _subject.Value; }
            set { _subject.OnNext(value); }
        }

        protected IObservable<T> Values => _subject.AsObservable();

        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public override string ToString() => Value.ToString();

        public static implicit operator T(PropertyBase<T> p) => p.Value;
    }

}
