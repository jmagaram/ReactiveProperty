using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Client {
    //public abstract class ValidatableModel : IValidate {
    //    public ValidatableModel() {
    //        IObservable<ValidationStatus> childrenStatus =
    //            Items
    //            .Select(i => i.Select(j => j.Errors))
    //            .Select(i => i.Select(j => j.Value.CompositeStatus))
    //            .Select(i => i.Aggregate((j, k) => j | k));
    //        // update composite status when children change, regardless of whether their errors change
    //        //IObservable<ValidationDataErrorInfo> crossPropertyErrors =
    //        //    childrenStatus.Select(i => {
    //        //        if (i!= ValidationStatus.IsValid) {
    //        //            return new ValidationDataErrorInfo(status: ValidationStatus.HasErrors, descendentStatus: i, errors: null, exception: null);
    //        //        }
    //        //        else {
    //        //            return CrossPropertyErrors.Latest();
    //        //        }
    //        //    });

    //    }
    //    protected abstract IObservable<IEnumerable<IValidate>> Items { get; }
    //    protected abstract IObservable<ValidationDataErrorInfo> CrossPropertyErrors { get; }

    //    protected IObservable<IEnumerable<KeyValuePair<IValidate, ValidationDataErrorInfo>>> LatestChildrenValidationStatus { get; }

    //    public IReadOnlyProperty<ValidationDataErrorInfo> Errors
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //}


    //public class Address2 : ValidatableModel {
    //    public Property<string> Street { get; }
    //    public Property<string> City { get; }
    //    public Property<string> Zip { get; }

    //    protected override IObservable<IEnumerable<IValidate>> Items => Observable.Return(new IValidate[] { Street, City, Zip });

    //    // THIS NEEDS TO KNOW
    //    //    Latest values for each property
    //    //    Validation status of each property
    //    // only want to ever call this if ALL children are ok. no, not really. some children might be bad but not those that correspond to some cross-property validation.
    //    // so can implement this BUT need helpers to make it easier to know what is going on with child properties - which have errors and which don't
    //    protected override IObservable<ValidationDataErrorInfo> CrossPropertyErrors
    //    {
    //        get
    //        {
    //            var result =
    //            Observable.CombineLatest(Street, City, (s, c) => new { Street = s, City = c }).Select(i => {
    //                if (i.Street.Length + i.City.Length > 10) {
    //                    // weird how child status is ignored here
    //                    return new ValidationDataErrorInfo(status: ValidationStatus.HasErrors, descendentStatus: null, errors: new string[] { "not same length" }, exception: null);
    //                }
    //                else {
    //                    return new ValidationDataErrorInfo(status: ValidationStatus.IsValid, descendentStatus: null, errors: null, exception: null);
    //                }
    //            });
    //            return result;
    //        }
    //    }
    //}

    // MAJOR issues
    // Data might get updated BEFORE error status and has changes; Observable.CombineLatest might not work. test it
    // Actually this only seems to work for IValidate since I have a status called Running
    // But doesn't work for synchronizing HasChanges and the core property; first core property gets updated and THEN the HasChanges is published after and value is stale
    public class Address : Model, IValidate<Address>, IRevertible {
        public Address() {
            Street = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            City = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Zip = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            HasChanges = new Property<bool>(values: Observable.CombineLatest(ChangeTrackers().Select(i => i.HasChanges)).Select(i => i.Contains(true)));
            var errors = Observable.CombineLatest(Street.Errors, City.Errors, Zip.Errors, (s, c, z) => new { Street = s, City = c, Zip = z })
                .Select(i => {
                    var compositeStatus = i.Street.CompositeStatus | i.City.CompositeStatus | i.Zip.CompositeStatus;
                    if (compositeStatus != ValidationStatus.IsValid) {
                        return new ValidationDataErrorInfo<Address>(value:this, status: ValidationStatus.Blocked, descendentStatus: compositeStatus, errors: null, exception: null);
                    }
                    else {
                        bool streetCityStartSameLetter = i.Street.Value.ToLower()[0] == i.City.Value.ToLower()[0];
                        return new ValidationDataErrorInfo<Address>(
                            value:this,
                            status: streetCityStartSameLetter ? ValidationStatus.IsValid : ValidationStatus.HasErrors,
                            descendentStatus: compositeStatus,
                            errors: streetCityStartSameLetter ? null : new string[] { "Street and city don't start with same letter" },
                            exception: null);
                    }
                });
            // shouldn't need this default value for error status, why?
            Errors = new PropertyBase<ValidationDataErrorInfo<Address>>(
                values: errors,
                value: new ValidationDataErrorInfo<Address>(value:null,status:ValidationStatus.None, descendentStatus: null, errors: null, exception: null));
            AddToDisposables(Street, City, Zip, Errors);
        }

        private IEnumerable<IRevertible> ChangeTrackers() {
            yield return Street;
            yield return City;
            yield return Zip;
        }

        public Property<string> Street { get; }
        public Property<string> City { get; }
        public Property<string> Zip { get; }
        public IReadOnlyProperty<IValidationDataErrorInfo<Address>> Errors { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }

        public void AcceptChanges() {
            foreach (var c in ChangeTrackers()) {
                c.AcceptChanges();
            }
        }
        public void RejectChanges() {
            foreach (var c in ChangeTrackers()) {
                c.RejectChanges();
            }
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
