namespace Tools {
    public interface IValidate<TError> {
        IReadOnlyProperty<TError[]> Errors { get; }
        IReadOnlyProperty<bool> HasErrors { get; }
    }
}
