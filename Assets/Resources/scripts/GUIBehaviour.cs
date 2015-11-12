using UnityEngine;
using System.Collections;

public class GUIBehaviour : MonoBehaviour {

	GUISkin buttonZoomInSkin,buttonZoomOutSkin;
	GUISkin buttonPlay,buttonPlayGrey;
	GUISkin buttonPrev,buttonNext;
	GUISkin buttonCharge,buttonChargeGrey;
	
	private int camScaleSpeed=5;
	
	
	void Start()
	{
		buttonZoomInSkin=(GUISkin) Resources.Load("gui/skins/button_zoomIn");
		buttonZoomOutSkin=(GUISkin) Resources.Load("gui/skins/button_zoomOut");
		buttonPlay=(GUISkin) Resources.Load("gui/skins/button_play");
		buttonPlayGrey=(GUISkin) Resources.Load("gui/skins/button_play_grey");
		buttonNext=(GUISkin) Resources.Load("gui/skins/button_next");
		buttonPrev=(GUISkin) Resources.Load("gui/skins/button_prev");
		buttonCharge=(GUISkin) Resources.Load("gui/skins/button_cha");
		buttonChargeGrey=(GUISkin) Resources.Load("gui/skins/button_cha_grey");
	}
	
	void OnGUI()
	{
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
		
		if(GameStorage.getInstance().isRunning)
		{
			GUI.skin=buttonChargeGrey;
			GUI.Button(new Rect(20+16+6,Screen.height-20-32-5-32,32,32),"");
		}
		else
		{
			GUI.skin=buttonCharge;
			if(GUI.Button(new Rect(20+16+6,Screen.height-20-32-5-32,32,32),""))
				GameStorage.getInstance().setAllShipsMaxTraec();
		}
		GUI.skin=null;
		if(GUI.Button(new Rect(50,50,20,20),"D"))
			GameStorage.getInstance().isDebug=!GameStorage.getInstance().isDebug;
	}
}
