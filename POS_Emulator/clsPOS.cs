using System;
using System.Collections.Generic;
using System.Linq;

namespace POS_Emulator
{
    public class POS
    {
        public class PAST2
        {
            #region 値取得プロパティ
            /// <summary>時間[s]</summary>
            public double TIME { get { return BitConverter.ToDouble(DATA, 2); } }
            /// <summary>Roll角[degree](Range -180 to +180 @ step0.01)</summary>
            public float ROLL { get { return BitConverter.ToInt16(DATA, 10) * 0.01f; } }
            /// <summary>Pitch角[degree](Range -180 to +180 @ step0.01)</summary>
            public float PITCH { get { return BitConverter.ToInt16(DATA, 12) * 0.01f; } }
            /// <summary>Heading(Yaw)角[degree](Range 0 to 359.99 @ step0.01)</summary>
            public float HEADING { get { return BitConverter.ToUInt16(DATA, 14) * 0.01f; } }
            /// <summary>緯度[arcsec](Range -90 to +90 @ step0.001)</summary>
            public float LATITUDE { get { return BitConverter.ToInt32(DATA, 16) * 0.001f; } }
            /// <summary>経度[arcsec](Range -180 to +180 @ step0.001)</summary>
            public float LONGITUDE { get { return BitConverter.ToInt32(DATA, 20) * 0.001f; } }
            /// <summary>高度[m](Range -1000 to +20000 @ step0.01)</summary>
            public float ALTITUDE { get { return BitConverter.ToInt32(DATA, 24) * 0.01f; } }
            /// <summary>速度[m/s](Range 0 to 300 @ step0.01)</summary>
            public float SPEED { get { return BitConverter.ToUInt16(DATA, 28) * 0.01f; } }
            /// <summary>TRACK各[degree](Range 0 to 359.99 @ step0.01)</summary>
            public float TRACK { get { return BitConverter.ToUInt16(DATA, 30) * 0.01f; } }
            /// <summary>LONG加速度[m/s^2](Range -16.384 to +16.3835 @ step0.0005)</summary>
            public float LONG_ACCEL { get { return BitConverter.ToInt16(DATA, 32) * 0.0005f; } }
            /// <summary>TRAN加速度[m/s^2](Range -16.384 to +16.3835 @ step0.0005)</summary>
            public float TRAN_ACCEL { get { return BitConverter.ToInt16(DATA, 34) * 0.0005f; } }
            /// <summary>DOWN加速度[m/s^2](Range -16.384 to +16.3835 @ step0.0005)</summary>
            public float DOWN_ACCEL { get { return BitConverter.ToInt16(DATA, 36) * 0.0005f; } }

            /// <summary>PAST2データ配列の取得</summary>
            public byte[] DATA { get; private set; } = new byte[40];

            #endregion

            /// <summary>コンストラクタ</summary>
            /// <param name="dat">PAST2 binaryデータ</param>
            /// <exception cref="ArgumentOutOfRangeException">データ長(40byte)外エラー</exception>
            /// <exception cref="FormatException">PAST2ヘッダ(0x00-0x96)外エラー</exception>
            /// <exception cref="Exception">チェックサムエラー</exception>
            public PAST2(IEnumerable<byte> dat)
            {
                if (dat.Count() != 40) { throw new ArgumentOutOfRangeException("The parameter index cannot be anything other than 40bytes."); }
                DATA = dat.ToArray();
                if (DATA[0] != 0x00 || DATA[1] != 0x96) { throw new FormatException("Not in PAST2 message format."); }
                UInt16 sum = 0;
                for (uint count = 2; count < DATA.Length - 2; count++)
                {
                    sum += DATA[count];
                }
                if (sum != (DATA[38] + DATA[39] * 0x0100))
                { throw new Exception("Checksum Error."); }
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="time">時間[s]</param>
            /// <param name="roll">Roll角[degree](Range -180 to +180 @ step0.01)</param>
            /// <param name="pitch">Pitch角[degree](Range -180 to +180 @ step0.01)</param>
            /// <param name="heading">Heading(Yaw)角[degree](Range 0 to 359.99 @ step0.01)</param>
            /// <param name="latitude">緯度[arcsec](Range -90deg to +90deg @ step0.001)</param>
            /// <param name="longitude">経度[arcsec](Range -180deg to +180deg @ step0.001)</param>
            /// <param name="altitude">高度[m](Range -1000 to +20000 @ step0.01)</param>
            /// <param name="speed">速度[m/s](Range 0 to 300 @ step0.01)</param>
            /// <param name="track">TRACK各[degree](Range 0 to 359.99 @ step0.01)</param>
            /// <param name="long_accel">LONG加速度[m/s^2](Range -16.384 to +16.3835 @ step0.0005)</param>
            /// <param name="tran_accel">TRAN加速度[m/s^2](Range -16.384 to +16.3835 @ step0.0005)</param>
            /// <param name="down_accel">DOWN加速度[m/s^2](Range -16.384 to +16.3835 @ step0.0005)</param>
            public PAST2(double time, float roll, float pitch, float heading, float latitude, float longitude, float altitude, float speed, float track, float long_accel, float tran_accel, float down_accel)
            {
                DATA[0] = 0x00;
                DATA[1] = 0x96;
                BitConverter.GetBytes(time).CopyTo(DATA, 2);
                BitConverter.GetBytes((Int16)Math.Round(normalizedValue(roll, -180.0f, 180.0f) / 0.01f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 10);
                BitConverter.GetBytes((Int16)Math.Round(normalizedValue(pitch, -180.0f, 180.0f) / 0.01f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 12);
                BitConverter.GetBytes((Int16)Math.Round(normalizedValue(heading, 0.0f, 360.0f) / 0.01f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 14);
                BitConverter.GetBytes((Int32)Math.Round(normalizedValue(latitude, ConvertToArcsec(-90.0f), ConvertToArcsec(90.0f)) / 0.001f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 16);
                BitConverter.GetBytes((Int32)Math.Round(normalizedValue(longitude, ConvertToArcsec(-180.0f), ConvertToArcsec(180.0f)) / 0.001f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 20);
                BitConverter.GetBytes((Int32)Math.Round(normalizedValue(altitude, -1000.0f, 20000.0f) / 0.01f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 24);
                BitConverter.GetBytes((UInt16)Math.Round(normalizedValue(speed, 0.0f, 300.0f) / 0.01f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 28);
                BitConverter.GetBytes((UInt16)Math.Round(normalizedValue(track, 0.0f, 360.0f) / 0.01f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 30);
                BitConverter.GetBytes((Int16)Math.Round(normalizedValue(long_accel, -16.384f, 16.3835f) / 0.0005f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 32);
                BitConverter.GetBytes((Int16)Math.Round(normalizedValue(tran_accel, -16.384f, 16.3835f) / 0.0005f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 34);
                BitConverter.GetBytes((Int16)Math.Round(normalizedValue(down_accel, -16.384f, 16.3835f) / 0.0005f, MidpointRounding.AwayFromZero)).CopyTo(DATA, 36);

                UInt16 sum = 0;
                for (uint count = 2; count < DATA.Length - 2; count++)
                {
                    sum += DATA[count];
                }
                BitConverter.GetBytes(sum).CopyTo(DATA, 38);
            }

            /// <summary>値の正規化(範囲 min <= return < max)</summary>
            /// <param name="value">正規化対象値</param>
            /// <param name="min">最小値</param>
            /// <param name="max">最大値</param>
            /// <returns>正規化値(範囲 min <= return < max)</returns>
            static private float normalizedValue(float value, float min, float max)
            {
                float cycle = max - min;
                float norm = (value - min) % cycle + min;
                if(norm < min) { norm += cycle; }
                return norm;
            }
        }

        /// <summary>degreeからarcsecへ変換</summary>
        /// <param name="degree">degree</param>
        /// <returns>arcsec</returns>
        public static float ConvertToArcsec(float degree)
        {
            return degree * 3600f;
        }

        /// <summary>arcsecからdegreeへ変換</summary>
        /// <param name="arcsec">arcsec</param>
        /// <returns>degree</returns>
        public static float ConvertToDegree(float arcsec)
        {
            return arcsec / 3600f;
        }
    }
}
