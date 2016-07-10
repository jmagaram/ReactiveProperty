using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using static Tools.ValidationStatus;

namespace Tools {
    public class Property<TValue> : PropertyBase<TValue>, IValidate {
        PropertyBase<ValidationDataErrorInfo> _errors;
        Func<IObservable<TValue>, IObservable<KeyValuePair<TValue, ValidationDataErrorInfo>>> _validator;

        public Property(
            TValue defaultValue = default(TValue),
            IObservable<TValue> values = null,
            Func<IObservable<TValue>, IObservable<KeyValuePair<TValue, ValidationDataErrorInfo>>> asyncValidator = null,
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
            // MUST set default error to null (no errors) or else async validators have a Null state until they get going
            // or maybe better approach is to set it to unknown?
            _errors = new PropertyBase<ValidationDataErrorInfo>(new ValidationDataErrorInfo(ValidationStatus.Unknown,descendentStatus:null,errors:null,exception:null)); 
            AddToDisposables(Original, Errors, HasChanges, IsVisible, IsEnabled);
            Observable
                .CombineLatest(this.Values, _validator(this.Values), (v, r) => new { LatestValue = v, ValidatedValue = r.Key, ErrorStatus = r.Value })
                .Select(i => {
                    if (!Equals(i.LatestValue, i.ValidatedValue) || i.ErrorStatus.Status == Running) {
                        return new ValidationDataErrorInfo(status: Running, descendentStatus: null, errors: null, exception: null);
                    }
                    else {
                        switch (i.ErrorStatus.Status) {
                            case Running: throw new NotImplementedException("This code should never be executed.");
                            case Canceled: return new ValidationDataErrorInfo(status: Canceled, descendentStatus: null, errors: null, exception: null);
                            case Faulted: return new ValidationDataErrorInfo(status: Faulted, descendentStatus: null, errors: null, exception: i.ErrorStatus.Exception);
                            case IsValid: return new ValidationDataErrorInfo(status: IsValid, descendentStatus: null, errors: null, exception: null);
                            case HasErrors: return new ValidationDataErrorInfo(status: HasErrors, descendentStatus: null, errors: i.ErrorStatus.Errors, exception: null);
                            default: throw new NotImplementedException(); // The result might have more than one bit set.
                        }
                    }
                })
                .Subscribe(i => _errors.Value = i)
                .AddTo(Disposables);
        }

        static Func<IObservable<TValue>, IObservable<KeyValuePair<TValue, ValidationDataErrorInfo>>> SyncValidator(Func<TValue, IEnumerable> synchronousValidator) {
            return (IObservable<TValue> values) =>
                     values
                     .Select(i => new KeyValuePair<TValue, ValidationDataErrorInfo>(key: i, value: new ValidationDataErrorInfo(errors: synchronousValidator(i))));
        }

        static IObservable<KeyValuePair<TValue, ValidationDataErrorInfo>> AlwaysValid(IObservable<TValue> values) {
            return
                values
                .Select(i => new KeyValuePair<TValue, ValidationDataErrorInfo>(
                 key: i,
                 value: new ValidationDataErrorInfo(status: IsValid, errors: Enumerable.Empty<object>(), descendentStatus: null, exception: null)));
        }

        public IReadOnlyProperty<ValidationDataErrorInfo> Errors => _errors;

        public void ForceValidation() => ResubmitCurrentValue();

        public PropertyBase<bool> IsVisible { get; }
        public PropertyBase<bool> IsEnabled { get; }
        private PropertyBase<TValue> Original { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
    }
}
