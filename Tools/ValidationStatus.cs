using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Tools.AsyncFunctionStatus;

namespace Tools {
    class ValidationStatus : IValidationStatus {
        public ValidationStatus(AsyncFunctionStatus status, IEnumerable errors = null, bool? hasErrors = default(bool?), Exception exception = null) {
            if (status == Completed && errors == null)
                throw new ArgumentException($"Since the validation completed, '{nameof(errors)}' must be provided; use an empty enumerable to indicate no errors.");
            if (status != Completed && (errors != null || hasErrors != null))
                throw new ArgumentException($"Since the validation did not complete, both '{nameof(errors)}' and '{nameof(hasErrors)}' must be null.");
            if (status == Faulted ^ exception != null)
                throw new ArgumentException($"Either '{nameof(exception)}' should be provided, or the validation did not fault.", nameof(exception));
            Status = status;
            Errors = errors?.Cast<object>().ToArray();
            HasErrors =
                (status != Completed) ? null :
                (hasErrors.HasValue) ? hasErrors.Value :
                (bool?)(Errors.Cast<object>().Any());
            Exception = exception;
        }

        public IEnumerable Errors { get; }
        public Exception Exception { get; }
        public bool? HasErrors { get; }
        public AsyncFunctionStatus Status { get; }
    }
}
