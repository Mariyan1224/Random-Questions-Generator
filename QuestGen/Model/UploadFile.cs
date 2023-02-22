using QuestGen.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QuestGen.Model
{
    public class UploadFile : INotifyPropertyChanged
    {
        private int _CountQuestionsPerFile = 1;
        private bool _IsInputCountQuestionsPerFileIncorrect;

        public virtual event PropertyChangedEventHandler PropertyChanged;
        private ViewModel_Question ViewModel { get; set; }
        public string FileName
        {
            get; internal set;
        }
        public bool IsInputCountQuestionsPerFileIncorrect
        {
            get => _IsInputCountQuestionsPerFileIncorrect;
            set
            {
                if (_IsInputCountQuestionsPerFileIncorrect != value)
                    _IsInputCountQuestionsPerFileIncorrect = value;
                OnPropertyChanged();
            }
        }
       public string CountQuestionsPerFile
        {
            get => _CountQuestionsPerFile.ToString();
            set
            {
                if (Regex.Match(value, @"^\d{2}$").Success || Regex.Match(value, @"^\d{1}$").Success)
                {
                    if (int.Parse(value) >= 1)
                    {
                        _CountQuestionsPerFile = int.Parse(value);
                        IsInputCountQuestionsPerFileIncorrect = false;
                    }
                    else
                        IsInputCountQuestionsPerFileIncorrect = true;
                }
                else
                    IsInputCountQuestionsPerFileIncorrect = true;
            }
        }
        public UploadFile(string fileName)
        {
            FileName = fileName;
        }
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
