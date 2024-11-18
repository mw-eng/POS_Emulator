using System;
using System.Windows.Controls;

namespace POS_Emulator
{
    /// <summary>
    /// cntPOS.xaml の相互作用ロジック
    /// </summary>
    public partial class cntPOS : UserControl
    {
        private POS.PAST2 _data;
        public POS.PAST2 DATA
        {
            get { return _data; }
            set
            {
                _data = value;
                Dispatcher.BeginInvoke(new Action(() => {
                    if (value != null)
                    {
                        int day = (int)Math.Truncate(_data.TIME / 24 / 60 / 60);
                        int hh = (int)Math.Truncate((_data.TIME - (day * 24 * 60 * 60)) / 60 / 60);
                        int mm = (int)Math.Truncate((_data.TIME - (day * 24 * 60 * 60) - (hh * 60 * 60)) / 60);
                        double ss = _data.TIME - day * 24 * 60 * 60 - hh * 60 * 60 - mm * 60;
                        TIME_LABEL.Content = day.ToString() + "day " + hh.ToString("00") + ":" + mm.ToString("00") + ":" + ss.ToString("00.000000");
                        SPEED_LABEL.Content = _data.SPEED.ToString("0.00").PadLeft(20, ' ');
                        TRACK_LABEL.Content = _data.TRACK.ToString("0.00").PadLeft(20, ' ');
                        ROLL_LABEL.Content = _data.ROLL.ToString("0.00").PadLeft(20, ' ');
                        PITCH_LABEL.Content = _data.PITCH.ToString("0.00").PadLeft(20, ' ');
                        HEADING_LABEL.Content = _data.HEADING.ToString("0.00").PadLeft(20, ' ');
                        string lal = "";
                        hh = (int)Math.Truncate(_data.LATITUDE / 60 / 60);
                        mm = (int)Math.Truncate((_data.LATITUDE - (hh * 60 * 60)) / 60);
                        ss = _data.LATITUDE - hh * 60 * 60 - mm * 60;
                        if(hh < 0) { hh = -hh; lal = "S"; }
                        else { lal = "N"; }
                        LATITUDE_LABEL.Content = hh.ToString().PadLeft(3, ' ') + "°" + Math.Abs(mm).ToString("00") + "'" + Math.Abs(ss).ToString("00.000000") + "\" " + lal;
                        hh = (int)Math.Truncate(_data.LONGITUDE / 60 / 60);
                        mm = (int)Math.Truncate((_data.LONGITUDE - (hh * 60 * 60)) / 60);
                        ss = _data.LONGITUDE - hh * 60 * 60 - mm * 60;
                        if (hh < 0) { hh = -hh; lal = "W"; }
                        else { lal = "E"; }
                        LONGITUDE_LABEL.Content = hh.ToString().PadLeft(3, ' ') + "°" + Math.Abs(mm).ToString("00") + "'" + Math.Abs(ss).ToString("00.000000") + "\" " + lal;
                        ALTITUDE_LABEL.Content = _data.ALTITUDE.ToString("0.00").PadLeft(20, ' ');
                        LONG_ACCEL_LABEL.Content = _data.LONG_ACCEL.ToString("0.0000").PadLeft(20, ' ');
                        TRAN_ACCEL_LABEL.Content = _data.TRAN_ACCEL.ToString("0.0000").PadLeft(20, ' ');
                        DOWN_ACCEL_LABEL.Content = _data.DOWN_ACCEL.ToString("0.0000").PadLeft(20, ' ');
                    }
                    else
                    {
                        TIME_LABEL.Content = "---";
                        ROLL_LABEL.Content = "---";
                        PITCH_LABEL.Content = "---";
                        HEADING_LABEL.Content = "---";
                        LATITUDE_LABEL.Content = "---";
                        LONGITUDE_LABEL.Content = "---";
                        ALTITUDE_LABEL.Content = "---";
                        SPEED_LABEL.Content = "---";
                        TRACK_LABEL.Content = "---";
                        LONG_ACCEL_LABEL.Content = "---";
                        TRAN_ACCEL_LABEL.Content = "---";
                        DOWN_ACCEL_LABEL.Content = "---";
                    }
                }));
            }
        }
        public cntPOS()
        {
            InitializeComponent();
            DATA = null;
        }

    }
}
