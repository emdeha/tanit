using Leap;
using UnityEngine;
using System.Collections;

public class LeapControl : MonoBehaviour 
{
    public bool isActive = false;
    public Controller m_leapController;

    private const float DURATION_FOR_VALID_SWIPE_GESTURE = 1.0f;

	void Start () 
    {
        m_leapController = new Controller();
        m_leapController.EnableGesture(Gesture.GestureType.TYPESWIPE);
	}
	
	void Update () 
    {
        if (isActive)
        {
            UpdateGestures();
        }
	}

    void UpdateGestures()
    {
        Frame frame = m_leapController.Frame();

        for (int g = 0; g < frame.Gestures().Count; g++)
        {
            Gesture currentGesture = frame.Gestures()[g];
            switch (currentGesture.Type)
            {
            case Gesture.GestureType.TYPESWIPE:
                if (currentGesture.State == Gesture.GestureState.STATESTOP &&
                    currentGesture.DurationSeconds < DURATION_FOR_VALID_SWIPE_GESTURE)
                {
                    Debug.Log("Swiping!");
                    NextSatellite();
                }
                break;
            }
        }

        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Swiping via space");
            NextSatellite();
        }
    }

    void NextSatellite()
    {
        transform.GetComponent<DataReceiver>().GoToNextSatellite();
    }
}
