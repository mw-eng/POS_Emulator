using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
        private List<List<byte>> _logDAT = new List<List<byte>>();
        private DateTime _logPlayNOW;
        private bool? _logPlayTASKstate = false;
        private Task _logPlayTASK;
        private UInt16 _logPlayCycle;
        private string _kmlPath;
        private DateTime _kmlExNOW;
        private bool _kmlExTASKstate;
        private Task _kmlExTASK;

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
            SerialReOpen();
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

            _kmlPath = "";
            _logView = false;
            MainGRID.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            MainGRID.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
            POS_OUTPUT_TASK_Start();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _logPlayCycle = (UInt16)Math.Truncate((1.0 / Properties.Settings.Default.spCycle) * 1000);
            if (_logPlayTASKstate == false) { LOG_PLAY_TASK_START(); }
            else if(_logPlayTASKstate == null) { LOG_PLAY_TASK_Restart(); }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            LOG_PLAY_TASK_Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            LOG_PLAY_TASK_Stop(true);
            SLIDER.Value = 0;
        }

        private void DecreasSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            try { _logPlayCycle *= 2; } catch { }
        }

        private void IncreasSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            _logPlayCycle /= 2;
            if(_logPlayCycle == 0) { _logPlayCycle = 1; }
        }

        private void SLIDER_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                LOGDATA_NOW.DATA = new POS.PAST2(_logDAT[(int)Math.Round(SLIDER.Value, MidpointRounding.AwayFromZero)]);
            }
            catch
            {

            }
        }

        private void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "log file|*.log|binary file|*.bin|All File|*.*";
            ofd.FilterIndex = 0;
            if(ofd.ShowDialog() != true)
            {
                return;
            }
            List<List<byte>> dat = new List<List<byte>>();
            List<byte> byBF = System.IO.File.ReadAllBytes(ofd.FileName).ToList();
            int posi_n = SearchList.SearchBytesSundayIndexOf(byBF, new byte[] { 0x00, 0x96 });
            int posi_n1 = SearchList.SearchBytesSundayIndexOf(byBF, new byte[] { 0x00, 0x96 }, posi_n + 1);
            while(posi_n >=0 && posi_n1 >= 0)
            {
                dat.Add(byBF.GetRange(posi_n, posi_n1 - posi_n));
                posi_n = posi_n1;
                posi_n1 = SearchList.SearchBytesSundayIndexOf(byBF, new byte[] { 0x00, 0x96 }, posi_n + 1);
            }
            if (dat.Count() == 0)
            {
                MessageBox.Show("Failed to read file.","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            POS_OUTPUT_TASK_Stop(true);
            _logDAT = dat;
            MainGRID.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            MainGRID.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
            MainGRID.RowDefinitions[2].Height = new GridLength(100, GridUnitType.Pixel);
            SLIDER.Maximum = _logDAT.Count();
            SLIDER.Value = 0;
            SLIDER.TickFrequency = _logDAT.Count() / 50;
            LOGDATA_NOW.DATA = new POS.PAST2(_logDAT[(int)Math.Round(SLIDER.Value, MidpointRounding.AwayFromZero)]);
            _logView = true;
            POS_OUTPUT_TASK_Start();
        }

        private void SaveKML_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "KML file|*.kml|All File|*.*";
            sfd.FilterIndex = 0;
            if (sfd.ShowDialog() != true)
            {
                return;
            }
            KML_OUTPUT_TASK_Stop(true);
            _kmlPath = sfd.FileName;
            KML_OUTPUT_TASK_Start(1000);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SerialConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                KML_OUTPUT_TASK_Stop(false);
                LOG_PLAY_TASK_Stop(false);
                POS_OUTPUT_TASK_Stop(false);
                if (_serial?.IsOpen == true) { _serial.Close(); _serial = null; }
            }
            catch { }
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
            _posExTASK?.Dispose();
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
                while(_posExTASKstate && (DateTime.Now - _posExNOW) < TimeSpan.FromMilliseconds(cycle)) { Thread.Sleep(taskLoopSleep); }
            }
        }

        private void LOG_PLAY_TASK_START()
        {
            _logPlayTASKstate = true;
            _logPlayTASK = Task.Factory.StartNew(() => { POS_PLAY_TASK(); });
        }
        private void LOG_PLAY_TASK_Stop(bool wait)
        {
            _logPlayTASKstate = false;
            if (wait)
            {
                _logPlayTASK?.ConfigureAwait(false);
                _logPlayTASK?.Wait();
            }
            _logPlayTASK?.Dispose();
            _logPlayTASK = null;
        }
        private void LOG_PLAY_TASK_Pause()
        {
            _logPlayTASKstate = null;
        }
        private void LOG_PLAY_TASK_Restart()
        {
            _logPlayTASKstate = true;
        }

        private void POS_PLAY_TASK()
        {
            while (_logPlayTASKstate != false)
            {
                while (_logPlayTASKstate == null) { Thread.Sleep(taskLoopSleep); }
                _logPlayNOW = DateTime.Now;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (SLIDER.Value >= SLIDER.Maximum) { _logPlayTASKstate = false; return; }
                    SLIDER.Value++;
                }));
                while (_logPlayTASKstate != false && (DateTime.Now - _logPlayNOW) < TimeSpan.FromMilliseconds(_logPlayCycle)) { Thread.Sleep(taskLoopSleep); }
            }
        }

        private void KML_OUTPUT_TASK_Start(UInt16 cycle)
        {
            _kmlExTASKstate = true;
            _kmlExTASK = Task.Factory.StartNew(() => { KML_OUTPUT_TASK(cycle); });
        }
        private void KML_OUTPUT_TASK_Stop(bool wait)
        {
            _kmlExTASKstate = false;
            if (wait)
            {
                _kmlExTASK?.ConfigureAwait(false);
                _kmlExTASK?.Wait();
            }
            _kmlExTASK?.Dispose();
            _kmlExTASK = null;
        }
        private void KML_OUTPUT_TASK(UInt16 cycle)
        {
            while (_kmlExTASKstate)
            {
                _kmlExNOW = DateTime.Now;
                float heading, latitude, longitude, altitude;

                if (_logView)
                {
                    heading = LOGDATA_NOW.DATA.HEADING;
                    latitude = POS.ConvertToDegree(LOGDATA_NOW.DATA.LATITUDE);
                    longitude = POS.ConvertToDegree(LOGDATA_NOW.DATA.LONGITUDE);
                    altitude = LOGDATA_NOW.DATA.ALTITUDE;
                }
                else
                {
                    heading = (float)TEXTBOX_Heading.Value;
                    latitude = POSITION.Latitude;
                    longitude = POSITION.Longitude;
                    altitude = POSITION.Altitude;
                }

                using (StreamWriter kml = new StreamWriter(_kmlPath, false, Encoding.UTF8))
                {
                    kml.Write(KML.Header(_kmlPath));
                    kml.Write(KML.AirPlaneMarker(0, "Position", "", 1, heading, longitude, latitude, altitude, true));
                    kml.Write(KML.Line(1, "TARGET LINE", "", Colors.Red, 3, new List<KML.POSITION> { new KML.POSITION(longitude, latitude,altitude), new KML.POSITION(138.163956f, 36.09266f, 1666.6f) }));
                    kml.Write(KML.Footer());
                    kml.Close();
                    kml.Dispose();
                }
                while (_kmlExTASKstate && (DateTime.Now - _kmlExNOW) < TimeSpan.FromMilliseconds(cycle)) { Thread.Sleep(taskLoopSleep); }
            }
        }

        private void SerialReOpen()
        {
            if (_serial?.IsOpen == true) { _serial.Close(); _serial = null; }
            ShowSerialPortName.SerialPortTable sp;
            if (!ShowSerialPortName.SearchDeviceCaption(Properties.Settings.Default.spCaption, out sp))
            {
                if (MessageBox.Show("No serial port exists.\nClose the app?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.Close();
                    return;
                }
                else
                {
                    winSettings win = new winSettings();
                    win.ShowDialog();
                    SerialReOpen();
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
                    SerialReOpen();
                    return;
                }
            }
        }

    }
}
