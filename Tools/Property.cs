using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Tools {
    public class Property<T> : INotifyPropertyChanged, IDisposable {
        T _latest;
        BehaviorSubject<Delta<T>> _changing;
        BehaviorSubject<Delta<T>> _changed;
        bool _disposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public Property(T value = default(T)) {
            Delta<T> delta = new Delta<T>(before: value, after: value);
            _changing = new BehaviorSubject<Delta<T>>(delta);
            _changed = new BehaviorSubject<Delta<T>>(delta);
            _latest = value;
        }

        public T Value
        {
            get { return _latest; }
            set
            {
                if (!Equals(_latest, value)) {
                    Delta<T> delta = new Delta<T>(before: _latest, after: value);
                    _changing.OnNext(delta);
                    _latest = value;
                    _changed.OnNext(delta);
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public IObservable<Delta<T>> Changing => _changing;

        public IObservable<Delta<T>> Changed => _changed;

        public IObservable<T> Values => _changed.Select(i => i.After);

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;
            if (disposing) {
                _changed.Dispose();
                _changing.Dispose();
            }
            _disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
