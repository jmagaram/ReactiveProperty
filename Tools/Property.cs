using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using static Tools.AsyncFunctionStatus;

namespace Tools {
    public class Property<TValue> : PropertyBase<TValue> {
        PropertyBase<IValidationStatus> _errors;
        Func<IObservable<TValue>, IObservable<KeyValuePair<TValue, IValidationStatus>>> _validator;

        public Property(
            TValue defaultValue = default(TValue),
            IObservable<TValue> values = null,
            Func<IObservable<TValue>, IObservable<KeyValuePair<TValue, IValidationStatus>>> asyncValidator = null,
            Func<TValue, IEnumerable> validator = null,
            IObservable<bool> isEnabled = null,
            IObservable<bool> isVisible = null)
            : base(value: defaultValue, values: values) {
            if (asyncValidator != null && validator != null) throw new ArgumentException("Can't have two kinds of validators.");
            Original = new PropertyBase<TValue>(defaultValue);
            HasChanges = new PropertyBase<bool>(values: Observable.CombineLatest(Original, Values, (o, c) => !Equals(o, c)));
            IsVisible = new PropertyBase<bool>(value: true, values: isVisible);
            IsEnabled = new PropertyBase<bool>(value: true, values: isEnabled);
            _validator = asyncValidator != null ? asyncValidator : validator != null ? SyncValidator(validator) : AlwaysValid;
            _errors = new PropertyBase<IValidationStatus>(null);
            AddToDisposables(Original, Errors, HasChanges, IsVisible, IsEnabled);
            Observable
                .CombineLatest(this.Values, _validator(this.Values), (v, r) => new { Value = v, ValidatedValue = r.Key, ErrorStatus = r.Value })
                .Where(i => i.ErrorStatus.Status != InProgress)
                .Select(i => {
                    if (!Equals(i.Value, i.ValidatedValue)) {
                        return new ValidationStatus(status: InProgress);
                    }
                    else {
                        switch (i.ErrorStatus.Status) {
                            case InProgress: throw new NotImplementedException("This code should never be executed.");
                            case Canceled: return new ValidationStatus(status: Canceled);
                            case Faulted: return new ValidationStatus(status: Faulted, exception: i.ErrorStatus.Exception);
                            case Completed: return new ValidationStatus(status: Completed, errors: i.ErrorStatus.Errors, hasErrors: i.ErrorStatus.HasErrors);
                            default: throw new NotImplementedException();
                        }
                    }
                })
                .Subscribe(i => _errors.Value = i)
                .AddTo(Disposables);
        }

        static Func<IObservable<TValue>, IObservable<KeyValuePair<TValue, IValidationStatus>>> SyncValidator(Func<TValue, IEnumerable> synchronousValidator) {
            return (IObservable<TValue> values) =>
                     values
                     .Select(i => new KeyValuePair<TValue, IValidationStatus>(
                         key: i,
                         value: new ValidationStatus(status: Completed, exception: null, errors: synchronousValidator(i))));
        }

        static IObservable<KeyValuePair<TValue, IValidationStatus>> AlwaysValid(IObservable<TValue> values) {
            return
                values
                .Select(i => new KeyValuePair<TValue, IValidationStatus>(
                 key: i,
                 value: new ValidationStatus(status: Completed, errors: Enumerable.Empty<object>(), hasErrors: false, exception: null)));
        }

        public IReadOnlyProperty<IValidationStatus> Errors => _errors;

        public void ForceValidation() => ResubmitCurrentValue();

        public PropertyBase<bool> IsVisible { get; }
        public PropertyBase<bool> IsEnabled { get; }
        private PropertyBase<TValue> Original { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
    }
}
