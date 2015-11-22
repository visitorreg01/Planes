using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {

	ArrayList levels; 
	int startPosition=100;
	int betweenDist=0;
	int levelButtonSize=96;
	
	int levelSelected=-1;
	
	//STARS
	private const int STAR_X_OFFSET=12;
	private const int STAR_Y_OFFSET=58;
	private const int THIRDSTAR_Y_OFFSET=5;
	//NUMBERS
	private const int NUMBER_OFFSET_X=15;
	private const int NUMBER_OFFSET_Y=13;
	
	Vector2 scrollPos = Vector2.zero;
	
	int lastMouseX=0,mouseXDiff=0,curMouseX=0;
	
	
	//menu blocks
	bool mainMenu=false;
	bool levelsMenu=false;
	bool campaignMenu=false;
	bool plotMenu=false;
	bool helpMenu=false;
	
	
	bool mouseLock=false;
	
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
	
	void Update()
	{
		if(Input.GetMouseButtonDown(0))
			mouseLock=true;
		if(Input.GetMouseButtonUp(0))
			mouseLock=false;
		
		mouseXDiff=0;
		if(mouseLock)
		{
			curMouseX=(int)Camera.main.WorldToScreenPoint(Input.mousePosition).x;
			mouseXDiff=lastMouseX-curMouseX;
		}
		lastMouseX=(int)Camera.main.WorldToScreenPoint(Input.mousePosition).x;
	}
	
	void OnGUI()
	{
		if(GameStorage.getInstance().allReady)
		{
			
			GUI.Label(new Rect(0,0,100,20),Screen.width+"x"+Screen.height);
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
					for(int j=0;j<Templates.getInstance().getAllLevels().Count;j++)
						PlayerPrefs.SetInt("level"+(j+1)+"Stars",3);
				}
				
				if(GUI.Button(new Rect(20,Screen.height-40,100,20),"Back"))
					switchMenu(GuiCategories.CampaignMenu);
				
				string current_rank="";
				int currentRankId=PlayerPrefs.GetInt("currentRankId"+MainMenuGui.selectedCampaign.id,-1);
				if(currentRankId==-1)
					current_rank=selectedCampaign.defaultRank;
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
				
				GUI.skin=Templates.getInstance().none_scroll_skin;
				scrollPos = GUI.BeginScrollView(new Rect(0,100,Screen.width,Screen.height),scrollPos,new Rect(0,100,startPosition+96/2*levels.Count+96/2+startPosition,Screen.height-200));
				scrollPos+=new Vector2(mouseXDiff/400,0);
				
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
							
							if(i!=levelSelected)
							{
								if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),""))
								levelSelected=i;
							
								GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,false);
								GUISkin sk = GUI.skin;
								GUI.skin=skins[0];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-25-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
								GUI.skin=skins[1];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-25+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
								GUI.skin=sk;
								
								aa=false;
							}
							else
							{
								if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),""))
								{
									playedLevelIndex=levelSelected;
									GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)levels[levelSelected]));
								}
								GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,false);
								GUISkin sk = GUI.skin;
								GUI.skin=skins[0];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-25-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
								GUI.skin=skins[1];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-25+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
								GUI.skin=sk;
								
								aa=false;
							}
						}
						else
						{
							GUI.enabled=false;
							GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),"");
							GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,true);
							GUISkin sk = GUI.skin;
							GUI.skin=skins[0];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-25-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
							GUI.skin=skins[1];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-25+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
							GUI.skin=sk;
							GUI.enabled=true;
						}
					}
					else
					{
					
						if(i!=levelSelected)
						{
							if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),""))
								levelSelected=i;
							
							GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,false);
							GUISkin sk = GUI.skin;
							GUI.skin=skins[0];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-25-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
							GUI.skin=skins[1];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-25+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
							GUI.skin=sk;
						}
						else
						{
							if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),""))
							{
								playedLevelIndex=levelSelected;
								GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)levels[levelSelected]));
							}
							GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,false);
							GUISkin sk = GUI.skin;
							GUI.skin=skins[0];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-25-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
							GUI.skin=skins[1];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-25+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,50,58),"");
							GUI.skin=sk;
						}
						GUI.skin=Templates.getInstance().label_level_star;
						if(k==3)
						{
							GUI.Label(new Rect(pos[1]+STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,28,28),"");
							GUI.Label(new Rect(pos[1]+levelButtonSize-28-STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,28,28),"");
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-14,pos[0]+STAR_Y_OFFSET+THIRDSTAR_Y_OFFSET,28,28),"");
						}
						else if(k==2)
						{
							GUI.Label(new Rect(pos[1]+STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,28,28),"");
							GUI.Label(new Rect(pos[1]+levelButtonSize-28-STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,28,28),"");
						}
						else if(k==1)
						{
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-14,pos[0]+STAR_Y_OFFSET+THIRDSTAR_Y_OFFSET,28,28),"");
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
				
				GUI.EndScrollView();
			}
			
			if(mainMenu)
			{
				if(plotMenu)
				{
					GUI.enabled=true;
					GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
					int mainBoxH,mainBoxW;
					mainBoxH=Screen.height/100*85;
					mainBoxW=Screen.width/100*80;
					GUI.FocusControl(null);
					GUISkin sk = GUI.skin;
					GUI.skin=Templates.getInstance().mainPopupRichtext;
					GUILayout.BeginArea(new Rect(Screen.width/2-mainBoxW/2,Screen.height/2-mainBoxH/2,mainBoxW,mainBoxH));
					GUILayout.BeginVertical();
					GUILayout.FlexibleSpace();
					scrollPos = GUILayout.BeginScrollView (scrollPos, GUILayout.Width (mainBoxW));
					GUILayout.TextArea(Templates.getInstance().plotContext);
					GUILayout.EndScrollView ();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if(GUILayout.Button("Назад",GUILayout.Width(100),GUILayout.Height(20)))
						plotMenu=false;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.FlexibleSpace();
					
					GUILayout.EndVertical();
					GUILayout.EndArea();
					GUI.skin=sk;
					GUI.enabled=false;
				}
				else if(helpMenu)
				{
					GUI.enabled=true;
					GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
					int mainBoxH,mainBoxW;
					mainBoxH=Screen.height/100*85;
					mainBoxW=Screen.width/100*80;
					GUI.FocusControl(null);
					GUISkin sk = GUI.skin;
					GUI.skin=Templates.getInstance().mainPopupRichtext;
					GUILayout.BeginArea(new Rect(Screen.width/2-mainBoxW/2,Screen.height/2-mainBoxH/2,mainBoxW,mainBoxH));
					GUILayout.BeginVertical();
					GUILayout.FlexibleSpace();
					scrollPos = GUILayout.BeginScrollView (scrollPos, GUILayout.Width (mainBoxW));
					GUILayout.TextArea(Templates.getInstance().helpContent);
					GUILayout.EndScrollView ();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if(GUILayout.Button("Назад",GUILayout.Width(100),GUILayout.Height(20)))
						helpMenu=false;
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.FlexibleSpace();
					
					GUILayout.EndVertical();
					GUILayout.EndArea();
					GUI.skin=sk;
					GUI.enabled=false;
				}
				else
				{
					GUI.enabled=true;
				
					int boxH,boxW;
					boxW=Screen.width/100*40;
					boxH=Screen.height/100*80;
					
					int buttonH,buttonW;
					buttonW=boxW-10;
					buttonH=boxH/4-20;
					
					if(GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+25,buttonW,buttonH),"Missions"))
						switchMenu(GuiCategories.CampaignMenu);
					if(GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+35+buttonH,buttonW,buttonH),"Plot"))
						plotMenu=true;
						
					GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+45+buttonH*2,buttonW,buttonH),"Credits");
					if(GUI.Button(new Rect(Screen.width-30-boxW+5,Screen.height/100*10+55+buttonH*3,buttonW,buttonH),"Help"))
						helpMenu=true;
				}
			}
			
			
		}
	}
}
