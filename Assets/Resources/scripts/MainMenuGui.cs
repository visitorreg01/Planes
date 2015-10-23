using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameStorage.getInstance();
		Templates.getInstance();
	}
	
	void OnGUI()
	{
		if(GameStorage.getInstance().allReady)
		{
			if(GUI.Button(new Rect(Screen.width/2,Screen.height/2,100,20),"Play"))
			{
				Application.LoadLevel("battleground");
			}
		}
	}
}
