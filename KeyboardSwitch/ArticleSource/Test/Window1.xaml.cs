using System;
using System.IO;
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
using System.Collections;
using Aviad.WPF.Controls;
using System.Diagnostics;
using System.Windows.Controls.Primitives;


namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class Window1 : Window
    {
        Keyboard curKeyboard;
        public KeyButton[] recentBoards;
        public int numBoards;
        public bool shiftOn = false;
        public bool capsOn = false;
        public Window1()
        {
            InitializeComponent();
            curKeyboard = new Keyboard();
            ShowKeyboard("Standard.txt");
            tb.SelectionStart = 0;
            LeftArrow.Content = "<-";
            RightArrow.Content = "->";
            EnterButton.Click += EnterButton_Click;
            Backspace.Click += BackspaceButton_Click;
            Space.Click += SpaceButton_Click;
            LeftArrow.Click += LeftButton_Click;
            RightArrow.Click += RightButton_Click;
            Clear.Click += ClearButton_Click;
            Shift.Click += Shift_Click;
            Caps.Click += Caps_Click;
            SwitchKeyboard.Click += SwitchKeyboard_Click;
        }

        public void ShowKeyboard(string filename)
        {
            for (int i = 0; i < numBoards; i++)
            {
                recentBoards[i].Visibility = Visibility.Collapsed;
            }
            curKeyboard.LoadKeyboard(filename, ref tb);
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

        private void LoadRecent()
        {
            List<string> boards = new List<string>();
            string line; int i = 0;
            using (StreamReader sr = new StreamReader("MRK.txt"))
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
                recBut[j] = new KeyButton(boards[j], this);
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
            if (capsOn == false & shiftOn == false)
            {
                curKeyboard.ToUpper();
                shiftOn = true;
            }
            else if (capsOn == false & shiftOn == true)
            {
                curKeyboard.ToLower();
                shiftOn = false;
            }
        }

        private void Caps_Click(object sender, RoutedEventArgs e)
        {
            if (capsOn == false)
            {
                curKeyboard.ToUpper();
                capsOn = true;
                shiftOn = false;
            }
            else if (capsOn == true)
            {
                curKeyboard.ToLower();
                capsOn = false;
            }
        }

        private void unhighlight(Button btn)
        {
            btn.ClearValue(Button.BackgroundProperty);
        }
    }
}

