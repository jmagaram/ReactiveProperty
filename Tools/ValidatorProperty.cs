using System;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;

namespace Tools {
    public class ValidatorProperty<T> : PropertyBase<ValidationDataErrorInfo<T>> {
        public ValidatorProperty(IObservable<T> values, Func<T, IEnumerable> validator = null)
            : base(values: values.Select(i => new ValidationDataErrorInfo<T>(i, validator?.Invoke(i)))) {
        }

        public ValidatorProperty(IObservable<T> values, Func<IObservable<T>, IObservable<ValidationDataErrorInfo<T>>> validator = null)
            : base(value: _unknownValidation, values: Generator(values, validator)) {
        }

        static IObservable<ValidationDataErrorInfo<T>> Generator(IObservable<T> values, Func<IObservable<T>, IObservable<ValidationDataErrorInfo<T>>> validator) {
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
    }
}


