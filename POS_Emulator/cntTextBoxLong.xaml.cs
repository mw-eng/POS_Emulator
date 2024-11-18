using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_Emulator
{
    /// <summary>
    /// cntTextBoxLong.xaml の相互作用ロジック
    /// </summary>
    public partial class cntTextBoxLong : UserControl
    {
        private long _val;
        public long Value
        {
            get
            {
                return _val;
            }
            set
            {
                _val = value;
                Dispatcher.BeginInvoke(new Action(() => { TEXTBOX_VALUE.Text = value.ToString(); }));
            }
        }
        public long MinimumValue;
        public long MaximumValue;
        public cntTextBoxLong(long min, long max, long val)
        {
            InitializeComponent();
            MinimumValue = min;
            MaximumValue = max;
            Value = val;
        }

        public cntTextBoxLong() : this(long.MinValue, long.MaxValue, 0) { }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9|-]").IsMatch(e.Text);
        }

        private void TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼付け場合
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        // 処理済み
                        e.Handled = true;
                        break;
                    }
                }
            }
        }

        private void TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                long val = long.Parse(((TextBox)sender).Text);
                if (MinimumValue <= val && val <= MaximumValue) { _val = val; return; }
                MessageBox.Show("Enter in the range " + MinimumValue.ToString() + " to " + MaximumValue.ToString());
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range " + MinimumValue.ToString() + " to " + MaximumValue.ToString());
                e.Handled = true;
            }
        }
    }
}
