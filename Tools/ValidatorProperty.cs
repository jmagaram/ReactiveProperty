using System;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;

namespace Tools {
    public class ValidatorProperty<T> : PropertyBase<ValidationDataErrorInfo<T>> {
        public ValidatorProperty(IObservable<T> values, Func<T, IEnumerable> validator = null)
            : base(values: values.Select(i => new ValidationDataErrorInfo<T>(i, validator?.Invoke(i)))) {
        }
    }
}
