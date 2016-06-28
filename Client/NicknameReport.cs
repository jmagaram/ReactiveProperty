using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client {
    public class NicknameReport {
        public NicknameReport(string name, ICommand delete) {
            Name = name;
            Delete = delete;
        }
        public string Name { get; }
        public ICommand Delete { get; }
    }
}
