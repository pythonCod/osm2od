using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Osm2Od;


//[XmlRoot("OpenDrive")]
//public class OpenDrive
//{
//    [XmlElement("road")]
//    public List<roads> road;
//}

//public class roads
//{
//    [XmlAttribute("road")]
//    public string name;
//    [XmlAttribute("length")]
//    public double length;
//    [XmlAttribute("id")]
//    public int id;
//    [XmlAttribute("junction")]
//    public int junction;
//    [XmlElement("link")]
//    public List<links> link;
//    [XmlElement("type")]
//    public string type;
//    [XmlElement("planView")]
//    public List<planViews> planView;
//}

//public class links
//{
//    [XmlElement("predecessor")]
//    public List<predecessors> predecessor;
//    [XmlElement("successor")]
//    public List<successors> successor;
//}

//public class predecessors
//{
//    [XmlAttribute("elementType")]
//    public string elementType;
//    [XmlAttribute("elementId")]
//    public string elementId;
//    [XmlAttribute("contactPoint")]
//    public string contactPoint;
//}

//public class successors
//{
//    [XmlAttribute("elementType")]
//    public string elementType;
//    [XmlAttribute("elementId")]
//    public string elementId;
//    [XmlAttribute("contactPoint")]
//    public string contactPoint;
//}

//public class types
//{
//    [XmlAttribute("s")]
//    public double s;
//    [XmlAttribute("type")]
//    public string type;
//}

//public class planViews
//{
//    [XmlElement("predecessor")]
//    public List<geometries> geometry;
//}
//public class geometries
//{
//    [XmlAttribute("s")]
//    public double s;
//    [XmlAttribute("x")]
//    public double x;
//    [XmlAttribute("y")]
//    public double y;
//    [XmlAttribute("hdg")]
//    public double hdg;
//    [XmlAttribute("x")]
//    public double length;
//    [XmlElement("shape")]
//    public List<shapes> shapes;
//}

//public class shapes
//{
//    public bool spiral;
//    public bool arc;
//    public bool line;
//}



