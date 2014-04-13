using UnityEngine;
using System.Collections;
using Leap;

public class LeapMove : MonoBehaviour {

    public Controller m_leapController;
    public bool isActive = true;

	void Start () 
    {
        m_leapController = new Controller();
        //if (transform.parent == null)
        //{
        //    Debug.LogError("LeapFly must have a parent object to control");
        //}
	}

	void Update () 
    {
        if (isActive)
        {
            Frame frame = m_leapController.Frame();

            if (frame.Hands.Count > 0)
            {
                Hand hand = frame.Hands[0];
                float velX = hand.PalmVelocity.x;
                float velY = hand.PalmVelocity.y;
                transform.Rotate(Vector3.right * Time.deltaTime * (-velY) * 0.5f);
                transform.Rotate(Vector3.up * Time.deltaTime * velX * 0.5f);
            }    
        }
	}
}
