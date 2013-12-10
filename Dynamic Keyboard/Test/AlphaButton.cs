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

namespace Test
{
    public class AlphaButton : Button
    {
        public AlphaButton(String _uppercase, String _lowercase, ref AutoCompleteTextBox tb,
                               Keyboard curKeyboard)
        {
            this.Width = 80;
            this.Height = 72;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.FontSize = 25;
            this.Content = _lowercase;
            this.uppercase = _uppercase;
            this.lowercase = _lowercase;
            this.tb = tb;
            this.Margin = new Thickness(3);
            this.Click += AlphaButton_Click;
            this.curKeyboard = curKeyboard;
            this.Background = Brushes.Black;
            this.Foreground = Brushes.White;
            this.MouseEnter += AlphaButton_MouseEnter;
            this.MouseLeave += AlphaButton_MouseLeave;
        }

        private void AlphaButton_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = Brushes.Black;
            this.Foreground = Brushes.White;
        }

        private void AlphaButton_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background = Brushes.Blue;
            this.Foreground = Brushes.Black;
        }
 
        public void setWidth(int width) {
            this.Width = width;
        }

        public void setHeight(int height)
        {
            this.Height = height;
        }

        protected void AlphaButton_Click(object sender, RoutedEventArgs e)
        {
            int start = tb.SelectionStart;
            int len = tb.SelectionLength;
            string firstHalf = tb.Text.Substring(0, start + len);
            string secondHalf = tb.Text.Substring(start + len);
            char c = this.Content.ToString()[0];
            bool addspace = false;
            bool deletespace = false;
            if (tb.Text.Length == 0 || tb.SelectionStart == 0)
            {
                c = Char.ToUpper(this.Content.ToString()[0]);
            }
            else if (firstHalf[firstHalf.Length - 1] == '.' || firstHalf[firstHalf.Length - 1] == '?')
            {
                addspace = true;
                c = Char.ToUpper(this.Content.ToString()[0]);
            }
            if (firstHalf.Length > 1 && (firstHalf[firstHalf.Length - 2] == '.' || firstHalf[firstHalf.Length - 2] == '?'))
            {
                c = Char.ToUpper(this.Content.ToString()[0]);
            }

            if ((c == '.' || c == '?') && firstHalf.Length > 0)
            {
                if (firstHalf[firstHalf.Length - 1] == ' ')
                {
                    int start2 = tb.SelectionStart;
                    string firstHalf2 = tb.Text.Substring(0, start2 - 1);
                    string secondHalf2 = tb.Text.Substring(start2 - 1, 1);
                    tb.Text = firstHalf2 + c + secondHalf2;
                    tb.SelectionStart = start2;
                    deletespace = true;
                }
            }

            if (!deletespace)
            {
                string space = addspace ? " " : "";
                tb.Text = firstHalf + space + c + this.Content.ToString().Substring(1) + secondHalf;
                int offset = addspace ? 1 : 0;
                tb.SelectionStart = start + offset + this.Content.ToString().Length;
            }
            tb.Focus();
            curKeyboard.ShiftHandler();
        }

        private TextBox tb;
        private Keyboard curKeyboard;
        public String uppercase;
        public String lowercase;
    }
}
