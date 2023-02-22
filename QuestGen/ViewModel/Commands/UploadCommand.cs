using QuestGen.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace QuestGen.ViewModel.Commands
{
    public class UploadCommand : ICommand
    {
        public ViewModel_Question ViewModel { get; set; }
       
        public event EventHandler CanExecuteChanged;

        public UploadCommand(ViewModel_Question viewModel)
        {
            ViewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            if (parameter != null)
               return (bool)parameter;
            return false;
        }
            

        public void Execute(object parameter)
        {
            ViewModel.LoadFile();
        }
    }
}
