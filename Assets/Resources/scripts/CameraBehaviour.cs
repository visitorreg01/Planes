﻿using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

	Vector3 startPoint;
	bool released = false;
	float scale = 30;
	public float cameraSize=20;
	public float cameraZoomSpeed=1;
	public float cameraSizeMin=5;
	public float cameraSizeMax=50;
	
	float x,y;
	float cameraH=20;
	float terrainH,terrainW;
	const float speed=45;
	
	bool showNextLevelWindow = false;
	int stars=0,nextLevel=0;
	
	void Start () {
		GameStorage.getInstance().cam=this;
	}
	
	void Update()
	{
		if(!showNextLevelWindow)
		{
			if(Input.GetButtonDown("Fire2"))
				released=true;
			
			if(Input.GetButtonUp("Fire2"))
				released=false;
			
			if(Input.GetAxis("Mouse ScrollWheel") > 0)
			{
				if(cameraSize<=cameraSizeMin)
					cameraSize=cameraSizeMin;
				else
					cameraSize-=cameraZoomSpeed;
			}
			if(Input.GetAxis("Mouse ScrollWheel") < 0)
			{
				if(cameraSize>=cameraSizeMax)
					cameraSize=cameraSizeMax;
				else
					cameraSize+=cameraZoomSpeed;
			}
			
			GameStorage.getInstance().zoom=cameraSize;
			
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
	
	void OnGUI()
	{
		if(showNextLevelWindow)
		{
			GameStorage.getInstance().overlap=true;
			GUI.depth=200;
			
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
			int boxW=Screen.width/100*20;
			int boxH=Screen.height/100*40;
			string boxLabel="End mission "+(nextLevel-1)+"\n";
			if(stars==-1)
				boxLabel+="LOSE!";
			else if(stars==0)
				boxLabel+="DRAW!";
			else
				boxLabel+="WIN!\nStars: "+stars;
			GUI.Box(new Rect(Screen.width/2-boxW/2,Screen.height/2-boxH/2,boxW,boxH),boxLabel);
			int buttonH,buttonW;
			buttonH=boxH/100*20;
			if(stars==-1 || nextLevel==31) buttonW=boxW/2-10;
			else buttonW=boxW/3-10;
			
			if(stars==-1 || stars==0)
			{
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+5,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"To Main"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					Application.LoadLevel("mainGui");
				}
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+15+buttonW,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"Reply"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel(nextLevel-1));
				}
			}
			else
			{
				int maxStars = PlayerPrefs.GetInt("level"+(nextLevel-2)+"Stars",0);
				if(maxStars==0)
					PlayerPrefs.SetInt("level"+(nextLevel-2)+"Stars",stars);
				else
				{
					if(maxStars<stars)
						PlayerPrefs.SetInt("level"+(nextLevel-2)+"Stars",stars);
				}
				PlayerPrefs.Save();
				
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+5,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"To Main"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					Application.LoadLevel("mainGui");
				}
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+15+buttonW,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"Reply"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel(nextLevel-1));
				}
				if(nextLevel==31)
				{
					if(GUI.Button(new Rect(Screen.width/2-boxW/2+25+buttonW*2,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"Next level"))
					{
						GameStorage.getInstance().overlap=false;
						showNextLevelWindow=false;
						GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel(nextLevel));
					}
				}
			}
			GUI.depth=GameStorage.getInstance().defaultDepth;
		}
		
	}
	
	public void nextLevelWindow(int stars, int nextLevel)
	{
		this.stars=stars;
		this.nextLevel=nextLevel;
		showNextLevelWindow=true;
	}
}
