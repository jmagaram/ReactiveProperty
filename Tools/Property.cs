using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Tools {
    public class Property<TValue, TError> : PropertyBase<TValue> {
        PropertyBase<IValidationStatus<TValue, TError>> _errors;
        Func<IObservable<TValue>, IObservable<IValidationStatus<TValue, TError>>> _validator;

        public Property(
            TValue defaultValue = default(TValue),
            IObservable<TValue> values = null,
            Func<IObservable<TValue>, IObservable<IValidationStatus<TValue, TError>>> asyncValidator = null,
            Func<TValue, IEnumerable<TError>> validator = null,
            IObservable<bool> isEnabled = null,
            IObservable<bool> isVisible = null)
            : base(value: defaultValue, values: values) {
            if (asyncValidator != null && validator != null) throw new ArgumentException("Can't have two kinds of validators.");
            Original = new PropertyBase<TValue>(defaultValue);
            HasChanges = new PropertyBase<bool>(values: Observable.CombineLatest(Original, Values, (o, c) => !Equals(o, c)));
            IsVisible = new PropertyBase<bool>(value: true, values: isVisible);
            IsEnabled = new PropertyBase<bool>(value: true, values: isEnabled);
            _validator =
                asyncValidator != null ? asyncValidator :
                validator != null ? SyncValidator(validator)
                : AlwaysValid;
            _errors = new PropertyBase<IValidationStatus<TValue, TError>>(null);
            AddToDisposables(Original, Errors, HasChanges, IsVisible, IsEnabled);
            Observable
                .CombineLatest(this.Values, _validator(this.Values), (v, r) => new { Value = v, Result = r })
                .Where(i => i.Result.Status != AsyncFunctionStatus.InProgress)
                .Select(i => {
                    if (!Equals(i.Value, i.Result.Value)) {
                        return new ValidationStatus<TValue, TError>(
                            status: AsyncFunctionStatus.InProgress,
                            value: i.Value);
                    }
                    else {
                        switch (i.Result.Status) {
                            case AsyncFunctionStatus.InProgress:
                                throw new NotImplementedException("This code should never be executed.");
                            case AsyncFunctionStatus.Canceled:
                                return new ValidationStatus<TValue, TError>(
                                    value: i.Result.Value,
                                    status: AsyncFunctionStatus.Canceled);
                            case AsyncFunctionStatus.Faulted:
                                return new ValidationStatus<TValue, TError>(
                                    status: AsyncFunctionStatus.Faulted,
                                    value: i.Result.Value,
                                    exception: i.Result.Exception);
                            case AsyncFunctionStatus.Completed:
                                return new ValidationStatus<TValue, TError>(
                                    status: AsyncFunctionStatus.Completed,
                                    value: i.Result.Value,
                                    errors: i.Result.Errors,
                                    hasErrors: i.Result.HasErrors);
                            default:
                                throw new NotImplementedException();
                        }
                    }
                })
                .Subscribe(i => _errors.Value = i)
                .AddTo(Disposables);
        }

        static Func<IObservable<TValue>, IObservable<IValidationStatus<TValue, TError>>> SyncValidator(Func<TValue, IEnumerable<TError>> synchronousValidator) {
            return (IObservable<TValue> values) =>
                     values
                     .Select(i => new ValidationStatus<TValue, TError>(
                         status: AsyncFunctionStatus.Completed,
                         value: i,
                         exception: null,
                         errors: synchronousValidator(i)));
        }

        static IObservable<IValidationStatus<TValue, TError>> AlwaysValid(IObservable<TValue> values) {
            return values.Select(i => new ValidationStatus<TValue, TError>(
                status: AsyncFunctionStatus.Completed,
                value: i,
                errors: Enumerable.Empty<TError>(),
                hasErrors: false,
                exception: null));
        }

        public IReadOnlyProperty<IValidationStatus<TValue, TError>> Errors => _errors;

        public void ForceValidation() => ResubmitCurrentValue();

        public PropertyBase<bool> IsVisible { get; }
        public PropertyBase<bool> IsEnabled { get; }
        private PropertyBase<TValue> Original { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
    }
}
