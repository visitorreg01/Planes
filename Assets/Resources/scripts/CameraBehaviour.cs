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
	
	float x,y;
	float cameraH=20;
	float terrainH,terrainW;
	const float speed=45;
	
	private GameObject currentSelected=null;
	
	bool showNextLevelWindow = false;
	bool showPause=false;
	int stars=0;
	
	void Start () {
		GameStorage.getInstance().cam=this;
	}
	
	void Update()
	{
		if(!GameStorage.getInstance().isRunning)
			checkMouse();
		if(!showNextLevelWindow)
		{
			if(Input.GetButtonDown("Fire2"))
				released=true;
			
			if(Input.GetButtonUp("Fire2"))
				released=false;
			
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
				Camera.main.transform.position += new Vector3(speed * Input.GetAxis("Mouse X") * Time.deltaTime, 0, speed * Input.GetAxis("Mouse Y") * Time.deltaTime)*-1;
				if(Camera.main.transform.position.z>scale)
					Camera.main.transform.position=new Vector3(Camera.main.transform.position.x,cameraH,scale);
				if(Camera.main.transform.position.z<-scale)
					Camera.main.transform.position=new Vector3(Camera.main.transform.position.x,cameraH,-scale);
				if(Camera.main.transform.position.x>scale)
					Camera.main.transform.position=new Vector3(scale,cameraH,Camera.main.transform.position.z);
				if(Camera.main.transform.position.x<-scale)
					Camera.main.transform.position=new Vector3(-scale,cameraH,Camera.main.transform.position.z);
				
			}
		}
	}
	
	private void dropSelection(GameObject o)
	{
		if(o.GetComponent<FriendlyShuttleBehaviour>()!=null)
			o.GetComponent<FriendlyShuttleBehaviour>().selected=false;
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
	
	private void setSelection(GameObject o)
	{
		if(o.GetComponent<FriendlyShuttleBehaviour>()!=null)
			o.GetComponent<FriendlyShuttleBehaviour>().selected=true;
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
		if(Input.GetButtonDown("Fire1"))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	    	RaycastHit hit;
	    	if(currentSelected!=null)
	    	{
	    		if(currentSelected.GetComponent<FriendlyShuttleBehaviour>()!=null)
	    		{
	    			if(currentSelected.GetComponent<FriendlyShuttleBehaviour>().cancelMouseDrop())
	    				return;
	    		}
	    		else
	    			dropSelection(currentSelected);
	    	}
	    	
	    	if (Physics.Raycast(ray, out hit))
	    	{
	    		currentSelected=hit.collider.gameObject;
	    		setSelection(hit.collider.gameObject);
	    	}
	    	else
	    		currentSelected=null;
		}
	}
	
	void OnGUI()
	{
		if(showNextLevelWindow)
		{
			showPause=false;
			GameStorage.getInstance().overlap=true;
			GUI.depth=200;
			
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
			int boxW=Screen.width/100*20;
			int boxH=Screen.height/100*40;
			string boxLabel="End mission "+(MainMenuGui.playedLevelIndex+1)+"\n";
			
			if(stars==-1)
				boxLabel+="LOSE!";
			else if(stars==0)
				boxLabel+="DRAW!";
			else
			{
				boxLabel+="WIN!\nStars: "+stars;
				if(reachedRank)
					boxLabel+="\nReached rank:\n"+reachedRankName;
			}
			GUI.Box(new Rect(Screen.width/2-boxW/2,Screen.height/2-boxH/2,boxW,boxH),boxLabel);
			int buttonH,buttonW;
			buttonH=boxH/100*20;
			
			if(stars==-1 || MainMenuGui.playedLevelIndex+1==MainMenuGui.selectedCampaign.levels.Count) buttonW=boxW/2-10;
			else buttonW=boxW/3-10;
			
			if(stars==-1 || stars==0)
			{
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+5,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"To Main"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					MainMenuGui.nextMenu=MainMenuGui.GuiCategories.LevelsMenu;
					Application.LoadLevel("mainGui");
				}
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+15+buttonW,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"Reply"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]));
				}
			}
			else
			{
				int maxStars = PlayerPrefs.GetInt("level"+((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex])+"Stars",0);
				if(maxStars==0)
					PlayerPrefs.SetInt("level"+((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex])+"Stars",stars);
				else
				{
					if(maxStars<stars)
						PlayerPrefs.SetInt("level"+((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex])+"Stars",stars);
				}
				PlayerPrefs.Save();
				
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+5,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"To Main"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					MainMenuGui.nextMenu=MainMenuGui.GuiCategories.LevelsMenu;
					Application.LoadLevel("mainGui");
				}
				if(GUI.Button(new Rect(Screen.width/2-boxW/2+15+buttonW,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"Reply"))
				{
					GameStorage.getInstance().overlap=false;
					showNextLevelWindow=false;
					GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]));
				}
				if(MainMenuGui.playedLevelIndex+1!=MainMenuGui.selectedCampaign.levels.Count)
				{
					if(GUI.Button(new Rect(Screen.width/2-boxW/2+25+buttonW*2,Screen.height/2+boxH/2-buttonH-5,buttonW,buttonH),"Next level"))
					{
						GameStorage.getInstance().overlap=false;
						showNextLevelWindow=false;
						GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex+1]));
					}
				}
			}
			GUI.depth=GameStorage.getInstance().defaultDepth;
		}
		
		if(showPause)
		{
			GameStorage.getInstance().overlap=true;
			GUI.depth=200;
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
			int boxW=Screen.width/100*20;
			int boxH=Screen.height/100*40;
			string boxLabel="Game Pause";
			GUI.Box(new Rect(Screen.width/2-boxW/2,Screen.height/2-boxH/2,boxW,boxH),boxLabel);
			int buttonH,buttonW;
			buttonW=boxW/3-10;
			buttonH=boxH/100*20;
			
			if(GUI.Button(new Rect(Screen.width/2-boxW/2+5,Screen.height/2+boxH/2-5-buttonH,buttonW,buttonH),"Main Menu"))
			{
				GameStorage.getInstance().overlap=false;
				showPause=false;
				GameStorage.getInstance().EndLevel();
				MainMenuGui.nextMenu=MainMenuGui.GuiCategories.LevelsMenu;
				Application.LoadLevel("mainGui");
				
			}
			
			if(GUI.Button(new Rect(Screen.width/2-boxW/2+15+buttonW,Screen.height/2+boxH/2-5-buttonH,buttonW,buttonH),"Reply"))
			{
				GameStorage.getInstance().overlap=false;
				showPause=false;
				GameStorage.getInstance().EndLevel();
				GameStorage.getInstance().LoadLevel(Templates.getInstance().getLevel((int)MainMenuGui.selectedCampaign.levels[MainMenuGui.playedLevelIndex]));
			}
			
			if(GUI.Button(new Rect(Screen.width/2-boxW/2+25+buttonW*2,Screen.height/2+boxH/2-5-buttonH,buttonW,buttonH),"Continue"))
			{
				GameStorage.getInstance().overlap=false;
				showPause=false;
			}
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
