using QuestGen.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace QuestGen.ViewModel.Commands
{
    public class DeleteCommand : Page, ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ViewModel_Question ViewModel { get; set; }

        public DeleteCommand(ViewModel_Question viewModel)
        {
            ViewModel = viewModel;
        }
        public  bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var file = parameter as UploadFile;
            bool result = await App.Current.MainPage.DisplayAlert("Warning", "Сигурни ли сте, че искате да изтриете файл " + file.FileName + "?", "Изтрий", "Отказ");
            if (result)
            {
                ViewModel.DeleteFile(file);
            }
        }
    }
}
