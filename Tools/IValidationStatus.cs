using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IValidationStatus<TValue, out TError> {
        TValue Value { get; }
        Exception Exception { get; }
        AsyncFunctionStatus Status { get; }
        bool? HasErrors { get; }
        IEnumerable<TError> Errors { get; }
    }
}
