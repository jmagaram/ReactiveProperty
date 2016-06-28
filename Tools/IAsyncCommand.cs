using System.Threading.Tasks;
using System.Windows.Input;

namespace Tools {
    public interface IAsyncCommand : ICommand {
        Task ExecuteAsync(object parameter);
    }
}
