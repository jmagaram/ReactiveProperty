using System;
using System.Collections;

namespace Tools {
    public interface IValidationDataErrorInfo {
        ValidationStatus CompositeStatus { get; }
        ValidationStatus? DescendentStatus { get; }
        IEnumerable Errors { get; }
        Exception Exception { get; }
        bool? HasErrors { get; }
        ValidationStatus Status { get; }
    }

    public interface IValidationDataErrorInfo<T> : IValidationDataErrorInfo {
        T Value { get; }
    }

    public interface IValidate {
        IReadOnlyProperty<IValidationDataErrorInfo> Errors { get; }
    }

    public interface IValidate<T> : IValidate {
        new IReadOnlyProperty<IValidationDataErrorInfo<T>> Errors { get; }
    }
}
