using System;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tools {
    public class Property<T> : PropertyBase<T>, IValidate<T> {
        PropertyBase<IValidationDataErrorInfo<T>> _errors;

        public Property(
            T value = default(T),
            IObservable<T> values = null,
            Func<T, IEnumerable> validator = null,
            Func<IObservable<T>, IObservable<IValidationDataErrorInfo<T>>> asyncValidator = null) : base(value) {
            if (values != null) {
                values
                .Subscribe(i => Value = i)
                .AddTo(Disposables);
            }
            _errors = (asyncValidator == null) ? new Validator(this, validator) : new Validator(this, asyncValidator);
            _errors.AddTo(Disposables);
            // order of adding to disposables?
        }

        class Validator : PropertyBase<IValidationDataErrorInfo<T>> {
            public Validator(IObservable<T> items, Func<T, IEnumerable> val) : base() {
                items
                    .Select(i => new ValidationDataErrorInfo<T>(value: i, errors: val?.Invoke(i) ?? null))
                    .Subscribe(i => Value = i)
                    .AddTo(Disposables);
            }

            public Validator(IObservable<T> items, Func<IObservable<T>, IObservable<IValidationDataErrorInfo<T>>> asyncValidator) : base() {
                var valuesPublished = items.Publish().RefCount();
                var validations = asyncValidator(items);
                valuesPublished.GroupJoin(
                    validations,
                    (_) => valuesPublished,
                    (_) => Observable.Empty<ValidationDataErrorInfo<T>>(),
                    (q, a) =>
                        a
                        .Where(j => Equals(q, j.Value))
                        .StartWith(new ValidationDataErrorInfo<T>(value: q, status: ValidationStatus.Running, descendentStatus: null, errors: null, exception: null))
                    )
                    .Concat()
                    .Subscribe(i=>Value=i)
                    .AddTo(Disposables); // or selectmany or merge? observable.switch?
            }
        }

        static IObservable<IValidationDataErrorInfo<T>> Sync(IObservable<T> values, Func<T, IEnumerable> val) {
            return values.Select(i => new ValidationDataErrorInfo<T>(value: i, errors: (val == null) ? new object[] { } : val.Invoke(i)));
        }

        static IObservable<IValidationDataErrorInfo<T>> Generator(IObservable<T> values, Func<IObservable<T>, IObservable<IValidationDataErrorInfo<T>>> validator) {
            var valuesPublished = values.Publish().RefCount();
            var validations = validator(values);
            return valuesPublished.GroupJoin(
                validations,
                (_) => valuesPublished,
                (_) => Observable.Empty<ValidationDataErrorInfo<T>>(),
                (q, a) =>
                    a
                    .Where(j => Equals(q, j.Value))
                    .StartWith(new ValidationDataErrorInfo<T>(value: q, status: ValidationStatus.Running, descendentStatus: null, errors: null, exception: null))
                )
                .Concat(); // or selectmany or merge? observable.switch?
        }

        // MUST set default error to null (no errors) or else async validators have a Null state until they get going
        static ValidationDataErrorInfo<T> _unknownValidation = new ValidationDataErrorInfo<T>(
                value: default(T),
                status: ValidationStatus.Unknown,
                descendentStatus: null,
                errors: null,
                exception: null);

        public IReadOnlyProperty<IValidationDataErrorInfo<T>> Errors => _errors;

        IReadOnlyProperty<IValidationDataErrorInfo> IValidate.Errors => _errors;
    }
}
