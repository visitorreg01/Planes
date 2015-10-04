using UnityEngine;
using System.Collections;
using System;

public class FriendlyShuttleBehaviour : MonoBehaviour {

	//scale
	const float scale = 10;
	
	public Templates.PlaneTemplates Template;
	private Templates.PlaneTemplate temp;
	
	//const
	float shuttleH=5;
	float attackIconH=6;
	float attackIconDistMin=2f;
	float attackIconDist=6f;
	float maxTurnAngle = 55;
	int hp = 300;
	float lastFired=0f;
	
	
	
	//operations
	bool needToUpdatePosition = false;
	Vector3 newPos;
	bool needToUpdateAngle = false;
	float newAngle;
	bool needToUpdateAttackIconPosition = false;
	
	//Path
	Vector2 point1,point2,point3;
	float d;
	float t;
	
	//mechanics
	float angle=0;
	Vector2 directionalVector;
	float sinPhi;
	Vector2 attackVec;
	ArrayList trackDots = new ArrayList();
	
	//states
	bool focused = false;
	bool selected = false;
	bool attackIconCaptured = false;
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
		
		attackIconDistMin = temp.minRange;
		attackIconDist = temp.maxRange;
		maxTurnAngle = temp.maxTurnAngle;
		hp=temp.hp;
		
		Debug.Log("Guns: "+temp.guns.Count);
	}
	
	void Update()
	{
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
		
		transform.eulerAngles=new Vector3(0,angle-90,0);
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
	
	private void clearLine()
	{
		LineRenderer lr = gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(0);
	}
	
	public void checkShuttleClickState()
	{
		if((Input.GetMouseButtonDown(0) && isMouseOver(gameObject)) || (Input.GetMouseButtonDown(0) && isMouseOver(attackIcon)))
			selected=true;
		else if(Input.GetMouseButtonDown(0) && !isMouseOver(gameObject))
			selected=false;
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
	
	public void Attacked(GameObject attacker, int damage, Defects.Defect defect)
	{
		Debug.Log("Get damage: "+damage);
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
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			if(!(defectInUse && curDefect.GetType()==typeof(Defects.DisableGuns)))
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
							
							int defect=-1,i;
							float ch = UnityEngine.Random.Range(0.0f,100f);
							float lower=0.0f,upper;
							for(i=0;i<gunTemp.defectsChance.Length;i++)
							{
								upper=gunTemp.defectsChance[i]+lower;
								if(ch>=lower && ch<=upper)
								{
									defect=i;
									break;
								}
								else
									lower=upper;
							}
							
							enemy.GetComponent<EnemyShuttleBehaviour>().Attacked(gameObject,gunTemp.damage, Defects.getDefect(defect));
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
			
			x = Mathf.Pow(1-t,2)*point1.x+2*(1-t)*t*point2.x+t*t*point3.x;
			y = Mathf.Pow(1-t,2)*point1.y+2*(1-t)*t*point2.y+t*t*point3.y;
			Vector2 pos = new Vector2(x-transform.position.x,y-transform.position.z);
			
			float nAngle;
			Vector2 v1 = new Vector2(0,5);
			float mySinPhi = (v1.x*pos.y - v1.y*pos.x);
			nAngle = Vector2.Angle(v1,pos);
			if(mySinPhi>0)
				nAngle=(180-nAngle)+180;
			angle=nAngle;
			transform.position=new Vector3(x,shuttleH,y);
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
		
		Vector2 v1 = Quaternion.Euler(0,0,angle)*new Vector2(0,5);
		Vector2 v2 = Quaternion.Euler(0,0,getAttackAngle())*new Vector2(0,5);
		
		float cos = (Vector2.Dot(v1,v2)/(v1.magnitude*v2.magnitude));
		
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
			attackIconCaptured=true;
		if(Input.GetMouseButtonUp(0))
			attackIconCaptured=false;
	}
	
	public void StepStart()
	{
		if(earnedDefect)
			defectInUse=true;
	}
	
	public void StepEnd()
	{
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
		Destroy(this.gameObject);
	}
	
	private bool isMouseOver(GameObject o)
	{
		Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return (o.GetComponent<Renderer>().bounds.IntersectRay(new Ray(new Vector3(pz.x,o.transform.position.y,pz.z),new Vector3(pz.x,o.transform.position.y,pz.z))));
	}
	
	private void CalculatePath()
	{
		if(!GameStorage.getInstance().isRunning)
		{
			point1 = new Vector2(transform.position.x,transform.position.z);
			point3 = new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z);
			d = new Vector2(point3.x-point1.x,point3.y-point1.y).magnitude/2;
			point2 = (Quaternion.Euler(0,0,-angle)*new Vector2(0,1)*d);
			point2=new Vector2(point2.x+point1.x,point2.y+point1.y);
			trackDots.Clear();
			float x,y,tt;
			float step = 0.005f;
			for(tt=0f;tt<=1;tt+=step)
			{
				x = Mathf.Pow(1-tt,2)*point1.x+2*(1-tt)*tt*point2.x+tt*tt*point3.x;
				y = Mathf.Pow(1-tt,2)*point1.y+2*(1-tt)*tt*point2.y+tt*tt*point3.y;
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