using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {

	int selectedLevel=0;
	bool showLevelsMenu=false;
	ArrayList levels;
	
	GUISkin skin; 
	
	// Use this for initialization
	void Start () {
		GameStorage.getInstance();
		Templates.getInstance();
		skin = (GUISkin) Resources.Load("gui/skins/ability_180");
	}
	
	void OnGUI()
	{
		if(GameStorage.getInstance().allReady)
		{
			GUI.Box(new Rect(20,20,120,20),"Selected level: "+selectedLevel);
			
			if(GUI.Button(new Rect(Screen.width/2,Screen.height/2,100,20),"Play"))
			{
				if(Templates.getInstance().getLevel(selectedLevel)!=null)
				{
					Application.LoadLevel(Templates.getInstance().getLevel(selectedLevel).file);
				}
			}
			
			if(GUI.Button(new Rect(20,50,100,20),"Levels"))
				showLevelsMenu=!showLevelsMenu;
			
			GUI.skin=skin;
			if(GUI.Button(new Rect(100,100,40,40),""))
				Debug.Log("Press");
			GUI.skin=null;
			
			if(showLevelsMenu)
			{
				levels=Templates.getInstance().getAllLevels();
				GUI.Box(new Rect(20,50,100,20+25*levels.Count),"");
				int i=0;
				foreach(Templates.LevelInfo lv in levels)
				{
					if(GUI.Button(new Rect(25,75+25*i,90,20),lv.num.ToString()))
						selectedLevel=lv.num;
					i++;
				}
			}
			
		}
	}
}
