using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IRevertible {
        bool IsChanged { get; }
        void AcceptChanges();
        void RejectChanges();
    }
}
