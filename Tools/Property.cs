using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    public class Property<TValue, TError> : IObservable<TValue>, IValidationErrorData<TError>, ITrackChanges, INotifyPropertyChanged, IDisposable {
        BehaviorSubject<State> _state;
        Func<TValue, IEnumerable<TError>> _validator;
        bool _disposed = false;

        public Property(TValue value = default(TValue), Func<TValue, IEnumerable<TError>> validator = null, bool isVisible = true, bool isEnabled = true) {
            _validator = validator ?? ((v) => new TError[] { });
            _state = new BehaviorSubject<State>(new State(
                original: value,
                current: value,
                errors: _validator(value),
                isEnabled: isEnabled,
                isVisible: isVisible));
            Observable
                .Zip(_state, _state.Skip(1), (b, a) => new { Before = b, After = a })
                .Subscribe(i => {
                    if (!Equals(i.Before.Value, i.After.Value)) {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                    }
                    if (!Equals(i.Before.HasErrors, i.After.HasErrors)) {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasErrors)));
                    }
                    if (!Equals(i.Before.HasChanges, i.After.HasChanges)) {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasChanges)));
                    }
                    if (!i.Before.Errors.SequenceEqual(i.After.Errors)) {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Errors)));
                    }
                    if (!Equals(i.Before.IsEnabled, i.After.IsEnabled)) {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEnabled)));
                    }
                    if (!Equals(i.Before.IsVisible, i.After.IsVisible)) {
                        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsVisible)));
                    }
                });
        }

        class State : IPropertyState<TValue, TError> {
            public State(TValue original, TValue current, IEnumerable<TError> errors, bool isEnabled, bool isVisible) {
                Original = original;
                Value = current;
                Errors = errors.ToArray();
                IsEnabled = isEnabled;
                IsVisible = isVisible;
            }

            public TValue Original { get; }
            public TValue Value { get; }
            public bool HasChanges => !Equals(Original, Value);
            public bool HasErrors => Errors.Any();
            public TError[] Errors { get; }
            public bool IsEnabled { get; }
            public bool IsVisible { get; }
        }

        public TError[] Errors => _state.Value.Errors;

        public bool HasErrors => _state.Value.HasErrors;

        public IObservable<IPropertyState<TValue, TError>> PropertyState => _state;

        public TValue Value
        {
            get { return _state.Value.Value; }
            set
            {
                if (!Equals(value, _state.Value.Value)) {
                    _state.OnNext(new State(
                        original: _state.Value.Original,
                        current: value,
                        errors: _validator(value),
                        isEnabled: _state.Value.IsEnabled,
                        isVisible: _state.Value.IsVisible));
                }
            }
        }

        private TValue Original
        {
            get { return _state.Value.Original; }
            set
            {
                if (!Equals(value, _state.Value.Original)) {
                    _state.OnNext(new State(
                        original: value,
                        current: _state.Value.Value,
                        errors: _state.Value.Errors,
                        isEnabled: _state.Value.IsEnabled,
                        isVisible: _state.Value.IsVisible));
                }
            }
        }

        public bool IsEnabled
        {
            get { return _state.Value.IsEnabled; }
            set
            {
                if (!Equals(value, _state.Value.IsEnabled)) {
                    _state.OnNext(new State(
                        original: _state.Value.Original,
                        current: _state.Value.Value,
                        errors: _state.Value.Errors,
                        isEnabled: value,
                        isVisible:_state.Value.IsVisible));
                }
            }
        }

        public bool IsVisible
        {
            get { return _state.Value.IsVisible; }
            set
            {
                if (!Equals(value, _state.Value.IsEnabled)) {
                    _state.OnNext(new State(
                        original: _state.Value.Original,
                        current: _state.Value.Value,
                        errors: _state.Value.Errors,
                        isEnabled: _state.Value.IsEnabled,
                        isVisible: value));
                }
            }
        }

        public bool HasChanges => _state.Value.HasChanges;

        public void AcceptChanges() => Original = Value;

        public void RejectChanges() => Value = Original;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

        public IDisposable Subscribe(IObserver<TValue> observer) => _state.Select(i => i.Value).Subscribe(observer);

        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;
            if (disposing) {
                _state.Dispose();
            }
            _disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
