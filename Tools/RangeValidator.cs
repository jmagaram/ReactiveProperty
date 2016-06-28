using System;
using System.Collections.Generic;

namespace Tools {
    public class RangeValidator<T> where T : struct, IComparable {
        public RangeValidator(
            T? minimum = null,
            T? maximum = null
            ) {
            Minimum = minimum;
            Maximum = maximum;
        }

        public IEnumerable<RangeError> Validate(T i) {
            Comparer<T> comparer = Comparer<T>.Default;
            if (Minimum.HasValue && comparer.Compare(Minimum.Value, i) > 0) {
                yield return RangeError.TooBig;
            }
            if (Maximum.HasValue && comparer.Compare(i, Maximum.Value) > 0) {
                yield return (RangeError.TooBig);
            }
        }

        public T? Minimum { get; }
        public T? Maximum { get; }
    }
}
