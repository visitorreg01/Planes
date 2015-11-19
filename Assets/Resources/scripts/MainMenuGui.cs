using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {

	ArrayList levels; 
	int startPosition=100;
	int betweenDist=0;
	int levelButtonSize=96;
	
	int levelSelected=-1;
	
	//menu blocks
	bool mainMenu=false;
	bool levelsMenu=false;
	bool campaignMenu=false;
	
	public static int playedLevelIndex=-1;
	public static Templates.CampaignInfo selectedCampaign=null;
	public static GuiCategories nextMenu=GuiCategories.MainMenu;
	
	private static MainMenuGui instance=null;
	
	public static MainMenuGui getInstance()
	{
		return instance;
	}
	
	public enum GuiCategories : int {
		MainMenu=0,
		LevelsMenu=1,
		CampaignMenu=2
	};
	
	// Use this for initialization
	void Start () {
		instance=this;
		GameStorage.getInstance();
		Templates.getInstance();
		switchMenu(nextMenu);
	}
	
	int[] calculateLevelPosition(int i)
	{
		int h,w;
		if(i%2==0)
			h=Screen.height/2+levelButtonSize;
		else
			h=Screen.height/2;
		
		w=startPosition+levelButtonSize/2*i+betweenDist*i;
		return new int[] {h,w};
	}
	
	int[] calculateStartButtonPosition(int i)
	{
		int h,w;
		if(i%2==0)
			h=Screen.height/2+2*levelButtonSize+2;
		else
			h=Screen.height/2-40;
		w=startPosition+levelButtonSize/2*i+betweenDist*i-1;
		return new int[] {h,w};
	}
	
	public void switchMenu(GuiCategories nextBlock)
	{
		levelSelected=-1;
		if(nextBlock==GuiCategories.MainMenu)
		{
			mainMenu=true;
			//other off
			levelsMenu=false;
			campaignMenu=false;
		}
		else if(nextBlock==GuiCategories.LevelsMenu)
		{
			levelsMenu=true;
			mainMenu=false;
			campaignMenu=false;
		}
		else if(nextBlock==GuiCategories.CampaignMenu)
		{
			campaignMenu=true;
			levelsMenu=false;
			mainMenu=false;
		}
	}
	
	public int nextLevel()
	{
		if(playedLevelIndex==selectedCampaign.levels.Count)
			return -1;
		else
			return (int) levels[playedLevelIndex+1];
	}
	
	void OnGUI()
	{
		if(GameStorage.getInstance().allReady)
		{
			if(campaignMenu)
			{
				if(GUI.Button(new Rect(20,Screen.height-40,100,20),"Back"))
					switchMenu(GuiCategories.MainMenu);
				
				ArrayList camp = Templates.getInstance().getCampaigns();
				int buttonH,buttonW;
				buttonW=Screen.width/100*40;
				buttonH=Screen.height/camp.Count-10*camp.Count;
				int i=0;
				foreach(Templates.CampaignInfo c in camp)
				{
					if(GUI.Button(new Rect(Screen.width-20-buttonW,10+buttonH*i+20*i,buttonW,buttonH),((Templates.CampaignInfo)c).name))
					{
						selectedCampaign=(Templates.CampaignInfo)c;
						switchMenu(GuiCategories.LevelsMenu);
					}
					i++;
				}
				
			}
			
			if(levelsMenu)
			{
				if(GUI.Button(new Rect(5,5,100,20),"full"))
				{
					for(int j=0;j<30;j++)
						PlayerPrefs.SetInt("level"+j+"Stars",3);
				}
				
				if(GUI.Button(new Rect(20,Screen.height-40,100,20),"Back"))
					switchMenu(GuiCategories.CampaignMenu);
				
				string current_rank="";
				int currentRankId=PlayerPrefs.GetInt("currentRankId",-1);
				if(currentRankId==-1)
					current_rank="Ensign";
				else
					current_rank=Templates.getInstance().getRank(currentRankId).name;
				GUI.Label(new Rect(Screen.width-5-200,10,200,50),"Current Rank: "+current_rank);
				
				if(GUI.Button(new Rect(110,5,100,20),"null"))
				{
					PlayerPrefs.DeleteAll();
				}
				
				levels=selectedCampaign.levels;
				Templates.LevelInfo lv;
				int i=0,k;
				bool aa=true;
				foreach(int l in levels)
				{
					if(i!=levelSelected) GUI.skin=Templates.getInstance().button_level; else GUI.skin=Templates.getInstance().button_level_selected;
					lv=Templates.getInstance().getLevel(l);
					int[] pos = calculateLevelPosition(i);
					k = PlayerPrefs.GetInt("level"+lv.num+"Stars",0);
					if(k==0)
					{
						if(aa)
						{
							if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),(i+1).ToString()))
								levelSelected=i;
							aa=false;
						}
						else
						{
							GUI.enabled=false;
							GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),(i+1).ToString());
							GUI.enabled=true;
						}
					}
					else
					{
						if(i!=levelSelected)
						{
							if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),(i+1).ToString()))
								levelSelected=i;
						}
						else
						{
							GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),(i+1).ToString());
						}
					}
					i++;
					GUI.skin=null;
				}
				
				if(levelSelected>=0)
				{
					int[] pos = calculateStartButtonPosition(levelSelected);
					GUI.skin=Templates.getInstance().button_level_start;
					if(GUI.Button(new Rect(pos[1],pos[0],98,38),""))
					{
						playedLevelIndex=levelSelected;
						GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)levels[levelSelected]));
					}
					GUI.skin=null;
				}
			}
			
			if(mainMenu)
			{
				int boxH,boxW;
				boxW=Screen.width/100*40;
				boxH=Screen.height/100*80;
				GUI.Box(new Rect(Screen.width-30-boxW,Screen.height/100*10,boxW,boxH),"");
				
				int buttonH,buttonW;
				buttonW=boxW-10;
				buttonH=boxH/4-20;
				
				if(GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+25,buttonW,buttonH),"Missions"))
					switchMenu(GuiCategories.CampaignMenu);
				GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+35+buttonH,buttonW,buttonH),"Plot");
				GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+45+buttonH*2,buttonW,buttonH),"Credits");
				GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+55+buttonH*3,buttonW,buttonH),"Help");
			}
			
			
		}
	}
}
