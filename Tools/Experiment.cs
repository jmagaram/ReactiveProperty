using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools {
    public class Validator<T> : PropertyBase<bool?> {
        public Validator(IObservable<T> values, Func<IObservable<T>, IObservable<bool?>> validator = null) {
            if (validator != null) {
                validator(values)
                    .Subscribe(i => Value = i)
                    .AddTo(Disposables);
            }
            else {
                Value = true;
            }
        }
    }
}