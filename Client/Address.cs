using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Client {
    public class Address : IValidate {
        public Address() {
            // disposable?
            Street = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 15).Validate);
            City = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 15).Validate);
            Zip = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 15).Validate);

            //var childrenLatestErrors = Observable.CombineLatest(Street.Errors, City.Errors, Zip.Errors);
            //var childrenHasErrors = childrenLatestErrors.Select(i => i.Select(j => j.HasErrors).Aggregate((bool?)false, (total, next) => (total == true || next == true) ? true : (total == null || next == null) ? (bool?)null : false));
            //var childrenStatus = childrenLatestErrors.Select(i => i.Select(j => j.Status).Aggregate((a, b) => a | b));
            //var entityErrors = Observable
            //    .CombineLatest(Street, City, Zip, (s, c, z) => new { Street = s?.Length ?? 0, City = c?.Length ?? 0, Zip = z?.Length ?? 0 })
            //    .Select(i => new ValidationDataErrorInfo(
            //        errors: (i.City + i.Street + i.Zip) > 10 ? new string[] { "too long" } : new string[] { }));
            //Errors = new Property<IValidationStatus>(
            //    values:
            //    Observable.CombineLatest(entityErrors,childrenStatus,childrenHasErrors,(e,cs,ch)=>new {Entity=e, ChildrenStatus = cs, ChildrenHasErrors = ch})
            //    .Select(i=>new ValidationStatus(i.ChildrenStatus | entityErrors)
            // any true, true, all false, false, null

            //Errors = new PropertyBase<IValidationStatus>(values: {  return null});
            //Observable.CombineLatest(Street, City, Zip, (s, c, z) => new { Street = s, City = c, Zip = z })
            //.Select(i => {
            //    bool anyInProgress = i.City. 
            //});

            //    InProgress,
            //Faulted,
            //Canceled,
            //Completed,

            //bool? HasErrors { get; } - APPLIES TO ENTITY AND CHILDREN
            //AsyncFunctionStatus Status { get; } - APPLIES TO ENTITY AND CHILDREN, flags
            //Exception Exception { get; } - APPLIES TO ENTITY ONLY
            //IEnumerable Errors { get; } - APPLIES TO ENTITY ONLY


        }
        public Property<string> Street { get; }
        public Property<string> City { get; }
        public Property<string> Zip { get; }

        public IReadOnlyProperty<ValidationDataErrorInfo> Errors { get; }
    }

    // read-only version
    //IValidationErrorData<TError>, ITrackChanges, INotifyPropertyChanged, IDisposable

    // some stuff for data binding ENTITY, probably same as for Property
    // some stuff for internally calculating what to do with the component, ESPECIALLY IF
    // have a collection of a bunch of IProperty items (a general purpose model) and want to revert, etc.
    // without knowing the specific types

    // know if address changes in case there is a cross property validation rule where we 
    // check address content; address might be fine by itself
    // know if address has any errors (to know if entity is ok)
    // know if address has any changes (for reverting)
    // ability to accept and reject
    //
    // model must expose an IObservable of various property statuses

}
