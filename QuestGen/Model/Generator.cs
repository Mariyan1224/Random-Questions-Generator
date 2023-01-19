using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QuestGen.Model
{
    public class Generator : INotifyPropertyChanged
    {
        private int _GroupsCount = 1;
        private int _QuestionsPerGroup = 4;
      
        private bool _IsInputGroupDataIncorrect = true;
        private bool _IsInputQuestionGroupDataIncorrect = true;
        private bool _IsUploadEnabled;

        public event PropertyChangedEventHandler PropertyChanged;
       
        public bool IsInputQuestionGroupDataIncorrect
        {
            get => _IsInputQuestionGroupDataIncorrect;
            private set
            {
                if (value != _IsInputQuestionGroupDataIncorrect)
                {
                    _IsInputQuestionGroupDataIncorrect = value;
                    if (!value && !IsInputGroupDataIncorrect)
                        IsUploadEnabled = true;
                    else
                        IsUploadEnabled = false;
                }
                OnPropertyChanged();
            }
        }
        public bool IsInputGroupDataIncorrect
        {
            get => _IsInputGroupDataIncorrect;
            private set
            {
                if (value != _IsInputGroupDataIncorrect)
                {
                    _IsInputGroupDataIncorrect = value;
                    if (!value && !IsInputQuestionGroupDataIncorrect)
                        IsUploadEnabled = true;
                    else
                        IsUploadEnabled = false;
                }
                OnPropertyChanged();
            }
        }
        public bool IsUploadEnabled
        {
            get => _IsUploadEnabled;
            set
            {
                if (_IsUploadEnabled != value)
                    _IsUploadEnabled = value;
                OnPropertyChanged();
            }
        }
        public string GroupsCounts
        {
            get => _GroupsCount.ToString();
            set
            {
                if (value != null && (Regex.Match(value, @"^\d{2}$").Success || Regex.Match(value, @"^\d{1}$").Success))
                {
                    if (int.Parse(value) >= 1)
                    {
                        _GroupsCount = int.Parse(value);
                        IsInputGroupDataIncorrect = false;
                    }
                    else
                        IsInputGroupDataIncorrect = true;
                }
                else
                    IsInputGroupDataIncorrect = true;
                
            }
        }
        public string QuestionsPerGroup
        {
            get => _QuestionsPerGroup.ToString();
            set
            {
                if (value != null && (Regex.Match(value, @"^\d{2}$").Success || Regex.Match(value, @"^\d{1}$").Success))
                {
                    if (int.Parse(value) >= 1)
                    {
                        _QuestionsPerGroup = int.Parse(value);
                        IsInputQuestionGroupDataIncorrect = false;
                    }
                    else
                        IsInputQuestionGroupDataIncorrect = true;
                }
                else
                    IsInputQuestionGroupDataIncorrect = true;
            }
        }
     
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
