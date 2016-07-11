using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Client {
    public abstract class ValidatableModel : IValidate {
        public ValidatableModel() {
            IObservable<ValidationStatus> childrenStatus =
                Items
                .Select(i => i.Select(j => j.Errors))
                .Select(i => i.Select(j => j.Value.CompositeStatus))
                .Select(i => i.Aggregate((j, k) => j | k));
            // update composite status when children change, regardless of whether their errors change
            //IObservable<ValidationDataErrorInfo> crossPropertyErrors =
            //    childrenStatus.Select(i => {
            //        if (i!= ValidationStatus.IsValid) {
            //            return new ValidationDataErrorInfo(status: ValidationStatus.HasErrors, descendentStatus: i, errors: null, exception: null);
            //        }
            //        else {
            //            return CrossPropertyErrors.Latest();
            //        }
            //    });

        }
        protected abstract IObservable<IEnumerable<IValidate>> Items { get; }
        protected abstract IObservable<ValidationDataErrorInfo> CrossPropertyErrors { get; }

        protected IObservable<IEnumerable<KeyValuePair<IValidate,ValidationDataErrorInfo>>> LatestChildrenValidationStatus { get; }

        public IReadOnlyProperty<ValidationDataErrorInfo> Errors
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class Address2 : ValidatableModel {
        public Property<string> Street { get; }
        public Property<string> City { get; }
        public Property<string> Zip { get; }

        protected override IObservable<IEnumerable<IValidate>> Items => Observable.Return(new IValidate[] { Street, City, Zip });

        // THIS NEEDS TO KNOW
        //    Latest values for each property
        //    Validation status of each property
        // only want to ever call this if ALL children are ok. no, not really. some children might be bad but not those that correspond to some cross-property validation.
        // so can implement this BUT need helpers to make it easier to know what is going on with child properties - which have errors and which don't
        protected override IObservable<ValidationDataErrorInfo> CrossPropertyErrors
        {
            get
            {
                var result =
                Observable.CombineLatest(Street, City, (s, c) => new { Street = s, City = c }).Select(i => {
                    if (i.Street.Length + i.City.Length > 10) {
                        // weird how child status is ignored here
                        return new ValidationDataErrorInfo(status: ValidationStatus.HasErrors, descendentStatus: null, errors: new string[] { "not same length" }, exception: null);
                    }
                    else {
                        return new ValidationDataErrorInfo(status: ValidationStatus.IsValid, descendentStatus: null, errors: null, exception: null);
                    }
                });
                return result;
            }
        }
    }

    public class Address : Model, IValidate, IRevertible {
        PropertyBase<ValidationDataErrorInfo> _errors;

        public Address() {
            Street = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            City = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Zip = new Property<string>(defaultValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);

            // could create some of this automatically
            var latestStreet = Observable.CombineLatest(Street, Street.Errors, (e, errors) => new { Item = e, Errors = errors });
            var latestCity = Observable.CombineLatest(City, City.Errors, (e, errors) => new { Item = e, Errors = errors });
            var latestZip = Observable.CombineLatest(Zip, Zip.Errors, (e, errors) => new { Item = e, Errors = errors });
            _errors = new PropertyBase<ValidationDataErrorInfo>(
                value: null,
                values: Observable
                .CombineLatest(latestStreet, latestCity, latestZip, (s, c, z) => new { Street = s, City = c, Zip = z })
                .Select(i => {
                    var descendentStatus = new ValidationDataErrorInfo(new ValidationDataErrorInfo[] { i.Street.Errors, i.City.Errors, i.Zip.Errors }).Status; // weird how this throws away Errors, Exception
                    if (i.City.Errors.HasErrors != false || i.Street.Errors.HasErrors != false || i.Zip.Errors.HasErrors != false) {
                        return new ValidationDataErrorInfo(
                            status: ValidationStatus.HasErrors, // should be a different option, like BlockedOn; weird since Errors list is empty or maybe not necessary?
                            descendentStatus: descendentStatus,
                            errors: null,
                            exception: null);
                    }
                    else {
                        bool streetCityStartSameLetter = i.Street.Item.ToLower().Take(1).SequenceEqual(i.City.Item.ToLower().Take(1));
                        return new ValidationDataErrorInfo(
                            status: streetCityStartSameLetter ? ValidationStatus.IsValid : ValidationStatus.HasErrors, // should be a different option, like BlockedOn; weird since Errors list is empty or maybe not necessary?
                            descendentStatus: descendentStatus,
                            errors: streetCityStartSameLetter ? null : new string[] { "Street and city don't start with same letter" },
                            exception: null);
                    }
                }));

            HasChanges = new Property<bool>(
                defaultValue: false,
                values: Observable.CombineLatest(Street.HasChanges, City.HasChanges, Zip.HasChanges).Select(i => i.Any(j => j)));
            AddToDisposables(Street, City, Zip);
        }
        public Property<string> Street { get; }
        public Property<string> City { get; }
        public Property<string> Zip { get; }
        public IReadOnlyProperty<ValidationDataErrorInfo> Errors => _errors;
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
