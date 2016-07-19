using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Collections.Immutable;

//Model, IValidate<Address>, IRevertible
namespace Client {
    public class Person : Model, IRevertible, IValidate<Person> {
        public Person() {
            Address = new Address();
            FirstName = new Revertible<string>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            LastName = new Revertible<string>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            FullName = new Property<string>(
                value: string.Empty,
                values: Observable.CombineLatest(FirstName, LastName, (f, l) => $"{f} {l}"));
            IsMarried = new Revertible<bool>(value: false);
            MarriageYear = new Revertible<int>(
                value: 2000,
                validator: new RangeValidator<int>(minimum: 1900, maximum: DateTime.Now.Year).Validate);
            Website = new Revertible<string>(
                value: "www.google.com",
                asyncValidator: (values) => {
                    return
                        values
                        .Throttle(TimeSpan.FromSeconds(3))
                        .Select(i => {
                            bool isOk = string.IsNullOrWhiteSpace(i) || i.ToLower().EndsWith(".com");
                            string[] errors = isOk ? new string[] { } : new string[] { "Does not end with .com but should" };
                            return new ValidationDataErrorInfo<string>(value: i, errors: errors);
                        })
                        .ObserveOnDispatcher();
                });
            NicknameToAdd = new Revertible<string>(
                value: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Nicknames = new Revertible<ImmutableList<NicknameReport>>(
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
            AddNickname = new DelegateCommand(
                action: () => {
                    string nameToAdd = NicknameToAdd.Value;
                    NicknameReport report = new NicknameReport(
                        name: nameToAdd,
                        delete: new DelegateCommand(action: () => {
                            var item = Nicknames.Value.First(j => j.Name == nameToAdd);
                            Nicknames.Value = Nicknames.Value.Remove(item);
                        }));
                    Nicknames.Value = Nicknames.Value.Add(report);
                    NicknameToAdd.Value = string.Empty;
                },
                canExecute: NicknameToAdd.Errors.Select(i => i.Status == ValidationStatus.IsValid));
            Errors = new Property<ValidationDataErrorInfo<Person>>(
                value: new ValidationDataErrorInfo<Person>(value: this, status: ValidationStatus.Blocked, descendentStatus: ValidationStatus.None),
                values: Observable
                    .CombineLatest(new IValidate[] { Address, FirstName, LastName, FullName, IsMarried, Nicknames, Website }.Select(i => i.Errors))
                    .Select(i => i.Select(j => j.CompositeStatus))
                    .Select(i => i.Aggregate((a, b) => a | b))
                    .Select(i => new ValidationDataErrorInfo<Person>(
                        value: this,
                        status: ValidationStatus.IsValid,
                        descendentStatus: i)));
            // NOTE: Not all properties affect whether can accept the form; subforms like nicknameToAdd!
            HasChanges = new Property<bool>(
                value: false,
                values: Observable
                    .CombineLatest(ChangeTrackers().Select(i => i.HasChanges))
                    .Select(i => i.Any(j => j == true)));
            AcceptAll = new DelegateCommand(
                action: () => { AcceptChanges(); },
                canExecute: Observable.CombineLatest(Errors, HasChanges, (errs, chg) => chg && errs.HasErrors == false));
            RejectAll = new DelegateCommand(
                action: () => { RejectChanges(); },
                canExecute: HasChanges);
            AddToDisposables(FirstName, LastName, FullName, MarriageYear, IsMarried, HasChanges, Website, Nicknames, NicknameToAdd, Website, Errors);
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

        public Revertible<string> FirstName { get; }
        public Revertible<string> LastName { get; }
        public Property<string> FullName { get; }
        public Revertible<bool> IsMarried { get; }
        public Revertible<string> Website { get; }
        public Revertible<int> MarriageYear { get; }
        public DelegateCommand AddNickname { get; }
        public Revertible<string> NicknameToAdd { get; }
        public Revertible<ImmutableList<NicknameReport>> Nicknames { get; }
        public Address Address { get; }
        public ICommand AcceptAll { get; }
        public ICommand RejectAll { get; }
        public IReadOnlyProperty<bool> HasChanges { get; }
        public IReadOnlyProperty<IValidationDataErrorInfo<Person>> Errors { get; }
        IReadOnlyProperty<IValidationDataErrorInfo> IValidate.Errors => Errors;
    }
}
