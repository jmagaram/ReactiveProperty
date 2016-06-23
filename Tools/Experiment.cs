using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public interface IProperty<TError> {
        IObservable<Task<bool>> IsValid { get; }
        IObservable<Task<bool>> HasErrors { get; }
        IObservable<TError> Errors { get; }

        void Accept();
        void Revert();
    }
}
