using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {

	ArrayList levels; 
	int startPosition=100;
	int betweenDist=40;
	int levelButtonSize=30;
	
	// Use this for initialization
	void Start () {
		GameStorage.getInstance();
		Templates.getInstance();
	}
	
	int[] calculateLevelPosition(int i)
	{
		int h,w;
		if(i%2==0)
		{
			h=Screen.height/2+50;
			w=startPosition+betweenDist*i+levelButtonSize;
		}
		else
		{
			h=Screen.height/2-50;
			w=startPosition+betweenDist*i+levelButtonSize;
		}
		return new int[] {h,w};
	}
	
	void OnGUI()
	{
		if(GameStorage.getInstance().allReady)
		{
			if(GUI.Button(new Rect(5,5,100,20),"full"))
			{
				for(int j=0;j<30;j++)
					PlayerPrefs.SetInt("level"+j+"Stars",3);
			}
			
			string current_rank="";
			int currentRankId=PlayerPrefs.GetInt("currentRankId",-1);
			if(currentRankId==-1)
				current_rank="No ranked";
			else
				current_rank=Templates.getInstance().getRank(currentRankId).name;
			GUI.Label(new Rect(Screen.width-5-200,10,200,30),"Current Rank: "+current_rank);
			
			if(GUI.Button(new Rect(110,5,100,20),"null"))
			{
				PlayerPrefs.DeleteAll();
			}
			
			levels=Templates.getInstance().getAllLevels();
			int i=0,k;
			foreach(Templates.LevelInfo lv in levels)
			{
				int[] pos = calculateLevelPosition(i);
				k = PlayerPrefs.GetInt("level"+i+"Stars",0);
				if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),lv.num.ToString()))
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel(lv.num));
				if(k==0) break;
				i++;
			}
			
		}
	}
}
