using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IRevertible : ITrackChanges {
        void AcceptChanges();
        void RejectChanges();
    }

    public interface ITrackChanges {
        IReadOnlyProperty<bool> HasChanges { get; }
    }
}
