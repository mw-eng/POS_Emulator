using System;

namespace MWComLibCS.CoordinateSystem
{
    /// <summary>GNSS座標値</summary>
    public class GNSS
    {
        public Angle Longitude;
        public Angle Latitude;
        public double Altitude;

        public GNSS() { }

        /// <summary>コンストラクタ</summary>
        /// <param name="_longitude">経度</param>
        /// <param name="_latitude">緯度</param>
        /// <param name="_altitude">高度[m]</param>
        public GNSS(Angle _longitude, Angle _latitude, double _altitude)
        { Longitude = _longitude; Latitude = _latitude; Altitude = _altitude; }

        /// <summary>コンストラクタ</summary>
        /// <param name="_longitude">経度[Arcsec]</param>
        /// <param name="_latitude">緯度[Arcsec]</param>
        /// <param name="_altitude">高度[m]</param>
        public GNSS(double _longitude, double _latitude, double _altitude) :
            this(Angle.AngleSec(_longitude), Angle.AngleSec(_latitude), _altitude)
        { }
    }

    /// <summary>
    /// <para>測地系座標</para>
    /// <para>原点:重心/Z軸:CIO方向/X軸:経度0deg/Y軸:Z,X軸と右手系の直交径</para>
    /// </summary>
    public class WGS84CS
    {
        /// <summary>扁平率の逆数f</summary>
        public static readonly double f = f_const;
        /// <summary>長(赤道)半径a[m]</summary>
        private const double a = 6378137;
        /// <summary>短(極)半径b[m]</summary>
        private const double b = 6356752.31425;
        /// <summary>離心率の2乗(e^2)</summary>
        private static readonly double e2 = e2_const;
        /// <summary>離心率e</summary>
        private static readonly double e = e_const;

        #region private static value
        /// <summary>扁平率の逆数f</summary>
        private const double f_const = 1.0 / 298.257223563;
        /// <summary>離心率の2乗(e^2)</summary>
        private const double e2_const = 0.00669437999013;
        /// <summary>離心率e</summary>
        private const double e_const = 0.0818191908426;
        /// <summary>扁平率の逆数f</summary>
        private static double f_calc { get { return (a - b) / a; } }
        /// <summary>離心率の2乗(e^2)</summary>
        private static double e2_calc { get { return 2.0 * f_calc - Math.Pow(f_calc, 2.0); } }
        /// <summary>離心率e</summary>
        private static double e_calc { get { return Math.Sqrt(Math.Pow(a, 2.0) - Math.Pow(b, 2.0)) / a; } }
        #endregion

        /// <summary>直交座標</summary>
        public OrthogonalCS OrthogonalCoordinate { get; private set; }
        /// <summary>経度</summary>
        public Angle Longitude { get; private set; }
        /// <summary>緯度</summary>
        public Angle Latitude { get; private set; }
        /// <summary>高度[m]</summary>
        public double Altitude { get; private set; }

        /// <summary>コンストラクタ(0で初期化)</summary>
        public WGS84CS() { OrthogonalCoordinate = new OrthogonalCS(0, 0, 0); }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_longitude">経度</param>
        /// <param name="_latitude">緯度</param>
        /// <param name="_altitude">高度[m]</param>
        public WGS84CS(Angle _longitude, Angle _latitude, double _altitude)
        {
            Longitude = _longitude; Latitude = _latitude; Altitude = _altitude;
            double n = a / Math.Sqrt(1.0 - Math.Pow(e, 2.0) * Math.Pow(Math.Sin(Latitude.Radian), 2.0));
            OrthogonalCoordinate = new OrthogonalCS(
                (n + Altitude) * Math.Cos(Latitude.Radian) * Math.Cos(Longitude.Radian),
                (n + Altitude) * Math.Cos(Latitude.Radian) * Math.Sin(Longitude.Radian),
                (n * (1 - e2) + Altitude) * Math.Sin(Latitude.Radian)
                );
        }

        public static OrthogonalCS ENU(WGS84CS POS,WGS84CS TARG)
        {
            OrthogonalCS ecef = new OrthogonalCS(
                TARG.OrthogonalCoordinate.X - POS.OrthogonalCoordinate.X,
                TARG.OrthogonalCoordinate.Y - POS.OrthogonalCoordinate.Y,
                TARG.OrthogonalCoordinate.Z - POS.OrthogonalCoordinate.Z
                );

            return new OrthogonalCS(
                -ecef.X * Math.Sin(POS.Longitude.Radian) + ecef.Y * Math.Cos(POS.Longitude.Radian),
                -ecef.X * Math.Sin(POS.Latitude.Radian) * Math.Cos(POS.Longitude.Radian) - ecef.Y * Math.Sin(POS.Latitude.Radian) * Math.Sin(POS.Longitude.Radian) + ecef.Z * Math.Cos(POS.Latitude.Radian),
                ecef.X * Math.Cos(POS.Latitude.Radian) * Math.Cos(POS.Longitude.Radian) + ecef.Y * Math.Cos(POS.Latitude.Radian) * Math.Sin(POS.Longitude.Radian) + ecef.Z * Math.Sin(POS.Latitude.Radian)
                );

            //return new OrthogonalCS(
            //    -ecef.X * Math.Sin(POS.Longitude.Radian) + ecef.Y * Math.Cos(POS.Longitude.Radian),
            //    ecef.X * Math.Sin(POS.Latitude.Radian) * Math.Cos(POS.Longitude.Radian) + ecef.Y * Math.Sin(POS.Latitude.Radian) * Math.Sin(POS.Longitude.Radian) - ecef.Z * Math.Cos(POS.Latitude.Radian),
            //    ecef.X * Math.Cos(POS.Latitude.Radian) * Math.Cos(POS.Longitude.Radian) + ecef.Y * Math.Cos(POS.Latitude.Radian) * Math.Sin(POS.Longitude.Radian) + ecef.Z * Math.Sin(POS.Latitude.Radian)
            //    );
        }
    }

    public static class AxisRotation
    {
        /// <summary>
        /// <para>座標系回転</para>
        /// <para>X軸を中心に回転(Roll角:Y軸からZ軸方向への回転が+方向)</para>
        /// </summary>
        /// <param name="ROLL">回転角(Y軸→Z軸方向が+)</param>
        /// <param name="Coord">回転前の座標</param>
        /// <returns>回転後の座標系における座標値</returns>
        public static OrthogonalCS RotateX(Angle ROLL, OrthogonalCS Coord)
        {
            return new OrthogonalCS(
                Coord.X,
                Coord.Y * Math.Cos(ROLL.Radian) - Coord.Z * Math.Sin(ROLL.Radian),
                Coord.Y * Math.Sin(ROLL.Radian) + Coord.Z * Math.Cos(ROLL.Radian)
                );
        }

        /// <summary>
        /// <para>座標系回転</para>
        /// <para>Y軸を中心に回転(Pitch角:Z軸からX軸方向への回転が+方向)</para>
        /// </summary>
        /// <param name="PITCH">回転角(Z軸→X軸方向が+)</param>
        /// <param name="Coord">回転前の座標</param>
        /// <returns>回転後の座標系における座標値</returns>
        public static OrthogonalCS RotateY(Angle PITCH, OrthogonalCS Coord)
        {
            return new OrthogonalCS(
                Coord.X * Math.Cos(PITCH.Radian) + Coord.Z * Math.Sin(PITCH.Radian),
                Coord.Y,
                -Coord.X * Math.Sin(PITCH.Radian) + Coord.Z * Math.Cos(PITCH.Radian)
                );
        }

        /// <summary>
        /// <para>座標系回転</para>
        /// <para>X軸を中心に回転(Yaw/Heading角:X軸からY軸方向への回転が+方向)</para>
        /// </summary>
        /// <param name="YAW">回転角(X軸→Y軸方向が+)</param>
        /// <param name="Coord">回転前の座標</param>
        /// <returns>回転後の座標系における座標値</returns>
        public static OrthogonalCS RotateZ(Angle YAW, OrthogonalCS Coord)
        {
            return new OrthogonalCS(
                Coord.X * Math.Cos(YAW.Radian) - Coord.Y * Math.Sin(YAW.Radian),
                Coord.X * Math.Sin(YAW.Radian) + Coord.Y * Math.Cos(YAW.Radian),
                Coord.Z
                );
        }
    }

}
