using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using QuestGen.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using QuestGen.ViewModel.Commands;

namespace QuestGen.ViewModel
{
    public class ViewModel_Question : INotifyPropertyChanged
    {
        
        private int _fileId = -1;
        private int _GroupsCount = 1;
        private int _QuestionsPerGroup = 4;

        private bool _HasUploadedFiles;
        private bool _IsInputQuestionDataValid;
        private bool _IsInputGroupDataIncorrect = true;
        private bool _IsInputQuestionGroupDataIncorrect = true;
        private bool _IsUploadEnabled;

        public event PropertyChangedEventHandler PropertyChanged;
        public UploadCommand UploadCommand { get; set; }
        public DeleteCommand DeleteCommand { get; set; }
        public ObservableCollection<UploadFile> ShowFileInfoCollection { get; set; }
        private ObservableCollection<FileResult> FileResultCollection { get; set; }


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
                UploadCommand.CanExecuteChanged += UploadCommand_CanExecuteChanged;
            }
        }

        private void UploadCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
        public bool IsInputQuestionDataValid
        {
            get => _IsInputQuestionDataValid;
            set
            {
                if (_IsInputQuestionDataValid != value)
                    _IsInputQuestionDataValid = value;
                OnPropertyChanged();
            }
        }
        public bool HasUploadedFiles
        {
            get => _HasUploadedFiles;
            internal set
            {
                if (_HasUploadedFiles != value)
                {
                    _HasUploadedFiles = value;
                }
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

        public ViewModel_Question()
        {
            UploadCommand = new UploadCommand(this);
            DeleteCommand = new DeleteCommand(this);
           // Generator = new Generator();
           // Question = new Question(Generator);
            ShowFileInfoCollection = new ObservableCollection<UploadFile>();
            FileResultCollection = new ObservableCollection<FileResult>();
           // UploadFile = new UploadFile(ShowFileInfoCollection, FileResultCollection);
        }

        public async void LoadFile()
        {
            try
            {
                IEnumerable<FileResult> pickResult = await FilePicker.PickMultipleAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            {DevicePlatform.UWP, new [] {".pdf", ".docx", ".txt"} }
                        }),
                        PickerTitle = "Please select a .pdf, .txt, or .docx file."
                    });
                if (pickResult == null)
                {
                    return;
                }
                AddPickResultInCollection(pickResult);
                foreach (FileResult file in FileResultCollection)
                {
                    if (!IfMyCollectionContains(file.FileName))
                    {
                        ShowFileInfoCollection.Add(new UploadFile(++_fileId, file.FileName));
                    }
                    
                }
                if(ShowFileInfoCollection.Count != 0 && !HasUploadedFiles)
                    HasUploadedFiles = true;
            }
            catch (Exception e)
            {
                throw new NotImplementedException(e.Message);
            }
        }
        public void DeleteFile(UploadFile uploadedfile)
        {
            ShowFileInfoCollection.Remove(uploadedfile);
            foreach (var file in FileResultCollection)
            {
                if (file.FileName == uploadedfile.FileName)
                {
                    FileResultCollection.Remove(file);
                    break;
                }
            }
            if (FileResultCollection.Count == 0)
                HasUploadedFiles = false;
        }
        private bool IfMyCollectionContains(string fileName)
        {
            for (int indx = 0; indx < ShowFileInfoCollection.Count; indx++)
            {
                if (ShowFileInfoCollection[indx].FileName == fileName)
                    return true;
            }
            return false;
        }
        private void AddPickResultInCollection(IEnumerable<FileResult> pickResult)
        {
            bool flag;
            foreach (var pickedFile in pickResult)
            {
                flag = true;
                foreach (var file in FileResultCollection)
                {
                    if (pickedFile.FileName == file.FileName)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    FileResultCollection.Add(pickedFile);
            }
        }
        private bool InputQuestionsDataValidated()
        {
            int countQuestionPerGroup = 0;
            for (int i = 0; i < ShowFileInfoCollection.Count; i++)
            {
                countQuestionPerGroup += int.Parse(ShowFileInfoCollection[i].CountQuestionsPerFile);
            }
            if (int.Parse(QuestionsPerGroup) == countQuestionPerGroup)
                return true;
            return false;
        }
    }
}
