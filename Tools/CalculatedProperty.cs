using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Tools {
    public class CalculatedProperty<T> : Property<T> {
        public CalculatedProperty(T initialValue, IObservable<T> values) : base(value: initialValue) {
            values.Subscribe((i) => base.Value = i);
        }

        public new T Value
        {
            get { return base.Value; }
            private set { throw new NotImplementedException(); }
        }
    }
}
