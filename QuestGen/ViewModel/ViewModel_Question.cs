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
using System.Threading.Tasks;
using Windows.Storage;
using Syncfusion.DocIO.DLS;
using Windows.Storage.Streams;
using Syncfusion.DocIO;

namespace QuestGen.ViewModel
{
    public class ViewModel_Question : INotifyPropertyChanged
    {

        private int _GroupsCount = 1;
        private int _QuestionsPerGroup = 1;

        private bool _HasUploadedFiles;
        private bool _IsInputGroupDataIncorrect = true;
        private bool _IsInputQuestionGroupDataIncorrect = true;
        private bool _IsUploadEnabled;

        public event PropertyChangedEventHandler PropertyChanged;
        public UploadCommand UploadCommand { get; set; }
        public DeleteCommand DeleteCommand { get; set; }
        public GenerateCommand GenerateCommand { get; set; }
        public ObservableCollection<UploadFile> ShowFileInfoCollection { get; set; }
        private ObservableCollection<FileResult> FileResultCollection { get; set; }
        private Generator Generator { get; set; }

        private UploadFile UploadFile { get; set; }

        public bool IsInputQuestionGroupDataIncorrect
        {
            get => _IsInputQuestionGroupDataIncorrect;
            private set
            {
                if (value != _IsInputQuestionGroupDataIncorrect)
                {
                    _IsInputQuestionGroupDataIncorrect = value;
                    if (!value && !IsInputGroupDataIncorrect && UploadFileEnabled())
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
                    if (!value && !IsInputQuestionGroupDataIncorrect && UploadFileEnabled())
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
        public string GroupsCount
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
        public string QuestionsPerGroupCount
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
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ViewModel_Question()
        {
            UploadCommand = new UploadCommand(this);
            DeleteCommand = new DeleteCommand(this);
            GenerateCommand = new GenerateCommand(this);
            ShowFileInfoCollection = new ObservableCollection<UploadFile>();
            FileResultCollection = new ObservableCollection<FileResult>();
            Generator = new Generator(FileResultCollection, ShowFileInfoCollection);
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
                            {DevicePlatform.UWP, new [] {".docx", ".txt"} }
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
                        ShowFileInfoCollection.Add(new UploadFile(file.FileName));
                    }
                    
                }
                IsUploadEnabled = UploadFileEnabled();
                if (ShowFileInfoCollection.Count != 0 && !HasUploadedFiles)
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
            if (int.Parse(QuestionsPerGroupCount) > FileResultCollection.Count)
                IsUploadEnabled = true;
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
                if (flag && int.Parse(QuestionsPerGroupCount) > FileResultCollection.Count)
                    FileResultCollection.Add(pickedFile);
                if (int.Parse(QuestionsPerGroupCount) == FileResultCollection.Count)
                {
                    IsUploadEnabled = false;
                    return;
                }
            }
        }
        bool UploadFileEnabled()
        {
            int countQuestionPerGroup = 0;
            int countLoadedFiles = ShowFileInfoCollection.Count;
            for (int i = 0; i < countLoadedFiles; i++)
            {
                countQuestionPerGroup += int.Parse(ShowFileInfoCollection[i].CountQuestionsPerFile);
            }
            if (int.Parse(QuestionsPerGroupCount) > countQuestionPerGroup)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async void GenerateGroups()
        {
            //

            //Pick a folder to save the file in
            string fileName = "Groups.docx";
            StorageFile file = null;
            try
            {
                file = await Generator.PickSaveFolder(fileName);
                if (file != null)
                {
                    int countGroups = int.Parse(GroupsCount);
                    int fileCounter = FileResultCollection.Count;
                    Generator.CreateDocxDocument();

                    // Create groups

                    while (countGroups > 0)
                    {
                        for (int fileIndex = 0; fileIndex < fileCounter; fileIndex++)
                        {
                            if (FileResultCollection[fileIndex].FileName.EndsWith(".txt"))
                                await Generator.ReadQuestionsFromTxTFileAsync(fileIndex);
                            if (FileResultCollection[fileIndex].FileName.EndsWith(".docx"))
                                await Generator.ReadQuestionsFromDocxFileAsync(fileIndex);
                        }
                        Generator.WriteAListInDocxFile(countGroups);
                            --countGroups;
                        
                    }
                    await Generator.SaveDocxDocumentInFile(file);
                    bool result = await App.Current.MainPage.DisplayAlert("Open file", "Желаете ли да отворите файла?", "Да", "Не");
                    if (result)
                        await Windows.System.Launcher.LaunchFileAsync(file);
                }
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Warning", e.Message, "Oк");
            }

            //
            //Generator.Generating();
        }
        public async Task<bool> ValidateFilesAsync()
        {
           return await Generator.AreFilesValidatedAsync();
        }
    }
}
