using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Tools {
    public class Property<TValue, TError> : EditableProperty<TValue>, IRevertible, IValidate<TError> {
        public Property(TValue value = default(TValue), Func<TValue, IEnumerable<TError>> validator = null, bool isEnabled = true, bool isVisible = true, IObservable<TValue> values = null)
            : base(value) {
            Value = value;
            Original = new EditableProperty<TValue>(value);
            Errors = new CalculatedProperty<TError[]>(validator == null ? Observable.Return(new TError[] { }) : Values.Select(i => validator(i).ToArray()));
            HasErrors = new CalculatedProperty<bool>(Errors.Select(i => i != null && i.Length > 0));
            HasChanges = new CalculatedProperty<bool>(Observable.CombineLatest(Original, Values, (o, c) => !Equals(o, c)));
            IsVisible = new EditableProperty<bool>(true);
            IsEnabled = new EditableProperty<bool>(true);
            AddToDisposables(Original, Errors, HasErrors, HasChanges, IsVisible, IsEnabled);
            if (values != null) {
                values.Subscribe(i => Value = i).AddTo(Disposables);
            }
        }

        public IEditableProperty<bool> IsVisible { get; }
        public IEditableProperty<bool> IsEnabled { get; }
        private EditableProperty<TValue> Original { get; }
        public IReadOnlyProperty<TError[]> Errors { get; }
        public IReadOnlyProperty<bool> HasErrors { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
    }
}
