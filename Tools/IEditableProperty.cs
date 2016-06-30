using System;
using System.ComponentModel;

namespace Tools {
    public interface IEditableProperty<T> : IReadOnlyProperty<T> {
        new T Value { get; set; }
    }
}