using UnityEngine;
using System.Collections;

public class EnemyShuttleBehaviour : MonoBehaviour {

	public Templates.PlaneTemplates Template;
	private Templates.PlaneTemplate temp;
	
	int shuttleH = 5;
	
	float angle=0;
	int hp;
	
	float newAngle;
	Vector2 newPos;
	private Defects.Defect curDefect = null;
	bool earnedDefect = false;
	bool defectInUse = false;
	
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
		return angle;
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
			defectInUse=false;
			earnedDefect=false;
			curDefect=null;
		}
	}
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
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
						
						friendly.GetComponent<FriendlyShuttleBehaviour>().Attacked(gameObject,gunTemp.damage, Defects.getDefect(defect));
					}
				}
			}
			
//			float x,y;
//			t+=1*Time.deltaTime/3;
//			
//			x = Mathf.Pow(1-t,2)*point1.x+2*(1-t)*t*point2.x+t*t*point3.x;
//			y = Mathf.Pow(1-t,2)*point1.y+2*(1-t)*t*point2.y+t*t*point3.y;
//			Vector2 pos = new Vector2(x-transform.position.x,y-transform.position.z);
//			
//			float nAngle;
//			Vector2 v1 = new Vector2(0,5);
//			float mySinPhi = (v1.x*pos.y - v1.y*pos.x);
//			nAngle = Vector2.Angle(v1,pos);
//			if(mySinPhi>0)
//				nAngle=(180-nAngle)+180;
//			angle=nAngle;
//			transform.position=new Vector3(x,shuttleH,y);
		}
		else
		{
			GameStorage.getInstance().StepStop();
		}
	}
	
	public void Attacked(GameObject attacker, int damage, Defects.Defect defect)
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
	
	// Use this for initialization
	void Start () {
		GameStorage.getInstance().addEnemyShuttle(this.gameObject);
		temp = Templates.getInstance().getPlaneTemplate(Template);
		hp=temp.hp;
	}
	
	// Update is called once per frame
	void Update () 
	{
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
		
		transform.eulerAngles=new Vector3(0,angle-90,0);
	}
	
	public void Die()
	{
		GameStorage.getInstance().removeEnemyShuttle(this.gameObject);
	}
	
	public void ByeBye()
	{
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
	
	private bool isMouseOver(GameObject o)
	{
		Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return (o.GetComponent<Renderer>().bounds.IntersectRay(new Ray(new Vector3(pz.x,o.transform.position.y,pz.z),new Vector3(pz.x,o.transform.position.y,pz.z))));
	}
}
