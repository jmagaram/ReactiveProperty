using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Tools {
    public class ValidatedProperty<T> {
        Property<T> _original;
        Property<T> _current;
        CalculatedProperty<bool> _isValid;
        CalculatedProperty<bool> _isChanged;
        Func<T, bool> _validator;

        public ValidatedProperty(T value, Func<T,bool> validator) {
            _original = new Property<T>(value);
            _current = new Property<T>(value);
            _validator = validator;
            _isValid = new CalculatedProperty<bool>(
                initialValue: false,
                values: _current.Values.Select(_validator));
            _isChanged = new CalculatedProperty<bool>(
                initialValue: false,
                values: Observable.CombineLatest(_original.Values, _current.Values, (o, c) => !Equals(o, c)));
        }

        public void Revert() => Current.Value = Original.Value;
        public void Accept() => Original.Value = Current.Value;
        public CalculatedProperty<bool> IsChanged => _isChanged;
        public Property<T> Original => _original;
        public Property<T> Current => _current;
        public CalculatedProperty<bool> IsValid => _isValid;
    }
}
