using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace Crafter
{
    public class ConsoleTextBoxOutput : TextWriter
    {
        private TextBox _textBox;

        public ConsoleTextBoxOutput(TextBox textBox)
        {
            this._textBox = textBox;
        }

        public override void Write(char value)
        {
            base.Write(value);
            
            this._textBox.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    this._textBox.AppendText(value.ToString());
                    this._textBox.ScrollToEnd();
                }));
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
