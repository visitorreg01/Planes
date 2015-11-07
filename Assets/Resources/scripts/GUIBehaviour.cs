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
	
	
	
	bool viewMenu=false;
	void OnGUI()
	{
		if(GameStorage.getInstance().isRunning)
		{
			GUI.skin=buttonPlayGrey;
			GUI.Button(new Rect(20,20,32,32),"");
		}
		else
		{
			GUI.skin=buttonPlay;
			if(GUI.Button(new Rect(20,20,32,32),""))
				GameStorage.getInstance().StepStart();
		}
		
		GUI.skin=buttonZoomInSkin;
		if(GUI.Button(new Rect(62,20,32,32),""))
		{
			if(GameStorage.getInstance().cam.cameraSize-camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed>=GameStorage.getInstance().cam.cameraSizeMin)
			{
				GameStorage.getInstance().cam.cameraSize-=camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed;
				GameStorage.getInstance().zoom=GameStorage.getInstance().cam.cameraSize;
			}
		}
		
		GUI.skin=buttonZoomOutSkin;
		if(GUI.Button(new Rect(104,20,32,32),""))
		{
			if(GameStorage.getInstance().cam.cameraSize+camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed<=GameStorage.getInstance().cam.cameraSizeMax)
			{
				GameStorage.getInstance().cam.cameraSize+=camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed;
				GameStorage.getInstance().zoom=GameStorage.getInstance().cam.cameraSize;
			}
		}
		
		GUI.skin=buttonPrev;
		if(GUI.Button(new Rect(146,20,32,32),""))
		{
			GameObject obj;
			if(GameStorage.getInstance().currentSelectedFriendly==null)
			{
				foreach(GameObject o in GameStorage.getInstance().getFriendlyShuttles())
					o.GetComponent<FriendlyShuttleBehaviour>().selected=false;
				obj=GameStorage.getInstance().getFriendlyShuttles()[GameStorage.getInstance().getFriendlyShuttles().Length-1];
				GameStorage.getInstance().currentSelectedFriendly=obj;
				GameStorage.getInstance().cam.transform.position=new Vector3(obj.transform.position.x,GameStorage.getInstance().cam.transform.position.y,obj.transform.position.z);
			}
			else
			{
				int i=0;
				int nextSelected=0;
				foreach(GameObject o in GameStorage.getInstance().getFriendlyShuttles())
				{
					if(o==GameStorage.getInstance().currentSelectedFriendly)
						nextSelected=(i-1)%GameStorage.getInstance().getFriendlyShuttles().Length;
					o.GetComponent<FriendlyShuttleBehaviour>().selected=false;
					i++;
				}
				obj=GameStorage.getInstance().getFriendlyShuttles()[nextSelected];
				GameStorage.getInstance().currentSelectedFriendly=obj;
				GameStorage.getInstance().cam.transform.position=new Vector3(obj.transform.position.x,GameStorage.getInstance().cam.transform.position.y,obj.transform.position.z);
			}
			
			foreach(GameObject o in GameStorage.getInstance().getFriendlyShuttles())
			{
				
			}
		}
		
		GUI.skin=buttonNext;
		GUI.Button(new Rect(188,20,32,32),"");
		GUI.skin=null;
		
		if(GUI.Button(new Rect(230,20,80,20),"Debug"))
				GameStorage.getInstance().isDebug=!GameStorage.getInstance().isDebug;
		
	}
}
