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
using ReactiveUI;
using System.Reactive.Subjects;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reactive.Concurrency;

namespace Client {
    public class Person {
        Property<string> _firstName;
        Property<string> _lastName;
        Property<string> _url;
        CalculatedProperty<string> _fullName;
        CalculatedProperty<bool> _isNameValid;
        ValidatedProperty<string> _city;
        Command<Unit, Unit> _cityAccept;
        Command<Unit, Unit> _cityRevert;
        CalculatedProperty<int> _randomNumber;
        Command<object, int> _randomNumberCommand;
        Command<object, Unit> _longTask;
        Property<string> _nicknameToAdd;
        ValidatedProperty<ImmutableList<string>> _nicknames;
        ObservableCollection<NicknameReport> _nicknamesForBinding;
        Command<Unit, Unit> _nicknameAdd;

        public Person() {
            _firstName = new Property<string>(string.Empty);
            _lastName = new Property<string>(string.Empty);
            _city = new ValidatedProperty<string>(string.Empty, i => !string.IsNullOrWhiteSpace(i) && !i.Contains(" ") && i.Length > 3);
            _url = new Property<string>(string.Empty);
            _fullName = new CalculatedProperty<string>(
                initialValue: string.Empty,
                values: _firstName.Values.CombineLatest(_lastName.Values, (a, b) => $"{a} {b}"));
            _isNameValid = new CalculatedProperty<bool>(
                initialValue: false,
                values: Observable.CombineLatest(_firstName.Values, _lastName.Values, _fullName.Values, (first, last, full) => {
                    return !string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last) && (full.Length > 5) && full.Length < 20;
                }));
            _cityAccept = new Command<Unit, Unit>(
                execute: (Unit u) => { _city.Accept(); return Task.FromResult<Unit>(Unit.Default); },
                canExecute: Observable.CombineLatest(_city.IsChanged.Values, _city.IsValid.Values, (changed, valid) => changed && valid));
            _cityRevert = new Command<Unit, Unit>(
                execute: (Unit u) => { _city.Revert(); return Task.FromResult<Unit>(Unit.Default); },
                canExecute: _city.IsChanged.Values);
            _longTask = new Command<object, Unit>(
                execute: async (object parameter) => {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    return Unit.Default;
                },
                canExecute: Observable.Return<bool>(true));
            _randomNumberCommand = new Command<object, int>(
                execute: (object p) => Task.FromResult<int>(new Random().Next()),
                canExecute: Observable.Return<bool>(true));
            _randomNumber = new CalculatedProperty<int>(0, _randomNumberCommand);
            _nicknames = new ValidatedProperty<ImmutableList<string>>(
                value: ImmutableList<string>.Empty,
                validator: (ImmutableList<string> v) => (v.Count == v.Distinct().Count()));
            _nicknameToAdd = new Property<string>(string.Empty);
            _nicknameAdd = new Command<Unit, Unit>(
                execute: (p) => {
                    _nicknames.Current.Value = _nicknames.Current.Value.Add(_nicknameToAdd.Value);
                    _nicknameToAdd.Value = string.Empty;
                    return Task.FromResult<Unit>(Unit.Default);
                },
                canExecute: _nicknameToAdd.Values.Select(i => !string.IsNullOrWhiteSpace(i)));
            _nicknamesForBinding = new ObservableCollection<NicknameReport>();
            _nicknames.Current.Changed.Subscribe(i => {
                var added = i.After.Except(i.Before);
                var removed = i.Before.Except(i.After);
                foreach (var n in added) {
                    NicknameReport report = new NicknameReport(
                        name: n,
                        delete: new Command<Unit, Unit>(
                            execute: (Unit p) => {
                                _nicknames.Current.Value = _nicknames.Current.Value.Remove(n);
                                return Task.FromResult<Unit>(Unit.Default);
                            },
                            canExecute: Observable.Return<bool>(true)));
                    _nicknamesForBinding.Add(report);
                }
                foreach (var n in removed) {
                    _nicknamesForBinding.Remove(_nicknamesForBinding.First(j => j.Name == n));
                }
            });
        }

        public class NicknameReport {
            public NicknameReport(string name, ICommand delete) {
                Name = name;
                Delete = delete;
            }
            public string Name { get; }
            public ICommand Delete { get; }
        }

        public Property<string> FirstName => _firstName;
        public Property<string> LastName => _lastName;
        public Property<string> FullName => _fullName;
        public Property<string> Url => _url;
        public CalculatedProperty<bool> IsNameValid => _isNameValid;
        public ValidatedProperty<string> City => _city;
        public ICommand CityAccept => _cityAccept;
        public ICommand CityRevert => _cityRevert;
        public ICommand LongTask => _longTask;
       public ICommand CreateRandomNumber => _randomNumberCommand;
        public CalculatedProperty<int> RandomNumber => _randomNumber;
        public IEnumerable<NicknameReport> Nicknames => _nicknamesForBinding;
        public Property<string> NicknameToAdd => _nicknameToAdd;
        public ICommand AddNickname => _nicknameAdd;
        public CalculatedProperty<string> Clock { get; } =
            new CalculatedProperty<string>(
                initialValue: DateTime.Now.ToString(),
                values: Observable.Interval(TimeSpan.FromSeconds(1)).Select(i => DateTime.Now.ToString()).ObserveOnDispatcher());
    }

    public class Status<T> {
        public Status(T value, Func<T, bool> validator) {
            Value = value;
            IsValid = validator(value);
        }

        public T Value { get; }
        public bool IsValid { get; }
    }

    public class Validator<T> : Property<Status<T>> {
        Func<T, bool> _validator;

        public Validator(T value, Func<T, bool> validator) : base(new Status<T>(value, validator)) {
            _validator = validator;
        }

        public new T Value
        {
            get { return base.Value.Value; }
            set { base.Value = new Status<T>(value, _validator); }
        }
    }

    public interface IPropertyDetails<T> {
        T Value { get; }
        bool IsChanged { get; }
        bool IsValid { get; }
    }
    public interface IProperty<T> : INotifyPropertyChanged {
        void Revert();
        void Accept();
        T Value { get; set; }
        IObservable<IPropertyDetails<T>> Values { get; }
        IObservable<Delta<IPropertyDetails<T>>> Changing { get; }
        IObservable<Delta<IPropertyDetails<T>>> Changed { get; }
    }

    // value changes atomically with validity!
    // can't raise event on one without the other

    //public class Revertible<T> {
    //    public Revertible(T current, T original, Func<T, bool> validator) {
    //        Current = new Property<T>(value: current);
    //        Original = new Property<T>(value: original);
    //        IsChanged = Observable.CombineLatest(Current.Values, Original.Values, (o, c) => !Equals(o, c)).DistinctUntilChanged();
    //        IsValid = Current.Values.Select(i => validator(i));
    //    }

    //    public Property<T> Current { get; }

    //    public Property<T> Original { get; }

    //    public T Value
    //    {
    //        get { return Current.Value; }
    //        set { Current.Value = value; }
    //    }

    //    public void Accept() => Original.Value = Current.Value;

    //    public void Reject() => Current.Value = Original.Value;

    //    public IObservable<bool> IsChanged { get; }

    //    public IObservable<bool> IsValid { get; }

    //}


    //public class Validated<T> : Property<T> {
    //    public Validated(T value, Func<T,bool> validator) {

    //    }
    //    public T Value { get; }
    //    public bool IsValid { get; }
    //}

    //public class Address {
    //    Revertible<int> _zip = new Revertible<int>(0, 0, (i) => i > 1000);

    //    ValidatedProperty<string> _street = new ValidatedProperty<string>(
    //        value: string.Empty,
    //        validator: (string i) => !string.IsNullOrWhiteSpace(i));

    //    public Address() {
    //        //_street.IsValid.Values // whether the streets are valid
    //        //_street.Current.Values // the streets
    //        _zip.IsValid
    //    }
    //}
}
