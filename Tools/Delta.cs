using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public class Delta<T> {
        public Delta(T before, T after) {
            Before = before;
            After = after;
        }

        public T Before { get; }
        public T After { get; }
    }
}
