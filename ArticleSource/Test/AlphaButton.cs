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
        public AlphaButton(String uppercase, String _lowercase, ref AutoCompleteTextBox tb,
                                ref ToggleButton Shift, ref ToggleButton caps)
        {
            this.Width = 80;
            this.Height = 72;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.Content = uppercase;
            this.lowercase = _lowercase;
            this.FontSize = 25;
            this.shift = Shift;
            this.capsLock = caps;
            this.tb = tb;
            this.Click += AlphaButton_Click;
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
            string firstHalf = tb.Text.Substring(0, start);
            string secondHalf = tb.Text.Substring(start);
            if (shift.IsChecked == true || capsLock.IsChecked == true)
            {
                tb.Text = firstHalf + this.Content.ToString() + secondHalf;
                shift.IsChecked = false;
            }
            else
                tb.Text = firstHalf + this.lowercase + secondHalf;
            tb.Focus();
            tb.SelectionStart = start + 1;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.ClearValue(Button.BackgroundProperty);
        }

        private TextBox tb;
        private ToggleButton shift;
        private ToggleButton capsLock;
        public String lowercase;
    }
}
