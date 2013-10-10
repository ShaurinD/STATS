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
using System.Diagnostics;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class AlphaButton : Button
    {
        public AlphaButton(String value, ref TextBox tb)
        {
            this.Width = 80;
            this.Height = 72;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.Content = value;
            this.tb = tb;
            this.Click += AlphaButton_Click;
        }

        protected void AlphaButton_Click(object sender, RoutedEventArgs e)
        {
            tb.Text += this.Content.ToString();
        }

        private TextBox tb;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            loadStandardKeyboard();
            EnterButton.Click += EnterButton_Click;
            Backspace.Click += BackspaceButton_Click;
            //Keyboard.Visibility = Visibility.Collapsed;
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Text += System.Environment.NewLine;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            var key = Key.Delete;                    
            var target = Keyboard.FocusedElement;    
            var routedEvent = Keyboard.KeyDownEvent; 

            target.RaiseEvent(
              new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(target),
                0,
                key) { RoutedEvent = routedEvent }
            );
        }

        private void loadStandardKeyboard()
        {
            AlphaButton Q = new AlphaButton("Q", ref textBox1);
            col1.Children.Add(Q);
            AlphaButton A = new AlphaButton("A", ref textBox1);
            col1.Children.Add(A);
            AlphaButton Z = new AlphaButton("Z", ref textBox1);
            col1.Children.Add(Z);
            AlphaButton W = new AlphaButton("W", ref textBox1);
            col2.Children.Add(W);
            AlphaButton S = new AlphaButton("S", ref textBox1);
            col2.Children.Add(S);
            AlphaButton X = new AlphaButton("X", ref textBox1);
            col2.Children.Add(X);
            AlphaButton E = new AlphaButton("E", ref textBox1);
            col3.Children.Add(E);
            AlphaButton D = new AlphaButton("D", ref textBox1);
            col3.Children.Add(D);
            AlphaButton C = new AlphaButton("C", ref textBox1);
            col3.Children.Add(C);
            AlphaButton R = new AlphaButton("R", ref textBox1);
            col4.Children.Add(R);
            AlphaButton F = new AlphaButton("F", ref textBox1);
            col4.Children.Add(F);
            AlphaButton V = new AlphaButton("V", ref textBox1);
            col4.Children.Add(V);
            AlphaButton T = new AlphaButton("T", ref textBox1);
            col5.Children.Add(T);
            AlphaButton G = new AlphaButton("G", ref textBox1);
            col5.Children.Add(G);
            AlphaButton B = new AlphaButton("B", ref textBox1);
            col5.Children.Add(B);
            AlphaButton Y = new AlphaButton("Y", ref textBox1);
            col6.Children.Add(Y);
            AlphaButton H = new AlphaButton("H", ref textBox1);
            col6.Children.Add(H);
            AlphaButton N = new AlphaButton("N", ref textBox1);
            col6.Children.Add(N);
            AlphaButton U = new AlphaButton("U", ref textBox1);
            col7.Children.Add(U);
            AlphaButton J = new AlphaButton("J", ref textBox1);
            col7.Children.Add(J);
            AlphaButton M = new AlphaButton("M", ref textBox1);
            col7.Children.Add(M);
            AlphaButton I = new AlphaButton("I", ref textBox1);
            col8.Children.Add(I);
            AlphaButton K = new AlphaButton("K", ref textBox1);
            col8.Children.Add(K);
            AlphaButton Comma = new AlphaButton(",", ref textBox1);
            col8.Children.Add(Comma);
            AlphaButton O = new AlphaButton("O", ref textBox1);
            col9.Children.Add(O);
            AlphaButton L = new AlphaButton("L", ref textBox1);
            col9.Children.Add(L);
            AlphaButton Period = new AlphaButton(".", ref textBox1);
            col9.Children.Add(Period);
            AlphaButton P = new AlphaButton("P", ref textBox1);
            col10.Children.Add(P);
            AlphaButton AP = new AlphaButton("'", ref textBox1);
            col10.Children.Add(AP);
            AlphaButton Question = new AlphaButton("?", ref textBox1);
            col10.Children.Add(Question);
        }
    }
}
