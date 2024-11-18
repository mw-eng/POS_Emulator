using Microsoft.Win32;
using System;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace POS_Emulator
{
    /// <summary>
    /// winMain.xaml の相互作用ロジック
    /// </summary>
    public partial class winMain : Window
    {
        private const int taskLoopSleep = 1;
        private SerialPort _serial;
        private bool _logView;
        private DateTime _posExNOW;
        private bool _posExTASKstate;
        private Task _posExTASK;

        public winMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Properties.Settings.Default.Reset();
            this.Title += "_DEBUG MODE";
#endif
            if (_serial?.IsOpen == true) { _serial.Close(); _serial = null; }
            ShowSerialPortName.SerialPortTable sp;
            if(!ShowSerialPortName.SearchDeviceCaption(Properties.Settings.Default.spCaption, out sp))
            {
                if(MessageBox.Show("No serial port exists.\nClose the app?","Question",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.Close();
                    return;
                }
                else
                {
                    winSettings win = new winSettings();
                    win.ShowDialog();
                    Window_Loaded(sender, e);
                    return;
                }
            }
            _serial = new SerialPort(sp.Name);
            _serial.BaudRate = Properties.Settings.Default.spBaudRate;
            _serial.DataBits = 8;
            _serial.Parity = Parity.None;
            _serial.StopBits = StopBits.One;
            _serial.Handshake = Handshake.None;
            _serial.DtrEnable = true;
            _serial.Encoding = Encoding.ASCII;
            _serial.NewLine = "\r\n";
            _serial.ReadBufferSize = 1024;
            _serial.WriteTimeout = 1000;
            _serial.ReadTimeout = 1000;
            try
            {
                _serial.Open();
            }
            catch
            {
                if (MessageBox.Show("Failed to open serial port.\nClose the app?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    this.Close();
                    return;
                }
                else
                {
                    winSettings win = new winSettings();
                    win.ShowDialog();
                    Window_Loaded(sender, e);
                    return;
                }
            }

            TEXTBOX_Roll.MinimumValue = -180;
            TEXTBOX_Roll.MaximumValue = 180;
            TEXTBOX_Pitch.MinimumValue = -180;
            TEXTBOX_Pitch.MaximumValue = 180;
            TEXTBOX_Heading.MinimumValue = 0;
            TEXTBOX_Heading.MaximumValue = 360;
            TEXTBOX_Speed.MinimumValue = 0;
            TEXTBOX_Speed.MaximumValue = 300;
            TEXTBOX_LongAccel.MinimumValue = -16.384;
            TEXTBOX_LongAccel.MaximumValue = 16.3835;
            TEXTBOX_TranAccel.MinimumValue = -16.384;
            TEXTBOX_TranAccel.MaximumValue = 16.3835;
            TEXTBOX_DownAccel.MinimumValue = -16.384;
            TEXTBOX_DownAccel.MaximumValue = 16.3835;
            POSITION.Latitude = Properties.Settings.Default.posLatitude;
            POSITION.Longitude = Properties.Settings.Default.posLongitude;
            POSITION.Altitude = Properties.Settings.Default.posAltitude;
            TEXTBOX_Roll.Value = Properties.Settings.Default.posRoll;
            TEXTBOX_Pitch.Value = Properties.Settings.Default.posPitch;
            TEXTBOX_Heading.Value = Properties.Settings.Default.posHeading;
            TEXTBOX_Speed.Value = Properties.Settings.Default.posSpeed;
            TEXTBOX_Track.Value = Properties.Settings.Default.posTrack;
            TEXTBOX_LongAccel.Value = Properties.Settings.Default.posLongAccel;
            TEXTBOX_TranAccel.Value = Properties.Settings.Default.posTranAccel;
            TEXTBOX_DownAccel.Value = Properties.Settings.Default.posDownAccel;
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;


            _logView = false;
            MainGRID.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            MainGRID.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
            POS_OUTPUT_TASK_Start();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DecreasSpeedButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void IncreasSpeedButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SLIDER_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
        }

        private void SaveKML_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SerialConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void POS_OUTPUT_TASK_Start()
        {
            _posExTASKstate = true;
            _posExTASK = Task.Factory.StartNew(() => { POS_OUTPUT_TASK((UInt16)Math.Truncate((1.0 / Properties.Settings.Default.spCycle) * 1000)); });
        }

        private void POS_OUTPUT_TASK_Stop(bool wait)
        {
            _posExTASKstate = false;
            if (wait)
            {
                _posExTASK?.ConfigureAwait(false);
                _posExTASK?.Wait();
            }
            _posExTASK.Dispose();
            _posExTASK = null;
        }

        private void POS_OUTPUT_TASK(UInt16 cycle)
        {
            while (_posExTASKstate)
            {
                _posExNOW = DateTime.Now;
                POS.PAST2 dat;
                if (_logView) { dat = LOGDATA_NOW.DATA; }
                else
                {
                    double nowTime = GPS.UTCsecondsOfTheWeek(_posExNOW);
                    dat = new POS.PAST2(nowTime,
                        (float)TEXTBOX_Roll.Value, (float)TEXTBOX_Pitch.Value, (float)TEXTBOX_Heading.Value,
                        POS.ConvertToArcsec(POSITION.Latitude), POS.ConvertToArcsec(POSITION.Longitude), POSITION.Altitude,
                        (float)TEXTBOX_Speed.Value, (float)TEXTBOX_Track.Value,
                        (float)TEXTBOX_LongAccel.Value, (float)TEXTBOX_TranAccel.Value, (float)TEXTBOX_DownAccel.Value);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int day = (int)Math.Truncate(nowTime / 24 / 3600);
                        int hh = (int)Math.Truncate((nowTime - (day * 24 * 3600)) / 3600);
                        int mm = (int)Math.Truncate((nowTime - (day * 24 * 3600) - (hh * 3600)) / 60);
                        double ss = nowTime - day * 24 * 3600 - hh * 3600 - mm * 60;
                        LABEL_Time.Content = day.ToString("0") + "day " + hh.ToString("00") + ":" + mm.ToString("00") + ":" + ss.ToString("00.000000");
                    }));
                }
                try
                {
                    _serial.Write(dat.DATA, 0, dat.DATA.Count());
                }
                catch(Exception ex)
                {
                    if (MessageBox.Show("Serial Port Error.\n" + ex.Message + "\nClose the app?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        this.Close();
                        return;
                    }
                }
                while(_posExTASKstate && (DateTime.Now - _posExNOW).Milliseconds < cycle) { Thread.Sleep(taskLoopSleep); }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                POS_OUTPUT_TASK_Stop(false);
                if (_serial?.IsOpen == true) { _serial.Close(); _serial = null; }
            }
            catch { }
        }
    }
}
