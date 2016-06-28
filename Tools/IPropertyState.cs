using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IPropertyState<TValue, TError> : IValidationErrorData<TError>, ITrackChanges {
        TValue Value { get; }
        bool IsEnabled { get; }
        bool IsVisible { get; }
    }
}
