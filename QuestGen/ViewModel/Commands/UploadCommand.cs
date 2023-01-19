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
            var vm = parameter as ViewModel_Question;
            if (vm != null)
            {
                //if(ViewModel.ShowFileInfoCollection.Count <= int.Parse(ViewModel.QuestionsPerGroup))
                return !vm.IsInputQuestionGroupDataIncorrect && !vm.IsInputGroupDataIncorrect;
            }
            return false;
        }
            

        public void Execute(object parameter)
        {
            ViewModel.LoadFile();
        }
    }
}
