using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_Emulator
{
    /// <summary>
    /// cntLaLinput.xaml の相互作用ロジック
    /// </summary>
    public partial class cntLaLinput : UserControl
    {
        private float _latitude;
        private float _longitude;
        private float _altitude;
        public float Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                _latitude = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    int deg = (int)Math.Truncate(_latitude);
                    int min = (int)Math.Truncate((_latitude - deg) * 60.0f);
                    float sec = ((_latitude - deg) * 60.0f - min) * 60.0f;
                    if (deg < 0) { LATITUDE_COMBOBOX.SelectedIndex = 1; }
                    else { LATITUDE_COMBOBOX.SelectedIndex = 0; }
                    LATITUDE_DEGREE_TEXTBOX.Text = Math.Abs(deg).ToString();
                    LATITUDE_MINUTES_TEXTBOX.Text = Math.Abs(min).ToString("00");
                    LATITUDE_SECOND_TEXTBOX.Text = Math.Abs(sec).ToString("00.000000");
                }));
            }
        }
        public float Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                _longitude = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    int deg = (int)Math.Truncate(_longitude);
                    int min = (int)Math.Truncate((_longitude - deg) * 3600.0f / 60.0f);
                    float sec = (_longitude - deg) * 3600.0f - min * 60.0f;
                    if (deg < 0) { LONGITUDE_COMBOBOX.SelectedIndex = 1; }
                    else { LONGITUDE_COMBOBOX.SelectedIndex = 0; }
                    LONGITUDE_DEGREE_TEXTBOX.Text = Math.Abs(deg).ToString();
                    LONGITUDE_MINUTES_TEXTBOX.Text = Math.Abs(min).ToString("00");
                    LONGITUDE_SECOND_TEXTBOX.Text = Math.Abs(sec).ToString("00.000000");
                }));
            }
        }
        public float Altitude
        {
            get { return _altitude; }
            set
            {
                _altitude = value;
                Dispatcher.BeginInvoke(new Action(() => { ALTITUDE_TEXTBOX.Text = value.ToString("0.00"); }));
            }
        }


        public cntLaLinput()
        {
            InitializeComponent();
        }

        private void TEXTBOX_GotFocus(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }


        private void LATITUDE_DEGREE_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //If Paste
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                //Only 0 to 90
                if (sender is TextBox tb)
                {
                    string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, strTXT);
                    int val;
                    if (int.TryParse(strBF, out val))
                    {
                        if (val < 0) { LATITUDE_COMBOBOX.SelectedIndex = 1; val = -val; }
                        if (val < 0 || 90 < val) { e.Handled = true; return; }
                    }
                    else { e.Handled = true; return; }
                }
            }
        }
        private void LATITUDE_DEGREE_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // "-" is Combobox select change.
            if (e.Text == "-") { LATITUDE_COMBOBOX.SelectedIndex = 1; e.Handled = true; return; }
            if(e.Text == "+") { LATITUDE_COMBOBOX.SelectedIndex = 0; e.Handled = true;return; }
            //Only 0 to 9.
            if(!new Regex("[0-9]").IsMatch(e.Text)) { e.Handled = true; return; }
            //Only 0 to 90
            if (sender is TextBox tb)
            {
                string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
                int val;
                if(int.TryParse(strBF,out val))
                {
                    if(val < 0 || 90 < val) {  e.Handled = true; return; }
                }
                else { e.Handled = true; return; }
            }
        }

        private void LONGITUDE_DEGREE_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //If Paste
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                //Only 0 to 90
                if (sender is TextBox tb)
                {
                    string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, strTXT);
                    int val;
                    if (int.TryParse(strBF, out val))
                    {
                        if (val < 0) { LONGITUDE_COMBOBOX.SelectedIndex = 1; val = -val; }
                        if (val < 0 || 180 < val) { e.Handled = true; return; }
                    }
                    else { e.Handled = true; return; }
                }
            }
        }
        private void LONGITUDE_DEGREE_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // "-" is Combobox select change.
            if (e.Text == "-") { LONGITUDE_COMBOBOX.SelectedIndex = 1; e.Handled = true; return; }
            if (e.Text == "+") { LONGITUDE_COMBOBOX.SelectedIndex = 0; e.Handled = true; return; }
            //Only 0 to 9.
            if (!new Regex("[0-9]").IsMatch(e.Text)) { e.Handled = true; return; }
            //Only 0 to 180
            if (sender is TextBox tb)
            {
                string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
                int val;
                if (int.TryParse(strBF, out val))
                {
                    if (val < 0 || 180 < val) { e.Handled = true; return; }
                }
                else { e.Handled = true; return; }
            }
        }

        private void MINUTES_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //If Paste
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                //Only 0 to 59
                if (sender is TextBox tb)
                {
                    string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, strTXT);
                    int val;
                    if (int.TryParse(strBF, out val))
                    {
                        if (val < 0 || 59 < val) { e.Handled = true; return; }
                    }
                    else { e.Handled = true; return; }
                }
            }
        }
        private void MINUTES_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Only 0 to 9.
            if (!new Regex("[0-9]").IsMatch(e.Text)) { e.Handled = true; return; }
            //Only 0 to 59
            if (sender is TextBox tb)
            {
                string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
                int val;
                if (int.TryParse(strBF, out val))
                {
                    if (val < 0 || 59 < val) { e.Handled = true; return; }
                }
                else { e.Handled = true; return; }
            }
        }

        private void SECOND_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //If Paste
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                //Only 0 to 59.9999...
                if (sender is TextBox tb)
                {
                    string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, strTXT);
                    float val;
                    if (float.TryParse(strBF, out val))
                    {
                        if (val < 0.0f || 60.0f <= val) { e.Handled = true; return; }
                    }
                    else { e.Handled = true; return; }
                }
            }
        }
        private void SECOND_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Only 0 to 9.
            if (!new Regex("[0-9]|.").IsMatch(e.Text)) { e.Handled = true; return; }
            //Only 0 to 59.9999,,,
            if (sender is TextBox tb)
            {
                string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
                float val;
                if (float.TryParse(strBF, out val))
                {
                    if (val < 0.0f || 60.0f <= val) { e.Handled = true; return; }
                }
                else { e.Handled = true; return; }
            }
        }

        private void FLOAT_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //If Paste
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                //Only float
                if (sender is TextBox tb)
                {
                    string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, strTXT);
                    e.Handled = !float.TryParse(strBF, out _);
                }
            }
        }
        private void FLOAT_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Only 0 to 9.
            if (!new Regex("[0-9]|.").IsMatch(e.Text)) { e.Handled = true; return; }
            //Only 0 to 59.9999,,,
            if (sender is TextBox tb)
            {
                string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
                e.Handled = !float.TryParse(strBF, out _);
            }
        }


        private void LATITUDE_TEXTBOX_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                float val = float.Parse(LATITUDE_DEGREE_TEXTBOX.Text);
                val += float.Parse(LATITUDE_MINUTES_TEXTBOX.Text) * 60.0f / 3600.0f;
                val += float.Parse(LATITUDE_SECOND_TEXTBOX.Text) / 3600.0f;
                if (LATITUDE_COMBOBOX.SelectedIndex == 1) { val *= -1.0f; }
                if (-90 <= val && val <= 90) { _latitude = val; return; }
                MessageBox.Show("Enter in the range -90 to 90");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range -90 to 90");
                e.Handled = true;
            }

        }
        private void LONGITUDE_TEXTBOX_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                float val = float.Parse(LONGITUDE_DEGREE_TEXTBOX.Text);
                val += float.Parse(LONGITUDE_MINUTES_TEXTBOX.Text) * 60.0f / 3600.0f;
                val += float.Parse(LONGITUDE_SECOND_TEXTBOX.Text) / 3600.0f;
                if (LONGITUDE_COMBOBOX.SelectedIndex == 1) { val *= -1.0f; }
                if (-180 <= val && val <= 180) { _longitude = val; return; }
                MessageBox.Show("Enter in the range -180 to 180");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range -180 to 180");
                e.Handled = true;
            }
        }
        private void ALTITUDE_TEXTBOX_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                float val = float.Parse(ALTITUDE_TEXTBOX.Text);
                if (-1000 <= val && val <= 20000) { _altitude = val; return; }
                MessageBox.Show("Enter in the range -1000 to 20000");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range -1000 to 20000");
                e.Handled = true;
            }
        }

        private void LATITUDE_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LATITUDE_TEXTBOX_PreviewLostKeyboardForcus(this,null);
        }

        private void LONGITUDE_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LONGITUDE_TEXTBOX_PreviewLostKeyboardForcus(this, null);
        }
    }
}
