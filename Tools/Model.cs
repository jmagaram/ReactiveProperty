using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tools {
    public class Model {
        CompositeDisposable _disposables;
        bool _isDisposed;

        public Model() {
            _isDisposed = false;
            _disposables = new CompositeDisposable();
        }

        protected CompositeDisposable Disposables => _disposables;

        protected void AddToDisposables(params IDisposable[] disposables) {
            foreach (var d in disposables) {
                _disposables.Add(d);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        protected void OnPropertyChanged(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
}
