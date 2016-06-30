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

namespace Client {
    public class Person {
        Property<string, StringError> _firstName;
        Property<string, StringError> _lastName;
        Property<string, StringError> _fullName;
        Property<string, StringError> _nicknameToAdd;
        Property<ImmutableList<NicknameReport>, string> _nicknames;
        Property<bool, Unit> _isMarried;
        Property<int, RangeError> _marriageYear;
        DelegateCommand _acceptName;
        DelegateCommand _rejectName;
        DelegateCommand _addNickname;
        DelegateCommand _acceptAll;
        DelegateCommand _rejectAll;

        public Person() {
            _firstName = new Property<string, StringError>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            _lastName = new Property<string, StringError>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            _fullName = new Property<string, StringError>(
                value: string.Empty,
                values: Observable.CombineLatest(_firstName, _lastName, (f, l) => $"{f} {l}"));
            _isMarried = new Property<bool, Unit>(value: false);
            _marriageYear = new Property<int, RangeError>(
                value: 2000,
                validator: new RangeValidator<int>(minimum: 1900, maximum: DateTime.Now.Year).Validate);
            _isMarried.Subscribe(i => {
                _marriageYear.IsEnabled.Value = i;
            });
            _acceptName = new DelegateCommand(
                action: () => {
                    _firstName.AcceptChanges();
                    _lastName.AcceptChanges();
                },
                canExecute:
                    Observable.CombineLatest(
                        _firstName.HasErrors,
                        _firstName.HasChanges,
                        _lastName.HasErrors,
                        _lastName.HasChanges,
                        (fe, fc, le, lc) => !fe && !le && (fc || lc)));
            _rejectName = new DelegateCommand(
                action: () => {
                    _firstName.RejectChanges();
                    _lastName.RejectChanges();
                },
                canExecute:
                    Observable.CombineLatest(
                        _firstName.HasChanges,
                        _lastName.HasChanges,
                        (f, l) => (f || l)));
            _nicknameToAdd = new Property<string, StringError>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            _nicknames = new Property<ImmutableList<NicknameReport>, string>(
                value: ImmutableList<NicknameReport>.Empty,
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
                canExecute: _nicknameToAdd.HasErrors.Select(i => !i));
            var anyErrors = Observable.CombineLatest(
                _firstName.HasErrors,
                _lastName.HasErrors,
                _nicknames.HasErrors,
                _isMarried.HasErrors,
                _marriageYear.HasErrors,
                (a, b, c,d,e) => a || b || c || d || e);
            var anyChanges = Observable.CombineLatest(
                _firstName.HasChanges,
                _lastName.HasChanges,
                _nicknames.HasChanges,
                _isMarried.HasChanges,
                _marriageYear.HasChanges,
                (a, b, c, d, e) => a || b || c || d || e);
            _acceptAll = new DelegateCommand(
                action: () => {
                    _firstName.AcceptChanges();
                    _lastName.AcceptChanges();
                    _nicknames.AcceptChanges();
                    _isMarried.AcceptChanges();
                    _marriageYear.AcceptChanges();
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
                },
                canExecute: anyChanges);
            // add all to disposables
        }

        public Property<string, StringError> FirstName => _firstName;
        public Property<string, StringError> LastName => _lastName;
        public Property<string, StringError> FullName => _fullName;
        public Property<bool, Unit> IsMarried => _isMarried;
        public Property<int, RangeError> MarriageYear => _marriageYear;
        public ICommand AcceptName => _acceptName;
        public ICommand RejectName => _rejectName;
        public ICommand AddNickname => _addNickname;
        public Property<string, StringError> NicknameToAdd => _nicknameToAdd;
        public Property<ImmutableList<NicknameReport>, string> Nicknames => _nicknames;
        public ICommand AcceptAll => _acceptAll;
        public ICommand RejectAll => _rejectAll;
    }
}
