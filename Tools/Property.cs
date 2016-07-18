using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using static Tools.ValidationStatus;

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

            IObservable<ValidationDataErrorInfo<TValue>> errorValues = null;
            if (validator == null && asyncValidator == null) {
                errorValues = Values.Select(i => new ValidationDataErrorInfo<TValue>(value: i, errors: null));
            }
            else if (validator != null) {
                errorValues = Values.Select(i => new ValidationDataErrorInfo<TValue>(value: i, errors: validator(i)));
            }
            else {
                var valuesPublished = Values.Publish().RefCount();
                var validations = asyncValidator(Values);
                errorValues = valuesPublished.GroupJoin(
                    validations,
                    (_) => valuesPublished,
                    (_) => Observable.Empty<ValidationDataErrorInfo<TValue>>(),
                    (q, a) =>
                        a
                        .Where(j => Equals(q, j.Value))
                        .StartWith(new ValidationDataErrorInfo<TValue>(value: q, status: Running, descendentStatus: null, errors: null, exception: null))
                    )
                    .Concat(); // or selectmany or merge? observable.switch?
            }
            Errors = new PropertyBase<ValidationDataErrorInfo<TValue>>(value: new ValidationDataErrorInfo<TValue>(
                value: default(TValue), status: ValidationStatus.Unknown, descendentStatus: null, errors: null, exception: null),
                values: errorValues);
            // MUST set default error to null (no errors) or else async validators have a Null state until they get going

            AddToDisposables(Original, Errors, HasChanges, IsVisible, IsEnabled);
        }

        public IReadOnlyProperty<IValidationDataErrorInfo<TValue>> Errors { get; }
        public PropertyBase<bool> IsVisible { get; }
        public PropertyBase<bool> IsEnabled { get; }
        private PropertyBase<TValue> Original { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
    }
}
