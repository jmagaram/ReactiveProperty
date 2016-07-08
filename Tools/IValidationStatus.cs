using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IValidationStatus {
        Exception Exception { get; }
        AsyncFunctionStatus Status { get; }
        bool? HasErrors { get; }
        IEnumerable Errors { get; }
    }
}
