using UnityEngine;
using System.Collections;
using System;
using System.Threading;

public class FriendlyShuttleBehaviour : MonoBehaviour {

	//scale
	const float scale = 10;
	
	//UISETTINGS
	
	public Templates.PlaneTemplates Template;
	private Templates.PlaneTemplate temp;
	
	private const float damageShowTimer=2;
	private float damageShowTime=0;
	private int receivedDamage = 0;
	private bool receivedDefect = false;
	private bool block = false;
	public bool iconsShowed=false;
	public bool showPopup=false;
	
	public GameObject TEST_CURSORS;
	private Vector3 cursorPos;
	
	private float cursorXoffset,cursorYoffset;
	
	private Color goodColor = new Color(1,1,1,1);
	private Color disColor = new Color(1,1,1,0.2f);
	
	private Vector3 Paintvec;
	
	private float clickDist=3f;
	
	private Material lineMat;
	private ArrayList minesList=new ArrayList();
	
	//abil positions
	Vector3 firstAbilPos,secondAbilPos,thirdAbilPos,fourthAbilPos;
	// abil rotateDirection
	int turnRotateDir=1;
	float routeDist=0.0f;
	int gasSpawned=0;
	int minesSpawned=0;
	bool rocketSpawned=false;
	bool thorpedeSpawned=false;
	
	//DEBUG
	private ArrayList shuttleGunsMeshes = new ArrayList();
	private ArrayList shuttleGunsGos = new ArrayList();
	
	private ArrayList arcObjs = new ArrayList();
	
	//const
	float shuttleH=5;
	float attackIconH=6;
	float attackIconDistMin=2f;
	float attackIconDist=6f;
	float maxTurnAngle = 55;
	public int hp = 300;
	float lastFired=0f;
	bool abilityInReuse=false;
	Abilities.AbilityType origin0,origin1,origin2,origin3;
	
	Abilities.AbilityType activeAbil=Abilities.AbilityType.none;
	Abilities.AbilityType prevAbil = Abilities.AbilityType.none;
	
	//operations
	bool needToUpdatePosition = false;
	Vector3 newPos;
	bool needToUpdateAngle = false;
	float newAngle;
	bool needToUpdateAttackIconPosition = false;
	
	//Path
	Vector2 point1,point2,point3,point4,point5;
	float d;
	float t;
	
	//mechanics
	public float angle=0;
	Vector2 directionalVector;
	float sinPhi;
	Vector2 attackVec;
	ArrayList trackDots = new ArrayList();
	
	//states
	bool focused = false;
	public bool selected = false;
	public bool attackIconCaptured = false;
	bool spawn=true;
	private ArrayList privateAbils=new ArrayList();
	
	private Defects.Defect curDefect = null;
	bool earnedDefect = false;
	bool defectInUse = false;
	
	//objects
	GameObject attackIcon;
	
	public void setPosition(Vector2 pos)
	{
		newPos=new Vector3(pos.x,shuttleH,pos.y);
		needToUpdatePosition=true;
	}
	
	public void setActiveThorpedeAbil()
	{
		if(!abilityInReuse)
		{
			if(activeAbil!=Abilities.AbilityType.homingThorpede)
			{
				int i=0,ch=-1;
				foreach(Abilities.AbilityType ab in temp.abilities)
				{
					if(ab==Abilities.AbilityType.homingThorpede)
					{
						ch=i;
						break;
					}
					i++;
				}
				if(ch>=0)
				{
					Abilities.AbilityType selectedAbil = Abilities.AbilityType.homingThorpede;
					prevAbil=activeAbil;
					temp.abilities[ch]=(int)activeAbil;
					activeAbil=selectedAbil;
					AbilitySwitched();
				}
			}
		}
	}
	
	public void setActiveRocketAbil()
	{
		if(!abilityInReuse)
		{
			if(activeAbil!=Abilities.AbilityType.homingMissle)
			{
				int i=0,ch=-1;
				foreach(Abilities.AbilityType ab in temp.abilities)
				{
					if(ab==Abilities.AbilityType.homingMissle)
					{
						ch=i;
						break;
					}
					i++;
				}
				if(ch>=0)
				{
					Abilities.AbilityType selectedAbil = Abilities.AbilityType.homingMissle;
					prevAbil=activeAbil;
					temp.abilities[ch]=(int)activeAbil;
					activeAbil=selectedAbil;
					AbilitySwitched();
				}
			}
		}
	}
	
	public bool haveRockets()
	{
		if(abilityInReuse)
			return false;
		bool result=false;
		foreach(Abilities.AbilityType ab in temp.abilities)
		{
			if(ab==Abilities.AbilityType.homingMissle)
				result=true;
		}
		return result;
	}
	
	public bool haveThorpeds()
	{
		if(abilityInReuse)
			return false;
		
		bool result=false;
		foreach(Abilities.AbilityType ab in temp.abilities)
		{
			if(ab==Abilities.AbilityType.homingThorpede)
				result=true;
		}
		return result;
	}
	
	public void updateAttackPosition()
	{
		needToUpdateAttackIconPosition=true;
	}
	
	public void setAngle(float angle)
	{
		newAngle=angle;
		needToUpdateAngle=true;
	}
	
	private void loadOriginAbilities()
	{
		if(temp.abilities.Count>=1)
			temp.abilities[0]=(int)origin0;
		if(temp.abilities.Count>=2)
			temp.abilities[1]=(int)origin1;
		if(temp.abilities.Count>=3)
			temp.abilities[2]=(int)origin2;
		if(temp.abilities.Count>=4)
			temp.abilities[3]=(int)origin3;
	}
	
	public float getAngle()
	{
		return angle;
	}

	private void updateAttackIconPosition()
	{
		float ds;
		if(earnedDefect && curDefect.GetType() == typeof(Defects.EngineCrash))
			ds=temp.minRange*((Defects.EngineCrash)curDefect).newRangeCoeff;
		else
			ds = attackIconDist;
		Vector2 vec =Quaternion.Euler(0,0,-angle)*(new Vector2(0,1)*ds);
		attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
	}
	
	public void setMaxAttackIcon()
	{
		float ds;
		if(earnedDefect && curDefect.GetType() == typeof(Defects.EngineCrash))
			ds=temp.minRange*((Defects.EngineCrash)curDefect).newRangeCoeff;
		else if(activeAbil==Abilities.AbilityType.doubleThrottle)
			ds=temp.maxRange*2;
		else if(activeAbil==Abilities.AbilityType.turnAround)
			ds=(temp.minRange+temp.maxRange)/2;
		else if(activeAbil==Abilities.AbilityType.halfRoundTurn)
			ds=0.7f*temp.minRange;
		else
			ds = temp.maxRange;
		Vector2 vec =Quaternion.Euler(0,0,-angle)*(new Vector2(0,1)*ds);
		attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
	}
	
	void Start()
	{
		attackIcon = Instantiate(Resources.Load("prefab/attackIcon") as GameObject);
		attackIcon.SetActive(false);
		if(GameStorage.getInstance().getFixedTime()==-1)
			GameStorage.getInstance().overlap=false;
		
		LineRenderer lr = gameObject.AddComponent<LineRenderer>();
		lr.SetWidth(0.05f, 0.05f);
		GameStorage.getInstance().addFriendlyShuttle(this.gameObject);
		temp = Templates.getInstance().getPlaneTemplate(Template);
		foreach(int abils in temp.abilities)
			privateAbils.Add(abils);
		GameStorage.getInstance().totalHp+=temp.hp;
		GameObject go;
		MeshRenderer mr;
		lineMat = Resources.Load("materials/holograms/Hologram3") as Material;
		lr.material=lineMat;
		Mesh m;
		foreach(Templates.GunOnShuttle goss in temp.guns)
		{
			go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Templates.GunTemplate gt = Templates.getInstance().getGunTemplate(goss.gunId);
			
			Destroy(go.GetComponent<Collider>());
			mr = go.GetComponent<MeshRenderer>();
			mr.material=Resources.Load("materials/arc") as Material;
	      	m = go.GetComponent<MeshFilter>().mesh;
	      	m.Clear();
	        Vector3[] vertices = new Vector3[101];
	        vertices[0]=new Vector3(0,1,0);
	        float ds=gt.attackRange;
	        
	        Vector2 va;
	        int i=1;
	        for(float ang = -gt.attackAngle; ang<=gt.attackAngle; ang+=(2*gt.attackAngle)/100.0f)
	        {
	        	va=Quaternion.Euler(0,0,ang)*new Vector2(0,ds);
	        	vertices[i]=new Vector3(va.x,0,va.y);
	        	i++;
	        	if(i==101) break;
	        }
	        
	        int[] triangles = new int[vertices.Length*3];
	        int bb=0;
	        for(i=0;i<99;i++)
	        {
	        	triangles[bb*3]=0;
	        	triangles[bb*3+1]=i+1;
	        	triangles[bb*3+2]=i+2;
	        	bb++;
	        }
	   
	        Vector2[] uvs = new Vector2[vertices.Length];
			for (i = 0; i < uvs.Length; i++) {
				uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
			}
       
	        m.vertices=vertices;
	        m.triangles=triangles;
	        m.uv=uvs;
	       	m.RecalculateBounds();
	        m.RecalculateNormals();
	        arcObjs.Add(go);
	        
	        go=(GameObject) Instantiate(Resources.Load("prefab/testGunMesh") as GameObject,new Vector3(transform.position.x+goss.pos.x,1,transform.position.z+goss.pos.y),Quaternion.Euler(0,goss.turnAngle,0));
			go.SetActive(false);
			shuttleGunsMeshes.Add(go);
			shuttleGunsGos.Add(goss);
		}
		
		attackIconDistMin = temp.minRange;
		attackIconDist = temp.maxRange;
		maxTurnAngle = temp.maxTurnAngle;
		hp=temp.hp;
		
		if(temp.abilities.Count>=1)
			origin0=(Abilities.AbilityType) temp.abilities[0];
		if(temp.abilities.Count>=2)
			origin1=(Abilities.AbilityType) temp.abilities[1];
		if(temp.abilities.Count>=3)
			origin2=(Abilities.AbilityType) temp.abilities[2];
		if(temp.abilities.Count>=4)
			origin3=(Abilities.AbilityType) temp.abilities[3];
	}
	
	void AbilitySwitched()
	{
		if((prevAbil==Abilities.AbilityType.halfRoundTurn || prevAbil==Abilities.AbilityType.turnAround) && (activeAbil!=Abilities.AbilityType.halfRoundTurn && activeAbil!=Abilities.AbilityType.turnAround))
		{
			maxTurnAngle=temp.maxTurnAngle;
			attackIconDist=temp.maxRange;
			attackIconDistMin=temp.minRange;
			
			updateAttackIconPosition();
		}
		
		if(prevAbil==Abilities.AbilityType.doubleThrottle)
		{
			attackIconDist=temp.maxRange;
			attackIconDistMin=temp.minRange;
			
			float dst = Vector2.Distance(new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z),new Vector2(transform.position.x,transform.position.z));
			float angles = getAttackIconAngle();
			
			Vector2 newPosAt=Quaternion.Euler(0,0,-angles)*new Vector2(0,1)*dst/2;
			attackIcon.transform.position=new Vector3(newPosAt.x+transform.position.x,0,newPosAt.y+transform.position.z);
		}
		
		if(activeAbil==Abilities.AbilityType.none)
		{
			if(prevAbil==Abilities.AbilityType.homingMissle)
			{
				rocketSpawned=false;
			}
			
			if(prevAbil==Abilities.AbilityType.homingThorpede)
			{
				thorpedeSpawned=false;
			}
			
			if(prevAbil==Abilities.AbilityType.gas)
			{
				gasSpawned=0;
				routeDist=0f;
			}
			
			if(prevAbil==Abilities.AbilityType.mines)
			{
				minesSpawned=0;
				routeDist=0f;
				foreach(GameObject o in minesList)
				{
					o.GetComponent<MineBehaviour>().total=minesList.Count;
					o.GetComponent<MineBehaviour>().ready=true;
				}
				minesList.Clear();
			}
		}
		else
		{
			if(activeAbil==Abilities.AbilityType.doubleThrottle)
			{
				attackIconDist=temp.maxRange*2;
				attackIconDistMin=temp.minRange*2;
				
				float dst = Vector2.Distance(new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z),new Vector2(transform.position.x,transform.position.z));
				float angles = getAttackIconAngle();
				
				Vector2 newPosAt=Quaternion.Euler(0,0,-angles)*new Vector2(0,1)*dst*2;
				attackIcon.transform.position=new Vector3(newPosAt.x+transform.position.x,0,newPosAt.y+transform.position.z);
			
			}
			
			if(activeAbil==Abilities.AbilityType.halfRoundTurn)
			{
				maxTurnAngle=0;
				attackIconDist=0.7f*temp.minRange;
				attackIconDistMin=0.7f*temp.minRange;
				turnRotateDir=UnityEngine.Random.Range(0,2);
					if(turnRotateDir==0)
						turnRotateDir=-1;
				updateAttackIconPosition();
			}
			
			if(activeAbil==Abilities.AbilityType.turnAround)
			{
				attackIconDist=(temp.minRange+temp.maxRange)/2;
				attackIconDistMin=attackIconDist;
				turnRotateDir=UnityEngine.Random.Range(0,2);
					if(turnRotateDir==0)
						turnRotateDir=-1;
				updateAttackIconPosition();
			}
		}
	}
	
	void Update()
	{
		
		if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
			showPopup=selected;
		
		Vector2 av = Camera.main.WorldToScreenPoint(attackIcon.transform.position);
		Vector2 bv = av+new Vector2(Templates.ResolutionProblems.getActionAbilityOffset(Screen.width)+Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,0);
		
		av=new Vector2(Camera.main.ScreenToWorldPoint(av).x,Camera.main.ScreenToWorldPoint(av).z);
		bv=new Vector2(Camera.main.ScreenToWorldPoint(bv).x,Camera.main.ScreenToWorldPoint(bv).z);
		clickDist=Vector2.Distance(av,bv);
		
		if(showPopup)
		{
			int i=0;
			foreach(GameObject go in arcObjs)
			{
				Templates.GunOnShuttle goss = (Templates.GunOnShuttle)temp.guns[i];
				go.SetActive(true);
				Vector2 gosPos = Quaternion.Euler(0,0,-angle)*new Vector2(goss.pos.x,goss.pos.y);
				go.transform.position=new Vector3(transform.position.x+gosPos.x,0,transform.position.z+gosPos.y);
				go.transform.localRotation=Quaternion.Euler(0,angle+goss.turnAngle,0);
				i++;
			}
		}
		else
		{
			foreach(GameObject go in arcObjs)
				go.SetActive(false);
		}
		
		for(int i = 0;i<shuttleGunsMeshes.Count;i++)
		{
			GameObject goss = (GameObject) shuttleGunsMeshes[i];
			Vector2 f = Quaternion.Euler(0,0,-angle)*((Templates.GunOnShuttle)shuttleGunsGos[i]).pos;
			goss.transform.position=new Vector3(f.x+transform.position.x,10,f.y+transform.position.z);
			goss.transform.eulerAngles=new Vector3(0,angle+((Templates.GunOnShuttle)shuttleGunsGos[i]).turnAngle,0);
			goss.SetActive(GameStorage.getInstance().isDebug);
		}
		
		
		if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurn))
			maxTurnAngle=0f;
		
		if(needToUpdatePosition)
		{
			transform.position=newPos;
			needToUpdatePosition=false;
		}
		
		if(needToUpdateAngle)
		{
			angle=newAngle;
			needToUpdateAngle=false;
		}
		angle=Mathf.Repeat(angle,360);
		
		
		
		if(spawn)
		{
			updateAttackIconPosition();
			spawn=false;
		}
		

		if(needToUpdateAttackIconPosition)
		{
			updateAttackIconPosition();
			needToUpdateAttackIconPosition=false;
		}
		
		
		if(isMouseOver(gameObject))
			focused=true;
		else
			focused=false;
		
		checkShuttleClickState();
		checkAttackIconClickState();
		
		CalculatePath();
		DrawLine();
		
		
		if(GameStorage.getInstance().isRunning)
		{
			Accelerate();
			clearLine();
		}
		else
		{
			DrawLine();
		}
		if(attackIconCaptured)
			dragAttackIcon();
		
		transform.eulerAngles=new Vector3(0,angle,0);
	}
	
	public Vector3 isOutOfViewport()
	{
		Vector3 ruc = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,0,0));
		Vector3 luc = Camera.main.ScreenToWorldPoint(new Vector3(0,0,0));
		Vector3 rlc = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0));
		Vector3 llc = Camera.main.ScreenToWorldPoint(new Vector3(0,Screen.height,0));
		float maxX,maxY,minX,minY;
		if(ruc.x>luc.x) { maxX=ruc.x; minX=luc.x;} else {maxX=luc.x; minX=ruc.x;}
		if(ruc.y>rlc.y) { maxY=ruc.z; minY=rlc.z;} else {maxY=rlc.z; minY=ruc.z;}
		
		if(gameObject.transform.position.x <= maxX && gameObject.transform.position.x >= minX && gameObject.transform.position.z <= maxY && gameObject.transform.position.z >= minY)
			return new Vector3(-1,-1,-1);
		
		int code=0;
		if(gameObject.transform.position.x >= minX && gameObject.transform.position.x <= maxX)
		{
			if(gameObject.transform.position.z <= minY)
				code=5;
			else if(gameObject.transform.position.z >= maxY)
				code=1;
		}
		else if(gameObject.transform.position.z >= minY && gameObject.transform.position.z <= maxY)
		{
			if(gameObject.transform.position.x <= minX)
				code=3;
			else if(gameObject.transform.position.x >= maxX)
				code=7;
		}
		else
		{
			if(gameObject.transform.position.x < minX)
			{
				if(gameObject.transform.position.z < minY)
					code=4;
				else if(gameObject.transform.position.z > maxY)
					code=2;
			}
			else if(gameObject.transform.position.x > maxX)
			{
				if(gameObject.transform.position.z < minY)
					code=6;
				else if(gameObject.transform.position.z > maxY)
					code=8;
			}
		}
		
		Vector3 pos = new Vector3(-1,-1,-1);
		switch(code)
		{
			case 1:
				pos = new Vector3(gameObject.transform.position.x,0,maxY);
				break;
			case 2:
				pos = new Vector3(minX,0,maxY);
				break;
			case 3:
				pos = new Vector3(minX,0,gameObject.transform.position.z);
				break;
			case 4:
				pos = new Vector3(minX,0,minY);
				break;
			case 5:
				pos = new Vector3(gameObject.transform.position.x,0,minY);
				break;
			case 6:
				pos = new Vector3(maxX,0,minY);
				break;
			case 7:
				pos = new Vector3(maxX,0,gameObject.transform.position.z);
				break;
			case 8:
				pos = new Vector3(maxX,0,maxY);
				break;
		}
		
		Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2,Screen.height/2,0));
		Vector2 v1,v2;
		v1=new Vector2(0,5);
		v2=new Vector2(transform.position.x,transform.position.z)-new Vector2(center.x,center.z);
		float mySinPhi = (v1.x*v2.y - v1.y*v2.x);
		float mangle = Vector2.Angle(v1,v2);
		if(mySinPhi<=0)
			mangle=(180-mangle)+180;
		
		float retAng=0;
		if(code==5)
			retAng=180;
		else if(code==1)
			retAng=0;
		else if(code==7)
			retAng=90;
		else if(code==3)
			retAng=270;
		else
			retAng=-mangle;
		
		Vector3 ret;
		pos = Camera.main.WorldToScreenPoint(pos);
		ret = new Vector3(pos.x,pos.y,retAng);
		return ret;
	}
	
	void OnGUI()
	{
		
		if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
			showPopup=selected;
		GUI.enabled=!GameStorage.getInstance().overlap;
		if(GameStorage.getInstance().overlap)
			GUI.color=disColor;
		else
			GUI.color=goodColor;
		
		//TEST
		//GUI.Label(new Rect(100,100,40,40),"",Templates.getInstance().arrowRedSkin.label);
		//TEST_END
		
		if(damageShowTime>0)
		{
			float posX,posY;
			Vector3 pzp = Camera.main.WorldToScreenPoint(transform.position);
			posX=pzp.x-50;
			posY=Screen.height-pzp.y-50;
			string text="";
			if(block)
				text="<color=green>Blocked</color>";
			else
			{
				if(receivedDefect)
					text="<color=red>-"+receivedDamage+"</color>";
				else
					text="-"+receivedDamage;
			}
			GUI.Box(new Rect(posX,posY,100,20),text);
			damageShowTime-=Time.deltaTime;
			if(damageShowTime<0)
			{				
				damageShowTime=0;
				receivedDamage=0;
				receivedDefect=false;
			}
		}
		
		
		if(showPopup && !GameStorage.getInstance().isRunning)
		{	
			GUI.FocusControl(null);
			Paintvec = Camera.main.WorldToScreenPoint(transform.position);
			GUISkin progressSkin = Templates.getInstance().progressHpSkin;
			GUILayout.BeginArea(new Rect(Templates.ResolutionProblems.getPopupBannerOffset(Screen.width),Templates.ResolutionProblems.getPopupBannerOffset(Screen.width),Templates.ResolutionProblems.getPopupBannerWidth(Screen.width),Screen.height-Templates.ResolutionProblems.getPopupBannerOffset(Screen.width)*2));
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Name: "+temp.classname);
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Box(hp+"/"+temp.hp,progressSkin.box,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerHpWidth(Screen.width)*(((float)hp)/((float)temp.hp))));
			GUILayout.EndVertical();
			if(curDefect!=null)
				GUILayout.Label("Defect: <color=brown>"+curDefect.getName()+"</color>");
			if(privateAbils.Count>0)
			{
				GUILayout.Label("Abils:");
				GUILayout.BeginHorizontal();
				GUISkin s;
				foreach(int ab in privateAbils)
				{
					s=Templates.getInstance().getAbilityIcon(ab);
					GUILayout.Label("",s.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerAbilSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerAbilSize(Screen.width)));
				}
				GUILayout.EndHorizontal();
			}
			
			//Weapon
			GUILayout.Label("Weapons:");
			GUILayout.BeginHorizontal();
			for(int i = 0;i<temp.weapons;i++)
				GUILayout.Label("",Templates.getInstance().statPointBlue.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			for(int i = 0;i<5-temp.weapons;i++)
				GUILayout.Label("",Templates.getInstance().statPointGrey.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			GUILayout.EndHorizontal();
			GUILayout.Label("Armor:");
			GUILayout.BeginHorizontal();
			for(int i = 0;i<temp.armor;i++)
				GUILayout.Label("",Templates.getInstance().statPointBlue.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			for(int i = 0;i<5-temp.armor;i++)
				GUILayout.Label("",Templates.getInstance().statPointGrey.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			GUILayout.EndHorizontal();
			GUILayout.Label("Speed:");
			GUILayout.BeginHorizontal();
			for(int i = 0;i<temp.speed;i++)
				GUILayout.Label("",Templates.getInstance().statPointBlue.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			for(int i = 0;i<5-temp.speed;i++)
				GUILayout.Label("",Templates.getInstance().statPointGrey.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			GUILayout.EndHorizontal();
			GUILayout.Label("Maneuverability:");
			GUILayout.BeginHorizontal();
			for(int i = 0;i<temp.maneuverability;i++)
				GUILayout.Label("",Templates.getInstance().statPointBlue.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			for(int i = 0;i<5-temp.maneuverability;i++)
				GUILayout.Label("",Templates.getInstance().statPointGrey.label,GUILayout.Width(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)),GUILayout.Height(Templates.ResolutionProblems.getPopupBannerPointSize(Screen.width)));
			GUILayout.EndHorizontal();
			
			GUILayout.Label(temp.description);
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
			
		
		if(!GameStorage.getInstance().isRunning)
		{
			Vector3 v11 = attackIcon.transform.position;
			Vector2 aPos = new Vector2(Camera.main.WorldToScreenPoint(v11).x,Camera.main.WorldToScreenPoint(v11).y);
			
			GUI.skin = Templates.getInstance().getAbilityIcon(activeAbil);
			if(GUI.RepeatButton(new Rect(aPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-aPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),""))
			{
					attackIconCaptured=true;
					GameStorage.getInstance().cam.GetComponent<CameraBehaviour>().canReleaseMouse=false;
					GameStorage.getInstance().cam.GetComponent<CameraBehaviour>().currentSelected=gameObject;
					iconsShowed=true;
			}
			GUI.skin=null;
		}
		
		if(temp.abilities.Count>0 && (selected || iconsShowed) && !attackIconCaptured)
		{
			
			Vector2 startVec = new Vector2(Camera.main.WorldToScreenPoint(attackIcon.transform.position).x,Camera.main.WorldToScreenPoint(attackIcon.transform.position).y);
			
			firstAbilPos=startVec+new Vector2(Templates.ResolutionProblems.getActionAbilityOffset(Screen.width),0);
			secondAbilPos=startVec+new Vector2(-Templates.ResolutionProblems.getActionAbilityOffset(Screen.width),0);
			thirdAbilPos=startVec+new Vector2(0,Templates.ResolutionProblems.getActionAbilityOffset(Screen.width));
			fourthAbilPos=startVec+new Vector2(0,-Templates.ResolutionProblems.getActionAbilityOffset(Screen.width));
			
			if(temp.abilities.Count>=1)
			{
				if(abilityInReuse || earnedDefect)
				{
					GUI.skin=Templates.getInstance().getAbilityIconGrey((int)temp.abilities[0]);
					GUI.Button(new Rect(firstAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-firstAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"");
					GUI.skin=null;
				}
				else
				{
					GUI.skin=Templates.getInstance().getAbilityIcon((int)temp.abilities[0]);
					if(GUI.Button(new Rect(firstAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-firstAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),""))
					{
						Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[0];
						prevAbil=activeAbil;
						temp.abilities[0]=(int)activeAbil;
						activeAbil=selectedAbil;
						AbilitySwitched();
					}
					GUI.skin=null;
				}
			}
			if(temp.abilities.Count>=2)
			{
				if(abilityInReuse || earnedDefect)
				{
					GUI.skin=Templates.getInstance().getAbilityIconGrey((int)temp.abilities[1]);
					GUI.Button(new Rect(secondAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-secondAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"");
					GUI.skin=null;
				}
				else
				{
					GUI.skin=Templates.getInstance().getAbilityIcon((int)temp.abilities[1]);
					if(GUI.Button(new Rect(secondAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-secondAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),""))
					{
						Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[1];
						prevAbil=activeAbil;
						temp.abilities[1]=(int)activeAbil;
						activeAbil=selectedAbil;
						AbilitySwitched();
					}
					GUI.skin=null;
				}
			}
			if(temp.abilities.Count>=3)
			{
				if(abilityInReuse || earnedDefect)
				{
					GUI.skin=Templates.getInstance().getAbilityIconGrey((int)temp.abilities[2]);
					GUI.Button(new Rect(thirdAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-thirdAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"");
					GUI.skin=null;
				}
				else
				{
					GUI.skin=Templates.getInstance().getAbilityIcon((int)temp.abilities[2]);
					if(GUI.Button(new Rect(thirdAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-thirdAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),""))
					{
						Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[2];
						prevAbil=activeAbil;
						temp.abilities[2]=(int)activeAbil;
						activeAbil=selectedAbil;
						AbilitySwitched();
					}
					GUI.skin=null;
				}
			}
			if(temp.abilities.Count>=4)
			{
				if(abilityInReuse || earnedDefect)
				{
					GUI.skin=Templates.getInstance().getAbilityIconGrey((int)temp.abilities[3]);
					GUI.Button(new Rect(fourthAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-fourthAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),"");
					GUI.skin=null;
				}
				else
				{
					GUI.skin=Templates.getInstance().getAbilityIcon((int)temp.abilities[3]);
					if(GUI.Button(new Rect(fourthAbilPos.x-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Screen.height-fourthAbilPos.y-Templates.ResolutionProblems.getActionAbilitySize(Screen.width)/2,Templates.ResolutionProblems.getActionAbilitySize(Screen.width),Templates.ResolutionProblems.getActionAbilitySize(Screen.width)),""))
					{
						Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[3];
						prevAbil=activeAbil;
						temp.abilities[3]=(int)activeAbil;
						activeAbil=selectedAbil;
						AbilitySwitched();
					}
					GUI.skin=null;
				}
			}
		}
	}
	
	private void clearLine()
	{
		LineRenderer lr = gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(0);
	}
	
	public bool cancelMouseDrop()
	{
		return !(Vector2.Distance(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z),new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z))>clickDist);
	}
	
	public void checkShuttleClickState()
	{
		if(!GameStorage.getInstance().overlap)
		{
			if(!GameStorage.getInstance().isRunning)
			{
				if((Input.GetMouseButtonDown(0) && isMouseOver(gameObject)) || (Input.GetMouseButtonDown(0) && isMouseOver(attackIcon)))
				{
					//selected=true;
					GameStorage.getInstance().currentSelectedFriendly=gameObject;
				}
				else if(Input.GetMouseButtonDown(0) && !isMouseOver(gameObject))
				{
					if(Vector2.Distance(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z),new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z))>clickDist)
					{
						selected=false;
						GameStorage.getInstance().currentSelectedFriendly=null;
					}
				}
			}
		}
	}
	
	private float getAttackAngle()
	{
		
		Vector2 v1 = new Vector2(0,5);
		Vector2 v2 = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x-transform.position.x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z-transform.position.z);
		float mySinPhi = (v1.x*v2.y - v1.y*v2.x);
		float mangle = Vector2.Angle(v1,v2);
		if(mySinPhi<=0)
			return mangle;
		else
			return (180-mangle)+180;
	}
	
	private float getAttackIconAngle()
	{
		
		Vector2 v1 = new Vector2(0,5);
		Vector2 v2 = new Vector2(attackIcon.transform.position.x-transform.position.x,attackIcon.transform.position.z-transform.position.z);
		float mySinPhi = (v1.x*v2.y - v1.y*v2.x);
		float mangle = Vector2.Angle(v1,v2);
		if(mySinPhi<=0)
			return mangle;
		else
			return (180-mangle)+180;
	}
	
	public void Attacked(GameObject attacker, int damage, Defects.Defect defect)
	{
		damageShowTime=damageShowTimer;
		if(activeAbil!=Abilities.AbilityType.shield)
		{
			block=false;
			if(curDefect==null && defect!=null)
			{
				receivedDefect=true;
				curDefect=defect;
				earnedDefect=true;
			}
			receivedDamage+=damage;
			this.hp-=damage;
			if(this.hp<=0)
				this.Die();
		}
		else
			block=true;
	}
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			if(!((defectInUse && curDefect.GetType()==typeof(Defects.DisableGuns))))
			{
				if(!Abilities.getLockGun(activeAbil))
				{
					Templates.GunTemplate gunTemp;
					GameObject enemy;
					foreach(Templates.GunOnShuttle gun in temp.guns)
					{
						gunTemp=Templates.getInstance().getGunTemplate(gun.gunId);
						if(!gun.ready)
							if(gun.shotTime+gunTemp.reuse<Time.time)
								gun.ready=true;
						
						enemy=GameStorage.getInstance().getEnemyInFireZone(gameObject,gun);
						if(enemy!=null)
						{
							if(gun.ready)
							{
								gun.shotTime=Time.time;
								gun.ready=false;
								// WARN
								Vector2 f = Quaternion.Euler(0,0,-angle)*gun.pos;
								GameObject bullet = (GameObject) Instantiate(Resources.Load("prefab/"+gunTemp.bulletMesh) as GameObject,new Vector3(transform.position.x+f.x,0,transform.position.z+f.y),Quaternion.Euler(0,angle,0));
								bullet.GetComponent<BulletBehaviour>().Launch(new Vector2(enemy.transform.position.x,enemy.transform.position.z),new Vector2(transform.position.x+f.x,transform.position.z+f.y),gun);
								
							}
						}
					}
				}
			}
			
			if(defectInUse && curDefect.GetType() == typeof(Defects.Fired))
			{
				Defects.Fired fire = (Defects.Fired)curDefect;
				if(lastFired==0)
				{
					Attacked(null,fire.damage,null);
					lastFired=Time.time;
				}
				else
				{
					if(Time.time>=lastFired+fire.reuse)
					{
						Attacked(null,fire.damage,null);
						lastFired=Time.time;
					}
				}
			}
			
			float x,y;
			t+=1*Time.deltaTime/3;
			
			x=(1-t)*(1-t)*(1-t)*point1.x+3*(1-t)*(1-t)*t*point2.x+3*(1-t)*t*t*point3.x+t*t*t*point4.x;
			y=(1-t)*(1-t)*(1-t)*point1.y+3*(1-t)*(1-t)*t*point2.y+3*(1-t)*t*t*point3.y+t*t*t*point4.y;
			
			if(activeAbil==Abilities.AbilityType.halfRoundTurn)
				angle=Mathf.Repeat(angle+(180/3*Time.deltaTime*turnRotateDir),360);
			else if(activeAbil==Abilities.AbilityType.turnAround)
				angle=Mathf.Repeat(angle+(360/3*Time.deltaTime*turnRotateDir),360);
			else
			{
				Vector2 pos = new Vector2(x-transform.position.x,y-transform.position.z);
				float nAngle;
				Vector2 v1 = new Vector2(0,5);
				float mySinPhi = (v1.x*pos.y - v1.y*pos.x);
				nAngle = Vector2.Angle(v1,pos);
				if(mySinPhi>0)
					nAngle=(180-nAngle)+180;
				angle=nAngle;
				if(activeAbil==Abilities.AbilityType.gas)
				{
					if(routeDist>=gasSpawned*Abilities.GasParameters.betweenDist)
					{
						Instantiate(Resources.Load("prefab/gasPrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						gasSpawned++;
					}
					routeDist+=pos.magnitude;
				}
				if(activeAbil==Abilities.AbilityType.mines)
				{
					if(routeDist>=minesSpawned*Abilities.MinesParameters.betweenDist)
					{
						GameObject mine = (GameObject) Instantiate(Resources.Load("prefab/minePrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						mine.GetComponent<MineBehaviour>().cur=minesSpawned;
						minesList.Add(mine);
						minesSpawned++;
					}
					routeDist+=pos.magnitude;
				}
				if(activeAbil==Abilities.AbilityType.homingMissle && !rocketSpawned)
				{
					if(t>=1.0/3.0)
					{
						Instantiate(Resources.Load("prefab/rocketPrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						rocketSpawned=true;
					}
				}
				if(activeAbil==Abilities.AbilityType.homingThorpede && !thorpedeSpawned)
				{
					if(t>=1.0/3.0)
					{
						Instantiate(Resources.Load("prefab/thorpedePrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						thorpedeSpawned=true;
					}
				}
			}
			transform.position=new Vector3(x,0,y);
			updateAttackIconPosition();
		}
		else
		{
			GameStorage.getInstance().StepStop();
		}
	}
	
	public void dragAttackIcon()
	{
		Vector2 mouse = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z);
		float dst = new Vector2(mouse.x-transform.position.x,mouse.y-transform.position.z).magnitude;
		Vector2 vec;
		float ds = getAngleDst(angle,getAttackAngle());
		
		if(earnedDefect && curDefect.GetType() == typeof(Defects.EngineCrash))
		{
			if(Mathf.Abs(ds)>maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*attackIconDistMin*((Defects.EngineCrash)curDefect).newRangeCoeff;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*attackIconDistMin*((Defects.EngineCrash)curDefect).newRangeCoeff;
			}
			else
				vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDistMin*((Defects.EngineCrash)curDefect).newRangeCoeff;
			
			attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
		}
		else
		{
			if(dst>=attackIconDist)
			{
				if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnLeft))
				{
					if(Mathf.Abs(ds)>maxTurnAngle && ds<=0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*attackIconDist;
					else if(ds>0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle,360))*new Vector2(0,1)*attackIconDist;
					else
						vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDist;
				}
				else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnRight))
				{
					if(Mathf.Abs(ds)>maxTurnAngle && ds>=0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*attackIconDist;
					else if(ds<0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle,360))*new Vector2(0,1)*attackIconDist;
					else
						vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDist;
				}
				else
				{
					if(Mathf.Abs(ds)>=maxTurnAngle)
					{
						if(ds<0)
							vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*attackIconDist;
						else
							vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*attackIconDist;
					}
					else
						vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDist;
				}
				attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
			}
			else if(dst<=attackIconDistMin)
			{
				if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnLeft))
				{
					if(Mathf.Abs(ds)>maxTurnAngle && ds<=0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*attackIconDistMin;
					else if(ds>0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle,360))*new Vector2(0,1)*attackIconDistMin;
					else
						vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDistMin;
				}
				else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnRight))
				{
					if(Mathf.Abs(ds)>maxTurnAngle && ds>=0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*attackIconDistMin;
					else if(ds<0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle,360))*new Vector2(0,1)*attackIconDistMin;
					else
						vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDistMin;
				}
				else
				{
					if(Mathf.Abs(ds)>=maxTurnAngle)
					{
						if(ds<0)
							vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*attackIconDistMin;
						else
							vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*attackIconDistMin;
					}
					else
						vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDistMin;
				}
				attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
			}
			else
			{
				if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnLeft))
				{
					if(Mathf.Abs(ds)>maxTurnAngle && ds<=0)
					{
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*dst;
						attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
					}
					else if(ds>0)
					{
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle,360))*new Vector2(0,1)*dst;
						attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
					}
					else
					{
						vec = new Vector2(mouse.x,mouse.y);
						attackIcon.transform.position=new Vector3(vec.x,attackIconH,vec.y);
					}
				}
				else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnRight))
				{
					if(Mathf.Abs(ds)>maxTurnAngle && ds>=0)
					{
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*dst;
						attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
					}
					else if(ds<0)
					{
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle,360))*new Vector2(0,1)*dst;
						attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
					}
					else
					{
						vec = new Vector2(mouse.x,mouse.y);
						attackIcon.transform.position=new Vector3(vec.x,attackIconH,vec.y);
					}
				}
				else
				{
					if(Mathf.Abs(ds)>=maxTurnAngle)
					{
						if(ds<0)
							vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+maxTurnAngle,360))*new Vector2(0,1)*dst;
						else
							vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-maxTurnAngle,360))*new Vector2(0,1)*dst;
						attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
					}
					else
					{
						vec = new Vector2(mouse.x,mouse.y);
						attackIcon.transform.position=new Vector3(vec.x,attackIconH,vec.y);
					}
				}
			}
		}
	}
	
	public void checkAttackIconClickState()
	{
		if(Input.GetMouseButtonDown(0) && isMouseOver(attackIcon))
		{
			GameStorage.getInstance().cam.GetComponent<CameraBehaviour>().canReleaseMouse=false;
			attackIconCaptured=true;
			
		}
		if(Input.GetMouseButtonUp(0))
		{
			attackIconCaptured=false;
			GameStorage.getInstance().cam.GetComponent<CameraBehaviour>().canReleaseMouse=true;
		}
	}
	
	public void StepStart()
	{
		if(earnedDefect)
			defectInUse=true;
		selected=false;
		iconsShowed=false;
		showPopup=false;
	}
	
	public void StepEnd()
	{
		if(activeAbil==Abilities.AbilityType.none && abilityInReuse)
			abilityInReuse=false;
		loadOriginAbilities();
		
		if(activeAbil!=Abilities.AbilityType.none && !abilityInReuse)
		{
			prevAbil=activeAbil;
			abilityInReuse=true;
			activeAbil=Abilities.AbilityType.none;
			
			AbilitySwitched();
		}
		if(defectInUse)
		{
			if(curDefect.GetType() == typeof(Defects.DisableTurn))
				maxTurnAngle=temp.maxTurnAngle;
			defectInUse=false;
			earnedDefect=false;
			curDefect=null;
		}
		t=0;
		updateAttackPosition();
	}
	
	private float getAngleDst(float fr, float to)
	{
		Vector2 v1 = Quaternion.Euler(0,0,fr)*new Vector2(0,5);
		Vector2 v2 = Quaternion.Euler(0,0,to)*new Vector2(0,5);
		
		float sin = (v1.x*v2.y - v1.y*v2.x);
		float a = fr-to;
		if(Mathf.Abs(a)>180)
			a=360-Mathf.Abs(a);
		
		if(sin>0)
			return -Mathf.Abs(a);
		else
			return Mathf.Abs(a);
	}
	
	public void Die()
	{
		if(activeAbil!=Abilities.AbilityType.none && !abilityInReuse)
		{
			prevAbil=activeAbil;
			abilityInReuse=true;
			activeAbil=Abilities.AbilityType.none;
			AbilitySwitched();
		}
		GameStorage.getInstance().removeFriendlyShuttle(this.gameObject);
	}
	
	public void ByeBye()
	{
		foreach(GameObject g in shuttleGunsMeshes)
			Destroy(g);
		Destroy(this.gameObject);
	}
	
	private bool isMouseOver(GameObject o)
	{
    	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    	RaycastHit hit;
    	if (Physics.Raycast(ray, out hit)){
    		return hit.collider.gameObject==o;
    	}
    	return false;
	}
	
	private void CalculatePath()
	{
		if(!GameStorage.getInstance().isRunning)
		{         
			point1=new Vector2(transform.position.x,transform.position.z);
			if(activeAbil==Abilities.AbilityType.doubleThrottle)
				point2=Quaternion.Euler(0,0,-angle)*new Vector2(0,temp.minRange*2*Mathf.Abs(getAngleDst(angle,getAttackIconAngle())/temp.maxTurnAngle)*Vector2.Distance(point1,point4)/temp.maxRange/2)*temp.lowerSmooth;
			else
				point2=Quaternion.Euler(0,0,-angle)*new Vector2(0,temp.minRange*Mathf.Abs(getAngleDst(angle,getAttackIconAngle())/temp.maxTurnAngle)*Vector2.Distance(point1,point4)/temp.maxRange)*temp.lowerSmooth;
			point2+=point1;
			point4=new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z);
			Vector2 pointz = new Vector2(point4.x-point2.x,point4.y-point2.y)/2;
			if(activeAbil==Abilities.AbilityType.doubleThrottle)
				point3 = new Vector2(pointz.y,-pointz.x)*getAngleDst(angle,getAttackIconAngle())/temp.maxTurnAngle*Vector2.Distance(point1,point4)/temp.maxRange/2*temp.upperSmooth;
			else
				point3 = new Vector2(pointz.y,-pointz.x)*getAngleDst(angle,getAttackIconAngle())/temp.maxTurnAngle*Vector2.Distance(point1,point4)/temp.maxRange*temp.upperSmooth;
			
			point3 = point3+point2+pointz;
			
			trackDots.Clear();
			float x,y,tt;
			float step = 0.001f;
			for(tt=0f;tt<=1;tt+=step)
			{
				x = Mathf.Pow((1-tt),3)*point1.x+3*(1-tt)*(1-tt)*tt*point2.x+3*(1-tt)*tt*tt*point3.x+tt*tt*tt*point4.x;
				y = Mathf.Pow((1-tt),3)*point1.y+3*(1-tt)*(1-tt)*tt*point2.y+3*(1-tt)*tt*tt*point3.y+tt*tt*tt*point4.y;
				trackDots.Add(new Vector2(x,y));
			}                                       
		}
	}
	
	private void DrawLine()
	{
		LineRenderer lr = gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(trackDots.Count);
		int i;
		for(i=0;i<trackDots.Count;i++)
			lr.SetPosition(i,new Vector3(((Vector2)trackDots[i]).x,attackIconH,((Vector2)trackDots[i]).y));
	}
	
	
}