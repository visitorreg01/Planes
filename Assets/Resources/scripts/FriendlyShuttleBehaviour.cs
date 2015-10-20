using UnityEngine;
using System.Collections;
using System;

public class FriendlyShuttleBehaviour : MonoBehaviour {

	//scale
	const float scale = 10;
	
	public Templates.PlaneTemplates Template;
	private Templates.PlaneTemplate temp;
	
	//abil positions
	Vector3 firstAbilPos,secondAbilPos,thirdAbilPos,fourthAbilPos;
	// abil rotateDirection
	int turnRotateDir=1;
	float routeDist=0.0f;
	int gasSpawned=0;
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
	int hp = 300;
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
	Vector2 point1,point2,point3,point4;
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
	bool selected = false;
	bool attackIconCaptured = false;
	bool attackIconFocused = false;
	bool spawn=true;
	
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
			ds = (attackIconDist-attackIconDistMin)/2+attackIconDistMin;
		Vector2 vec =Quaternion.Euler(0,0,-angle)*(new Vector2(0,1)*ds);
		attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
	}
	
	void Start()
	{
		attackIcon = Instantiate(Resources.Load("prefab/attackIcon") as GameObject);
		attackIcon.SetActive(false);
		LineRenderer lr = gameObject.AddComponent<LineRenderer>();
		lr.SetWidth(0.05f, 0.05f);
		GameStorage.getInstance().addFriendlyShuttle(this.gameObject);
		temp = Templates.getInstance().getPlaneTemplate(Template);
		
		GameObject gD;
		foreach(Templates.GunOnShuttle goss in temp.guns)
		{
			gD=new GameObject();
			LineRenderer line = gD.AddComponent<LineRenderer>();
			line.SetWidth(0.05f, 0.05f);
			line.material = Resources.Load("materials/lineMaterial") as Material;
			arcObjs.Add(gD);
			gD=(GameObject) Instantiate(Resources.Load("prefab/testGunMesh") as GameObject,new Vector3(transform.position.x+goss.pos.x,10,transform.position.z+goss.pos.y),Quaternion.Euler(0,goss.turnAngle,0));
			gD.SetActive(false);
			shuttleGunsMeshes.Add(gD);
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
		if(activeAbil==Abilities.AbilityType.none)
		{
			if(prevAbil==Abilities.AbilityType.halfRoundTurn)
			{
				maxTurnAngle=temp.maxTurnAngle;
				attackIconDist=temp.maxRange;
				attackIconDistMin=temp.minRange;
				
				updateAttackIconPosition();
			}
			
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
			
			if(prevAbil==Abilities.AbilityType.turnAround)
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
				maxTurnAngle=0;
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
		
		if(selected)
		{
			int ii=0;
			//Templates.GunOnShuttle gos = (Templates.GunOnShuttle) temp.guns[0];
			foreach(Templates.GunOnShuttle gos in temp.guns)
			{
				LineRenderer line = ((GameObject) arcObjs[ii]).GetComponent<LineRenderer>();
				Templates.GunTemplate gunTmp = Templates.getInstance().getGunTemplate(gos.gunId);
				Vector2 startP = Quaternion.Euler(0,0,-angle)*gos.pos;
				startP=new Vector2(transform.position.x+startP.x,transform.position.z+startP.y);
				Vector2 lowerP = Quaternion.Euler(0,0,-(angle+gos.turnAngle-gunTmp.attackAngle))*new Vector2(0,gunTmp.attackRange);
				lowerP=new Vector2(lowerP.x+transform.position.x,lowerP.y+transform.position.z);
				Vector2 upperP = Quaternion.Euler(0,0,-(angle+gos.turnAngle+gunTmp.attackAngle))*new Vector2(0,gunTmp.attackRange);
				upperP=new Vector2(upperP.x+transform.position.x,upperP.y+transform.position.z);
				
				int le = (int) (2*gunTmp.attackAngle/1f),i;
				
				line.SetVertexCount(3+le);
				line.SetPosition(0,new Vector3(startP.x,0,startP.y));
				//line.SetPosition(1,new Vector3(upperP.x,0,upperP.y));
				for(i=0;i<le;i++)
				{
					Vector2 po = Quaternion.Euler(0,0,-(angle+gos.turnAngle+gunTmp.attackAngle-1*(i)))*new Vector2(0,gunTmp.attackRange);
					po=new Vector2(po.x+transform.position.x,po.y+transform.position.z);
					line.SetPosition(1+i,new Vector3(po.x,0,po.y));
				}
				line.SetPosition(1+le,new Vector3(lowerP.x,0,lowerP.y));
				line.SetPosition(2+le,new Vector3(startP.x,0,startP.y));
				ii++;
			}
		}
		else
		{
			foreach(GameObject gg in arcObjs)
			{
				LineRenderer line = gg.GetComponent<LineRenderer>();
				line.SetVertexCount(0);
			}
		}
		
		
		if(isMouseOver(gameObject))
			focused=true;
		else
			focused=false;
		
		checkShuttleClickState();
		checkAttackIconClickState();
		
		CalculatePath();
		DrawLine();
		if(selected)
		{
			if(!GameStorage.getInstance().isRunning)
			{
				attackIcon.SetActive(true);
			}
			else
			{
				attackIcon.SetActive(false);
			}
		}
		else
		{
			attackIcon.SetActive(false);
		}
		
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
	
	void OnGUI()
	{
		if(focused && !GameStorage.getInstance().isRunning)
		{
			Vector3 vec = Camera.main.WorldToScreenPoint(transform.position);
			GUI.Box(new Rect(vec.x,Screen.height-vec.y,100,200),temp.classname);
			GUI.Label(new Rect(vec.x+2,Screen.height-vec.y+20,100,20),("HP: "+hp+"/"+temp.hp));
			GUI.Label(new Rect(vec.x+2,Screen.height-vec.y+40,100,180),temp.description);
		}
		
		if(temp.abilities.Count>0 && attackIconFocused && !attackIconCaptured && !abilityInReuse && !earnedDefect)
		{
			firstAbilPos = Camera.main.WorldToScreenPoint(new Vector3(attackIcon.transform.position.x-1f,0,attackIcon.transform.position.z));
			secondAbilPos = Camera.main.WorldToScreenPoint(new Vector3(attackIcon.transform.position.x+1f,0,attackIcon.transform.position.z));
			thirdAbilPos = Camera.main.WorldToScreenPoint(new Vector3(attackIcon.transform.position.x,0,attackIcon.transform.position.z+1f));
			fourthAbilPos = Camera.main.WorldToScreenPoint(new Vector3(attackIcon.transform.position.x,0,attackIcon.transform.position.z-1f));
			if(temp.abilities.Count>=1)
			{
				if(GUI.Button(new Rect(firstAbilPos.x-10,Screen.height-firstAbilPos.y-10,20,20),temp.abilities[0].ToString()))
				{
					Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[0];
					prevAbil=activeAbil;
					temp.abilities[0]=(int)activeAbil;
					activeAbil=selectedAbil;
					AbilitySwitched();
				}
			}
			if(temp.abilities.Count>=2)
			{
				if(GUI.Button(new Rect(secondAbilPos.x-10,Screen.height-secondAbilPos.y-10,20,20),temp.abilities[1].ToString()))
				{
					Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[1];
					prevAbil=activeAbil;
					temp.abilities[1]=(int)activeAbil;
					activeAbil=selectedAbil;
					AbilitySwitched();
				}
			}
			if(temp.abilities.Count>=3)
			{
				if(GUI.Button(new Rect(thirdAbilPos.x-10,Screen.height-thirdAbilPos.y-10,20,20),temp.abilities[2].ToString()))
				{
					Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[2];
					prevAbil=activeAbil;
					temp.abilities[2]=(int)activeAbil;
					activeAbil=selectedAbil;
					AbilitySwitched();
				}
			}
			if(temp.abilities.Count>=4)
			{
				if(GUI.Button(new Rect(fourthAbilPos.x-10,Screen.height-fourthAbilPos.y-10,20,20),temp.abilities[3].ToString()))
				{
					Abilities.AbilityType selectedAbil = (Abilities.AbilityType)temp.abilities[3];
					prevAbil=activeAbil;
					temp.abilities[3]=(int)activeAbil;
					activeAbil=selectedAbil;
					AbilitySwitched();
				}
			}
		}
	}
	
	private void clearLine()
	{
		LineRenderer lr = gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(0);
	}
	
	public void checkShuttleClickState()
	{
		if(!GameStorage.getInstance().isRunning)
		{
			if((Input.GetMouseButtonDown(0) && isMouseOver(gameObject)) || (Input.GetMouseButtonDown(0) && isMouseOver(attackIcon)))
				selected=true;
			else if(Input.GetMouseButtonDown(0) && !isMouseOver(gameObject))
			{
				if(temp.abilities.Count>0)
				{
					if(Vector2.Distance(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z),new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z))>1.7f)
						selected=false;
				}
				else
					selected=false;
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
		if(activeAbil!=Abilities.AbilityType.shield)
		{
			if(curDefect==null && defect!=null)
			{
				Debug.Log("Earned defect: "+defect);
				curDefect=defect;
				earnedDefect=true;
			}
			this.hp-=damage;
			if(this.hp<=0)
				this.Die();
		}
	}
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			if(!(defectInUse && curDefect.GetType()==typeof(Defects.DisableGuns)) && activeAbil!=Abilities.AbilityType.gas && activeAbil!=Abilities.AbilityType.homingMissle && activeAbil!=Abilities.AbilityType.homingThorpede)
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
			
			x = Mathf.Pow((1-t),3)*point1.x+3*(1-t)*(1-t)*t*point2.x+3*(1-t)*t*t*point3.x+t*t*t*point4.x;
			y = Mathf.Pow((1-t),3)*point1.y+3*(1-t)*(1-t)*t*point2.y+3*(1-t)*t*t*point3.y+t*t*t*point4.y;
			
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
		if(selected)
		{
			if(isMouseOver(attackIcon))
				attackIconFocused=true;
			else
			{
				if(temp.abilities.Count>0)
				{
					if(Vector2.Distance(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z),new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z))>1.7f)
						attackIconFocused=false;
				}
				else
					attackIconFocused=false;
			}
		}
		
		if(Input.GetMouseButtonDown(0) && isMouseOver(attackIcon))
		{
			attackIconCaptured=true;
		}
		if(Input.GetMouseButtonUp(0))
		{
			attackIconCaptured=false;
		}
	}
	
	public void StepStart()
	{
		if(earnedDefect)
			defectInUse=true;
	}
	
	public void StepEnd()
	{
		if(activeAbil==Abilities.AbilityType.none && abilityInReuse)
		{
			abilityInReuse=false;
			loadOriginAbilities();
		}
		
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
			Vector2 vvec = Quaternion.Euler(0,0,-getAngle())*new Vector2(0,1);
			Vector3 tempVec = GetComponent<Renderer>().bounds.ClosestPoint(new Vector3(vvec.x+transform.position.x,0,vvec.y+transform.position.z));
			point2=new Vector2(tempVec.x-transform.position.x,tempVec.z-transform.position.z);
			//point2*=dd;
			point2=new Vector2(transform.position.x+point2.x,transform.position.z+point2.y);
			
			point4=new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z);
			Vector2 pointz = new Vector2(point4.x-point2.x,point4.y-point2.y)/2;
			point3 = new Vector2(pointz.y,-pointz.x)*GameStorage.getInstance().getAngleDst(getAngle(),getAttackIconAngle())*0.02f;
			point3 = point3+point2+pointz;
			
			
			trackDots.Clear();
			float x,y,tt;
			float step = 0.005f;
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