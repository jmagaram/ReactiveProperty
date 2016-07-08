using System;
using System.Collections.Generic;
using System.Linq;

namespace Tools {
    public class CalculatedProperty<T> : PropertyBase<T> {
        public CalculatedProperty(IObservable<T> values, T defaultValue = default(T)) : base(defaultValue) {
            values
                .Subscribe(i => Value = i)
                .AddTo(Disposables);
        }

        public new T Value
        {
            get { return base.Value; }
            private set { base.Value = value; }
        }
    }
}

// calculated
// validatable
// editable
// revertible

// editable  , revertible, validatable
// calculated, validatable
//
// Propertybase
//   ValidatablePropertyBase
//     editable, revertible
//     calculated