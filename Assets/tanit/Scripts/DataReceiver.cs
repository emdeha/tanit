using Leap;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO.Ports;

public class DataReceiver : MonoBehaviour
{
    public Vector3 earthScale;
    public GameObject sat;
    public Controller m_leapController;
    public int zoomCoef = 20;

    private string dataUrl = "http://localhost:52912/api/Satelites?type=json&t=";
    private WWW satelliteData;
    private GameObject currentSatellite;
    private List<GameObject> satellites; 
    private const float DURATION_FOR_VALID_CIRCLE_GESTURE = 0.6f;
    private const float MAX_SAT_SPEED = 10.0f;
    private bool isLookAt = false;

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
        satellites = new List<GameObject>();
	}

    void StartNewDownload()
    {
        string newDataUrl = dataUrl + (Time.time + 1000.0);
        Debug.Log("url: " + newDataUrl);
        satelliteData = new WWW(newDataUrl);
        UpdateSatellites();
    }

    void UpdateSatellites()
    {
        if (satelliteData.isDone)
        {
            if (satelliteData.error == null)
            {
                foreach(GameObject _sat in satellites)
                {
                    _sat.transform.DetachChildren();
                    Destroy(_sat);
                }
                satellites.Clear();
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

                    GameObject newSat = 
                        Instantiate(sat, satPos + earthScale, Quaternion.identity) as GameObject;
                    newSat.GetComponent<SatelliteMover>().speed = satVel;
                    newSat.name = satName;
                    currentSatellite = newSat;
                    satellites.Add(newSat);
                }

                transform.position = new Vector3(3.0f, 3.0f, 3.0f) + currentSatellite.transform.position;
                Vector3 pos = transform.position;
                Debug.Log("pos: " + pos.x + " " + pos.y + " " + pos.z);
                transform.parent = currentSatellite.transform;

                SendSatDataToSerialPort(currentSatellite.transform.position, 
                                        currentSatellite.GetComponent<SatelliteMover>().speed.magnitude);
                
                Invoke("StartNewDownload", 30);
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

    string FormatNumArd(float num)
    {
        string lon = num.ToString("f2");
        if (num < 0.0f)
        {
            lon = lon.PadLeft(5, '_');
            lon = '-' + lon;
        }
        else lon = lon.PadLeft(6, '_');

        return lon;
    }
    void SendSatDataToSerialPort(Vector3 satPos, float satSpeed)
    {
        satSpeed = satSpeed * 100.0f;
        float clampedSpeed = (float)(Math.Round(satSpeed, 0) / MAX_SAT_SPEED) * 10;
        int iClampedSpeed = (int)clampedSpeed;
        Debug.Log("sat speed: " + satSpeed);
        Debug.Log("Rounded: " + Math.Round(satSpeed, 0));
        Debug.Log("clamped: " + (int)(Math.Round(satSpeed, 0) / MAX_SAT_SPEED));
        Vector3 sphericalPos;
        sphericalPos.z = (float)Math.Sqrt((double)(satPos.x * satPos.x + satPos.y * satPos.y + satPos.z * satPos.z));
        sphericalPos.x = (float)Math.Acos((double)satPos.z / (double)sphericalPos.z);
        sphericalPos.y = (float)Math.Atan((double)satPos.y / (double)satPos.z);
        
        string lon = FormatNumArd(sphericalPos.x);
        string len = FormatNumArd(sphericalPos.y);
        string height = FormatNumArd(sphericalPos.z);

        Debug.Log("lon: " + lon + " len: " + len + " height: " + height + " speed: " + clampedSpeed); 

        SerialPort port = new SerialPort("COM7", 9600, Parity.None, 8, StopBits.One);

        port.Open();
        port.Write(lon + "," + len + "," + height + ",");
        port.Write(new byte[] {(byte)iClampedSpeed}, 0, 1);
        port.Write(";");
        Debug.Log("contents: " + port.ReadExisting());
        port.Close();
    }

    void Update()
    {
        UpdateGestures();
        if (isLookAt)
            transform.LookAt(Vector3.zero);
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

        if (Input.GetKeyDown("a"))
        {
            Debug.Log("Changing mode via 'a'");
            SwitchMode();
        }
        if (Input.GetKey("="))
        {
            Time.timeScale += zoomCoef;
            //camera.fieldOfView += Time.deltaTime * zoomCoef;
        }
        else if (Input.GetKey("-"))
        {
            Time.timeScale -= zoomCoef;
            //camera.fieldOfView -= Time.deltaTime * zoomCoef;
        }
        if (Input.GetKeyDown("t"))
        {
            isLookAt = !isLookAt;
        }


        //SerialPort port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);

        //port.Open();
        //port.Close();
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
        int currIdx = satellites.IndexOf(currentSatellite);
        currIdx += 1;
        currentSatellite = 
            satellites[currIdx == -1 ? 0 : currIdx % (satellites.Count - 1)];

        transform.position = currentSatellite.transform.position;
        transform.parent = currentSatellite.transform;
    }
}
