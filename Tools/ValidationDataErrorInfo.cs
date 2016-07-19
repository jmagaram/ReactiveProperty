using System;
using System.Collections;
using System.Linq;

namespace Tools {
    public class ValidationDataErrorInfo<T> : IValidationDataErrorInfo<T> {
        static object[] _noErrors = new object[] { };

        public ValidationDataErrorInfo(
            T value,
            ValidationStatus status,
            ValidationStatus? descendentStatus = null,
            IEnumerable errors = null,
            Exception exception = null) {
            Value = value;
            Status = status;
            DescendentStatus = descendentStatus;
            Errors = errors?.Cast<object>().ToArray() ?? _noErrors;
            Exception = exception;
        }

        public ValidationDataErrorInfo(T value, IEnumerable errors) : this(
            value: value,
            status: (errors == null || errors.Cast<object>().Any()) ? ValidationStatus.HasErrors : ValidationStatus.IsValid,
            descendentStatus: null,
            errors: errors,
            exception: null) {
        }

        public ValidationStatus Status { get; }

        public bool? HasErrors =>
            (CompositeStatus == ValidationStatus.IsValid) ? false
            : (CompositeStatus.HasFlag(ValidationStatus.HasErrors)) ? true
            : (bool?)null;

        public ValidationStatus? DescendentStatus { get; }

        public ValidationStatus CompositeStatus => DescendentStatus.HasValue ? (Status | DescendentStatus.Value) : Status;

        public IEnumerable Errors { get; }

        public Exception Exception { get; }

        public T Value { get; }
    }
}
