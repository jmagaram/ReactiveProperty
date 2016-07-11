using System;

namespace Tools {
    [Flags]
    public enum ValidationStatus {
        None = 0,

        /// <summary>
        /// Asynchronous validation is in progress.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Asynchronous validation was canceled (e.g. the user grew tired of waiting for a lengthy
        /// web request to complete).
        /// </summary>
        Canceled = 2,

        /// <summary>
        /// Validation ended unexpectedly (e.g. network is down). 
        /// </summary>
        Faulted = 4,

        /// <summary>
        /// Cross-property validation can not start or is irrelevant until validation rules on
        /// dependent properties are satisfied.
        /// </summary>
        /// <example>
        /// Street, city, and zip are components of a valid address. If any of these properties are
        /// missing or formatted incorrectly, there is no point in attempting to validate the address
        /// as a whole (perhaps by using an internet mapping service).
        /// </example>
        Blocked = 8,

        /// <summary>
        /// Validation completed. One or more validation rules were not satisfied.
        /// </summary>
        HasErrors = 16,

        /// <summary>
        /// Validation completed. All validation rules were satisfied.
        /// </summary>
        IsValid = 32,

        /// <summary>
        /// It is unknown if all validation rules are satisfied; the validation process is running or did not complete normally.
        /// </summary>
        Unknown = Blocked | Running | Canceled | Faulted
    }
}
