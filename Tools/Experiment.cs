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
using static Tools.AsyncFunctionStatus;

namespace Tools {
    //public class Validator<T> : PropertyBase<IValidationStatus> {
    //    public Validator(IObservable<T> values, Func<IObservable<T>, IObservable<KeyValuePair<T, IValidationStatus>>> validator) : base(
    //        values:
    //            Observable
    //            .CombineLatest(values, validator(values), (v, r) => new { Value = v, ValidatedValue = r.Key, ErrorStatus = r.Value })
    //            .Where(i => i.ErrorStatus.Status != InProgress)
    //            .Select(i => {
    //                if (!Equals(i.Value, i.ValidatedValue)) {
    //                    return new ValidationStatus(status: InProgress);
    //                }
    //                else {
    //                    switch (i.ErrorStatus.Status) {
    //                        case InProgress: throw new NotImplementedException("This code should never be executed.");
    //                        case Canceled: return new ValidationStatus(status: Canceled);
    //                        case Faulted: return new ValidationStatus(status: Faulted, exception: i.ErrorStatus.Exception);
    //                        case Completed: return new ValidationStatus(status: Completed, errors: i.ErrorStatus.Errors, hasErrors: i.ErrorStatus.HasErrors);
    //                        default: throw new NotImplementedException();
    //                    }
    //                }
    //            })) {
    //    }
    //}

    // validated ALWAYS, in core
    // editable
    // calculated

    //public class Prop<TEnabled, TValue> where TEnabled : IReadOnlyProperty<bool> where TValue : IReadOnlyProperty<TValue> {
    //    public TEnabled IsReadOnly { get; }
    //    public TValue Value { get; }
    //}

    //class zzz {
    //    public zzz() {
    //        var x = new Prop<IEditableProperty<bool>>();
    //        x.IsReadOnly.Value = true;

    //        var y = new Prop<TEnabled= IReadOnlyProperty < bool >> ();
    //        y.IsReadOnly;
    //    }
    //}


    //public class Validated<T> : ValidationStatus {
    //    public Validated(T value) : base(AsyncFunctionStatus.) {
    //        Value = value;
    //    }

    //    T Value { get; }
    //}

    //public class ValProperty<T> : PropertyBase<Validated<T>> {
    //    public ValProperty() {
    //        ValProperty<string> s;
    //        s.Value = new Validated<string>(string.Empty)
    //    }
    //}

    //public class FullProperty<T> : PropertyBase<T> {
    //    public FullProperty(T value, IObservable<T> values = null, Func<IReadOnlyProperty<IValidationStatus>> validationStatus = null, Func<IEditableProperty<bool>> isVisible = null) : base(value: value, values: values) {
    //        ValidationStatus = validationStatus();
    //        IsVisible = isVisible();
    //        AddToDisposables(ValidationStatus, IsVisible);
    //    }

    //    public IReadOnlyProperty<IValidationStatus> ValidationStatus { get; set; }
    //    public IEditableProperty<bool> IsVisible { get; set; }
    //    public IEditableProperty<bool> IsEnabled { get; set; }
    //}

    //class X {
    //    public X() {
    //        var fullName = new FullProperty<string>(string.Empty) { ValidationStatus = new Validator<string>(fullName, validator: null) }
    //        fullName.ValidationStatus = new Validator<string>(fullName, null);
    //    }
    //}
}