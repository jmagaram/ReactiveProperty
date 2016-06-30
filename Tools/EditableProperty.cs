using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools {
    public class EditableProperty<T> : PropertyBase<T>, IEditableProperty<T> {
        public EditableProperty(T value) : base(value: value) { }

        public new T Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }
    }
}
