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
using MWComLibCS.CoordinateSystem;
using MWComLibCS;

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
        private double targLatitude = 0;
        private double targLongitude = 0;
        private double targAltitude = 0;
        private bool? coordSYS = true;

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
            targLongitude = Properties.Settings.Default.targLongitude;
            targLatitude = Properties.Settings.Default.targLatitude;
            targAltitude = Properties.Settings.Default.targAltitude;
            this.Title += " Ver," + System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.Topmost = Properties.Settings.Default.topMost;
            if (Properties.Settings.Default.topMost) { FixedInForeground.IsChecked = true; }
            else { FixedInForeground.IsChecked = false; }
            _kmlPath = "";
            _logView = false;
            MainGRID.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
            MainGRID.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
            if (SerialReOpen() == false) { this.Close(); return; }
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
            if (_logView)
            {
                LOG_PLAY_TASK_Stop(true);
                POS_OUTPUT_TASK_Stop(true);
                _logDAT = null;
                _logView = false;
                OLF.Header = "Open Log File";
                MainGRID.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                MainGRID.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                MainGRID.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Pixel);
                POS_OUTPUT_TASK_Start();
            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "log file|*.log|binary file|*.bin|All File|*.*";
                ofd.FilterIndex = 0;
                if (ofd.ShowDialog() != true)
                {
                    return;
                }
                List<List<byte>> dat = new List<List<byte>>();
                List<byte> byBF = System.IO.File.ReadAllBytes(ofd.FileName).ToList();
                int posi_n = SearchList.SearchBytesSundayIndexOf(byBF, new byte[] { 0x00, 0x96 });
                int posi_n1 = SearchList.SearchBytesSundayIndexOf(byBF, new byte[] { 0x00, 0x96 }, posi_n + 1);
                while (posi_n >= 0 && posi_n1 >= 0)
                {
                    dat.Add(byBF.GetRange(posi_n, posi_n1 - posi_n));
                    posi_n = posi_n1;
                    posi_n1 = SearchList.SearchBytesSundayIndexOf(byBF, new byte[] { 0x00, 0x96 }, posi_n + 1);
                }
                if (dat.Count() == 0)
                {
                    MessageBox.Show("Failed to read file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                OLF.Header = "Close Log File";
                POS_OUTPUT_TASK_Start();
            }
        }

        private void SaveKML_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_kmlPath) && !_kmlExTASKstate)
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
                SKML.Header = "Auto Save KML Stop";
                KML_OUTPUT_TASK_Start(1000);
            }
            else
            {
                KML_OUTPUT_TASK_Stop(true);
                _kmlPath = "";
                SKML.Header = "Auto Save KML Start";
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SerialConfig_Click(object sender, RoutedEventArgs e)
        {
            POS_OUTPUT_TASK_Stop(true);
            if (_serial?.IsOpen == true) { _serial.Close(); _serial = null; }
            winSettings win = new winSettings();
            win.ShowDialog();
            targLongitude = Properties.Settings.Default.targLongitude;
            targLatitude = Properties.Settings.Default.targLatitude;
            targAltitude = Properties.Settings.Default.targAltitude;
            if (SerialReOpen() == false) { this.Close(); return; }
            POS_OUTPUT_TASK_Start();
        }

        private void FixedInForeground_Click(object sender, RoutedEventArgs e)
        {
            if (FixedInForeground.IsChecked == true) { this.Topmost = true; }
            else { this.Topmost = false; }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.topMost = this.Topmost;
            Properties.Settings.Default.Save();
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

                double corrLatitude = 0;
                double corrLongitude = 0;
                double corrAltitude = 0;
                double corrRoll = 0;
                double corrPitch = 0;
                double corrHeading = 0;
                double corrAz = 0;
                double corrPol = 0;
                double trackCalc_az;
                double trackCalc_pol;
                if (dat.DATA != null)
                {
                    Angle pos_longitude = Angle.AngleSec(dat.LONGITUDE) + Angle.AngleDeg(corrLongitude);
                    Angle pos_latitude = Angle.AngleSec(dat.LATITUDE) + Angle.AngleDeg(corrLatitude);
                    double pos_altitude = dat.ALTITUDE + corrAltitude;
                    Angle pos_rollOFFSET = Angle.AngleDeg(dat.ROLL + corrRoll);
                    Angle pos_pitchOFFSET = Angle.AngleDeg(dat.PITCH + corrPitch);
                    Angle pos_headingOFFSET = Angle.AngleDeg(dat.HEADING + corrHeading);
                    WGS84CS target = new WGS84CS(Angle.AngleDeg(targLongitude), Angle.AngleDeg(targLatitude), targAltitude);
                    WGS84CS pos = new WGS84CS(pos_longitude, pos_latitude, pos_altitude);
                    OrthogonalCS enu = WGS84CS.ENU(pos, target);
                    OrthogonalCS targNWU = new OrthogonalCS(enu.Y, -enu.X, enu.Z);
                    OrthogonalCS targCalc = AxisRotation.RotateX(-pos_rollOFFSET, AxisRotation.RotateY(pos_pitchOFFSET, AxisRotation.RotateZ(pos_headingOFFSET, targNWU)));
                    OrthogonalCS targetCalcOffset = new OrthogonalCS(-targCalc.X, -targCalc.Y, -targCalc.Z);

                    CoordinateSystem3D targCalc3D = new CoordinateSystem3D(targetCalcOffset);
                    AntennaCS targCalcANT = new AntennaCS(targCalc3D.Phi, targCalc3D.Theta, null);
                    Angle targCalcAngle_az, targCalcAngle_pol;

                    if (targCalcANT.NormalizedAngle(coordSYS, out targCalcAngle_az, out targCalcAngle_pol))
                    {
                        trackCalc_az = Angle.Normalize180(targCalcAngle_az).Degree + corrAz;
                        trackCalc_pol = Angle.Normalize180(targCalcAngle_pol).Degree + corrPol;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TargLabel_Az.Content = trackCalc_az.ToString("0.0000").PadLeft(9, ' ');
                            TargLabel_El.Content = trackCalc_pol.ToString("0.0000").PadLeft(9, ' ');
                        }));
                    }
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DatLabel.Content = BitConverter.ToString(dat.DATA);
                }));
                try
                {
                    if (_serial?.IsOpen == true) { _serial.Write(dat.DATA, 0, dat.DATA.Count()); }
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show("Serial Port Error.\n" + ex.Message + "\nClose the app?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        this.Close();
                        return;
                    }
                }
                while (_posExTASKstate && (DateTime.Now - _posExNOW) < TimeSpan.FromMilliseconds(cycle)) { Thread.Sleep(taskLoopSleep); }
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
                    kml.Write(KML.AirPlaneMarker(0, "Position", "", Colors.Red, 1, heading, longitude, latitude, altitude, true));
                    kml.Write(KML.TargetCrossMarker(1, "Target", "", Colors.Red, 1, Properties.Settings.Default.targLongitude, Properties.Settings.Default.targLatitude, Properties.Settings.Default.targAltitude, true));
                    kml.Write(KML.Line(2, "TARGET LINE", "", Colors.Red, 3, new List<KML.POSITION> { new KML.POSITION(longitude, latitude,altitude), new KML.POSITION(Properties.Settings.Default.targLongitude, Properties.Settings.Default.targLatitude, Properties.Settings.Default.targAltitude) }));
                    kml.Write(KML.Footer());
                    kml.Close();
                    kml.Dispose();
                }
                while (_kmlExTASKstate && (DateTime.Now - _kmlExNOW) < TimeSpan.FromMilliseconds(cycle)) { Thread.Sleep(taskLoopSleep); }
            }
        }

        private bool? SerialReOpen()
        {
            if (_serial?.IsOpen == true) { _serial.Close(); _serial = null; }
            ShowSerialPortName.SerialPortTable sp;
            if (!ShowSerialPortName.SearchDeviceCaption(Properties.Settings.Default.spCaption, out sp))
            {
                if (MessageBox.Show("No serial port exists.\nClose the app?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    return false;
                }
                else
                {
                    if (MessageBox.Show("Do you want to use the app without outputting POS signals from the serial port?", "Question", MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes) { return null; }
                    winSettings win = new winSettings();
                    win.ShowDialog();
                    return SerialReOpen();
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
                return true;
            }
            catch
            {
                if (MessageBox.Show("Failed to open serial port.\nClose the app?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    return false;
                }
                else
                {
                    if (MessageBox.Show("Do you want to use the app without outputting POS signals from the serial port?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) { return null; }
                    winSettings win = new winSettings();
                    win.ShowDialog();
                    return SerialReOpen();
                }
            }
        }

    }
}
