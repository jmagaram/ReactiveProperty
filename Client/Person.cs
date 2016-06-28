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
        Property<string, StringError> _nicknameToAdd;
        Property<ImmutableList<NicknameReport>, string> _nicknames;
        CalculatedProperty<string, StringError> _fullName;
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
            _fullName = new CalculatedProperty<string, StringError>(
                initialValue: string.Empty,
                values: Observable.CombineLatest(_firstName, _lastName, (f, l) => $"{f} {l}"));
            _isMarried = new Property<bool, Unit>(value: false);
            _marriageYear = new Property<int, RangeError>(
                value: 2000,
                validator: new RangeValidator<int>(minimum: 1900, maximum: DateTime.Now.Year).Validate);
            _isMarried.Subscribe(i => _marriageYear.IsEnabled = i);
            _acceptName = new DelegateCommand(
                action: () => {
                    _firstName.AcceptChanges();
                    _lastName.AcceptChanges();
                },
                canExecute:
                    Observable.CombineLatest(
                        _firstName.PropertyState,
                        _lastName.PropertyState,
                        (f, l) => !f.HasErrors && !l.HasErrors && (f.HasChanges || l.HasChanges)));
            _rejectName = new DelegateCommand(
                action: () => {
                    _firstName.RejectChanges();
                    _lastName.RejectChanges();
                },
                canExecute:
                    Observable.CombineLatest(
                        _firstName.PropertyState,
                        _lastName.PropertyState,
                        (f, l) => (f.HasChanges || l.HasChanges)));
            _nicknameToAdd = new Property<string, StringError>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            _nicknames = new Property<ImmutableList<NicknameReport>, string>(
                value: ImmutableList<NicknameReport>.Empty,
                validator: (list) => {
                    List<string> errors = new List<string>();
                    if (list.Count > 5) {
                        errors.Add("Too many!");
                    }
                    if (list.Select(i => i.Name).Distinct().Count() != list.Count) {
                        errors.Add("Duplicates");
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
                canExecute: _nicknameToAdd.PropertyState.Select(i => !i.HasErrors));
            var anyErrors = Observable.CombineLatest(
                _firstName.PropertyState.Select(i => i.HasErrors),
                _lastName.PropertyState.Select(i => i.HasErrors),
                _nicknames.PropertyState.Select(i => i.HasErrors),
                (a, b, c) => a || b || c);
            var anyChanges = Observable.CombineLatest(
                _firstName.PropertyState.Select(i => i.HasChanges),
                _lastName.PropertyState.Select(i => i.HasChanges),
                _nicknames.PropertyState.Select(i => i.HasChanges),
                (a, b, c) => a || b || c);
            _acceptAll = new DelegateCommand(
                action: () => {
                    _firstName.AcceptChanges();
                    _lastName.AcceptChanges();
                    _nicknames.AcceptChanges();
                },
                canExecute: Observable.CombineLatest(anyErrors,anyChanges,(errs,chg)=>chg && !errs)
                );
            _rejectAll = new DelegateCommand(
                action: () => {
                    _firstName.RejectChanges();
                    _lastName.RejectChanges();
                    _nicknames.RejectChanges();
                },
                canExecute: anyChanges);
        }

        public Property<string, StringError> FirstName => _firstName;
        public Property<string, StringError> LastName => _lastName;
        public CalculatedProperty<string, StringError> FullName => _fullName;
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
