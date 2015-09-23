using UnityEngine;
using System.Collections;

public class GUIBehaviour : MonoBehaviour {

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
			if(GUI.Button(new Rect(20,60,80,20), "Step")) 
			{
				GameStorage.getInstance().registerFriendlyShuttle(new Vector2(10,10),15);
        	}
			if(GUI.Button(new Rect(20,90,80,20),"Clear"))
			{
				foreach(GameObject f in GameStorage.getInstance().getFiendlyShuttles())
				{
					GameStorage.getInstance().removeFriendlyShuttle(f);
				}
			}
			if(GUI.Button(new Rect(20,120,80,20),"GO"))
			{
				MoveObjects();
			}
		}
	}
	
	void MoveObjects()
	{
		
	}
}
