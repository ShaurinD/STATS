using System;

namespace WpfApplication2
{
    public class AlphaButton : Button
    {
        public AlphaButton(TextBox tb)
        {
            this.Width = 80;
            this.Height = 72;
            this.HorizontalAlignment = "Left";
            this.VerticalAlignment = "Top";
            this.tb = TextBox;
        }

        public void AlphaButton_Click(object sender, RoutedEventArgs e)
        {
            tb.text += this.content;
        }

        private TextBox tb;
    }
}