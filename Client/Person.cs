using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Reactive;

namespace Client {
    public class Person : Model, IRevertible, IValidate<Person> {
        public Person() {
            GenerateRandomNumber = new AsyncCommand<object, int>(
                execute: async (token, parameter) => {
                    Random r = new Random();
                    await Task.Delay(TimeSpan.FromSeconds(r.Next(0, 5)));
                    if (r.Next() % 3 == 0) {
                        throw new InvalidOperationException("Weird unexpected error!");
                    }
                    else {
                        return new Random().Next(0, 100);
                    }
                },
                canExecute: null,
                initialCanExecute: true);
            RandomNumber = new Property<string>(
                initialValue: string.Empty,
                values:
                    GenerateRandomNumber
                    .SelectMany(i => i
                        .Materialize()
                        .Where(j => j.Kind == NotificationKind.OnNext || j.Kind == NotificationKind.OnError)
                        .Select(j => j.Kind == NotificationKind.OnError ? "error" : j.Value.ToString()))
                    .Select(i => i.ToString())
                    .ObserveOnDispatcher());
            Address = new Address();
            FirstName = new Revertible<string>(
                initialValue: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            LastName = new Revertible<string>(
                initialValue: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3, maxLength: 10).Validate);
            FullName = new Property<string>(
                initialValue: string.Empty,
                values: Observable.CombineLatest(FirstName, LastName, (f, l) => $"{f} {l}"));
            IsMarried = new Revertible<bool>(initialValue: false);
            MarriageYear = new Revertible<int>(
                initialValue: 2000,
                validator: new RangeValidator<int>(minimum: 1900, maximum: DateTime.Now.Year).Validate);
            Website = new Revertible<string>(
                initialValue: "www.google.com",
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
                initialValue: string.Empty,
                validator: new StringValidator(isRequired: true, minLength: 3).Validate);
            Nicknames = new Revertible<ImmutableList<NicknameReport>>(
                initialValue: ImmutableList<NicknameReport>.Empty,
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
                initialValue: new ValidationDataErrorInfo<Person>(value: this, status: ValidationStatus.Blocked, descendentStatus: ValidationStatus.None),
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
                initialValue: false,
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

        public Property<string> RandomNumber { get; }
        public AsyncCommand<object, int> GenerateRandomNumber { get; }
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
