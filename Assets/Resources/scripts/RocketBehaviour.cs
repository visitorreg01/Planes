using UnityEngine;
using System.Collections;

public class RocketBehaviour : MonoBehaviour {

	public bool enemy=false;
	private int stepCount=0;
	GameObject attackIcon;
	bool attackIconCaptured=false;
	
	ArrayList trackDots = new ArrayList();
	Vector2 point1,point2,point3,point4;
	public float angle=0;
	Vector2 startpoint1,startpoint2;
	float dx,dy,dt;
	private float t=0;
	
	bool spawned=true;
	
	bool selected=false;
	
	void Start () {
		GameStorage.getInstance().registerRocketUnit(this.gameObject);
		attackIcon = Instantiate(Resources.Load("prefab/attackIcon") as GameObject);
		attackIcon.SetActive(false);
		startpoint1=new Vector2(transform.position.x,transform.position.z);
		angle=transform.eulerAngles.y;
		startpoint2=Quaternion.Euler(0,0,angle)*new Vector2(0,1)*Abilities.RocketParameters.startRange;
		startpoint2=new Vector2(startpoint2.x+transform.position.x,startpoint2.y+transform.position.z);
		dx=-(startpoint2.x-startpoint1.x)/3.0f*Time.deltaTime;
		dy=(startpoint2.y-startpoint1.y)/3.0f*Time.deltaTime;
		LineRenderer lr = gameObject.AddComponent<LineRenderer>();
		lr.SetWidth(0.05f, 0.05f);
	}
	
	private void updateAttackIconPosition()
	{
		float ds = (Abilities.RocketParameters.maxRange-Abilities.RocketParameters.minRange)/2+Abilities.RocketParameters.minRange;
		Vector2 vec =Quaternion.Euler(0,0,-angle)*(new Vector2(0,1)*ds);
		attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
	}
	
	// Update is called once per frame
	void Update () 
	{
		angle=Mathf.Repeat(angle,360);
		if(spawned && GameStorage.getInstance().isRunning)
			transform.position=new Vector3(transform.position.x+dx,0,transform.position.z+dy);
		
		CalculatePath();
		if(!spawned && !GameStorage.getInstance().isRunning)
		{
			if(!enemy)
			{
				checkShuttleClickState();
				checkAttackIconClickState();
				if(selected)
					attackIcon.SetActive(true);
				else
					attackIcon.SetActive(false);
				
				if(attackIconCaptured)
					dragAttackIcon();
				
				DrawLine();
			}
		}
		else
			clearLine();
		if(!spawned)
			Accelerate();
		
		transform.eulerAngles=new Vector3(0,angle,0);
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
	
	private void dragAttackIcon()
	{
		Vector2 mouse = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).z);
		float dst = new Vector2(mouse.x-transform.position.x,mouse.y-transform.position.z).magnitude;
		Vector2 vec;
		float ds = getAngleDst(angle,getAttackAngle());
		
		if(dst>=Abilities.RocketParameters.maxRange)
		{
			if(Mathf.Abs(ds)>=Abilities.RocketParameters.maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+Abilities.RocketParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.RocketParameters.maxRange;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-Abilities.RocketParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.RocketParameters.maxRange;
			}
			else
				vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*Abilities.RocketParameters.maxRange;
			attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
		}
		else if(dst<=Abilities.RocketParameters.minRange)
		{
			if(Mathf.Abs(ds)>=Abilities.RocketParameters.maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+Abilities.RocketParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.RocketParameters.minRange;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-Abilities.RocketParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.RocketParameters.minRange;
			}
			else
				vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*Abilities.RocketParameters.minRange;
			attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
		}
		else
		{
			if(Mathf.Abs(ds)>=Abilities.RocketParameters.maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+Abilities.RocketParameters.maxTurnAngle,360))*new Vector2(0,1)*dst;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-Abilities.RocketParameters.maxTurnAngle,360))*new Vector2(0,1)*dst;
				attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
			}
			else
			{
				vec = new Vector2(mouse.x,mouse.y);
				attackIcon.transform.position=new Vector3(vec.x,0,vec.y);
			}
		}
	}
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			float x,y;
			t+=1*Time.deltaTime/3;
			
			x = Mathf.Pow((1-t),3)*point1.x+3*(1-t)*(1-t)*t*point2.x+3*(1-t)*t*t*point3.x+t*t*t*point4.x;
			y = Mathf.Pow((1-t),3)*point1.y+3*(1-t)*(1-t)*t*point2.y+3*(1-t)*t*t*point3.y+t*t*t*point4.y;
			
			Vector2 pos = new Vector2(x-transform.position.x,y-transform.position.z);
			float nAngle;
			Vector2 v1 = new Vector2(0,5);
			float mySinPhi = (v1.x*pos.y - v1.y*pos.x);
			nAngle = Vector2.Angle(v1,pos);
			if(mySinPhi>0)
				nAngle=(180-nAngle)+180;
			angle=nAngle;
			
			transform.position=new Vector3(x,0,y);
			
			if(!enemy)
			{
				GameObject target = GameStorage.getInstance().getNearbyEnemy(this.gameObject);
				if(GetComponent<Renderer>().bounds.Intersects(target.GetComponent<Renderer>().bounds))
				{
					target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.RocketParameters.damage,null);
					GameStorage.getInstance().removeRocketUnit(this.gameObject);
					this.Die();
				}
			}
			
			if(!enemy)
				updateAttackIconPosition();
		}
	}
	
	private void checkAttackIconClickState()
	{	
		if(Input.GetMouseButtonDown(0) && isMouseOver(attackIcon))
			attackIconCaptured=true;
		if(Input.GetMouseButtonUp(0))
			attackIconCaptured=false;
	}
	
	private bool isMouseOver(GameObject o)
	{
		Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return (o.GetComponent<Renderer>().bounds.IntersectRay(new Ray(new Vector3(pz.x,o.transform.position.y,pz.z),new Vector3(pz.x,o.transform.position.y,pz.z))));
	}
	
	private void clearLine()
	{
		LineRenderer lr = gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(0);
	}
	
	void checkShuttleClickState()
	{
		if((Input.GetMouseButtonDown(0) && isMouseOver(gameObject)) || (Input.GetMouseButtonDown(0) && isMouseOver(attackIcon)))
			selected=true;
		else if(Input.GetMouseButtonDown(0) && !isMouseOver(gameObject))
			selected=false;
	}
	
	public int updateStepCounter()
	{
		t=0;
		if(spawned)
		{
			spawned=false;
			updateAttackIconPosition();
		}
		return stepCount++;
	}
	
	private void CalculatePath()
	{
		if(!GameStorage.getInstance().isRunning)
		{
			point1=new Vector2(transform.position.x,transform.position.z);
			Vector2 vvec = Quaternion.Euler(0,0,-angle)*new Vector2(0,1);
			Vector3 tempVec = GetComponent<Renderer>().bounds.ClosestPoint(new Vector3(vvec.x+transform.position.x,0,vvec.y+transform.position.z));
			point2=new Vector2(tempVec.x-transform.position.x,tempVec.z-transform.position.z);
			//point2*=dd;
			point2=new Vector2(transform.position.x+point2.x,transform.position.z+point2.y);
			
			point4=new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z);
			Vector2 pointz = new Vector2(point4.x-point2.x,point4.y-point2.y)/2;
			point3 = new Vector2(pointz.y,-pointz.x)*GameStorage.getInstance().getAngleDst(angle,getAttackIconAngle())*0.02f;
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
	
	private void DrawLine()
	{
		LineRenderer lr = gameObject.GetComponent<LineRenderer>();
		lr.SetVertexCount(trackDots.Count);
		int i;
		for(i=0;i<trackDots.Count;i++)
			lr.SetPosition(i,new Vector3(((Vector2)trackDots[i]).x,1,((Vector2)trackDots[i]).y));
	}
	
	public void Die()
	{
		Destroy(this.gameObject);
	}
}
