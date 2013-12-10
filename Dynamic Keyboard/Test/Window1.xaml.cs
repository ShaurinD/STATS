using System;
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
using System.ComponentModel;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Threading.Tasks;


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
        SpeechRecognitionEngine recoEngine = new SpeechRecognitionEngine();
        long lastTime = 0;
        bool voiceOn = false;

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
            LeftArrow.Content = "←";
            RightArrow.Content = "→";
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
            SaveText.Click += SaveText_Click;
            TextToSpeech.Click += TextToSpeech_Click;
            Calculator.Click += Calculator_Click;
            SpeechToText.Click += SpeechToText_Click;
            ExitButton.Click += Exit_Click;
            loadSavedData();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if ((currentTime - lastTime) < 5000)
            {
                this.Close();
            }
            lastTime = currentTime;
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
            finishButton.Background = Brushes.Black;
            finishButton.Foreground = Brushes.White;
            lettersButton.Background = Brushes.Black;
            lettersButton.Foreground = Brushes.White;
            wordsButton.Background = Brushes.Black;
            wordsButton.Foreground = Brushes.White;
            sentButton.Background = Brushes.Black;
            sentButton.Foreground = Brushes.White;
            finishButton.MouseEnter += AlphaButton_MouseEnter;
            finishButton.MouseLeave += AlphaButton_MouseLeave;
            lettersButton.MouseEnter += AlphaButton_MouseEnter;
            lettersButton.MouseLeave += AlphaButton_MouseLeave;
            wordsButton.MouseEnter += AlphaButton_MouseEnter;
            wordsButton.MouseLeave += AlphaButton_MouseLeave;
            sentButton.MouseEnter += AlphaButton_MouseEnter;
            sentButton.MouseLeave += AlphaButton_MouseLeave;
        }

        private void AlphaButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button temp = (Button)sender;
            temp.Background = Brushes.Black;
            temp.Foreground = Brushes.White;
        }

        private void AlphaButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button temp = (Button)sender;
            temp.Background = Brushes.Blue;
            temp.Foreground = Brushes.Black;
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
            "to be displayed. Give each button should have its own line in the text file. Save " + "the text file when you are done. Then press the load button to load the new keyboard." +
            " You must load the keyboard to access the new keyboard through the switch keyboard button.";
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
                    usedFiles.Add(name);
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

        private void TextToSpeech_Click(object send, RoutedEventArgs e)
        {
            SpeechSynthesizer reader = new SpeechSynthesizer();
            reader.SpeakAsync(tb.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            tb.Focus();
            tb.Clear();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int start = tb.SelectionStart;
            string firstHalf = tb.Text.Substring(0, start);
            string secondHalf = tb.Text.Substring(start);
            tb.Text = firstHalf + System.Environment.NewLine + secondHalf;
            tb.Focus();
            tb.SelectionStart = start + 1;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            int start = tb.SelectionStart;
            if (tb.Text.Length != 0 && start > 0)
            {
                if (start > 1 && tb.Text.Substring(start - 2, 2) == System.Environment.NewLine)
                {
                    string firstHalf = tb.Text.Substring(0, start - 2);
                    string secondHalf = tb.Text.Substring(start);
                    tb.Text = firstHalf + secondHalf;
                    tb.SelectionStart = start - 2;
                }
                else
                {
                    string firstHalf = tb.Text.Substring(0, start - 1);
                    string secondHalf = tb.Text.Substring(start);
                    tb.Text = firstHalf + secondHalf;
                    tb.SelectionStart = start - 1;
                }
            }
            tb.Focus();
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            int start = tb.SelectionStart;
            string firstHalf = tb.Text.Substring(0, start);
            string secondHalf = tb.Text.Substring(start);
            int index = firstHalf.LastIndexOf(" ");
            string word = "";
            if (index != -1)
            {
                word = firstHalf.Substring(index + 1);
            }
            else
            {
                word = firstHalf;
            }
            if (word.Length != 0)
            {
                if (word[word.Length - 1] == '.' || word[word.Length - 1] == '?' || word[word.Length - 1] == ',')
                    word = word.Substring(0, word.Length - 1);
                word = word.ToLower();
                if (!ViewModel.wordBank.ContainsKey(word))
                {
                    for (int i = word.Length; i > 0; i--)
                    {
                        string hash = word.Substring(0, i);
                        if (!ViewModel.wordBank.ContainsKey(hash))
                        {
                            ViewModel.wordBank[hash] = new List<AutoCompleteEntry>();
                        }
                        if (!ViewModel.wordBank[hash].Any(ac => ac.value == word))
                        {
                            AutoCompleteEntry ac = new AutoCompleteEntry(word);
                            ViewModel.wordBank[hash].Add(ac);
                        }
                        else
                        {
                            foreach (AutoCompleteEntry en in ViewModel.wordBank[hash])
                            {
                                if (en.value == word)
                                {
                                    en.count++;
                                    break;
                                }
                            }
                        }
                        ViewModel.wordBank[hash] = ViewModel.wordBank[hash].OrderBy(x => x.count).ToList();
                    }
                }
            }
            tb.Text = firstHalf + " " + secondHalf;
            tb.Focus();
            tb.SelectionStart = start + 1;
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
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

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            string path = @"Config\AC.txt";
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                foreach (KeyValuePair<string, List<AutoCompleteEntry>> pair in ViewModel.wordBank)
                {
                    foreach (AutoCompleteEntry ent in pair.Value)
                    {
                        sw.WriteLine(ent.value + " " + ent.count);
                    }
                }
            }
        }
        private void SaveText_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, args) => SavedText();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
            string filename = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".txt";
            string path = @"SavedFiles\" + filename;
            StreamWriter storeText = new StreamWriter(path, true);
            storeText.WriteLine(tb.Text);
            storeText.Close();
            SaveText.Background = Brushes.Orange;
            SaveText.Content = "Text Saved!";

        }

        private void SavedText()
        {
            SaveText.ClearValue(Button.BackgroundProperty);
            SaveText.Content = "Save Text";
        }

        private void Calculator_Click(object sender, RoutedEventArgs e)
        {
            Window2 calculator = new Window2();
            calculator.Show();
        }

        private void SpeechToText_Click(object sender, RoutedEventArgs e)
        {
            if (!voiceOn){
                Choices options = new Choices();
                options.Add(new string[] { "How Many", "What would you like to eat", "What is your favorite Food", "What would you like to do", "Solve" });
                GrammarBuilder gb = new GrammarBuilder();
                gb.Append(options);
                Grammar g = new Grammar(gb);
                recoEngine.LoadGrammar(g);
                recoEngine.RequestRecognizerUpdate();
                recoEngine.SpeechRecognized += _recognizer_SpeechRecognized;
                recoEngine.SetInputToDefaultAudioDevice();
                voiceOn = true;
                recoEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            recoEngine.RecognizeAsyncCancel();
            voiceOn = false;
            if (e.Result.Confidence > .7)
            {



                if (e.Result.Text == "What would you like to eat" || e.Result.Text == "What is your favorite Food")
                {
                    curKeyboard.HideKeys();
                    curKeyboard.LoadKeyboard("Food.txt", ref tb, filenameToType);

                    ShowKeyboard("Food.txt");

                }
                else if (e.Result.Text == "What would you like to do")
                {
                    curKeyboard.HideKeys();
                    curKeyboard.LoadKeyboard("Activities.txt", ref tb, filenameToType);
                    ShowKeyboard("Activities.txt");
                }
                else if (e.Result.Text == "How Many")
                {
                    curKeyboard.HideKeys();
                    curKeyboard.LoadKeyboard("Number.txt", ref tb, filenameToType);
                    ShowKeyboard("Number.txt");
                }
                else if (e.Result.Text == "Solve")
                {
                    Window2 calculator = new Window2();
                    calculator.Show();
                }


            }


        }
    }
}