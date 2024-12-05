using POS_Emulator.Properties;
using System;
using System.Windows;
using System.Windows.Controls;
using static POS_Emulator.ShowSerialPortName;

namespace POS_Emulator
{
    /// <summary>
    /// winSettings.xaml の相互作用ロジック
    /// </summary>
    public partial class winSettings : Window
    {
        private SerialPortTable[] ports;
        public winSettings()
        {
            InitializeComponent();
        }

        private void OK_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.spCaption = COMBOBOX_SerialPort.Text;
            Settings.Default.spBaudRate = (int)TEXTBOX_BaudRate.Value;
            Settings.Default.spCycle = (sbyte)TEXTBOX_Cycle.Value;
            Settings.Default.targLongitude = POSITION.Longitude;
            Settings.Default.targLatitude = POSITION.Latitude;
            Settings.Default.targAltitude = POSITION.Altitude;
            Settings.Default.Save();
            this.Close();
        }

        private void Cancel_BUTTON_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            COMBOBOX_SerialPort_RELOAD(sender, e);
            COMBOBOX_SerialPort.SelectedIndex = 0;
            TEXTBOX_BaudRate.Value = Settings.Default.spBaudRate;
            TEXTBOX_Cycle.Value = Settings.Default.spCycle;
            TEXTBOX_BaudRate.MinimumValue = 110;
            TEXTBOX_BaudRate.MaximumValue = 115200;
            TEXTBOX_Cycle.MinimumValue = 1;
            TEXTBOX_Cycle.MaximumValue = 400;
            POSITION.Longitude = Settings.Default.targLongitude;
            POSITION.Latitude = Settings.Default.targLatitude;
            POSITION.Altitude = Settings.Default.targAltitude;
            if (ports != null)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    if (Settings.Default.spCaption == ports[i].Caption)
                    { COMBOBOX_SerialPort.SelectedIndex = i; break; }
                }
            }
        }

        private void COMBOBOX_SerialPort_RELOAD(object sender, EventArgs e)
        {
            ports = GetDeviceNames();
            if (sender is ComboBox)
            {
                ((ComboBox)sender).Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        ((ComboBox)sender).Items.Add(port.Caption);
                    }
                }
            }
            else
            {
                COMBOBOX_SerialPort.Items.Clear();
                if (ports != null)
                {
                    foreach (SerialPortTable port in ports)
                    {
                        COMBOBOX_SerialPort.Items.Add(port.Caption);
                    }
                }
            }
        }

        private void COMBOBOX_SerialPort_DropDownOpened(object sender, EventArgs e)
        {
            COMBOBOX_SerialPort_RELOAD(sender, e);
        }

    }
}
