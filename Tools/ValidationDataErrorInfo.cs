using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tools {
    public class ValidationDataErrorInfo {
        public ValidationDataErrorInfo(ValidationStatus status, ValidationStatus? descendentStatus, IEnumerable errors, Exception exception) {
            Status = status;
            DescendentStatus = descendentStatus;
            Errors = errors?.Cast<object>().ToArray() ?? new object[] { };
            Exception = exception;
        }

        public ValidationDataErrorInfo(IEnumerable errors) : this(
            status: (errors == null) ? ValidationStatus.IsValid : errors.Cast<object>().Any() ? ValidationStatus.HasErrors : ValidationStatus.IsValid,
            descendentStatus: null,
            errors: errors,
            exception: null) {
        }

        public ValidationDataErrorInfo(IEnumerable<ValidationDataErrorInfo> items) : this(status: items.Select(i => i.CompositeStatus).Aggregate((j, k) => j | k), descendentStatus: null, errors: null, exception: null) { }

        public ValidationStatus Status { get; }

        public bool? HasErrors => (CompositeStatus == ValidationStatus.IsValid) ? false : (CompositeStatus == ValidationStatus.HasErrors) ? true : (bool?)null;

        public ValidationStatus? DescendentStatus { get; }

        public ValidationStatus CompositeStatus => DescendentStatus.HasValue ? (Status | DescendentStatus.Value) : Status;

        public IEnumerable Errors { get; }

        public Exception Exception { get; }
    }
}
