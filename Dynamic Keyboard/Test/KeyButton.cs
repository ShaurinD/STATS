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
    public class KeyButton : Button
    {
        public KeyButton(String text, Window1 wind, bool _create)
        {
            this.Width = 80;
            this.Height = 72;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.FontSize = 16;
            this.Content = text;
            this.win = wind;
            this.Click += KeyButton_Click;
            this.Background = Brushes.DarkBlue;
            this.Foreground = Brushes.Yellow;
            this.MouseEnter += AlphaButton_MouseEnter;
            this.MouseLeave += AlphaButton_MouseLeave;
        }

        private void AlphaButton_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = Brushes.DarkBlue;
            this.Foreground = Brushes.Yellow;
        }

        private void AlphaButton_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background = Brushes.Yellow;
            this.Foreground = Brushes.DarkBlue;
        }

        protected void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            this.Background = Brushes.Yellow;
            this.Foreground = Brushes.Black;
            win.ShowKeyboard(this.Content.ToString() + ".txt");
        }

        public Window1 win;
    }
}
