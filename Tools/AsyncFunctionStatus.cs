using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    [Flags]
    public enum AsyncFunctionStatus {
        InProgress = 1,
        Faulted = 2,
        Canceled = 4,
        Completed = 8
    }
}
