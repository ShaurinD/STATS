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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class Window1 : Window
    {
        HashSet<String> usedWords = new HashSet<String>();
        public Window1()
        {
            InitializeComponent();
            loadStandardKeyboard();
            LeftArrow.Content = "<-";
            RightArrow.Content = "->";
            EnterButton.Click += EnterButton_Click;
            Backspace.Click += BackspaceButton_Click;
            Space.Click += SpaceButton_Click;
            LeftArrow.Click += LeftButton_Click;
            RightArrow.Click += RightButton_Click;
            Clear.Click += ClearButton_Click;
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

        private void unhighlight(Button btn)
        {
            btn.ClearValue(Button.BackgroundProperty);
        }

        private void loadStandardKeyboard()
        {
            AlphaButton Q = new AlphaButton("Q", "q", ref tb, ref Shift, ref Caps);
            col1.Children.Add(Q);
            AlphaButton A = new AlphaButton("A", "a", ref tb, ref Shift, ref Caps);
            col1.Children.Add(A);
            AlphaButton Z = new AlphaButton("Z", "z", ref tb, ref Shift, ref Caps);
            col1.Children.Add(Z);
            AlphaButton W = new AlphaButton("W", "w", ref tb, ref Shift, ref Caps);
            col2.Children.Add(W);
            AlphaButton S = new AlphaButton("S", "s", ref tb, ref Shift, ref Caps);
            col2.Children.Add(S);
            AlphaButton X = new AlphaButton("X", "x", ref tb, ref Shift, ref Caps);
            col2.Children.Add(X);
            AlphaButton E = new AlphaButton("E", "e", ref tb, ref Shift, ref Caps);
            col3.Children.Add(E);
            AlphaButton D = new AlphaButton("D", "d", ref tb, ref Shift, ref Caps);
            col3.Children.Add(D);
            AlphaButton C = new AlphaButton("C", "c", ref tb, ref Shift, ref Caps);
            col3.Children.Add(C);
            AlphaButton R = new AlphaButton("R", "r", ref tb, ref Shift, ref Caps);
            col4.Children.Add(R);
            AlphaButton F = new AlphaButton("F", "f", ref tb, ref Shift, ref Caps);
            col4.Children.Add(F);
            AlphaButton V = new AlphaButton("V", "v", ref tb, ref Shift, ref Caps);
            col4.Children.Add(V);
            AlphaButton T = new AlphaButton("T", "t", ref tb, ref Shift, ref Caps);
            col5.Children.Add(T);
            AlphaButton G = new AlphaButton("G", "g", ref tb, ref Shift, ref Caps);
            col5.Children.Add(G);
            AlphaButton B = new AlphaButton("B", "b", ref tb, ref Shift, ref Caps);
            col5.Children.Add(B);
            AlphaButton Y = new AlphaButton("Y", "y", ref tb, ref Shift, ref Caps);
            col6.Children.Add(Y);
            AlphaButton H = new AlphaButton("H", "h", ref tb, ref Shift, ref Caps);
            col6.Children.Add(H);
            AlphaButton N = new AlphaButton("N", "n", ref tb, ref Shift, ref Caps);
            col6.Children.Add(N);
            AlphaButton U = new AlphaButton("U", "u", ref tb, ref Shift, ref Caps);
            col7.Children.Add(U);
            AlphaButton J = new AlphaButton("J", "j", ref tb, ref Shift, ref Caps);
            col7.Children.Add(J);
            AlphaButton M = new AlphaButton("M", "m", ref tb, ref Shift, ref Caps);
            col7.Children.Add(M);
            AlphaButton I = new AlphaButton("I", "i", ref tb, ref Shift, ref Caps);
            col8.Children.Add(I);
            AlphaButton K = new AlphaButton("K", "k", ref tb, ref Shift, ref Caps);
            col8.Children.Add(K);
            AlphaButton Comma = new AlphaButton(",", ",", ref tb, ref Shift, ref Caps);
            col8.Children.Add(Comma);
            AlphaButton O = new AlphaButton("O", "o", ref tb, ref Shift, ref Caps);
            col9.Children.Add(O);
            AlphaButton L = new AlphaButton("L", "l", ref tb, ref Shift, ref Caps);
            col9.Children.Add(L);
            AlphaButton Period = new AlphaButton(".", ".", ref tb, ref Shift, ref Caps);
            col9.Children.Add(Period);
            AlphaButton P = new AlphaButton("P", "p", ref tb, ref Shift, ref Caps);
            col10.Children.Add(P);
            AlphaButton AP = new AlphaButton("'", "'", ref tb, ref Shift, ref Caps);
            col10.Children.Add(AP);
            AlphaButton Question = new AlphaButton("?", "?", ref tb, ref Shift, ref Caps);
            col10.Children.Add(Question);
        }
    }
}

