using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Threading;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Subjects;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Threading;
using System.Diagnostics;

namespace Client {
    public class Person : Model {
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
                            ValidationDataErrorInfo errorInfo = new ValidationDataErrorInfo(
                                status: isOk ? ValidationStatus.IsValid : ValidationStatus.HasErrors,
                                descendentStatus: null,
                                errors: errors,
                                exception: null);
                            return new KeyValuePair<string, ValidationDataErrorInfo>(i, errorInfo);
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
            var anyErrors = Observable.CombineLatest(
                _firstName.Errors.Select(j => j.Status != ValidationStatus.IsValid),
                _lastName.Errors.Select(j => j.Status != ValidationStatus.IsValid),
                _nicknames.Errors.Select(j => j.Status != ValidationStatus.IsValid),
                _isMarried.Errors.Select(j => j.Status != ValidationStatus.IsValid),
                _website.Errors.Select(j => j.Status != ValidationStatus.IsValid), // why need the null check here but not elsewhere?
                _address.Errors.Select(j => j.Status != ValidationStatus.IsValid),
                _marriageYear.Errors.Select(j => j.Status != ValidationStatus.IsValid))
                .Select(i => i.Any(j => j != false));

            // NOTE: Not all properties affect whether can accept the form; subforms like nicknameToAdd!
            //var anyErrors =
            //    Observable.CombineLatest((new IValidate[] { _firstName, _lastName, _nicknames, _website, _address, _marriageYear }).Select(i => i.Errors.Select(z => z.Status != ValidationStatus.IsValid))).Select(i=>i.Any(j=>j!=false));
            var anyChanges = Observable.CombineLatest(
                _firstName.HasChanges,
                _lastName.HasChanges,
                _nicknames.HasChanges,
                _isMarried.HasChanges,
                _website.HasChanges,
                _address.HasChanges,
                _marriageYear.HasChanges)
                .Select(i => i.Any(j => j));
            _acceptAll = new DelegateCommand(
                action: () => {
                    _firstName.AcceptChanges();
                    _lastName.AcceptChanges();
                    _nicknames.AcceptChanges();
                    _isMarried.AcceptChanges();
                    _marriageYear.AcceptChanges();
                    _address.AcceptChanges();
                    _website.AcceptChanges();
                    //_fullName.AcceptChanges(); // does not make sense. should not be allowed. same with editing. weird.
                },
                canExecute: Observable.CombineLatest(anyErrors, anyChanges, (errs, chg) => chg && !errs)
                );
            _rejectAll = new DelegateCommand(
                action: () => {
                    _firstName.RejectChanges();
                    _lastName.RejectChanges();
                    _nicknames.RejectChanges();
                    _isMarried.RejectChanges();
                    _marriageYear.RejectChanges();
                    _website.RejectChanges();
                    _address.RejectChanges();
                },
                canExecute: anyChanges);
            AddToDisposables(_firstName, _lastName, _website);
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
    }
}
