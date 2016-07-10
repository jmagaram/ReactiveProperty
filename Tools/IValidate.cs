namespace Tools {
    public interface IValidate {
        IReadOnlyProperty<ValidationDataErrorInfo> Errors { get; }
    }
}
