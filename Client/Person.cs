using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Collections.Immutable;
using System.Diagnostics;

//Model, IValidate<Address>, IRevertible
namespace Client {
    public class Person : Model, IRevertible, IValidate<Person> {
        Property<string> _firstName;
        Property<string> _lastName;
        Property<string> _fullName;
        Property<string> _nicknameToAdd;
        Property<string> _website;
        Property<ImmutableList<NicknameReport>> _nicknames;
        Property<bool> _isMarried;
        Property<int> _marriageYear;
        Address _address;
        DelegateCommand _addNickname;
        DelegateCommand _acceptAll;
        DelegateCommand _rejectAll;

        public Person() {
            ILogger logger = new DelegateLogger(i => Debug.WriteLine(i));
            _address = new Address();
            _firstName = new Property<string>(
                defaultValue: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            _lastName = new Property<string>(
                defaultValue: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            _fullName = new Property<string>(
                defaultValue: string.Empty,
                values: Observable.CombineLatest(_firstName, _lastName, (f, l) => $"{f} {l}"));
            _isMarried = new Property<bool>(defaultValue: false);
            _marriageYear = new Property<int>(
                defaultValue: 2000,
                validator: new RangeValidator<int>(minimum: 1900, maximum: DateTime.Now.Year).Validate,
                isEnabled: _isMarried);
            _website = new Property<string>(
                defaultValue: "www.google.com",
                asyncValidator: (values) => {
                    return
                        values
                        .Throttle(TimeSpan.FromSeconds(3))
                        .Select(i => {
                            bool isOk = string.IsNullOrWhiteSpace(i) || i.ToLower().EndsWith(".com");
                            string[] errors = isOk ? new string[] { } : new string[] { "Does not end with .com but should" };
                            ValidationDataErrorInfo<string> errorInfo = new ValidationDataErrorInfo<string>(
                                value: i,
                                status: isOk ? ValidationStatus.IsValid : ValidationStatus.HasErrors,
                                descendentStatus: null,
                                errors: errors,
                                exception: null);
                            return errorInfo;
                        })
                        .ObserveOnDispatcher();
                });
            _nicknameToAdd = new Property<string>(
                defaultValue: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            _nicknames = new Property<ImmutableList<NicknameReport>>(
                defaultValue: ImmutableList<NicknameReport>.Empty,
                validator: (list) => {
                    List<string> errors = new List<string>();
                    if (list == null) {
                        errors.Add("Can not be null");
                    }
                    else {
                        if (list.Count > 5) {
                            errors.Add("Too many!");
                        }
                        if (list.Select(i => i.Name).Distinct().Count() != list.Count) {
                            errors.Add("Duplicates");
                        }
                    }
                    return errors;
                });
            _addNickname = new DelegateCommand(
                action: () => {
                    string nameToAdd = _nicknameToAdd.Value;
                    NicknameReport report = new NicknameReport(
                        name: nameToAdd,
                        delete: new DelegateCommand(action: () => {
                            var item = _nicknames.Value.First(j => j.Name == nameToAdd);
                            _nicknames.Value = _nicknames.Value.Remove(item);
                        }));
                    _nicknames.Value = _nicknames.Value.Add(report);
                    _nicknameToAdd.Value = string.Empty;
                },
                canExecute: _nicknameToAdd.Errors.Select(i => i.Status == ValidationStatus.IsValid));

            // Marriage year conditional
            Errors = new PropertyBase<ValidationDataErrorInfo<Person>>(
                value: new ValidationDataErrorInfo<Person>(value: this, status: ValidationStatus.Blocked, descendentStatus: ValidationStatus.None, errors: null, exception: null),
                values: Observable
                    .CombineLatest(new IValidate[] { Address, FirstName, LastName, FullName, IsMarried, Nicknames, Website }.Select(i => i.Errors))
                    .Select(i => i.Select(j => j.CompositeStatus))
                    .Select(i => i.Aggregate((a, b) => a | b))
                    .Log(logger,"errors")
                    .Select(i => new ValidationDataErrorInfo<Person>(
                        value: this,
                        status: ValidationStatus.IsValid,
                        descendentStatus: i,
                        errors: null,
                        exception: null)));
            // NOTE: Not all properties affect whether can accept the form; subforms like nicknameToAdd!
            HasChanges = new PropertyBase<bool>(
                value: false,
                values:
                    Observable
                    .CombineLatest(ChangeTrackers().Select(i => i.HasChanges))
                    .Select(i => i.Any(j => j == true)));
            _acceptAll = new DelegateCommand(
                action: () => { AcceptChanges(); },
                canExecute: Observable.CombineLatest(Errors, HasChanges, (errs, chg) => chg && errs.HasErrors==false));
            _rejectAll = new DelegateCommand(
                action: () => { RejectChanges(); },
                canExecute: HasChanges);
            AddToDisposables(_firstName, _lastName, _fullName, _marriageYear, _isMarried, HasChanges, _website, _nicknames, _nicknameToAdd, _website, Errors);
        }

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
            yield return Address;
            yield return FirstName;
            yield return LastName;
            yield return IsMarried;
            yield return MarriageYear;
            yield return Nicknames;
            yield return Website;
        }

        public Property<string> FirstName => _firstName;
        public Property<string> LastName => _lastName;
        public Property<string> FullName => _fullName;
        public Property<bool> IsMarried => _isMarried;
        public Property<string> Website => _website;
        public Property<int> MarriageYear => _marriageYear;
        public ICommand AddNickname => _addNickname;
        public Property<string> NicknameToAdd => _nicknameToAdd;
        public Property<ImmutableList<NicknameReport>> Nicknames => _nicknames;
        public Address Address => _address;
        public ICommand AcceptAll => _acceptAll;
        public ICommand RejectAll => _rejectAll;
        public IReadOnlyProperty<bool> HasChanges { get; }
        public IReadOnlyProperty<IValidationDataErrorInfo<Person>> Errors { get; }

        IReadOnlyProperty<IValidationDataErrorInfo> IValidate.Errors => Errors;
    }
}
