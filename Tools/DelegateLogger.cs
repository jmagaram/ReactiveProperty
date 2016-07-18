using System;

namespace Tools {
    public class DelegateLogger : ILogger {
        Action<string> _printer;
        public DelegateLogger(Action<string> printer) {
            _printer = printer;
        }

        public void Log(string input) => _printer(input);
    }
}
