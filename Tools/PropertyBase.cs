using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tools {
    public class PropertyBase<T> : IReadOnlyProperty<T> {
        BehaviorSubject<T> _subject;
        CompositeDisposable _disposables;
        bool _isDisposed;

        public PropertyBase(T value = default(T)) {
            _isDisposed = false;
            _disposables = new CompositeDisposable(_subject);
            _subject = new BehaviorSubject<T>(value);
            _subject
                .Subscribe(i => { OnPropertyChanged(nameof(Value)); })
                .AddTo(_disposables);
        }

        public T Value
        {
            get { return _subject.Value; }
            protected set
            {
                if (!Equals(value, _subject.Value)) {
                    _subject.OnNext(value);
                }
            }
        }

        protected IObservable<T> Values => _subject.AsObservable();

        protected CompositeDisposable Disposables => _disposables;

        protected void AddToDisposables(params IDisposable[] disposables) {
            foreach (var d in disposables) {
                _disposables.Add(d);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString() => Value.ToString();

        public static implicit operator T(PropertyBase<T> p) => p.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

        protected virtual void Dispose(bool disposing) {
            if (_isDisposed)
                return;
            if (disposing) {
                _disposables.Dispose();
            }
            _isDisposed = true;
        }

        private void OnPropertyChanged(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
}
