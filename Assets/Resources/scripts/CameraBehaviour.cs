using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

	Vector3 startPoint;
	bool released = false;
	float scale = 30;
	float cameraSize=20;
	const float cameraZoomSpeed=1;
	const float cameraSizeMin=5;
	const float cameraSizeMax=50;
	
	float x,y;
	float cameraH=20;
	float terrainH,terrainW;
	const float speed=30;
	void Start () {
	}
	
	void Update()
	{
		if(Input.GetButtonDown("Fire2"))
			released=true;
		
		if(Input.GetButtonUp("Fire2"))
			released=false;
		
		if(Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			if(cameraSize<=cameraSizeMin)
				cameraSize=cameraSizeMin;
			else
				cameraSize-=cameraZoomSpeed;
		}
		if(Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			if(cameraSize>=cameraSizeMax)
				cameraSize=cameraSizeMax;
			else
				cameraSize+=cameraZoomSpeed;
		}
		
		Camera.main.orthographicSize=cameraSize;
		
		if(released)
		{
			Camera.main.transform.position += new Vector3(speed * Input.GetAxis("Mouse X") * Time.deltaTime, 0, speed * Input.GetAxis("Mouse Y") * Time.deltaTime)*-1;
			if(Camera.main.transform.position.z>scale)
				Camera.main.transform.position=new Vector3(Camera.main.transform.position.x,cameraH,scale);
			if(Camera.main.transform.position.z<-scale)
				Camera.main.transform.position=new Vector3(Camera.main.transform.position.x,cameraH,-scale);
			if(Camera.main.transform.position.x>scale)
				Camera.main.transform.position=new Vector3(scale,cameraH,Camera.main.transform.position.z);
			if(Camera.main.transform.position.x<-scale)
				Camera.main.transform.position=new Vector3(-scale,cameraH,Camera.main.transform.position.z);
			
		}
	}
	
}
