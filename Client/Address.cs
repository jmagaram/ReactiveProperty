using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Client {
    public class Address : IValidate, IRevertible {
        public Address() {
            // disposable?
            Street = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            City = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Zip = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Errors = new PropertyBase<ValidationDataErrorInfo>(
                value: null,
                values: Observable.CombineLatest(Street.Errors, City.Errors, Zip.Errors).Select(i => {
                    return new ValidationDataErrorInfo(i);
                }));
            HasChanges = new Property<bool>(
                defaultValue: false,
                values: Observable.CombineLatest(Street.HasChanges, City.HasChanges, Zip.HasChanges).Select(i => i.Any(j => j)));
        }
        public Property<string> Street { get; }
        public Property<string> City { get; }
        public Property<string> Zip { get; }

        public IReadOnlyProperty<ValidationDataErrorInfo> Errors { get; }


        public IReadOnlyProperty<bool> HasChanges { get; }
        public void AcceptChanges() {
            Street.AcceptChanges();
            City.AcceptChanges();
            Zip.AcceptChanges();
        }

        public void RejectChanges() {
            Street.RejectChanges();
            City.RejectChanges();
            Zip.RejectChanges();
        }
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
