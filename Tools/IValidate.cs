namespace Tools {
    public interface IValidate<T> {
        IReadOnlyProperty<IValidationDataErrorInfo<T>> Errors { get; }
    }
}
