using System;
using System.ComponentModel;

namespace Tools {
    public interface IReadOnlyProperty<T> : IObservable<T>, INotifyPropertyChanged, IDisposable {
        T Value { get; }
    }
}