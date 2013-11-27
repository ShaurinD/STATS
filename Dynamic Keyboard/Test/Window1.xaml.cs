﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;
using Aviad.WPF.Controls;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Microsoft.VisualBasic;


namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class Window1 : Window
    {
        public Keyboard curKeyboard;
        public KeyButton[] recentBoards;
        public int numBoards;
        public Button finishButton = new Button();
        public Button lettersButton = new Button();
        public Button wordsButton = new Button();
        public Button sentButton = new Button();
        string curFilename;
        public Dictionary<string, string> filenameToType = new Dictionary<string, string>();
        HashSet<string> usedFiles = new HashSet<string>();
     
        public Window1()
        {
            InitializeComponent();
            curFilename = "Standard.txt";
            filenameToType[curFilename] = "letter";
            curKeyboard = new Keyboard("Standard.txt", ref tb, ref Shift);
            curKeyboard.LoadKeyboard("Standard.txt", ref tb, filenameToType);
            ShowKeyboard("Standard.txt");
            tb.SelectionStart = 0;
            tb.TextWrapping = TextWrapping.Wrap;
            LeftArrow.Content = "<-";
            RightArrow.Content = "->";
            initializeAddKeyButtons();
            EnterButton.Click += EnterButton_Click;
            Backspace.Click += BackspaceButton_Click;
            Space.Click += SpaceButton_Click;
            LeftArrow.Click += LeftButton_Click;
            RightArrow.Click += RightButton_Click;
            Clear.Click += ClearButton_Click;
            Shift.Click += Shift_Click;
            Caps.Click += Caps_Click;
            SwitchKeyboard.Click += SwitchKeyboard_Click;
            AddKeyboard.Click += AddKeyboard_Click;
            loadSavedData();
        }

        private void initializeAddKeyButtons()
        {
            
            finishButton.Content = "Load";
            finishButton.Height = 72;
            finishButton.Width = 250;
            finishButton.HorizontalAlignment = HorizontalAlignment.Left;
            finishButton.VerticalAlignment = VerticalAlignment.Top;
            finishButton.FontSize = 32;
            finishButton.Click += finishButton_Click;
            lettersButton.Content = "letters";
            lettersButton.Height = 72;
            lettersButton.Width = 165;
            lettersButton.HorizontalAlignment = HorizontalAlignment.Left;
            lettersButton.VerticalAlignment = VerticalAlignment.Top;
            lettersButton.FontSize = 32;
            lettersButton.Click += lettersButton_Click;
            wordsButton.Content = "words";
            wordsButton.Height = 72;
            wordsButton.Width = 165;
            wordsButton.HorizontalAlignment = HorizontalAlignment.Left;
            wordsButton.VerticalAlignment = VerticalAlignment.Top;
            wordsButton.FontSize = 32;
            wordsButton.Click += wordsButton_Click;
            sentButton.Content = "sentences";
            sentButton.Height = 72;
            sentButton.Width = 220;
            sentButton.HorizontalAlignment = HorizontalAlignment.Left;
            sentButton.VerticalAlignment = VerticalAlignment.Top;
            sentButton.FontSize = 32;
            sentButton.Click += sentButton_Click;
        }

        void sentButton_Click(object sender, RoutedEventArgs e)
        {
            curFilename = Microsoft.VisualBasic.Interaction.InputBox("Please enter a name for the keyboard", "FileName", "new", -1, -1) + ".txt";
            saveData(curFilename, "sentence");
            filenameToType[curFilename] = "sentence";
            Process.Start(@"notepad.exe", "Keyboards//" + curFilename);
        }

        void wordsButton_Click(object sender, RoutedEventArgs e)
        {
            curFilename = Microsoft.VisualBasic.Interaction.InputBox("Please enter a name for the keyboard", "FileName", "new", -1, -1) + ".txt";
            filenameToType[curFilename] = "word";
            saveData(curFilename, "word");
            Process.Start(@"notepad.exe", "Keyboards//" + curFilename);
        }

        private void lettersButton_Click(object sender, RoutedEventArgs e)
        {
            curFilename = Microsoft.VisualBasic.Interaction.InputBox("Please enter a name for the keyboard", "FileName", "new", -1, -1) + ".txt";
            filenameToType[curFilename] = "letter";
            saveData(curFilename, "letter");
            Process.Start(@"notepad.exe", "Keyboards//" + curFilename);
        }
        
        private void clearKeyboard()
        {
            curKeyboard.HideKeys();
            col1.Children.Clear();
            col2.Children.Clear();
            col3.Children.Clear();
            col4.Children.Clear();
            col5.Children.Clear();
            col6.Children.Clear();
            col7.Children.Clear();
            col8.Children.Clear();
            col9.Children.Clear();
            col10.Children.Clear();
        }
        
        private void AddKeyboard_Click(object sender, RoutedEventArgs e)
        {
            clearKeyboard();
            col1.Children.Add(lettersButton);
            col3.Children.Add(wordsButton);
            col5.Children.Add(sentButton);
            col8.Children.Add(finishButton);
            TextBlock text = new TextBlock();
            text.Height = 200;
            text.Width = 845;
            text.FontSize = 16;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            text.VerticalAlignment = VerticalAlignment.Top;
            text.TextWrapping = TextWrapping.Wrap;
            text.Text = "INSTRUCTIONS : Select a type of keyboard to create above. Buttons will be bigger for sentences and smaller for letters."
            + " After selecting a type, you will be prompted to enter a name for the keyboard. Once you enter a keyboard name, a text file will open. In the text file type the buttons you want " +
            "to be displayed. Each line is expected to have two values, the first for the upper case value and the second for the lower case value. (Shift and Caps Lock will toggle these values) " + 
            "These values should be seperated by a |. Save " + 
            "the text file when you are done. Then press the load button to load the new keyboard. You must load the keyboard to access the new keyboard through the switch keyboard button.";
            col1.Children.Add(text);
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            if (!usedFiles.Contains(curFilename))
            {
                writeToMRK(curFilename);
                usedFiles.Add(curFilename);
            }
            clearKeyboard();
            ShowKeyboard(curFilename);
        }

        private void writeToMRK(string keyboardName)
        {
            string path = @"Config\MRK.txt";
            keyboardName = keyboardName.Substring(0, keyboardName.Length - 4);
            // This text is added only once to the file. 
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(keyboardName);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(keyboardName);
                }	
            }
        }

        private void saveData(string keyboardName, string type)
        {
            string path = @"Config\DKdata.txt";
            // This text is added only once to the file. 
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(keyboardName + " " + type);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(keyboardName + " " + type);
                }
            }
        }

        private void loadSavedData()
        {
            string path = @"Config\DKdata.txt";
            if (!File.Exists(path))
                return;
            string line;
            using (StreamReader sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    int index = line.IndexOf(" ");
                    string name = line.Substring(0, index);
                    string type = line.Substring(index + 1);
                    filenameToType[name] = type;
                }
            }
        }
        private void showKeyboardLetters()
        {
            for (int j = 0; j < curKeyboard.buttonCount; j++)
            {
                if (j / 3 == 0)
                    col1.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 1)
                    col2.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 2)
                    col3.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 3)
                    col4.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 4)
                    col5.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 5)
                    col6.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 6)
                    col7.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 7)
                    col8.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 8)
                    col9.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 9)
                    col10.Children.Add(curKeyboard.buttons[j]);
            }
        }

        private void showKeyboardWords()
        {
            for (int j = 0; j < curKeyboard.buttonCount; j++)
            {
                if (j / 3 == 0)
                    col1.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 1)
                    col3.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 2)
                    col5.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 3)
                    col7.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 4)
                    col9.Children.Add(curKeyboard.buttons[j]);
            }
        }

        private void showKeyboardSent()
        {
            for (int j = 0; j < curKeyboard.buttonCount; j++)
            {
                if (j / 3 == 0)
                    col1.Children.Add(curKeyboard.buttons[j]);
                else if (j / 3 == 1)
                    col6.Children.Add(curKeyboard.buttons[j]);
            }
        }

        public void ShowKeyboard(string filename)
        {
            for (int i = 0; i < numBoards; i++)
            {
                recentBoards[i].Visibility = Visibility.Collapsed;
            }
            curFilename = filename;
            curKeyboard.LoadKeyboard(filename, ref tb, filenameToType);
            if (filenameToType[filename] == "letter")
                showKeyboardLetters();
            else if (filenameToType[filename] == "word")
                showKeyboardWords();  
            else if (filenameToType[filename] == "sentence")
                showKeyboardSent();
        }

        private void LoadRecent()
        {
            List<string> boards = new List<string>();
            string line; int i = 0;
            using (StreamReader sr = new StreamReader(@"Config\MRK.txt"))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    boards.Add(line);
                    i++;
                }
            }
            numBoards = i;
            KeyButton[] recBut = new KeyButton[numBoards];
            for (int j = 0; j < numBoards; j++)
            {
                recBut[j] = new KeyButton(boards[j], this, false);
            }
            for (int j = 0; j < numBoards; j++)
            {
                if (j / 3 == 0)
                    col1.Children.Add(recBut[j]);
                else if (j / 3 == 1)
                    col2.Children.Add(recBut[j]);
                else if (j / 3 == 2)
                    col3.Children.Add(recBut[j]);
                else if (j / 3 == 3)
                    col4.Children.Add(recBut[j]);
                else if (j / 3 == 4)
                    col5.Children.Add(recBut[j]);
                else if (j / 3 == 5)
                    col6.Children.Add(recBut[j]);
                else if (j / 3 == 6)
                    col7.Children.Add(recBut[j]);
                else if (j / 3 == 7)
                    col8.Children.Add(recBut[j]);
                else if (j / 3 == 8)
                    col9.Children.Add(recBut[j]);
                else if (j / 3 == 9)
                    col10.Children.Add(recBut[j]);
            }
            for (int j = 0; j < numBoards; j++)
            {
                recBut[j].Visibility = Visibility.Visible;
            }
            recentBoards = recBut;
        }

        private void SwitchKeyboard_Click(object sender, RoutedEventArgs e)
        {
            clearKeyboard();
            LoadRecent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => unhighlight(Clear);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            Clear.Background = Brushes.Yellow;
            tb.Focus();
            tb.Clear();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => unhighlight(EnterButton);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            EnterButton.Background = Brushes.Yellow;
            int start = tb.SelectionStart;
            string firstHalf = tb.Text.Substring(0, start);
            string secondHalf = tb.Text.Substring(start);
            tb.Text = firstHalf + System.Environment.NewLine + secondHalf;
            tb.Focus();
            tb.SelectionStart = start + 1;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => unhighlight(Backspace);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            Backspace.Background = Brushes.Yellow;
            int start = tb.SelectionStart;
            if(tb.Text.Length != 0 && start >0)
            {
                if (start > 1 && tb.Text.Substring(start - 2, 2) == System.Environment.NewLine)
                {
                    string firstHalf = tb.Text.Substring(0, start-2);
                    string secondHalf = tb.Text.Substring(start);
                    tb.Text = firstHalf + secondHalf;
                    tb.SelectionStart = start - 2;
                }
                else
                {
                    string firstHalf = tb.Text.Substring(0, start- 1);
                    string secondHalf = tb.Text.Substring(start);
                    tb.Text = firstHalf + secondHalf;
                    tb.SelectionStart = start - 1;
                }
            }
            tb.Focus();
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => unhighlight(Space);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            Space.Background = Brushes.Yellow;
            int start = tb.SelectionStart;
            string firstHalf = tb.Text.Substring(0, start);
            string secondHalf = tb.Text.Substring(start);
            tb.Text = firstHalf + " " + secondHalf;
            tb.Focus();
            tb.SelectionStart = start + 1;
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => unhighlight(LeftArrow);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            LeftArrow.Background = Brushes.Yellow;
            int start = tb.SelectionStart;
            tb.Focus();
            if (start == 0)
                return;
            if (start > 1 && tb.Text.Substring(start - 2, 2) == System.Environment.NewLine)
                tb.SelectionStart = start - 2;
            else
                tb.SelectionStart = start - 1;
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => unhighlight(RightArrow);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            RightArrow.Background = Brushes.Yellow;
            int start = tb.SelectionStart;
            tb.Focus();
            tb.SelectionStart = start + 1;
        }

        private void Shift_Click(object sender, RoutedEventArgs e)
        {
            if (Caps.IsChecked == true)
            {
                Shift.IsChecked = false;
                return;
            }
            if (Shift.IsChecked == true)
            {
                curKeyboard.ToUpper();
            }
            else
            {
                curKeyboard.ToLower();
            }
        }

        private void Caps_Click(object sender, RoutedEventArgs e)
        {
            if (Shift.IsChecked == true)
                Shift.IsChecked = false;
            if (Caps.IsChecked == true)
            {
                curKeyboard.ToUpper();
            }
            else
            {
                curKeyboard.ToLower();
            }
        }

        private void unhighlight(Button btn)
        {
            btn.ClearValue(Button.BackgroundProperty);
        }
    }
}
