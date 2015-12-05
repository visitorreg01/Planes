using UnityEngine;
using System.Collections;

public class GUIBehaviour : MonoBehaviour {
	private int camScaleSpeed=5;
	private Color normColor = new Color(1,1,1,1);
	private Color disabledColor = new Color(1,1,1,0.3f);
	
	void OnGUI()
	{
		GUI.enabled=!GameStorage.getInstance().overlap;
		if(GameStorage.getInstance().overlap)
			GUI.color=disabledColor;
		else
			GUI.color=normColor;
		
		//GUI.Label(new Rect(50,50,100,100),"Tries: "+GameStorage.tries);
		
		if(GameStorage.getInstance().isRunning)
			GUI.Button(new Rect(20+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2+5,Screen.height-20-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)*2-10,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().zalp_button_grey.button);
		else
		{
			bool res=false;
			foreach(GameObject g in GameStorage.getInstance().getFriendlyShuttles())
			{
				if(g.GetComponent<FriendlyShuttleBehaviour>().haveRockets() || g.GetComponent<FriendlyShuttleBehaviour>().haveThorpeds())
					res=true;
			}
			
			if(res)
			{
				if(GUI.Button(new Rect(20+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2+5,Screen.height-20-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)*2-10,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().zalp_button.button))
					GameStorage.getInstance().setThorpedesAndRocketsAbils();
			}
			else
			{
				GUI.Button(new Rect(20+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2+5,Screen.height-20-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)*2-10,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().zalp_button_grey.button);
			}
		}
		
		GUI.enabled=!GameStorage.getInstance().overlap;
		
		if(GUI.Button(new Rect(Screen.width-20-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)-10-Templates.ResolutionProblems.getActionAbilitySize(Screen.width),20,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonPause.button))
			GameStorage.getInstance().cam.GetComponent<CameraBehaviour>().gamePause();
		
		if(GameStorage.getInstance().isRunning)
		{
			GUI.Button(new Rect(Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)-20,20,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonPlayGrey.button);
		}
		else
		{
			if(GUI.Button(new Rect(Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)-20,20,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonPlay.button))
				GameStorage.getInstance().StepStart();
		}
		
		if(GUI.Button(new Rect(Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)*2-30,Screen.height-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)-20,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonZoomInSkin.button))
		{
			if(GameStorage.getInstance().cam.cameraSize-camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed>=GameStorage.getInstance().cam.cameraSizeMin)
			{
				GameStorage.getInstance().cam.cameraSize-=camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed;
				GameStorage.getInstance().zoom=GameStorage.getInstance().cam.cameraSize;
			}
		}
		
		if(GUI.Button(new Rect(Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)-20,Screen.height-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)-20,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonZoomOutSkin.button))
		{
			if(GameStorage.getInstance().cam.cameraSize+camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed<=GameStorage.getInstance().cam.cameraSizeMax)
			{
				GameStorage.getInstance().cam.cameraSize+=camScaleSpeed*GameStorage.getInstance().cam.cameraZoomSpeed;
				GameStorage.getInstance().zoom=GameStorage.getInstance().cam.cameraSize;
			}
		}
		
		if(GUI.Button(new Rect(20,Screen.height-20-Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonPrev.button))
			GameStorage.getInstance().prevShipFocus();
		
		if(GUI.Button(new Rect(20+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)+10,Screen.height-20-Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().buttonNext.button))
			GameStorage.getInstance().nextShipFocus();
		
		GUI.skin=null;
		//if(GUI.Button(new Rect(50,50,20,20),"D"))
		//	GameStorage.getInstance().isDebug=!GameStorage.getInstance().isDebug;
	}
}
