using System;
using System.ComponentModel;

namespace Tools {
    public interface IReadOnlyProperty<out T> : IObservable<T>, INotifyPropertyChanged, IDisposable {
        T Value { get; }
    }
}