using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tools {
    //public class Validator<T> : PropertyBase<IValidationErrorInfo> {
    //    public Validator(IObservable<T> values, Func<IObservable<T>, IObservable<KeyValuePair<T, IValidationErrorInfo>>> validator) : base() {
    //        Observable
    //            .CombineLatest(values, validator(values), (v, r) => new { Value = v, Result = r })
    //            .Where(i => i.Result.Value.Status != AsyncFunctionStatus.InProgress)
    //            .Select(i => new KeyValuePair<T, IValidationErrorInfo>(key: default(T), value: null))
    //            .Subscribe(i => Value = i.Value)
    //            .AddTo(Disposables);
    //    }
    //}
}