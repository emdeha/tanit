using UnityEngine;
using System.Collections;

public class DataReceiver : MonoBehaviour
{
    public string dataUrl = "http://localhost:52912/api/Satelites?type=json";
    public Vector3 earthScale;
    public Transform sat;

    private WWW satelliteData;

	void Start () 
    {
        StartNewDownload();
	}

    void StartNewDownload()
    {
        satelliteData = new WWW(dataUrl);
        UpdateSatellites();
    }

    void UpdateSatellites()
    {
        if (satelliteData.isDone)
        {
            if (satelliteData.error == null)
            {
                Debug.Log("Loaded data!!!");
                JSONObject jsonSats = new JSONObject(satelliteData.text);
                foreach(JSONObject satData in jsonSats.list)
                {
                    Debug.Log("DATA: " + satData);
                    string satName = satData["Name"].str;
                    Vector3 satPos = 
                        new Vector3(satData["Position"]["X"].n * 0.01f,
                                    satData["Position"]["Y"].n * 0.01f,
                                    satData["Position"]["Z"].n * 0.01f);
                    Vector3 satVel =
                       new Vector3(satData["Speed"]["X"].n * 0.01f,
                                   satData["Speed"]["Y"].n * 0.01f,
                                   satData["Speed"]["Z"].n * 0.01f);
                    Debug.Log("name: " + satName);
                    Debug.Log("pos: " + satPos.x + " " + satPos.y + " " + satPos.z);
                    Debug.Log("speed: " + satVel.x + " " + satVel.y + " " + satVel.z);

                    Transform newSat = 
                        Instantiate(sat, satPos + earthScale, Quaternion.identity) as Transform;
                    newSat.GetComponent<SatelliteMover>().speed = satVel;
                }

                Invoke("StartNewDownload", 60);
            }
            else
            {
                Debug.Log("Error loading data: " + satelliteData.error);
                Debug.Log("Url: " + dataUrl);
            }
        }
        else
        {
            Invoke("UpdateSatellites", 1);
        }
    }
}
