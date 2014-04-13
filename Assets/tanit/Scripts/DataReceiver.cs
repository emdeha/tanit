using Leap;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataReceiver : MonoBehaviour
{
    public string dataUrl = "http://localhost:52912/api/Satelites?type=json";
    public Vector3 earthScale;
    public Transform sat;
    public Controller m_leapController;

    private WWW satelliteData;
    private int currentSatelliteIndex = 0;
    private int maxSatelliteIndex = -1;
    private List<Transform> satellites; 
    private const float DURATION_FOR_VALID_CIRCLE_GESTURE = 0.6f;

    enum Mode
    {
        MOVE,
        CONTROL,
    };
    private Mode currentMode = Mode.MOVE;

	void Start () 
    {
        StartNewDownload();
        m_leapController = new Controller();
        m_leapController.EnableGesture(Gesture.GestureType.TYPECIRCLE);
        satellites = new List<Transform>();
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
                    string satName = satData["Name"].str;
                    Vector3 satPos = 
                        new Vector3(satData["Position"]["X"].n * 0.01f,
                                    satData["Position"]["Y"].n * 0.01f,
                                    satData["Position"]["Z"].n * 0.01f);
                    Vector3 satVel =
                       new Vector3(satData["Speed"]["X"].n * 0.01f,
                                   satData["Speed"]["Y"].n * 0.01f,
                                   satData["Speed"]["Z"].n * 0.01f);
                    //Debug.Log("name: " + satName);
                    //Debug.Log("pos: " + satPos.x + " " + satPos.y + " " + satPos.z);
                    //Debug.Log("speed: " + satVel.x + " " + satVel.y + " " + satVel.z);

                    Transform newSat = 
                        Instantiate(sat, satPos + earthScale, Quaternion.identity) as Transform;
                    newSat.GetComponent<SatelliteMover>().speed = 100 * satVel;
                    newSat.name = satName;
                    satellites.Add(newSat);
                    if (maxSatelliteIndex == -1)
                        currentSatelliteIndex++;
                }
                if (maxSatelliteIndex == -1)
                {
                    currentSatelliteIndex--;
                    maxSatelliteIndex = currentSatelliteIndex;
                }

                transform.position = satellites[currentSatelliteIndex].position;
                transform.parent = satellites[currentSatelliteIndex];

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

    void Update()
    {
        UpdateGestures();
    }

    void UpdateGestures()
    {
        Frame frame = m_leapController.Frame();

        for (int g = 0; g < frame.Gestures().Count; g++)
        {
            Gesture currentGesture = frame.Gestures()[g];
            switch (currentGesture.Type)
            {
            case Gesture.GestureType.TYPECIRCLE:
                if (currentGesture.State == Gesture.GestureState.STATESTOP &&
                    currentGesture.DurationSeconds < DURATION_FOR_VALID_CIRCLE_GESTURE)
                {
                    SwitchMode();
                }
                break;
            }
        }
    }

    void SwitchMode()
    {
        if (currentMode == Mode.MOVE)
        {
            transform.GetComponent<LeapMove>().isActive = false;
            transform.GetComponent<LeapControl>().isActive = true;
            currentMode = Mode.CONTROL;
            Debug.Log("Mode.CONTROL");
        }
        else if (currentMode == Mode.CONTROL)
        {
            transform.GetComponent<LeapMove>().isActive = true;
            transform.GetComponent<LeapControl>().isActive = false;
            currentMode = Mode.MOVE;
            Debug.Log("Mode.MOVE");
        }
        else
        {
            Debug.LogWarning("Mode switching problem!");
        }
        Debug.Log("Switching mode!");
    }

    public void GoToNextSatellite()
    {
        if (currentSatelliteIndex >= maxSatelliteIndex)
        {
            currentSatelliteIndex = 0;
        }

        transform.position = satellites[currentSatelliteIndex++].position;
        transform.parent = satellites[currentSatelliteIndex++];
    }
}
