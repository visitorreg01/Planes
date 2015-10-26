using UnityEngine;
using System.Collections;

public class EnemyShuttleBehaviour : MonoBehaviour {

	public Templates.PlaneTemplates Template;
	private Templates.PlaneTemplate temp;
	
	int shuttleH = 0;
	private ArrayList minesList=new ArrayList();
	int minesSpawned=0;
	public float angle=0;
	int hp;
	float lastFired=0f;
	int turnRotateDir=1;
	float newAngle;
	Vector2 newPos;
	private Defects.Defect curDefect = null;
	bool earnedDefect = false;
	bool defectInUse = false;
	float routeDist=0.0f;
	int gasSpawned=0;
	bool rocketSpawned=false;
	bool thorpedeSpawned=false;
	
	int preferencedDirection=0; // 0 - bez raznicy, -1 left, -right
	
		//DEBUG
	private ArrayList shuttleGunsMeshes = new ArrayList();
	private ArrayList shuttleGunsGos = new ArrayList();
	
	Vector2 point1,point2,point3,point4;
	float t=0;
	
	Abilities.AbilityType activeAbil=Abilities.AbilityType.none;
	Abilities.AbilityType prevAbil = Abilities.AbilityType.none;
	
	// orientation
	Vector2 movePoint;
	bool abilityInReuse=false;
	
	//DEBUG!
	public GameObject viewGO = null;
	
	bool focused=false;
	bool needToUpdateAngle=false;
	bool needToUpdatePosition=false;
	
	public void setPosition(Vector2 pos)
	{
		newPos=new Vector3(pos.x,shuttleH,pos.y);
		needToUpdatePosition=true;
	}
	
	public void setAngle(float angle)
	{
		newAngle=angle;
		needToUpdateAngle=true;
	}
	
	public float getAngle()
	{
		return Mathf.Repeat(angle,360);
	}
	
	public void StepStart()
	{
		calculateMovePosition();
		if(earnedDefect)
			defectInUse=true;
	}
	
	void AbilitySwitched()
	{
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
			
			if(prevAbil==Abilities.AbilityType.gas)
			{
				gasSpawned=0;
				routeDist=0f;
			}
		}
		else
		{
			if(activeAbil==Abilities.AbilityType.halfRoundTurn)
			{
				turnRotateDir=UnityEngine.Random.Range(0,2);
					if(turnRotateDir==0)
						turnRotateDir=-1;
			}
		}
	}
	
	public void StepEnd()
	{
		if(activeAbil==Abilities.AbilityType.none && abilityInReuse)
		{
			abilityInReuse=false;
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
			defectInUse=false;
			earnedDefect=false;
			curDefect=null;
		}
		t=0;
		
		if(!abilityInReuse && !earnedDefect)
			TryChoiceAbil();
	}
	
	void TryChoiceAbil()
	{
		if(temp.abilities.Count>0)
		{
			if(UnityEngine.Random.Range(0,100)>=30)
			{
				prevAbil=activeAbil;
				activeAbil=(Abilities.AbilityType) temp.abilities[UnityEngine.Random.Range(0,temp.abilities.Count)];
				AbilitySwitched();
				Debug.Log("USED: "+activeAbil);
			}
		}
	}
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			if(!(defectInUse && curDefect.GetType()==typeof(Defects.DisableGuns)) && activeAbil!=Abilities.AbilityType.gas && activeAbil!=Abilities.AbilityType.homingMissle && activeAbil!=Abilities.AbilityType.homingThorpede)
			{
				Templates.GunTemplate gunTemp;
				GameObject friendly;
				foreach(Templates.GunOnShuttle gun in temp.guns)
				{
					gunTemp=Templates.getInstance().getGunTemplate(gun.gunId);
					if(!gun.ready)
						if(gun.shotTime+gunTemp.reuse<Time.time)
							gun.ready=true;
					
					friendly=GameStorage.getInstance().getFriendlyInFireZone(gameObject,gun);
					if(friendly!=null)
					{
						if(gun.ready)
						{
							gun.shotTime=Time.time;
							gun.ready=false;
							// WARN
							Vector2 f = Quaternion.Euler(0,0,-angle)*gun.pos;
							GameObject bullet = (GameObject) Instantiate(Resources.Load("prefab/"+gunTemp.bulletMesh) as GameObject,new Vector3(transform.position.x+f.x,0,transform.position.z+f.y),Quaternion.Euler(0,angle,0));
							bullet.GetComponent<BulletBehaviour>().enemy=true;
							bullet.GetComponent<BulletBehaviour>().Launch(new Vector2(friendly.transform.position.x,friendly.transform.position.z),new Vector2(transform.position.x+f.x,transform.position.z+f.y),gun);	
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
			Vector2 pos = new Vector2(x-transform.position.x,y-transform.position.z);
			
			if(activeAbil==Abilities.AbilityType.halfRoundTurn)
				angle=Mathf.Repeat(angle+(180/3*Time.deltaTime*turnRotateDir),360);
			else if(activeAbil==Abilities.AbilityType.turnAround)
				angle=Mathf.Repeat(angle+(360/3*Time.deltaTime*turnRotateDir),360);
			else
			{
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
						GameObject go = (GameObject) Instantiate(Resources.Load("prefab/gasPrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						go.GetComponent<GasBehaviour>().enemy=true;
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
						GameObject go = (GameObject) Instantiate(Resources.Load("prefab/rocketPrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						go.GetComponent<RocketBehaviour>().enemy=true;
						rocketSpawned=true;
					}
				}
				if(activeAbil==Abilities.AbilityType.homingThorpede && !thorpedeSpawned)
				{
					if(t>=1.0/3.0)
					{
						GameObject go = (GameObject) Instantiate(Resources.Load("prefab/thorpedePrefab") as GameObject,transform.position,Quaternion.Euler(0,angle,0));
						go.GetComponent<ThorpedeBehaviour>().enemy=true;
						thorpedeSpawned=true;
					}
				}
			}
			transform.position=new Vector3(x,0,y);
		}
		else
		{
			GameStorage.getInstance().StepStop();
		}
	}
	
	public void Attacked(GameObject attacker, int damage, Defects.Defect defect)
	{
		if(activeAbil!=Abilities.AbilityType.shield)
		{
			if(curDefect==null && defect!=null)
			{
				curDefect=defect;
				earnedDefect=true;
			}
			this.hp-=damage;
			if(this.hp<=0)
				this.Die();
		}
	}
	
	// Use this for initialization
	void Start () {
		GameStorage.getInstance().addEnemyShuttle(this.gameObject);
		temp = Templates.getInstance().getPlaneTemplate(Template);
		GameObject gD;
		int gunsl=0,gunsr=0;
		foreach(Templates.GunOnShuttle goss in temp.guns)
		{
			if(goss.pos.x<0) gunsl++;
			if(goss.pos.x>0) gunsr++;
			gD=(GameObject) Instantiate(Resources.Load("prefab/testGunMesh") as GameObject,new Vector3(transform.position.x+goss.pos.x,1,transform.position.z+goss.pos.y),Quaternion.Euler(0,goss.turnAngle,0));
			gD.SetActive(false);
			shuttleGunsMeshes.Add(gD);
			shuttleGunsGos.Add(goss);
		}
		if(gunsr>gunsl) preferencedDirection=1;
		if(gunsl>gunsr) preferencedDirection=-1;
		hp=temp.hp;
		calculateMovePosition();
	}
	
	// Update is called once per frame
	void Update () 
	{
		for(int i = 0;i<shuttleGunsMeshes.Count;i++)
		{
			GameObject goss = (GameObject) shuttleGunsMeshes[i];
			Vector2 f = Quaternion.Euler(0,0,-angle)*((Templates.GunOnShuttle)shuttleGunsGos[i]).pos;
			goss.transform.position=new Vector3(f.x+transform.position.x,10,f.y+transform.position.z);
			goss.transform.eulerAngles=new Vector3(0,angle+((Templates.GunOnShuttle)shuttleGunsGos[i]).turnAngle,0);
			goss.SetActive(GameStorage.getInstance().isDebug);
		}
		
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
		
		if(isMouseOver(gameObject))
			focused=true;
		else
			focused=false;
		
		if(GameStorage.getInstance().isRunning)
			Accelerate();
		
		transform.eulerAngles=new Vector3(0,angle,0);
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
		GameStorage.getInstance().removeEnemyShuttle(this.gameObject);
	}
	
	public void ByeBye()
	{
		foreach(GameObject g in shuttleGunsMeshes)
			Destroy(g);
		Destroy(this.gameObject);
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
	}
	
	private float getAttackIconAngle()
	{
		
		Vector2 v1 = new Vector2(0,5);
		Vector2 v2 = new Vector2(movePoint.x-transform.position.x,movePoint.y-transform.position.z);
		float mySinPhi = (v1.x*v2.y - v1.y*v2.x);
		float mangle = Vector2.Angle(v1,v2);
		if(mySinPhi<=0)
			return mangle;
		else
			return (180-mangle)+180;
	}
	
	private void calculateMovePosition()
	{
		GameObject target = GameStorage.getInstance().getNearbyTarget(gameObject);
		if(target!=null)
		{
			float p = UnityEngine.Random.Range(0,100);
			GameObject ast = GameStorage.getInstance().getNearestAsteroid(gameObject);
			if(p>33 && ast!=null)
			{	
				if(Vector2.Distance(new Vector2(ast.GetComponent<Collider>().ClosestPointOnBounds(transform.position).x,ast.GetComponent<Collider>().ClosestPointOnBounds(transform.position).z),new Vector2(transform.position.x,transform.position.z))<= temp.maxRange+1)
				{
					Vector2 chVec;
					Vector2 leftVec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+temp.maxTurnAngle,360f))*new Vector2(0,UnityEngine.Random.Range(temp.minRange,temp.maxRange));
					leftVec = new Vector2(transform.position.x+leftVec.x,transform.position.z+leftVec.y);
					Vector2 rightVec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-temp.maxTurnAngle,360f))*new Vector2(0,UnityEngine.Random.Range(temp.minRange,temp.maxRange));
					rightVec = new Vector2(transform.position.x+rightVec.x,transform.position.z+rightVec.y);
					Vector2 astPos = new Vector2(ast.transform.position.x,ast.transform.position.z);
					float d1,d2;
					d1=Vector2.Distance(leftVec,astPos);
					d2=Vector2.Distance(rightVec,astPos);
					
					if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnRight))
						chVec=leftVec;
					else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnLeft))
						chVec=rightVec;
					else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurn))
					{
						chVec=Quaternion.Euler(0,0,-angle)*new Vector2(0,UnityEngine.Random.Range(temp.minRange,temp.maxRange));
						chVec=new Vector2(chVec.x+transform.position.x,chVec.y+transform.position.z);
					}
					else if(earnedDefect && curDefect.GetType() == typeof(Defects.EngineCrash))
					{
						if(d1>=d2)
						{
							chVec=leftVec;
							chVec=new Vector2(chVec.x-transform.position.x,chVec.y-transform.position.z);
							chVec=chVec/chVec.magnitude*0.7f*temp.minRange;
							chVec=new Vector2(chVec.x+transform.position.x,chVec.y+transform.position.z);
						}
						else
						{
							chVec=rightVec;
							chVec=new Vector2(chVec.x-transform.position.x,chVec.y-transform.position.z);
							chVec=chVec/chVec.magnitude*0.7f*temp.minRange;
							chVec=new Vector2(chVec.x+transform.position.x,chVec.y+transform.position.z);
						}
					}
					else
					{
						if(d1>=d2)
							chVec=leftVec;
						else
							chVec=rightVec;
					}
					
					movePoint=chVec;
				}
				else
				{
					Vector2 firstVec = new Vector2(target.transform.position.x-transform.position.x,target.transform.position.z-transform.position.z);
			
					Vector2 v1 = new Vector2(0,5);
					float mySinPhi = (v1.x*firstVec.y - v1.y*firstVec.x);
					float mangle = Vector2.Angle(v1,firstVec);
					if(mySinPhi>=0)
						mangle=(180-mangle)+180;
					
					float between = GameStorage.getInstance().getAngleDst(getAngle(),mangle);
					float nnewAngle;
					
					if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnRight))
						nnewAngle=Mathf.Repeat(getAngle()-UnityEngine.Random.Range(temp.maxTurnAngle/2,temp.maxTurnAngle),360);
					else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnLeft))
						nnewAngle=Mathf.Repeat(getAngle()+UnityEngine.Random.Range(temp.maxTurnAngle/2,temp.maxTurnAngle),360);
					else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurn) || activeAbil==Abilities.AbilityType.halfRoundTurn || activeAbil==Abilities.AbilityType.turnAround)
						nnewAngle=getAngle();
					else
					{
						if(Mathf.Abs(between)>temp.maxTurnAngle)
						{
							if(preferencedDirection==-1)
							{
								nnewAngle=Mathf.Repeat(getAngle()-temp.maxTurnAngle,360);
							}
							else if(preferencedDirection==1)
							{
								nnewAngle=Mathf.Repeat(getAngle()+temp.maxTurnAngle,360);
							}
							else
							{
								if(between>0)
									nnewAngle=Mathf.Repeat(getAngle()-temp.maxTurnAngle,360);
								else
									nnewAngle=Mathf.Repeat(getAngle()+temp.maxTurnAngle,360);
							}
						}
						else
						{
							nnewAngle=Mathf.Repeat(getAngle()-between,360);
						}
					}
					
					movePoint=new Vector2(0,0);
					
					if(earnedDefect && curDefect.GetType() == typeof(Defects.EngineCrash))
						movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*temp.minRange*((Defects.EngineCrash)curDefect).newRangeCoeff;
					else if(activeAbil==Abilities.AbilityType.doubleThrottle)
						movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*UnityEngine.Random.Range(temp.minRange,temp.maxRange)*2;
					else if(activeAbil==Abilities.AbilityType.halfRoundTurn)
						movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*temp.minRange*0.7f;
					else if(activeAbil==Abilities.AbilityType.turnAround)
						movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*((temp.minRange+temp.maxRange)/2.0f);
					else
						movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*UnityEngine.Random.Range(temp.minRange,temp.maxRange);
					
					movePoint=new Vector2(movePoint.x+transform.position.x,movePoint.y+transform.position.z);
					
					
					if(viewGO!=null)
						viewGO.transform.position=new Vector3(movePoint.x,0,movePoint.y);
				}
			}
			else
			{
				Vector2 firstVec = new Vector2(target.transform.position.x-transform.position.x,target.transform.position.z-transform.position.z);
				Vector2 v1 = new Vector2(0,5);
				float mySinPhi = (v1.x*firstVec.y - v1.y*firstVec.x);
				float mangle = Vector2.Angle(v1,firstVec);
				if(mySinPhi>=0)
					mangle=(180-mangle)+180;
				
				float between = GameStorage.getInstance().getAngleDst(getAngle(),mangle);
				float nnewAngle;
				
				if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnRight))
					nnewAngle=Mathf.Repeat(getAngle()-UnityEngine.Random.Range(temp.maxTurnAngle/2,temp.maxTurnAngle),360);
				else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurnLeft))
					nnewAngle=Mathf.Repeat(getAngle()+UnityEngine.Random.Range(temp.maxTurnAngle/2,temp.maxTurnAngle),360);
				else if(earnedDefect && curDefect.GetType() == typeof(Defects.DisableTurn) || activeAbil==Abilities.AbilityType.halfRoundTurn || activeAbil==Abilities.AbilityType.turnAround)
					nnewAngle=getAngle();
				else
				{
					if(Mathf.Abs(between)>temp.maxTurnAngle)
					{
						if(preferencedDirection==-1)
						{
							nnewAngle=Mathf.Repeat(getAngle()-temp.maxTurnAngle,360);
						}
						else if(preferencedDirection==1)
						{
							nnewAngle=Mathf.Repeat(getAngle()+temp.maxTurnAngle,360);
						}
						else
						{
							if(between>0)
								nnewAngle=Mathf.Repeat(getAngle()-temp.maxTurnAngle,360);
							else
								nnewAngle=Mathf.Repeat(getAngle()+temp.maxTurnAngle,360);
						}
					}
					else
					{
						nnewAngle=Mathf.Repeat(getAngle()-between,360);
					}
				}
				
				movePoint=new Vector2(0,0);
				
				if(earnedDefect && curDefect.GetType() == typeof(Defects.EngineCrash))
					movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*temp.minRange*((Defects.EngineCrash)curDefect).newRangeCoeff;
				else if(activeAbil==Abilities.AbilityType.doubleThrottle)
					movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*UnityEngine.Random.Range(temp.minRange,temp.maxRange)*2;
				else if(activeAbil==Abilities.AbilityType.halfRoundTurn)
					movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*temp.minRange*0.7f;
				else if(activeAbil==Abilities.AbilityType.turnAround)
					movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*((temp.minRange+temp.maxRange)/2.0f);
				else
					movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*UnityEngine.Random.Range(temp.minRange,temp.maxRange);
				
				movePoint=new Vector2(movePoint.x+transform.position.x,movePoint.y+transform.position.z);
				
				
				if(viewGO!=null)
					viewGO.transform.position=new Vector3(movePoint.x,0,movePoint.y);
			}
			point1=new Vector2(transform.position.x,transform.position.z);
			Vector2 vvec = Quaternion.Euler(0,0,-angle)*new Vector2(0,1);
			point2=Quaternion.Euler(0,0,-angle)*new Vector2(0,temp.minRange*Mathf.Abs(GameStorage.getInstance().getAngleDst(angle,getAttackIconAngle())/temp.maxTurnAngle));
			point2+=point1;
			point4=new Vector2(movePoint.x,movePoint.y);
			Vector2 pointz = new Vector2(point4.x-point2.x,point4.y-point2.y)/2;
			point3 = new Vector2(pointz.y,-pointz.x)*GameStorage.getInstance().getAngleDst(angle,getAttackIconAngle())/temp.maxTurnAngle;
			point3 = point3+point2+pointz;
		}
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
}
