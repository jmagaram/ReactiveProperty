using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Client {
    //public class Address {
    //    public Address() {

    //    }
    //    Property<string, StringError> Street { get; }
    //    Property<string, StringError> City { get; }
    //}

    // read-only version
    //IValidationErrorData<TError>, ITrackChanges, INotifyPropertyChanged, IDisposable

    // some stuff for data binding ENTITY, probably same as for Property
    // some stuff for internally calculating what to do with the component, ESPECIALLY IF
    // have a collection of a bunch of IProperty items (a general purpose model) and want to revert, etc.
    // without knowing the specific types

    // know if address changes in case there is a cross property validation rule where we 
    // check address content; address might be fine by itself
    // know if address has any errors (to know if entity is ok)
    // know if address has any changes (for reverting)
    // ability to accept and reject
    //
    // model must expose an IObservable of various property statuses

}
