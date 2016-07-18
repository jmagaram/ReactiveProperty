using System;
using System.Collections;
using System.Reactive.Linq;

namespace Tools {
    public class Property<TValue> : PropertyBase<TValue>, IValidate<TValue>, IRevertible {
        public Property(
            TValue defaultValue = default(TValue),
            IObservable<TValue> values = null,
            Func<IObservable<TValue>, IObservable<ValidationDataErrorInfo<TValue>>> asyncValidator = null,
            Func<TValue, IEnumerable> validator = null,
            IObservable<bool> isEnabled = null,
            IObservable<bool> isVisible = null)
            : base(value: defaultValue, values: values) {
            if (asyncValidator != null && validator != null) throw new ArgumentException("Can't have two kinds of validators.");
            Original = new PropertyBase<TValue>(defaultValue);
            HasChanges = new PropertyBase<bool>(values: Observable.CombineLatest(Original, Values, (o, c) => !Equals(o, c)));
            IsVisible = new PropertyBase<bool>(value: true, values: isVisible);
            IsEnabled = new PropertyBase<bool>(value: true, values: isEnabled);
            if (asyncValidator == null) {
                Errors = new ValidatorProperty<TValue>(Values, validator);
            }
            else {
                Errors = new AsyncValidatorProperty<TValue>(Values, asyncValidator);
            }
            AddToDisposables(Original, Errors, HasChanges, IsVisible, IsEnabled);
        }

        public IReadOnlyProperty<IValidationDataErrorInfo<TValue>> Errors { get; }
        IReadOnlyProperty<IValidationDataErrorInfo> IValidate.Errors => Errors;
        public PropertyBase<bool> IsVisible { get; }
        public PropertyBase<bool> IsEnabled { get; }
        private PropertyBase<TValue> Original { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
    }
}
