using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Tools;

namespace Client {
    // Issues
    // Data might get updated BEFORE HasChanges; Observable.CombineLatest might not work. Zip?
    // Know if address changes in case there is a cross property validation rule that depends on portions of the address
    //    Parent could listen for individual pieces though 
    public class Address : Model, IValidate<Address>, IRevertible {
        public Address() {
            Street = new Revertible<string>(initialValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            City = new Revertible<string>(initialValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Zip = new Revertible<string>(initialValue: string.Empty, validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            HasChanges = new Property<bool>(values: Observable.CombineLatest(ChangeTrackers().Select(i => i.HasChanges)).Select(i => i.Contains(true)));
            var errors = Observable
                .CombineLatest(Street.Errors, City.Errors, Zip.Errors, (s, c, z) => new { Street = s, City = c, Zip = z })
                .Select(i => {
                    var compositeStatus = i.Street.CompositeStatus | i.City.CompositeStatus | i.Zip.CompositeStatus;
                    if (compositeStatus != ValidationStatus.IsValid) {
                        return new ValidationDataErrorInfo<Address>(value: this, status: ValidationStatus.Blocked, descendentStatus: compositeStatus);
                    }
                    else {
                        bool streetCityStartSameLetter = i.Street.Value.ToLower()[0] == i.City.Value.ToLower()[0];
                        return new ValidationDataErrorInfo<Address>(
                            value: this,
                            status: streetCityStartSameLetter ? ValidationStatus.IsValid : ValidationStatus.HasErrors,
                            descendentStatus: compositeStatus,
                            errors: streetCityStartSameLetter ? null : new string[] { "Street and city don't start with same letter" });
                    }
                });
            Errors = new Property<ValidationDataErrorInfo<Address>>(
                values: errors,
                initialValue: new ValidationDataErrorInfo<Address>(value: null, status: ValidationStatus.None));
            AddToDisposables(Street, City, Zip, Errors);
        }

        public Revertible<string> Street { get; }
        public Revertible<string> City { get; }
        public Revertible<string> Zip { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public IReadOnlyProperty<IValidationDataErrorInfo<Address>> Errors { get; }
        IReadOnlyProperty<IValidationDataErrorInfo> IValidate.Errors => Errors;

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
        private IEnumerable<IRevertible> ChangeTrackers() {
            yield return Street;
            yield return City;
            yield return Zip;
        }
    }
}
