using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace QuestGen.ViewModel.Commands
{
    public class GenerateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ViewModel_Question ViewModel { get; set; }
        public GenerateCommand(ViewModel_Question viewModel)
        {
            ViewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            int countQuestionPerGroup = 0;
            int countLoadedFiles = ViewModel.ShowFileInfoCollection.Count;
            for (int i = 0; i < countLoadedFiles; i++)
            {
                if (ViewModel.ShowFileInfoCollection[i].IsInputCountQuestionsPerFileIncorrect)
                {

                    return;
                }
                countQuestionPerGroup += int.Parse(ViewModel.ShowFileInfoCollection[i].CountQuestionsPerFile);
            }
            if (int.Parse(ViewModel.QuestionsPerGroupCount) != countQuestionPerGroup)
            {
               await App.Current.MainPage.DisplayAlert("Warning", "Сумата на посочения брой въпроси от файловете трябва да бъде " +
                    "равна на общия брой въпроси за всяка група.", "Oк");
                return;
            }
            if (await ViewModel.ValidateFilesAsync())
                ViewModel.GenerateGroups();
           
        }
    }
}
