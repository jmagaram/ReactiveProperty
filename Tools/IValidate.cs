namespace Tools {
    public interface IValidate {
        IReadOnlyProperty<IValidationDataErrorInfo> Errors { get; }
    }

    public interface IValidate<T> : IValidate {
        new IReadOnlyProperty<IValidationDataErrorInfo<T>> Errors { get; }
    }
}
