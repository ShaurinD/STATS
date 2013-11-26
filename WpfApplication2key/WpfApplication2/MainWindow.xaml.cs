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
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Microsoft.VisualBasic;

namespace WpfApplication2 {

    public class AlphaButton : Button
    {
        private TextBox tb;
        private Keyboard curKeyboard;
        public String uppercase;
        public String lowercase;

        public AlphaButton(String _uppercase, String _lowercase, ref TextBox tb, ref Keyboard curkeyboard) {
            this.Width = 80;
            this.Height = 72;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.Content = _lowercase;
            this.uppercase = _uppercase;
            this.lowercase = _lowercase;
            this.tb = tb;
            this.curKeyboard = curkeyboard;
            this.Click += AlphaButton_Click;
        }

        protected void AlphaButton_Click(object sender, RoutedEventArgs e)
        {
            int start = tb.SelectionStart;
            int len = tb.SelectionLength;
            string firstHalf = tb.Text.Substring(0, start + len);
            string secondHalf = tb.Text.Substring(start + len);
            tb.Text = firstHalf + this.Content.ToString() + secondHalf;
            tb.SelectionStart = start + this.Content.ToString().Length;
            tb.Focus();
            curKeyboard.ShiftHandler();
        }
    }

    public class Keyboard
    {
        public AlphaButton[] buttons;
        public int buttonCount;

        public Keyboard (string filename, ref TextBox tb)
        {
            LoadKeyboard(filename, ref tb);
        }

        public void LoadKeyboard(string filename, ref TextBox tb)
        {
            List<string> upper = new List<string>();
            List<string> lower = new List<string>();
            string line; int i = 0;
            using (StreamReader sr = new StreamReader(filename))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    upper.Add(line);
                    line = sr.ReadLine();
                    lower.Add(line);
                    i++;
                }
            }
            buttonCount = i;
            AlphaButton[] result = new AlphaButton[buttonCount];
            for (int j = 0; j < buttonCount; j++)
            {
                result[j] = new AlphaButton(upper[j], lower[j], ref tb, ref shift, ref caps);
            }
            buttons = result;
        }

        public void HideKeys()
        {
            for (int i = 0; i < buttonCount; i++)
                buttons[i].Visibility = Visibility.Collapsed;
        }

        public void ShowKeys()
        {
            for (int i = 0; i < buttonCount; i++)
                buttons[i].Visibility = Visibility.Visible;
        }

        public void ToUpper()
        {
            for (int i = 0; i < buttonCount; i++)
                buttons[i].Content = buttons[i].uppercase;
        }

        public void ToLower()
        {
            for (int i = 0; i < buttonCount; i++)
                buttons[i].Content = buttons[i].lowercase;
        }

        public void ShiftHandler()
        {

        }
    }

    public partial class MainWindow : Window 
    {
        Keyboard curKeyboard;
        public bool shiftOn = false;
        public bool capsOn = false;

        public MainWindow()
        {
            InitializeComponent();
            curKeyboard = new Keyboard("Standard.txt", ref textBox1);
            ShowKeyboard();
            textBox1.SelectionStart = 0;
            EnterButton.Click += EnterButton_Click;
            Backspace.Click += BackspaceButton_Click;
            Space.Click += SpaceButton_Click;
            Shift.Click += Shift_Click;
            Caps.Click += Caps_Click;
            SwitchKeyboard.Click += SwitchKeyboard_Click;
        }

        private void ShowKeyboard()
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
        
        private void SwitchKeyboard_Click(object sender, RoutedEventArgs e)
        {
            curKeyboard.HideKeys();
            string filename = Interaction.InputBox("What keyboard are you looking for?") + ".txt";
            if (File.Exists(filename))
            {
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
                curKeyboard.LoadKeyboard(filename, ref textBox1);
                ShowKeyboard();
            }
            else
            {
                Interaction.MsgBox("Unable to find the requested keyboard.");
            }
            curKeyboard.ShowKeys();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            int start = textBox1.SelectionStart;
            int len = textBox1.SelectionLength;
            string firstHalf = textBox1.Text.Substring(0, start + len);
            string secondHalf = textBox1.Text.Substring(start + len);
            textBox1.Text = firstHalf + System.Environment.NewLine + secondHalf;
            textBox1.Focus();
            textBox1.SelectionStart = start + 1;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            int start = textBox1.SelectionStart;
            int len = textBox1.SelectionLength;
            if(textBox1.Text.Length != 0 && start+len >0)
            {
                int check = (((start+len) - 2) > 0) ? (start+len)-2 : 0;
                if (textBox1.Text.Substring(check) == System.Environment.NewLine)
                {
                    string firstHalf = textBox1.Text.Substring(0, start + len-2);
                    string secondHalf = textBox1.Text.Substring(start + len);
                    textBox1.Text = firstHalf + secondHalf;
                    textBox1.SelectionStart = start - 2;
                }
                else
                {
                    string firstHalf = textBox1.Text.Substring(0, start + len - 1);
                    string secondHalf = textBox1.Text.Substring(start + len);
                    textBox1.Text = firstHalf + secondHalf;
                    textBox1.SelectionStart = start - 1;
                }
            }
            textBox1.Focus();
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            int start = textBox1.SelectionStart;
            int len = textBox1.SelectionLength;
            string firstHalf = textBox1.Text.Substring(0, start + len);
            string secondHalf = textBox1.Text.Substring(start + len);
            textBox1.Text = firstHalf + " " + secondHalf;
            textBox1.Focus();
            textBox1.SelectionStart = start + 1;
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
    }
}
