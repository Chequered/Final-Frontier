using UnityEngine;
using System.Collections;

using FinalFrontier.Serialization;

public class MakeXMLProperties : MonoBehaviour {

	private void Start()
    {
        Properties prop = new Properties("unspecified");
        prop.fileName = string.Format("xml-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now) + ".xml";
        PropertyXMLHandler.Save(prop, Properties.dataRootPath);
    }
}
