using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using System.Text;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Windows.Foundation;
using System.Diagnostics;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

//using System.Runtime.InteropServices.WindowsRuntime;

namespace QuestGen.Model
{
    public class Generator
    {
        //int CountGroups { get; set; }
        ObservableCollection<FileResult> RawFilesCollection { get; set; }
        ObservableCollection<UploadFile> FilesInfoCollection { get; set; }
        List<string> QuestionsInGroup { get; set; }

        WordDocument WordDocumentuDocx { get; set; }
        WSection section { get; set; }
        public Generator(ObservableCollection<FileResult> rawCollection, ObservableCollection<UploadFile> infoCollection)
        {
            RawFilesCollection = rawCollection;
            FilesInfoCollection = infoCollection;
            QuestionsInGroup = new List<string>();
        }
        //public async void Generating()
        //{
        //    //Pick a folder to save the file in
        //    string fileName = "Groups.docx";
        //    StorageFile file = null;

        //    try
        //    {
        //        file = await PickSaveFolder(fileName);
        //        if (file != null)
        //        {
        //            int countGroups = CountGroups;
        //            int fileCounter = RawFilesCollection.Count;
        //            WordDocument wordDocument = new WordDocument();
        //            WSection section = wordDocument.AddSection() as WSection;

        //            WParagraphStyle style = SetParagraphStylesToDocument(wordDocument);

        //            IWParagraph paragraph = section.HeadersFooters.Header.AddParagraph();

        //            // Create groups

        //            while (countGroups > 0)
        //            {
        //                for (int fileIndex = 0; fileIndex < fileCounter; fileIndex++)
        //                {
        //                    if (RawFilesCollection[fileIndex].FileName.EndsWith(".txt"))
        //                        ReadQuestionsFromTxTFileAsync(fileIndex);
        //                    if (RawFilesCollection[fileIndex].FileName.EndsWith(".docx"))
        //                        ReadQuestionsFromDocxFileAsync(fileIndex);
        //                }
        //                if (CountGroups > 0)
        //                {
        //                    WriteAListInDocxFile(section,countGroups);
        //                    --countGroups;
        //                }
        //            }

        //            // Save a docx file in a stream -> Storage file

        //            using (MemoryStream streamForSaveDocx = new MemoryStream())
        //            {
        //                wordDocument.Save(streamForSaveDocx, FormatType.Docx);
        //                using (IRandomAccessStream openStorageFile = await file.OpenAsync(FileAccessMode.ReadWrite))
        //                {
        //                    streamForSaveDocx.Position = 0;
        //                    using (Stream writeStream = openStorageFile.AsStreamForWrite())
        //                    {
        //                        //WordDocument wordDoc = new WordDocument(readStream, FormatType.Docx);
        //                        byte[] buffer = streamForSaveDocx.ToArray();
        //                        await writeStream.WriteAsync(buffer, 0, buffer.Length);
        //                        await writeStream.FlushAsync();
        //                    }
        //                }
        //            }
        //            bool result = await App.Current.MainPage.DisplayAlert("Open file", "Желаете ли да отворите файла?", "Да", "Не");
        //            if (result)
        //                await Windows.System.Launcher.LaunchFileAsync(file);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        await App.Current.MainPage.DisplayAlert("Warning", e.Message, "Oк");
        //    }
        //}
      
        public async Task ReadQuestionsFromTxTFileAsync(int fileIndex) //Read records from .txt file and write them in a List
        {
            
            var randomNum = new Random();
            byte[] buffer;
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
                    buffer = new byte[endPos - startPos];
                    int p = await fileStream.ReadAsync(buffer, 0, endPos - startPos);
                    record = Encoding.UTF8.GetString(buffer).ToString();
                        
                    if (!QuestionsInGroup.Contains(record)) //Check if record already exists in the List 
                    {
                        questionsCount--;
                        QuestionsInGroup.Add(record);
                    }
                    record = null;
                    buffer = null;
                }
            }
        }
        public async Task ReadQuestionsFromDocxFileAsync(int fileIndex)
        {
            int questionsCount = int.Parse(FilesInfoCollection[fileIndex].CountQuestionsPerFile);
            TextSelection[] SelectAllDocsQuestions;
            Random random = new Random();
            int randQuesIdx;
            using (WordDocument wordDocument = new WordDocument(await RawFilesCollection[fileIndex].OpenReadAsync(), FormatType.Docx))
            {
                Regex pattern = new Regex("(^.+)");
                SelectAllDocsQuestions = wordDocument.FindAll(pattern);
                if (SelectAllDocsQuestions == null)
                    return;
                while (questionsCount > 0)
                {
                    randQuesIdx = random.Next(SelectAllDocsQuestions.Length);
                    if (!QuestionsInGroup.Contains(SelectAllDocsQuestions[randQuesIdx].SelectedText))
                    {
                        QuestionsInGroup.Add(SelectAllDocsQuestions[randQuesIdx].SelectedText);
                        --questionsCount;
                    }
                }
            }
        }
        public void WriteAListInDocxFile(int countGroup)
        {
            section = WordDocumentuDocx.AddSection() as WSection;
            section.BreakCode = SectionBreakCode.NewPage;
            IWParagraph paragraph = section.AddParagraph();
            paragraph.ApplyStyle("Heading 1");
            paragraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            WTextRange textRange = paragraph.AppendText("Име: ........................................... .............................................. | клас .........") as WTextRange;
            textRange.CharacterFormat.FontSize = 15f;
            textRange.CharacterFormat.FontName = "Calibri";
            textRange.CharacterFormat.TextColor = Color.Black;
            paragraph = section.AddParagraph();
            paragraph = section.AddParagraph();
            paragraph.ApplyStyle("Heading 1");
            paragraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            textRange = paragraph.AppendText("Тема " + countGroup) as WTextRange;
            textRange.CharacterFormat.FontSize = 18f;
            textRange.CharacterFormat.FontName = "Calibri";
            textRange.CharacterFormat.TextColor = Color.Black;
            section.AddParagraph();
            for (int indx = 0; indx < QuestionsInGroup.Count; indx++)
            {
                paragraph = section.AddParagraph();
                paragraph.ApplyStyle("Normal");
                textRange = paragraph.AppendText(indx + 1 + ". " + QuestionsInGroup[indx]) as WTextRange;
            }
           
            QuestionsInGroup.Clear();
           
        }
        public async Task<StorageFile> PickSaveFolder(string fileName)
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");
            try
            {
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    return file;
                }
            }catch(Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Warning", e.Message, "Oк");
            }
            return null;

            //StorageFile file = null;
            //var savePicker = new FileSavePicker
            //{
            //    SuggestedStartLocation = PickerLocationId.Desktop,
            //    CommitButtonText = "Save File",
            //    SuggestedFileName = "GeneratingGroups.pdf",
                
            //    // SuggestedSaveFile = ApplicationData.Current.LocalFolder.CreateFileAsync("GeneratingGroups")
            //};
            //// Dropdown of file types the user can save the file as 
            //savePicker.FileTypeChoices.Add("Pdf Document", new List<string>() { ".pdf" });
            
            //try
            //{
            //    //var fileS =await savePicker.PickSaveFileAsync();
            //    //if (fileS != null)
            //    //{
            //    //   
            //    //}
            //}
            //catch (Exception e)
            //{
            //    await App.Current.MainPage.DisplayAlert("Warning", e.Message, "Oк");
            //}
        }
        public void CreateDocxDocument()
        {
            WordDocumentuDocx = new WordDocument();
           // section = WordDocumentuDocx.AddSection() as WSection;

            SetParagraphStylesToDocument();

           // IWParagraph paragraph = section.HeadersFooters.Header.AddParagraph();
        }
        void SetParagraphStylesToDocument()
        {
            WParagraphStyle style = WordDocumentuDocx.AddParagraphStyle("Normal") as WParagraphStyle;

            style.CharacterFormat.FontName = "Calibri";

            style.CharacterFormat.FontSize = 12f;

            style.ParagraphFormat.BeforeSpacing = 0;

            style.ParagraphFormat.AfterSpacing = 8;

            style.ParagraphFormat.LineSpacing = 13.8f;

            style = WordDocumentuDocx.AddParagraphStyle("Heading 1") as WParagraphStyle;

            style.ApplyBaseStyle("Normal");

            style.CharacterFormat.FontName = "Calibri Light";

            style.CharacterFormat.FontSize = 16f;

            style.CharacterFormat.TextColor = Color.FromArgb(46, 116, 181);

            style.ParagraphFormat.BeforeSpacing = 12;

            style.ParagraphFormat.AfterSpacing = 0;

            style.ParagraphFormat.Keep = true;

            style.ParagraphFormat.KeepFollow = true;

            style.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
        }
        public async Task SaveDocxDocumentInFile(StorageFile file)
        {
            using (MemoryStream streamForSaveDocx = new MemoryStream())
            {
                WordDocumentuDocx.Save(streamForSaveDocx, FormatType.Docx);
                using (IRandomAccessStream openStorageFile = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    streamForSaveDocx.Position = 0;
                    using (Stream writeStream = openStorageFile.AsStreamForWrite())
                    {
                        byte[] buffer = streamForSaveDocx.ToArray();
                        await writeStream.WriteAsync(buffer, 0, buffer.Length);
                        await writeStream.FlushAsync();
                    }
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
                    Regex pattern = new Regex("(?m:$)");
                    TextSelection[] selection = document.FindAll(pattern);
                    document.Close();
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
