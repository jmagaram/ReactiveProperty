using System;
using System.ComponentModel;

namespace Tools {
    public interface IEditableProperty<T> : IReadOnlyProperty<T> {
        new T Value { get; set; }
    }

    public interface IReadOnlyProperty<out T> : IObservable<T>, INotifyPropertyChanged, IDisposable {
        T Value { get; }
    }
}