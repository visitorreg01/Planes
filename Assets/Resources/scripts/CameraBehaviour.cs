using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

	Vector3 startPoint;
	bool released = false;
	float scale = 30;
	public float cameraSize=20;
	public float cameraZoomSpeed=1;
	public float cameraSizeMin=5;
	public float cameraSizeMax=50;
	int currentRankId=-1;
	int reachedRankId=-1;
	string reachedRankName="";
	bool reachedRank=false;
	public bool canReleaseMouse=false;
	
	private GameObject tile_bg_go;
	
	private Vector3 cursorPos;
	private float cursorXoffset,cursorYoffset;
	
	float x,y;
	float cameraH=20;
	float terrainH,terrainW;
	const float speed=45;
	
	public GameObject currentSelected=null;
	public GameObject primarySelected=null;
	
	Vector3 lastMousePos=new Vector3(0,0,0);
	Vector3 curMousePos;
	
	bool showNextLevelWindow = false;
	bool showPause=false;
	int stars=0;
	
	void Start () {
		GameStorage.getInstance().cam=this;
		loadTileGo();
		curMousePos=transform.position;
	}
	
	public void loadTileGo()
	{
		tile_bg_go=GameObject.Find("tile_bg");
	}
	
	void LateUpdate()
	{
		if(Input.GetButtonDown("Fire1"))
		{
			if(canReleaseMouse)
				released=true;
		}
		if(!canReleaseMouse) released=false;
		if(Input.GetButtonUp("Fire1"))
		{
			released=false;
		}
	}
	
	void Update()
	{
		if(GameStorage.fadeDirection==1)
		{
			GameStorage.fadeAlpha+=1.3f*Time.deltaTime;
			if(GameStorage.fadeAlpha > GameStorage.targetFadeAlpha)
				GameStorage.fadeAlpha = GameStorage.targetFadeAlpha;
			
		}
		else
		{
			GameStorage.fadeAlpha-=1.3f*Time.deltaTime;
			if(GameStorage.fadeAlpha < GameStorage.targetFadeAlpha)
				GameStorage.fadeAlpha = GameStorage.targetFadeAlpha;
		}
		
		if(tile_bg_go!=null)
		{
			Color c = tile_bg_go.GetComponent<Renderer>().material.color;
			c.a=GameStorage.fadeAlpha;
			tile_bg_go.GetComponent<Renderer>().material.color=c;
		}
		
		
		if(!GameStorage.getInstance().isRunning)
		{
			checkMouse();
		}
		if(!showNextLevelWindow)
		{
			/*
			
			*/	
			
			
			
			if(Input.GetAxis("Mouse ScrollWheel") > 0)
			{
				if(cameraSize<=cameraSizeMin)
					cameraSize=cameraSizeMin;
				else
					cameraSize-=cameraZoomSpeed;
			}
			if(Input.GetAxis("Mouse ScrollWheel") < 0)
			{
				if(cameraSize>=cameraSizeMax)
					cameraSize=cameraSizeMax;
				else
					cameraSize+=cameraZoomSpeed;
			}
			
			GameStorage.getInstance().zoom=cameraSize;
			
			Camera.main.orthographicSize=cameraSize;
			
			if(released)
			{
				curMousePos=(Camera.main.ScreenToWorldPoint (Input.mousePosition))- lastMousePos;
				Camera.main.transform.position += curMousePos*-1;
				if(Camera.main.transform.position.z>scale)
					Camera.main.transform.position=new Vector3(Camera.main.transform.position.x,cameraH,scale);
				if(Camera.main.transform.position.z<-scale)
					Camera.main.transform.position=new Vector3(Camera.main.transform.position.x,cameraH,-scale);
				if(Camera.main.transform.position.x>scale)
					Camera.main.transform.position=new Vector3(scale,cameraH,Camera.main.transform.position.z);
				if(Camera.main.transform.position.x<-scale)
					Camera.main.transform.position=new Vector3(-scale,cameraH,Camera.main.transform.position.z);
				
			}
			lastMousePos=Camera.main.ScreenToWorldPoint (Input.mousePosition);
		}
	}
	
	public void dropPopupShowed()
	{
		foreach(GameObject o in GameStorage.getInstance().getFriendlyShuttles())
			o.GetComponent<FriendlyShuttleBehaviour>().showPopup=false;
		foreach(GameObject o in GameStorage.getInstance().getEnemyShuttles())
			o.GetComponent<EnemyShuttleBehaviour>().showPopup=false;
	}
	
	public void dropSelection(GameObject o)
	{
		if(o.GetComponent<FriendlyShuttleBehaviour>()!=null)
		{
			o.GetComponent<FriendlyShuttleBehaviour>().selected=false;
		}
		if(o.GetComponent<EnemyShuttleBehaviour>()!=null)
			o.GetComponent<EnemyShuttleBehaviour>().selected=false;
		if(o.GetComponent<RocketBehaviour>()!=null)
		{
			if(!o.GetComponent<RocketBehaviour>().enemy)
			{
				o.GetComponent<RocketBehaviour>().selected=false;
			}
		}
		if(o.GetComponent<ThorpedeBehaviour>()!=null)
		{
			if(!o.GetComponent<ThorpedeBehaviour>().enemy)
			{
				o.GetComponent<ThorpedeBehaviour>().selected=false;
			}
		}
	}
	
	public void setSelection(GameObject o)
	{
		if(o.GetComponent<FriendlyShuttleBehaviour>()!=null)
			o.GetComponent<FriendlyShuttleBehaviour>().selected=true;
		if(o.GetComponent<EnemyShuttleBehaviour>()!=null)
			o.GetComponent<EnemyShuttleBehaviour>().selected=true;
		if(o.GetComponent<RocketBehaviour>()!=null)
		{
			if(!o.GetComponent<RocketBehaviour>().enemy)
			{
				o.GetComponent<RocketBehaviour>().selected=true;
			}
		}
		if(o.GetComponent<ThorpedeBehaviour>()!=null)
		{
			if(!o.GetComponent<ThorpedeBehaviour>().enemy)
			{
				o.GetComponent<ThorpedeBehaviour>().selected=true;
			}
		}
	}
	
	private void checkMouse()
	{
		if(!(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
		{
			GameObject pp=null;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	    	RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
	    		pp=hit.collider.gameObject;
			
			dropPopupShowed();
			if(pp!=null)
			{
				if(pp.GetComponent<FriendlyShuttleBehaviour>()!=null)
					pp.GetComponent<FriendlyShuttleBehaviour>().showPopup=true;
				if(pp.GetComponent<EnemyShuttleBehaviour>()!=null)
					pp.GetComponent<EnemyShuttleBehaviour>().showPopup=true;
			}
		}
		
		if(Input.GetButtonDown("Fire1"))
		{
			
			bool f = true;
			GameObject pp=null;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	    	RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
	    	{
	    		pp=hit.collider.gameObject;
	    	}
			
			if(currentSelected!=null)
			{
				if(currentSelected.GetComponent<FriendlyShuttleBehaviour>()!=null)
				{
					if(currentSelected.GetComponent<FriendlyShuttleBehaviour>().cancelMouseDrop())
					{
						f=false;
					}
				}
			}
			
		
			if(f)
			{
				if(pp!=null)
				{
					if(currentSelected==null)
					{
						setSelection(pp);
						currentSelected=pp;
					}
					else
					{
						if(currentSelected.GetComponent<FriendlyShuttleBehaviour>()!=null)
						{
							
							if(!currentSelected.GetComponent<FriendlyShuttleBehaviour>().cancelMouseDrop())
							{
								if(currentSelected.GetComponent<FriendlyShuttleBehaviour>()!=null)
									currentSelected.GetComponent<FriendlyShuttleBehaviour>().iconsShowed=false;
								dropSelection(currentSelected);
								setSelection(pp);
								currentSelected=pp;
							}
						}
						else
						{
							
							setSelection(pp);
							currentSelected=pp;
						}
					}
				}
				else
				{
					if(currentSelected!=null)
					{
						if(currentSelected.GetComponent<FriendlyShuttleBehaviour>()!=null)
							currentSelected.GetComponent<FriendlyShuttleBehaviour>().iconsShowed=false;
						dropSelection(currentSelected);
						currentSelected=null;
					}
				}
			}
			
			
		}
		
		
		/*
		GameObject pp=null;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    	RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
    		pp=hit.collider.gameObject;
		if(pp==null)
			Debug.Log("null");
		else
			Debug.Log("not null");
		*/
	}
	
	void OnGUI()
	{
		foreach(GameObject g in GameStorage.getInstance().getEnemyShuttles())
		{
			cursorPos=g.GetComponent<EnemyShuttleBehaviour>().isOutOfViewport();
			if(cursorPos!=new Vector3(-1,-1,-1))
			{
				if(cursorPos.x>=Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorXoffset=Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width);
				else if(cursorPos.x<=Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorXoffset=0;
				else
					cursorXoffset=cursorPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2;
				
				cursorYoffset=0;
				
				if(cursorPos.y<=Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorYoffset=Screen.height-Templates.ResolutionProblems.getActionAbilitySize(Screen.width);
				else if(cursorPos.y>=Screen.height-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorYoffset=0;
				else
					cursorYoffset=(Screen.height-cursorPos.y)-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2;
				
				GUIUtility.RotateAroundPivot(cursorPos.z,new Vector2(cursorXoffset+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,cursorYoffset+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2));
				GUI.Label(new Rect(cursorXoffset,cursorYoffset,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().arrowRedSkin.label);
				GUI.matrix=Matrix4x4.identity;
			}
		}
		
		foreach(GameObject g in GameStorage.getInstance().getFriendlyShuttles())
		{
			cursorPos=g.GetComponent<FriendlyShuttleBehaviour>().isOutOfViewport();
			if(cursorPos!=new Vector3(-1,-1,-1))
			{
				if(cursorPos.x>=Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorXoffset=Screen.width-Templates.ResolutionProblems.getActionAbilitySize(Screen.width);
				else if(cursorPos.x<=Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorXoffset=0;
				else
					cursorXoffset=cursorPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2;
				
				cursorYoffset=0;
				
				if(cursorPos.y<=Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorYoffset=Screen.height-Templates.ResolutionProblems.getActionAbilitySize(Screen.width);
				else if(cursorPos.y>=Screen.height-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2)
					cursorYoffset=0;
				else
					cursorYoffset=(Screen.height-cursorPos.y)-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2;
				
				GUIUtility.RotateAroundPivot(cursorPos.z,new Vector2(cursorXoffset+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,cursorYoffset+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2));
				GUI.Label(new Rect(cursorXoffset,cursorYoffset,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"",Templates.getInstance().arrowBlueSkin.label);
				GUI.matrix=Matrix4x4.identity;
			}
		}
		
		if(showNextLevelWindow)
		{
			showPause=false;
			GameStorage.getInstance().overlap=true;
			GUI.depth=200;
			
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
			float buttonH,buttonW;
			buttonW=Templates.ResolutionProblems.getPauseButtonStartW(Screen.width);
			buttonH=Templates.ResolutionProblems.getPauseButtonStartH(Screen.width);
			float boxW=buttonW*3+Templates.ResolutionProblems.getPauseButtonDopWidth(Screen.width);
			float boxH=Templates.ResolutionProblems.getPauseButtonBoxH(Screen.width);
			
			GUILayout.BeginArea(new Rect(Screen.width/2-boxW/2,Screen.height/2-boxH/2,boxW,boxH));
			GUILayout.BeginVertical(Templates.getInstance().popupWindow.box);
			
			GUILayout.Label("");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUISkin f = Templates.getInstance().gamePausedStyle;
			f.label.fontSize=Templates.ResolutionProblems.getPauseMenuFontSize(Screen.width);
			GUILayout.Label("Game paused",f.label);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			if(stars==-1 || stars==0)
			{
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("",Templates.getInstance().pauseMission.label,GUILayout.Width(Templates.ResolutionProblems.getPauseMissionW(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseMissionH(Screen.width)));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("",Templates.getInstance().pauseMissionFail.label,GUILayout.Width(Templates.ResolutionProblems.getPauseFailW(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseFailH(Screen.width)));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.FlexibleSpace();
				
				if(GameStorage.tries>=1 && Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]).hint.Length>0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUISkin zz = Templates.getInstance().gamePausedNameStyle;
					zz.label.fontSize = Templates.ResolutionProblems.getPauseNameFontSize(Screen.width);
					GUILayout.Label("Hint:",zz.label);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					
					zz = Templates.getInstance().gamePausedDescStyle;
					zz.label.fontSize=Templates.ResolutionProblems.getPauseDescFontSize(Screen.width);
					GUILayout.Label(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]).hint,zz.label);
					
				}
				
				
				GUILayout.EndVertical();
				
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.Label("",GUILayout.Width(Templates.ResolutionProblems.getPauseButtonOffsetX(Screen.width)));
				if(GUILayout.Button("",Templates.getInstance().buttonMenu.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					MainMenuGui.nextMenu=MainMenuGui.GuiCategories.LevelsMenu;
					GameStorage.tries=0;
					Application.LoadLevel("mainGui");
				}
				
				if(GUILayout.Button("",Templates.getInstance().buttonRestart.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]),false);
				}
				
				GUI.enabled=false;
				GUILayout.Button("",Templates.getInstance().buttonNextLevel.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH));
				GUI.enabled=true;
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("",Templates.getInstance().pauseMission.label,GUILayout.Width(Templates.ResolutionProblems.getPauseMissionW(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseMissionH(Screen.width)));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("",Templates.getInstance().pauseMissionOk.label,GUILayout.Width(Templates.ResolutionProblems.getPauseAccW(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseAccH(Screen.width)));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				//stars
				GUILayout.FlexibleSpace();
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(stars>=1)
					GUILayout.Label("",Templates.getInstance().startLarge.label,GUILayout.Width(Templates.ResolutionProblems.getPauseLargeStarSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseLargeStarSize(Screen.width)));
				if(stars>=2)
					GUILayout.Label("",Templates.getInstance().startLarge.label,GUILayout.Width(Templates.ResolutionProblems.getPauseLargeStarSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseLargeStarSize(Screen.width)));
				if(stars>=3)
					GUILayout.Label("",Templates.getInstance().startLarge.label,GUILayout.Width(Templates.ResolutionProblems.getPauseLargeStarSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPauseLargeStarSize(Screen.width)));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				if(reachedRank)
				{
					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical();
					
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUISkin pop = Templates.getInstance().gamePausedNameStyle;
					pop.label.fontSize = Templates.ResolutionProblems.getPauseNameFontSize(Screen.width);
					GUILayout.Label("Reached rank:",pop.label);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					pop = Templates.getInstance().rankStyle;
					pop.label.fontSize=Templates.ResolutionProblems.getPauseReachedRankFontSize(Screen.width);
					GUILayout.Label(reachedRankName,pop.label);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					
					GUILayout.EndVertical();
				}
				
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				
				
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.Label("",GUILayout.Width(Templates.ResolutionProblems.getPauseButtonOffsetX(Screen.width)));
				int maxStars = PlayerPrefs.GetInt("level"+((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex])+"Stars",0);
				if(maxStars==0)
					PlayerPrefs.SetInt("level"+((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex])+"Stars",stars);
				else
				{
					if(maxStars<stars)
						PlayerPrefs.SetInt("level"+((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex])+"Stars",stars);
				}
				PlayerPrefs.Save();
				
				if(GUILayout.Button("",Templates.getInstance().buttonMenu.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					MainMenuGui.nextMenu=MainMenuGui.GuiCategories.LevelsMenu;
					GameStorage.tries=0;
					Application.LoadLevel("mainGui");
				}
				if(GUILayout.Button("",Templates.getInstance().buttonRestart.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]),false);
				}
				if(MainMenuGui.playedLevelIndex+1!=MainMenuGui.selectedCampaign.levels.Count)
				{
					if(GUILayout.Button("",Templates.getInstance().buttonNextLevel.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
					{
						GameStorage.getInstance().overlap=false;
						showNextLevelWindow=false;
						MainMenuGui.playedLevelIndex++;
						GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]),true);
					}
				}
				GUILayout.EndHorizontal();
			}
			GUI.depth=GameStorage.getInstance().defaultDepth;
			
			GUILayout.Label("",GUILayout.Height(Templates.ResolutionProblems.getPauseButtonOffset(Screen.width)));
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		
		if(showPause)
		{
			GameStorage.getInstance().overlap=true;
			canReleaseMouse=false;
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
			float buttonH,buttonW;
			buttonW=Templates.ResolutionProblems.getPauseButtonStartW(Screen.width);
			buttonH=Templates.ResolutionProblems.getPauseButtonStartH(Screen.width);
			float boxW=buttonW*3+Templates.ResolutionProblems.getPauseButtonDopWidth(Screen.width);
			float boxH=Templates.ResolutionProblems.getPauseButtonBoxH(Screen.width);
			
			GUILayout.BeginArea(new Rect(Screen.width/2-boxW/2,Screen.height/2-boxH/2,boxW,boxH));
			GUILayout.BeginVertical(Templates.getInstance().popupWindow.box);
			
			GUILayout.Label("");
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUISkin f = Templates.getInstance().gamePausedStyle;
			f.label.fontSize=Templates.ResolutionProblems.getPauseMenuFontSize(Screen.width);
			GUILayout.Label("Game paused",f.label);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			f = Templates.getInstance().gamePausedNameStyle;
			f.label.fontSize=Templates.ResolutionProblems.getPauseNameFontSize(Screen.width);
			GUILayout.Label("Mission "+(MainMenuGui.playedLevelIndex+1)+": "+Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]).levelName,f.label);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Label("");
			
			
			f = Templates.getInstance().gamePausedDescStyle;
			f.label.fontSize=Templates.ResolutionProblems.getPauseDescFontSize(Screen.width);
			GUILayout.Label(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]).description,f.label);
			
			GUILayout.FlexibleSpace();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("",GUILayout.Width(Templates.ResolutionProblems.getPauseButtonOffsetX(Screen.width)));
			if(GUILayout.Button("",Templates.getInstance().buttonMenu.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
			{
				GameStorage.getInstance().overlap=false;
				showPause=false;
				canReleaseMouse=true;
				GameStorage.getInstance().EndLevel();
				MainMenuGui.nextMenu=MainMenuGui.GuiCategories.LevelsMenu;
				GameStorage.tries=0;
				Application.LoadLevel("mainGui");
				
			}
			
			if(GUILayout.Button("",Templates.getInstance().buttonRestart.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
			{
				GameStorage.getInstance().overlap=false;
				showPause=false;
				canReleaseMouse=true;
				GameStorage.getInstance().EndLevel();
				GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]),false);
			}
			
			if(GUILayout.Button("",Templates.getInstance().buttonContinue.button,GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
			{
				canReleaseMouse=true;
				GameStorage.getInstance().overlap=false;
				showPause=false;
			}
			
			GUILayout.EndHorizontal();
			GUILayout.Label("",GUILayout.Height(Templates.ResolutionProblems.getPauseButtonOffset(Screen.width)));
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
	
	public void gamePause()
	{
		showPause=true;
	}
	
	public void nextLevelWindow(int stars)
	{
		this.stars=stars;
		showNextLevelWindow=true;
		if(stars>0)
		{
			currentRankId=PlayerPrefs.GetInt("currentRankId"+MainMenuGui.selectedCampaign.id,-1);
			reachedRankId=Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]).rankReached;
			if(reachedRankId>=0)
			{
				if(currentRankId<reachedRankId)
				{
					PlayerPrefs.SetInt("currentRankId"+MainMenuGui.selectedCampaign.id,reachedRankId);
					reachedRankName=Templates.getInstance().getRank(reachedRankId).name;
					reachedRank=true;
				}
			}
		}
	}
}
