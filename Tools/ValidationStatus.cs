using System;

namespace Tools {
    [Flags]
    public enum ValidationStatus {
        /// <summary>
        /// Cross-property validation can not start or is irrelevant until validation rules on
        /// dependent properties are satisfied.
        /// </summary>
        /// <example>
        /// Street, city, and zip are necessary components of a valid address. If any of these
        /// properties are missing or formatted incorrectly, there is no point in attempting to
        /// validate the address as a whole (perhaps by using an internet mapping service). In such a
        /// situation, the cross-property validation status is <see cref="ValidationStatus.Blocked"/>.
        ///
        /// The status of an address with invalid components could optionally be reported as <see
        /// cref="ValidationStatus.HasErrors"/>. But this might draw the user's attention to a
        /// message like "Could not find this address using the Google mapping service." It is better
        /// focus the user on root-cause errors they can fix. A status of <see
        /// cref="ValidationStatus.Blocked"/> could be used to display a message like "The address
        /// could not be validated; fix the problems noted below."
        ///
        /// The cross-property validation status of the address could also be reported as <see
        /// cref="ValidationStatus.IsValid"/>. This will not not erase the errors on the individual
        /// components but it just feels wrong. Information is lost that could be useful for display
        /// error status in the UI.
        /// </example>
        Blocked = 1,

        /// <summary>
        /// Asynchronous validation is in progress.
        /// </summary>
        Running = 2,

        /// <summary>
        /// Asynchronous validation was canceled. 
        /// </summary>
        Canceled = 4,

        /// <summary>
        /// Validation ended unexpectedly (e.g. network is down). 
        /// </summary>
        Faulted = 8,

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
