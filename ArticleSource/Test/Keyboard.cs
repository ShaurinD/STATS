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
    public class Keyboard
    {
        public AlphaButton[] buttons;
        public int buttonCount;

        public Keyboard(string filename, ref AutoCompleteTextBox tb)
        {
            LoadKeyboard(filename, ref tb);
        }

        public void LoadKeyboard(string filename, ref AutoCompleteTextBox tb)
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
                result[j] = new AlphaButton(upper[j], lower[j], ref tb, this);
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
}
