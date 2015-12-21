using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {

	ArrayList levels; 
	float startPosition=100;
	int betweenDist=0;
	float levelButtonSize=96;
	
	int levelSelected=-1;
	
	//STARS
	private float STAR_X_OFFSET=12;
	private float STAR_Y_OFFSET=58;
	private float THIRDSTAR_Y_OFFSET=5;
	//NUMBERS
	private float NUMBER_OFFSET_X=15;
	private float NUMBER_OFFSET_Y=13;
	
	Vector2 scrollPos = Vector2.zero;
	
	private const float MENU_BUT_H_COEFF=745/362;
	private const float LEVEL_BUT_START_H_COEFF=98/38;
	
	int lastMouseX=0,mouseXDiff=0,curMouseX=0;
	
	
	//menu blocks
	bool mainMenu=false;
	bool levelsMenu=false;
	bool campaignMenu=false;
	bool plotMenu=false;
	bool helpMenu=false;
	
	
	bool mouseLock=false;
	
	string resWs="",resHs="";
	
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
	
	float[] calculateLevelPosition(int i)
	{
		float h,w;
		if(i%2==0)
			h=Screen.height/2+levelButtonSize;
		else
			h=Screen.height/2;
		
		w=startPosition+levelButtonSize/2*i+betweenDist*i;
		return new float[] {h,w};
	}
	
	float[] calculateStartButtonPosition(int i)
	{
		float h,w;
		
		float startH,startW;
		startW=Templates.ResolutionProblems.getLevelButtonStartW(Screen.width);
		startH=Templates.ResolutionProblems.getLevelButtonStartH(Screen.width);
		
		
		if(i%2==0)
			h=Screen.height/2+2*levelButtonSize+2;
		else
			h=Screen.height/2-2-startH;
		w=startPosition+levelButtonSize/2*i+betweenDist*i-1;
		return new float[] {h,w};
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
		
		if(mainMenu && !helpMenu && !plotMenu)
		{
			if(Input.GetKeyUp(KeyCode.Q))
				Application.Quit();
		}
		
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
			Templates.getInstance().menu_button.button.fontSize=Templates.ResolutionProblems.getMainMenuFontSize(Screen.width);
			Templates.getInstance().menu_button.button.padding=new RectOffset(0,0,Templates.ResolutionProblems.getMainMenuPaddingTop(Screen.width),0);
			
			
			GUI.Label(new Rect(0,0,100,20),Screen.width+"x"+Screen.height);
			resWs = GUI.TextField(new Rect(0,20,50,20),resWs,4);
			resHs = GUI.TextField(new Rect(50,20,50,20),resHs,4);
			if(GUI.Button(new Rect(100,20,50,20),"GO"))
			{
				int resW,resH;
				resW=int.Parse(resWs);
				resH=int.Parse(resHs);
				Screen.SetResolution(resW,resH,true,60);
				Templates.getInstance().reloadIcons();
				Debug.Log("Graphics reloaded");
			}
			
			if(campaignMenu)
			{
				GUI.Box(new Rect(0,0,Screen.width,Screen.height),"",Templates.getInstance().campaigns_bg.box);
				GUI.Box(new Rect(0,Screen.height*0.07f,Screen.width,Screen.height*0.85f),"",Templates.getInstance().mission_bg.box);
				
				
				if(GUI.Button(new Rect(Screen.width*0.06f,Screen.height-Templates.ResolutionProblems.getCompanyPanelBackButtonH(Screen.height)-10,Templates.ResolutionProblems.getCompanyPanelBackButtonW(Screen.width),Templates.ResolutionProblems.getCompanyPanelBackButtonH(Screen.height)),"",Templates.getInstance().company_panel_hover.button))
					switchMenu(GuiCategories.MainMenu);
				
				ArrayList camp = Templates.getInstance().getCampaigns();
				int i=0,fi=-1;
				Templates.getInstance().company_panel_hover.box.fontSize=Templates.ResolutionProblems.getCompanyPanelFontSize(Screen.width);
				Templates.getInstance().company_panel.box.fontSize=Templates.ResolutionProblems.getCompanyPanelFontSize(Screen.width);
				Templates.getInstance().company_panel.label.fontSize=Templates.ResolutionProblems.getCompanyPanelLabelFontSize(Screen.width);
				
				for(i=0;i<3;i++)
				{
					if(Input.mousePosition.x >= Screen.width*0.028f &&
					   Input.mousePosition.x <= Screen.width*0.028f+Screen.width*0.65f &&
					   Screen.height-Input.mousePosition.y >= Screen.height*0.12f+(Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))*i &&
					   Screen.height-Input.mousePosition.y <= Screen.height*0.12f+(Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))*i +Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))
						fi=i;
				}
				i=0;
				
				foreach(Templates.CampaignInfo c in camp)
				{
					if(fi==i)
						GUI.Box(new Rect(Screen.width*0.028f,Screen.height*0.12f+(Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))*i,Screen.width*0.65f,Templates.ResolutionProblems.getCompanyPanelH(Screen.height)),((Templates.CampaignInfo)c).name,Templates.getInstance().company_panel_hover.box);
					else
						GUI.Box(new Rect(Screen.width*0.028f,Screen.height*0.12f+(Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))*i,Screen.width*0.65f,Templates.ResolutionProblems.getCompanyPanelH(Screen.height)),((Templates.CampaignInfo)c).name,Templates.getInstance().company_panel.box);
					
					GUI.Label(new Rect(Screen.width*0.13f,Screen.height*0.2f+(Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))*i,Screen.width*0.5f,Templates.ResolutionProblems.getCompanyPanelH(Screen.height)),((Templates.CampaignInfo)c).desc,Templates.getInstance().company_panel.label);
					if(GUI.Button(new Rect(Screen.width*0.028f,Screen.height*0.12f+(Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height))*i,Screen.width*0.65f,Templates.ResolutionProblems.getCompanyPanelH(Screen.height)-Templates.ResolutionProblems.getCompanyPanelOffset(Screen.height)),"",Templates.getInstance().company_panel.button))
					{
						selectedCampaign=(Templates.CampaignInfo)c;
						switchMenu(GuiCategories.LevelsMenu);
					}
					i++;
				}
				
			}
			
			if(levelsMenu)
			{
				GUI.Box(new Rect(0,0,Screen.width,Screen.height),"",Templates.getInstance().campaigns_bg.box);
			
				startPosition=Templates.ResolutionProblems.getLevelsStartPosition(Screen.width);
				
				STAR_X_OFFSET=Templates.ResolutionProblems.getLevelsStarOffsetX(Screen.width);
				STAR_Y_OFFSET=Templates.ResolutionProblems.getLevelsStarOffsetY(Screen.width);
				THIRDSTAR_Y_OFFSET=Templates.ResolutionProblems.getLevelsThirdstarOffsetY(Screen.width);
				
				NUMBER_OFFSET_X=Templates.ResolutionProblems.getLevelsNumberOffsetX(Screen.width);
				NUMBER_OFFSET_Y=Templates.ResolutionProblems.getLevelsNumberOffsetY(Screen.width);
				
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
				scrollPos = GUI.BeginScrollView(new Rect(0,startPosition,Screen.width,Screen.height),scrollPos,new Rect(0,startPosition,startPosition+Templates.ResolutionProblems.getLevelButtonStartW(Screen.width)/2*levels.Count+Templates.ResolutionProblems.getLevelButtonStartW(Screen.width)/2+startPosition,Screen.height-2*startPosition));
				scrollPos+=new Vector2(mouseXDiff/400,0);
				
				levelButtonSize=Templates.ResolutionProblems.getLevelsButtonSize(Screen.width);
				
				foreach(int l in levels)
				{
					if(i!=levelSelected) GUI.skin=Templates.getInstance().button_level; else GUI.skin=Templates.getInstance().button_level_selected;
					lv=Templates.getInstance().getLevel(l);
					float[] pos = calculateLevelPosition(i);
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
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
								GUI.skin=skins[1];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
								GUI.skin=sk;
								
								aa=false;
							}
							else
							{
								if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),""))
								{
									playedLevelIndex=levelSelected;
									GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)levels[levelSelected]),true);
								}
								GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,false);
								GUISkin sk = GUI.skin;
								GUI.skin=skins[0];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
								GUI.skin=skins[1];
								GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
								GUI.skin=sk;
								
								aa=false;
							}
						}
						else
						{
							
							GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),"",Templates.getInstance().button_level_grey.button);
							GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,true);
							GUISkin sk = GUI.skin;
							GUI.skin=skins[0];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
							GUI.skin=skins[1];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
							GUI.skin=sk;
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
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
							GUI.skin=skins[1];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
							GUI.skin=sk;
						}
						else
						{
							if(GUI.Button(new Rect(pos[1],pos[0],levelButtonSize,levelButtonSize),""))
							{
								playedLevelIndex=levelSelected;
								GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)levels[levelSelected]),true);
							}
							GUISkin[] skins = Templates.getInstance().getNumberIcons(i+1,false);
							GUISkin sk = GUI.skin;
							GUI.skin=skins[0];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2-NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
							GUI.skin=skins[1];
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsNumberW(Screen.width)/2+NUMBER_OFFSET_X,pos[0]+NUMBER_OFFSET_Y,Templates.ResolutionProblems.getLevelsNumberW(Screen.width),Templates.ResolutionProblems.getLevelsNumberH(Screen.width)),"");
							GUI.skin=sk;
						}
						GUI.skin=Templates.getInstance().label_level_star;
						if(k==3)
						{
							GUI.Label(new Rect(pos[1]+STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,Templates.ResolutionProblems.getLevelsStarSize(Screen.width),Templates.ResolutionProblems.getLevelsStarSize(Screen.width)),"");
							GUI.Label(new Rect(pos[1]+levelButtonSize-Templates.ResolutionProblems.getLevelsStarSize(Screen.width)-STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,Templates.ResolutionProblems.getLevelsStarSize(Screen.width),Templates.ResolutionProblems.getLevelsStarSize(Screen.width)),"");
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsStarSize(Screen.width)/2,pos[0]+STAR_Y_OFFSET+THIRDSTAR_Y_OFFSET,Templates.ResolutionProblems.getLevelsStarSize(Screen.width),Templates.ResolutionProblems.getLevelsStarSize(Screen.width)),"");
						}
						else if(k==2)
						{
							GUI.Label(new Rect(pos[1]+STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,Templates.ResolutionProblems.getLevelsStarSize(Screen.width),Templates.ResolutionProblems.getLevelsStarSize(Screen.width)),"");
							GUI.Label(new Rect(pos[1]+levelButtonSize-Templates.ResolutionProblems.getLevelsStarSize(Screen.width)-STAR_X_OFFSET,pos[0]+STAR_Y_OFFSET,Templates.ResolutionProblems.getLevelsStarSize(Screen.width),Templates.ResolutionProblems.getLevelsStarSize(Screen.width)),"");
						}
						else if(k==1)
						{
							GUI.Label(new Rect(pos[1]+levelButtonSize/2-Templates.ResolutionProblems.getLevelsStarSize(Screen.width)/2,pos[0]+STAR_Y_OFFSET+THIRDSTAR_Y_OFFSET,Templates.ResolutionProblems.getLevelsStarSize(Screen.width),Templates.ResolutionProblems.getLevelsStarSize(Screen.width)),"");
						}
					}
					i++;
					GUI.skin=null;
				}
				
				if(levelSelected>=0)
				{
					float[] pos = calculateStartButtonPosition(levelSelected);
					GUI.skin=Templates.getInstance().button_level_start;
					
					float startH,startW;
					startW=Templates.ResolutionProblems.getLevelButtonStartW(Screen.width);
					startH=Templates.ResolutionProblems.getLevelButtonStartH(Screen.width);
					
					if(GUI.Button(new Rect(pos[1],pos[0],startW,startH),""))
					{
						playedLevelIndex=levelSelected;
						GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)levels[levelSelected]),true);
					}
					GUI.skin=null;
				}
				
				GUI.EndScrollView();
				
				if(levelSelected>=0)
				{
					Templates.LevelInfo lev = Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[levelSelected]);
					float[] q = calculateStartButtonPosition(1);
					GUILayout.BeginArea(new Rect(10,10,Screen.width-20,q[0]-Templates.ResolutionProblems.getLevelButtonStartH(Screen.width)/2),"",GUI.skin.box);
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Mission "+(levelSelected+1)+": "+lev.levelName);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					
					GUILayout.Label(lev.description);
					
					GUILayout.EndVertical();
					GUILayout.EndArea();
				}
			}
			
			if(mainMenu)
			{
				GUI.Box(new Rect(0,0,Screen.width,Screen.height),"",Templates.getInstance().bg.box);
			
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
				
					//new
					float helpplotH,helpplotW,menuBH,menuBW;
					
					helpplotH=Screen.width*0.07f;
					helpplotW=helpplotH;
					if(GUI.Button(new Rect(30,Screen.height-30-helpplotH,helpplotW,helpplotH),"",Templates.getInstance().menu_button_help.button))
						helpMenu=true;
					if(GUI.Button(new Rect(Screen.width-30-helpplotW,Screen.height-30-helpplotH,helpplotW,helpplotH),"",Templates.getInstance().menu_button_plot.button))
						plotMenu=true;
					
					float bw = Screen.width/2-(30+helpplotW);
					float bwdif = bw-bw*0.7f;
					menuBW=bw*0.7f;
					bwdif/=2;
					float campaingsPosX=30+helpplotW+bwdif;
					menuBH=menuBW/MENU_BUT_H_COEFF;
					
					float menuButY=Screen.height-menuBH+menuBH*0.09f;
					float creditsPosX=Screen.width-30-helpplotW-bwdif-menuBW;
					
					if(GUI.Button(new Rect(campaingsPosX,menuButY,menuBW,menuBH),"Campaings",Templates.getInstance().menu_button.button))
						switchMenu(GuiCategories.CampaignMenu);
					GUI.Button(new Rect(creditsPosX,menuButY,menuBW,menuBH),"Credits",Templates.getInstance().menu_button.button);
				}
			}
			
			
		}
	}
}
