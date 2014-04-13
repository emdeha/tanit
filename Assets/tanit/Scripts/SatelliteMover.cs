using UnityEngine;
using System.Collections;

public class SatelliteMover : MonoBehaviour 
{
    public Vector3 speed;

	void Start () 
    {
	}
	
	void Update () 
    {
        transform.Translate(speed.x * Time.deltaTime, speed.y * Time.deltaTime,
                            speed.z * Time.deltaTime);	
	}
}
