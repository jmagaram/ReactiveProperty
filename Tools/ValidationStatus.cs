using System;

namespace Tools {
    [Flags]
    public enum ValidationStatus {
        /// <summary>
        /// Validation started but has not yet completed.
        /// </summary>
        Running = 1,
        /// <summary>
        /// Validation was canceled. It is unknown if validation rules are satisfied. 
        /// </summary>
        Canceled = 2,
        /// <summary>
        /// Validation ended unexpectedly (e.g. network is down). It is unknown if validation rules are satisfied. 
        /// </summary>
        Faulted = 4,
        /// <summary>
        /// Validation completed and data errors were detected.
        /// </summary>
        HasErrors = 8,
        /// <summary>
        /// Validation completed. All validation rules were satisfied.
        /// </summary>
        IsValid = 16,
        /// <summary>
        /// It is unknown if all validation rules are satisfied; the validation process is running or did not complete normally.
        /// </summary>
        Unknown = Running | Canceled | Faulted
    }
}
