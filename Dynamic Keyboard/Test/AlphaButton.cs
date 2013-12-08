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
            //Highlight for a few seconds
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            this.Background = Brushes.Cyan;
            int start = tb.SelectionStart;
            int len = tb.SelectionLength;
            string firstHalf = tb.Text.Substring(0, start + len);
            string secondHalf = tb.Text.Substring(start + len);
            char c = this.Content.ToString()[0];
            bool addspace = false;
            if (tb.Text.Length == 0)
            {
                c = Char.ToUpper(this.Content.ToString()[0]);
            }
            else if (firstHalf[firstHalf.Length - 1] == '.' || firstHalf[firstHalf.Length - 1] == '?')
            {
                addspace = true;
                c = Char.ToUpper(this.Content.ToString()[0]);
            }
            string space = addspace ? " " : "";
            tb.Text = firstHalf + space + c + this.Content.ToString().Substring(1) + secondHalf;
            int offset = addspace ? 1 : 0;
            tb.SelectionStart = start + offset + this.Content.ToString().Length;
            tb.Focus();
            curKeyboard.ShiftHandler();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.ClearValue(Button.BackgroundProperty);
        }

        private TextBox tb;
        private Keyboard curKeyboard;
        public String uppercase;
        public String lowercase;
    }
}
