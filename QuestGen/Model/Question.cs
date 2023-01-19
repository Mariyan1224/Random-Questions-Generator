using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QuestGen.Model
{
    class Question 
    {
        
        
        private Generator _generator;

       // public event PropertyChangedEventHandler PropertyChanged;

        public Question(Generator generator)
        {

            _generator = generator;
        }
        //private void OnPropertyChanged([CallerMemberName] string name = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //}
       
    }
}
