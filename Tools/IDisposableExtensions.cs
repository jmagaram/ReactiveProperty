using System;
using System.Collections.Generic;

namespace Tools {
    public static class IDisposableExtensions {
        public static void AddTo(this IDisposable item, ICollection<IDisposable> collection) => collection.Add(item);
        public static void RemoveFrom(this IDisposable item, ICollection<IDisposable> collection) => collection.Remove(item);
    }
}
