using System;
using System.Collections;
using System.Reactive.Linq;

namespace Tools {
    public class Revertible<TValue> : Property<TValue>, IRevertible {
        public Revertible(
            TValue value = default(TValue),
            Func<IObservable<TValue>, IObservable<ValidationDataErrorInfo<TValue>>> asyncValidator = null,
            Func<TValue, IEnumerable> validator = null)
            : base(value: value, validator: validator, asyncValidator: asyncValidator) {
            Original = new Property<TValue>(value: value);
            HasChanges = new Property<bool>(values: Observable.CombineLatest(Original, Values, (o, c) => !Equals(o, c)));
            AddToDisposables(Original, HasChanges);
        }

        private IEditableProperty<TValue> Original { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() => Original.Value = Value;
        public void RejectChanges() => Value = Original.Value;
        // this causes revalidation even when value is exactly the same desirable? definitely with
        // calculated properties when you change the source, you want the destination calculated
        // property to be updated so you can zip them
    }
}
