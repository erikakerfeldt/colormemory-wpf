using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMemory
{
    public class CommandViewModel : ViewModelBase
    {
        public CommandViewModel(string name, Action<object> execute, Func<object, bool> canExecute = null)
            : base(name)
        {
            Command = new RelayCommand(execute, canExecute);
        }

        public RelayCommand Command
        {
            get;
            private set;
        }
    }
}
