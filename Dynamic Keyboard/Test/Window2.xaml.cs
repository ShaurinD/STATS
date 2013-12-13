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
using System.Windows.Shapes;

namespace Test
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        bool sRoot = false;
        bool PrevIsNum = true;
        bool JustSolved = false;
        List<double> numbers = new List<double>();
        List<string> ops = new List<string>();
        long lastTime = 0;
        bool period = false;

        public Window2()
        {
            InitializeComponent();
            overall.TextWrapping = TextWrapping.Wrap;
            current.TextWrapping = TextWrapping.Wrap;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if ((currentTime - lastTime) < 5000)
            {
                overall.Clear();
                numbers.Clear();
                ops.Clear();
                current.Clear();
                this.Hide();
            }
            lastTime = currentTime;
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (current.Text.Length > 0)
            {
                current.Text = current.Text.Substring(0, current.Text.Length - 1);
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            current.Clear();
            overall.Clear();
            numbers.Clear();
            ops.Clear();
            period = false;
            sRoot = false;
            PrevIsNum = false;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            current.Clear();
            period = false;
        }

        private void OnClickNum(object sender, RoutedEventArgs e)
        {
            if (sRoot)
            {
                return;
            }
            if (JustSolved)
            {
                current.Clear();
                overall.Clear();
                numbers.Clear();
                ops.Clear();
                JustSolved = false;
                period = false;
            }
            Button pressed = (Button)sender;
            if ((String)pressed.Content == ".")
            {
                if (period)
                {
                    return;
                }
                period = true;
            }
            current.Text += (String)pressed.Content;
            PrevIsNum = true;
        }

        private void OnClickOp(object sender, RoutedEventArgs e)
        {
            Button pressed = (Button)sender;
            if (current.Text.Length == 0 && overall.Text.Length == 0)
            {
                return;
            }
            if (JustSolved)
            {
                overall.Clear();
                numbers.Clear();
                ops.Clear();
                overall.Text += current.Text + " " + (String)pressed.Content + " ";
                ops.Add((string)pressed.Content);
                numbers.Add(Convert.ToDouble(current.Text));
                current.Clear();
                JustSolved = false;
                period = false;
            }
            else
            {
                if (sRoot)
                {
                    overall.Text += " " + (String)pressed.Content + " ";
                    ops.Add((string)pressed.Content);
                    current.Clear();
                    period = false;
                    sRoot = false;
                }
                else if (PrevIsNum)
                {
                    overall.Text += current.Text + " " + (String)pressed.Content + " ";
                    ops.Add((string)pressed.Content);
                    numbers.Add(Convert.ToDouble(current.Text));
                    current.Clear();
                    period = false;
                }
                else
                {
                    overall.Text = overall.Text.Substring(0, overall.Text.Length - 3);
                    overall.Text += " " + (String)pressed.Content + " ";
                    ops.RemoveAt(ops.Count() - 1);
                    ops.Add((string)pressed.Content);
                }
            }
            PrevIsNum = false;
        }

        private void Negate(object sender, RoutedEventArgs e)
        {
            if (current.Text.Length > 0)
            {
                if (current.Text[0] == '-')
                {
                    current.Text = current.Text.Substring(1, current.Text.Length - 1);
                }
                else
                {
                    current.Text = "-" + current.Text;
                }
            }
        }

        private void SquareRoot(object sender, RoutedEventArgs e)
        {
            if (current.Text.Length > 0){
                if (JustSolved)
                {
                    overall.Clear();
                    numbers.Clear();
                    ops.Clear();
                    JustSolved = false;
                }
                overall.Text += "sqrt(" + current.Text + ")";
                numbers.Add(Math.Sqrt(Convert.ToDouble(current.Text)));
                current.Clear();
                period = false;
                PrevIsNum = true;
                sRoot = true;
           }
        }

        private void Solve(object sender, RoutedEventArgs e)
        {
            if (JustSolved)
            {
                Double lastNum = numbers[numbers.Count() - 1];
                string lastOp = ops[ops.Count() - 1];
                numbers.Clear();
                ops.Clear();
                numbers.Add(Convert.ToDouble(current.Text));
                numbers.Add(lastNum);
                ops.Add(lastOp);
                overall.Text = numbers[0] + " " + lastOp + " " + lastNum;
            }
            else
            {
                if (!PrevIsNum || overall.Text.Length == 0)
                {
                    return;
                }
                if (current.Text.Length > 0)
                {
                    overall.Text += current.Text;
                    numbers.Add(Convert.ToDouble(current.Text));
                    current.Clear();
                    period = false;
                }
            }
            if (numbers.Count() != ops.Count() + 1) return;

            double solution = numbers[0];
            for (int i = 0; i < numbers.Count() - 1; i++)
            {
                switch (ops[i])
                {
                    case "/": solution = solution / numbers[i + 1];
                        break;
                    case "*": solution = solution * numbers[i + 1];
                        break;
                    case "-": solution = solution - numbers[i + 1];
                        break;
                    case "+": solution = solution + numbers[i + 1];
                        break;
                    case "^": solution = Math.Pow(solution, numbers[i + 1]);
                        break;
                    default:
                        break;
                }
            }
            current.Text = solution.ToString();
            JustSolved = true;
            sRoot = false;
        }
    }
}
