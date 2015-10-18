using UnityEngine;
using System.Collections;

public class GUIBehaviour : MonoBehaviour {

	
	void Start()
	{
		GameStorage.getInstance();
		Templates.getInstance();
	}
	
	bool viewMenu=false;
	void OnGUI()
	{
		if(GUI.Button(new Rect(10,20,100,20),"Menu"))
		{
			viewMenu=!viewMenu;
		}
		
		if(viewMenu)
		{
			GUI.Box(new Rect(10,50,100,90), "");
			
			if(GUI.Button(new Rect(20,60,80,20),"Do step"))
			{
				GameStorage.getInstance().StepStart();
			}
			
			if(GUI.Button(new Rect(20,80,80,20),"Debug"))
				GameStorage.getInstance().isDebug=!GameStorage.getInstance().isDebug;
		}
	}
}
