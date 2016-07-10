namespace Tools {
    public interface ITrackChanges {
        IReadOnlyProperty<bool> HasChanges { get; }
    }
}
