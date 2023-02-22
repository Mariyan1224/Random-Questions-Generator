using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Windows.Storage;
using System.Text;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;

namespace QuestGen.Model
{
    public class Generator
    {
        int CountGroups { get; set; }
       // int CountQuestionsPerFile { get; set; } //Not nesessery
        ObservableCollection<FileResult> RawFilesCollection { get; set; }
        ObservableCollection<UploadFile> FilesInfoCollection { get; set; }
        List<string> QuestionsInGroup { get; set; }
        SortedList<int, int> StartEndPositionsList { get; set; }
        public Generator(ObservableCollection<FileResult> rawCollection, ObservableCollection<UploadFile> infoCollection, int groups)//, int questions)
        {
            RawFilesCollection = rawCollection;
            FilesInfoCollection = infoCollection;
            CountGroups = groups;
            QuestionsInGroup = new List<string>();
            StartEndPositionsList = new SortedList<int,int>();
        }
        public void Generating()
        {
            //OpenReadFile
            //
            int fileLength = RawFilesCollection.Count;
            for (int fileIndex = 0; fileIndex < fileLength; fileIndex++)
            {
                if (RawFilesCollection[fileIndex].FileName.EndsWith(".txt"))
                    ReadQuestionsFromTxTFile(fileIndex);
                if (RawFilesCollection[fileIndex].FileName.EndsWith(".docx")) { }
                    //Todo
                    // var p = await RawFilesCollection[i].OpenReadAsync();
                    //   p.
            }
        }
        async void ReadQuestionsFromTxTFile(int fileIndex) //Read records from .txt file and write them in List
        {
            
            var randomNum = new Random();
            int questionsCount = int.Parse(FilesInfoCollection[fileIndex].CountQuestionsPerFile);
            int count, checkValue, startPos, parcialRecordValue, endPos, i;
            string record = null;
            using (Stream fileStream = await RawFilesCollection[fileIndex].OpenReadAsync())
            {
                while (questionsCount > 0)
                {
                    count = randomNum.Next((int)fileStream.Length - 1);
                    fileStream.Position = count;
                    checkValue = fileStream.ReadByte();
                    fileStream.Position--;
                    do                                      //Find the begining of a record
                    {
                        while (checkValue != 10)            //10 = new line 13 = begining of a line
                        {
                            if (count > 0)
                            {
                                // backward till you get value 10
                                fileStream.Seek(--count, SeekOrigin.Begin);
                                checkValue = fileStream.ReadByte();
                                fileStream.Position--;
                            }
                            else
                                break;
                        }
                        if (count > 2)
                        {
                            fileStream.Position = fileStream.Position - 2;  ///Check if it is at the begining of the question which means sequence of 13 10 13 10
                            checkValue = fileStream.ReadByte();
                            if (checkValue == 10)
                                fileStream.Position += 2;
                        }
                        else
                        {
                            fileStream.Position = 0;
                            break;
                        }

                    } while (checkValue != 10);
                    //Congrats!!! You are at the begining of the question
                    //Start reading question till you find twice 13
                    startPos = (int)fileStream.Position;

                    parcialRecordValue = fileStream.ReadByte();
                    do                                           //Find the end of a record
                    {
                        while (parcialRecordValue != 13 && fileStream.Position < fileStream.Length)
                        {
                            fileStream.Seek(fileStream.Position, SeekOrigin.Begin);  //forward till you get 13
                            parcialRecordValue = fileStream.ReadByte();
                        }
                        if (fileStream.Position > fileStream.Length) // when out of the file
                            break;
                        fileStream.Position += 1;
                        parcialRecordValue = fileStream.ReadByte();
                    } while (parcialRecordValue != 13);

                    if (fileStream.Position < fileStream.Length)
                        endPos = (int)fileStream.Position -  3;   // You are at the end of a record
                    else
                        endPos = (int)fileStream.Length; // You are at the end of the file

                    fileStream.Seek(startPos, SeekOrigin.Begin); //Read whole record
                    for (i = startPos; i < endPos; i++)
                    {
                        record += (char)fileStream.ReadByte();
                    }
                    //if (QuestionsInGroup.Count == 0)
                    //{
                    //    StartEndPositionsList.Add(startPos,endPos);
                    //}
                        
                    if (!QuestionsInGroup.Contains(record)) //Check if record already exists in the List 
                    {
                        questionsCount--;
                        QuestionsInGroup.Add(record);
                    }
                    record = null;
                }
            }
        }
        public async Task<bool> AreFilesValidatedAsync()
        {
            string failedFileReadingNames = "\n";
            int fileLength = RawFilesCollection.Count;
            for (int fileIndex = 0; fileIndex < fileLength; fileIndex++)
            {
                if (RawFilesCollection[fileIndex].FileName.EndsWith(".txt"))
                {
                    if (!await IsTxtFileValidatedAsync(fileIndex))
                        failedFileReadingNames += RawFilesCollection[fileIndex].FileName + "\n";
                }
                else //if(RawFilesCollection[fileIndex].FileName.EndsWith(".docx"))
                    if (!await IsDocxFileValidatedAsync(fileIndex))
                        failedFileReadingNames += RawFilesCollection[fileIndex].FileName + "\n";
                // else if()...

            }
            if (failedFileReadingNames != "\n")
            {
                await App.Current.MainPage.DisplayAlert("Warning", "Съдържанието на следните файлове: \n"+ failedFileReadingNames + "\n не отговаря на зададените стандарти. " +
                    "\n Моля, свържете се с центъра за обслужване на клиенти.", "Oк");
                return false;
            }
            return true;
        }
        private async Task<bool> IsTxtFileValidatedAsync(int fileIndex)
        {
            string word;
            int countQustions = 0;
            MatchCollection readQuestionsInFileCountCollection;
            using (Stream fileStream = await RawFilesCollection[fileIndex].OpenReadAsync())
            {
                byte[] buffer = new byte[fileStream.Length];
                try
                {
                    fileStream.Read(buffer, 0, (int)fileStream.Length);
                    word = Encoding.UTF8.GetString(buffer);
                }
                catch(Exception e)
                {
                    fileStream.Dispose();
                    return false;
                }
                
            }
            readQuestionsInFileCountCollection = Regex.Matches(word, "\r\n\r\n\r\n");
            foreach (Match item in readQuestionsInFileCountCollection)
            {
                ++countQustions;
            }
            if(countQustions + 1 >= int.Parse(FilesInfoCollection[fileIndex].CountQuestionsPerFile) && countQustions != 0)
                return true;
            return false;
        }
        private async Task<bool> IsDocxFileValidatedAsync(int fileIndex)
        {
            int countQustions = 0;
            using (Stream fileStream = await RawFilesCollection[fileIndex].OpenReadAsync())
            {
                try
                {
                    WordDocument document = new WordDocument(fileStream, FormatType.Docx);
                    string fileRead = document.GetText();
                    Regex pattern = new Regex("(?m:$)");
                    TextSelection[] selection = document.FindAll(pattern);
                    if (selection != null)
                    {
                        for (int i = 0; i < selection.Length; i++)
                            ++countQustions;
                        if (countQustions >= int.Parse(FilesInfoCollection[fileIndex].CountQuestionsPerFile) && countQustions != 0)
                            return true;
                    }
                }
                catch (Exception e)
                {
                    fileStream.Dispose();
                    return false;
                }
            }
            return false;
        }
    }
}
