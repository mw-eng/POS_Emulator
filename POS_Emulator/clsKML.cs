using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace POS_Emulator
{
    public static class KML
    {
        public static string Header(string filePath)
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<kml xmlns=\"http://www.opengis.net/kml/2.2\">\n<Document>\n\t<name>" + System.IO.Path.GetFileName(filePath) + "</name>\n";
        }

        public static string Footer()
        {
            return "</Document>\n</kml>";
        }

        public static string AirPlaneMarker(int id, string name,string description, float scale,float heading, float longitude, float latitude, float altitude, bool gndLine)
        {
            string strBF = "";
            strBF += "<Placemark id=\"" + id.ToString() + "\">\n";
            strBF += "\t<name>" + name + "</name>\n";
            strBF += "\t<description>" + description + "</description>\n";

            strBF += "\t<Style>\n";
            strBF += "\t\t<IconStyle>\n";
            strBF += "\t\t\t<scale>" + scale.ToString("0.0") + "</scale>\n";
            strBF += "\t\t\t<heading>" + heading.ToString("0.00") + "</heading>\n";
            strBF += "\t\t\t<Icon><href>root://icons/palette-2.png</href><w>32</w><h>32</h></Icon>\n";
            strBF += "\t\t</IconStyle>\n";
            strBF += "\t</Style>\n";
            strBF += "\t<Point>\n";
            strBF += "\t\t<coordinates>" + longitude.ToString("0.0000000") + "," + latitude.ToString("0.0000000") + "," + altitude.ToString("0.00") + "</coordinates>\n";
            if (gndLine) { strBF += "\t\t<extrude>1</extrude>\n"; }
            
            strBF += "\t\t<altitudeMode>absolute</altitudeMode>\n";
            strBF += "\t</Point>\n";
            strBF += "</Placemark>\n";
            return strBF;
        }

        public static string Line(int id, string name, string description, Color lineColor,uint lineWidth , List<POSITION> position)
        {
            if(position.Count < 2) { return ""; }
            string strBF = "";
            strBF += "<Placemark id=\"" + id.ToString() + "\">\n";
            strBF += "\t<name>" + name + "</name>\n";
            strBF += "\t<description>" + description + "</description>\n";

            strBF += "\t<Style>\n";
            strBF += "\t\t<LineStyle>\n";
            strBF += "\t\t\t<color>"+ lineColor.ToString().Trim('#') + "</color>\n";
            strBF += "\t\t\t<width>"+ lineWidth.ToString() + "</width>\n";
            strBF += "\t\t</LineStyle>\n";
            strBF += "\t</Style>\n";

            strBF += "\t<LineString>\n";
            strBF += "\t\t<altitudeMode>absolute</altitudeMode>\n";
            strBF += "\t\t<coordinates>\n";
            foreach (POSITION posi in position)
            {
                strBF += "\t\t\t" + posi.LONGITUDE.ToString("0.0000000") + "," + posi.LATITUDE.ToString("0.0000000") + "," + posi.ALTITUDE.ToString("0.00") + "\n";
            }
            strBF += "\t\t</coordinates>\n";
            strBF += "\t</LineString>\n";
            strBF += "</Placemark>\n";
            return strBF;
        }

        public struct POSITION
        {
            public float LONGITUDE { get; set; }
            public float LATITUDE { get; set; }
            public float ALTITUDE { get; set; }

            public POSITION(float longitude,float latitude,float altitude)
            {
                LONGITUDE = longitude;LATITUDE = latitude;ALTITUDE = altitude;
            }

        }
    }
}
