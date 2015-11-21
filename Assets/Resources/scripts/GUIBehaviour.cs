using UnityEngine;
using System.Collections;

public class GUIBehaviour : MonoBehaviour {

	GUISkin buttonZoomInSkin,buttonZoomOutSkin;
	GUISkin buttonPlay,buttonPlayGrey;
	GUISkin buttonPrev,buttonNext;
	
	private int camScaleSpeed=5;
	
	
	void Start()
	{
		buttonZoomInSkin=(GUISkin) Resources.Load("gui/skins/button_zoomIn");
		buttonZoomOutSkin=(GUISkin) Resources.Load("gui/skins/button_zoomOut");
		buttonPlay=(GUISkin) Resources.Load("gui/skins/button_play");
		buttonPlayGrey=(GUISkin) Resources.Load("gui/skins/button_play_grey");
		buttonNext=(GUISkin) Resources.Load("gui/skins/button_next");
		buttonPrev=(GUISkin) Resources.Load("gui/skins/button_prev");
	}
	
	void OnGUI()
	{
		GUI.enabled=!GameStorage.getInstance().overlap;
		
		if(GameStorage.getInstance().isRunning)
			GUI.enabled=false;
		else
		{
			bool res=false;
			foreach(GameObject g in GameStorage.getInstance().getFriendlyShuttles())
			{
				if(g.GetComponent<FriendlyShuttleBehaviour>().haveRockets() || g.GetComponent<FriendlyShuttleBehaviour>().haveThorpeds())
					res=true;
			}
			
			if(!res)
				GUI.enabled=false;
		}
		
		if(GameStorage.getInstance().overlap)
			GUI.enabled=false;
		
		if(GUI.Button(new Rect(5,5,32,32),"Q"))
			GameStorage.getInstance().setThorpedesAndRocketsAbils();
		
		GUI.enabled=!GameStorage.getInstance().overlap;
		
		if(GUI.Button(new Rect(Screen.width-20-32-10-32,20,32,32),"II"))
			GameStorage.getInstance().cam.GetComponent<CameraBehaviour>().gamePause();
		
		GUI.skin=Templates.getInstance().playBIG;
		GUI.Button(new Rect(Screen.width-32-20,30+32,32,32),"");
		
		if(GameStorage.getInstance().isRunning)
		{
			GUI.skin=buttonPlayGrey;
			GUI.Button(new Rect(Screen.width-32-20,20,32,32),"");
		}
		else
		{
			GUI.skin=buttonPlay;
			if(GUI.Button(new Rect(Screen.width-32-20,20,32,32),""))
				GameStorage.getInstance().StepStart();
		}
		
		GUI.skin=buttonZoomInSkin;
		if(GUI.Button(new Rect(Screen.width-32-62,Screen.height-32-20,32,32),""))
		{
			if(GameStorage.getInstance().cam.cameraSize-camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed>=GameStorage.getInstance().cam.cameraSizeMin)
			{
				GameStorage.getInstance().cam.cameraSize-=camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed;
				GameStorage.getInstance().zoom=GameStorage.getInstance().cam.cameraSize;
			}
		}
		
		GUI.skin=buttonZoomOutSkin;
		if(GUI.Button(new Rect(Screen.width-32-20,Screen.height-32-20,32,32),""))
		{
			if(GameStorage.getInstance().cam.cameraSize+camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed<=GameStorage.getInstance().cam.cameraSizeMax)
			{
				GameStorage.getInstance().cam.cameraSize+=camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed;
				GameStorage.getInstance().zoom=GameStorage.getInstance().cam.cameraSize;
			}
		}
		
		GUI.skin=buttonPrev;
		if(GUI.Button(new Rect(20,Screen.height-20-32,32,32),""))
			GameStorage.getInstance().prevShipFocus();
		
		GUI.skin=buttonNext;
		if(GUI.Button(new Rect(20+32+10,Screen.height-20-32,32,32),""))
			GameStorage.getInstance().nextShipFocus();
		
		GUI.skin=null;
		//if(GUI.Button(new Rect(50,50,20,20),"D"))
		//	GameStorage.getInstance().isDebug=!GameStorage.getInstance().isDebug;
	}
}
