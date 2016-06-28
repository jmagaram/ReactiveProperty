using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Tools {
    public class CalculatedProperty<TValue,TError> : Property<TValue,TError> {
        public CalculatedProperty(TValue initialValue, IObservable<TValue> values) : base(value: initialValue) {
            values.Subscribe((i) => base.Value = i);
        }

        public new TValue Value
        {
            get { return base.Value; }
            private set { throw new NotImplementedException(); }
        }
    }
}
