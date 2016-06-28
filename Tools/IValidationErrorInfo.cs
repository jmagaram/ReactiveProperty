using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IValidationErrorData<out TError> {
        bool HasErrors { get; }
        TError[] Errors { get; }
    }
}
