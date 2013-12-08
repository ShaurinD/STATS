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
        public ToggleButton shift;
        public Keyboard(string filename, ref AutoCompleteTextBox tb, ref ToggleButton _shift)
        {
            shift = _shift;
        }

        public void LoadKeyboard(string filename, ref AutoCompleteTextBox tb, Dictionary<string,string> type)
        {
            List<string> upper = new List<string>();
            List<string> lower = new List<string>();
            string line; int i = 0;
            try
            {
                using (StreamReader sr = new StreamReader("Keyboards\\" + filename))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        int index = line.IndexOf("|");
                        if (index == -1)
                        {
                            line = line.Trim();
                            lower.Add(line);
                            upper.Add(line);
                        }
                        else
                        {
                            string up = line.Substring(0, index);
                            up = up.Trim();
                            string low = line.Substring(index + 1);
                            low = low.Trim();
                            upper.Add(up);
                            lower.Add(low);
                        }
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                MessageBox.Show("Error reading the file : " + filename + ". Please ensure that the file name is correct,the file was placed in the bin/release or "
                + "bin/debug directories, and the text file is valid. The text file must contain uppercase and lowercase values for each button seperated by |"); 
            }
            buttonCount = i;
            AlphaButton[] result = new AlphaButton[buttonCount];
            for (int j = 0; j < buttonCount; j++)
            {
                result[j] = new AlphaButton(upper[j], lower[j], ref tb, this);
                if (type[filename] == "word")
                {
                    result[j].setWidth(160);
                }
                else if (type[filename] == "sentence")
                {
                    result[j].setWidth(400);
                }
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
            if (shift.IsChecked == true)
            {
                shift.IsChecked = false;
                ToLower();
            }
        }
    }
}
