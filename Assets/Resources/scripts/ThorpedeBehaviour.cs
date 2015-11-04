using UnityEngine;
using System.Collections;

public class ThorpedeBehaviour : MonoBehaviour {

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
		GameStorage.getInstance().registerThorpedeUnit(this.gameObject);
		attackIcon = Instantiate(Resources.Load("prefab/attackIcon") as GameObject);
		attackIcon.SetActive(false);
		startpoint1=new Vector2(transform.position.x,transform.position.z);
		angle=transform.eulerAngles.y;
		startpoint2=Quaternion.Euler(0,0,angle)*new Vector2(0,1)*Abilities.ThorpedeParameters.startRange;
		startpoint2=new Vector2(startpoint2.x+transform.position.x,startpoint2.y+transform.position.z);
		dx=-(startpoint2.x-startpoint1.x)/3.0f*Time.deltaTime;
		dy=(startpoint2.y-startpoint1.y)/3.0f*Time.deltaTime;
		LineRenderer lr = gameObject.AddComponent<LineRenderer>();
		lr.SetWidth(0.05f, 0.05f);
	}
	
	private void updateAttackIconPosition()
	{
		float ds = (Abilities.ThorpedeParameters.maxRange-Abilities.ThorpedeParameters.minRange)/2+Abilities.ThorpedeParameters.minRange;
		Vector2 vec =Quaternion.Euler(0,0,-angle)*(new Vector2(0,1)*ds);
		attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
	}
	
	// Update is called once per frame
	void Update () 
	{
		angle=Mathf.Repeat(angle,360);
		if(spawned && GameStorage.getInstance().isRunning)
			transform.position=new Vector3(transform.position.x+dx,0,transform.position.z+dy);
		
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
				CalculatePath();
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
		
		if(dst>=Abilities.ThorpedeParameters.maxRange)
		{
			if(Mathf.Abs(ds)>=Abilities.ThorpedeParameters.maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+Abilities.ThorpedeParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.ThorpedeParameters.maxRange;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-Abilities.ThorpedeParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.ThorpedeParameters.maxRange;
			}
			else
				vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*Abilities.ThorpedeParameters.maxRange;
			attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
		}
		else if(dst<=Abilities.ThorpedeParameters.minRange)
		{
			if(Mathf.Abs(ds)>=Abilities.ThorpedeParameters.maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+Abilities.ThorpedeParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.ThorpedeParameters.minRange;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-Abilities.ThorpedeParameters.maxTurnAngle,360))*new Vector2(0,1)*Abilities.ThorpedeParameters.minRange;
			}
			else
				vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*Abilities.ThorpedeParameters.minRange;
			attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
		}
		else
		{
			if(Mathf.Abs(ds)>=Abilities.ThorpedeParameters.maxTurnAngle)
			{
				if(ds<0)
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+Abilities.ThorpedeParameters.maxTurnAngle,360))*new Vector2(0,1)*dst;
				else
					vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-Abilities.ThorpedeParameters.maxTurnAngle,360))*new Vector2(0,1)*dst;
				attackIcon.transform.position=new Vector3(vec.x+transform.position.x,0,vec.y+transform.position.z);
			}
			else
			{
				vec = new Vector2(mouse.x,mouse.y);
				attackIcon.transform.position=new Vector3(vec.x,0,vec.y);
			}
		}
	}
	
	void OnCollisionEnter(Collision col)
	{
		GameObject target = col.gameObject;
		if(enemy)
		{
			if(target.GetComponent<FriendlyShuttleBehaviour>()!=null)
			{
				target.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.ThorpedeParameters.damage,null);
				this.Die();
			}
		}
		else
		{
			if(target.GetComponent<EnemyShuttleBehaviour>()!=null)
			{
				target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.ThorpedeParameters.damage,null);
				this.Die();
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
		
		if(enemy)
		{
			CalculateAttackIconPosition();
			CalculatePath();
		}
		return stepCount++;
	}
	
	private void CalculateAttackIconPosition()
	{
		GameObject target = GameStorage.getInstance().getNearbyTarget(gameObject);
		if(target!=null)
		{
			Vector2 firstVec = new Vector2(target.transform.position.x-transform.position.x,target.transform.position.z-transform.position.z);
			Vector2 movePoint;
			Vector2 accuracy=Quaternion.Euler(0,0,-target.GetComponent<FriendlyShuttleBehaviour>().angle)*new Vector2(0,1);
			Vector2 v1 = new Vector2(0,5);
			float mySinPhi = (v1.x*firstVec.y - v1.y*firstVec.x);
			float mangle = Vector2.Angle(v1,firstVec);
			if(mySinPhi>=0)
				mangle=(180-mangle)+180;
			
			float cos = (firstVec.x*v1.x+firstVec.y*v1.y)/(firstVec.magnitude*v1.magnitude);
			
			float between = GameStorage.getInstance().getAngleDst(angle,mangle);
			float nnewAngle;
			
			if(Mathf.Abs(between)>=Abilities.RocketParameters.maxTurnAngle)
			{
				if(between>0)
					nnewAngle=Mathf.Repeat(angle-Abilities.RocketParameters.maxTurnAngle,360);
				else
					nnewAngle=Mathf.Repeat(angle+Abilities.RocketParameters.maxTurnAngle,360);
			}
			else
				nnewAngle=Mathf.Repeat(angle-between,360);
			
			movePoint=new Vector2(0,0);
			
			if(cos>=0)
				movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*Abilities.RocketParameters.maxRange;
			else
				movePoint=Quaternion.Euler(0,0,-nnewAngle)*new Vector2(0,1)*Abilities.RocketParameters.minRange;
			
			movePoint=new Vector2(movePoint.x+accuracy.x+transform.position.x,movePoint.y+transform.position.z+accuracy.y);
			
			attackIcon.transform.position=new Vector3(movePoint.x,0,movePoint.y);
		}
	}
	
	private void CalculatePath()
	{
		if(!GameStorage.getInstance().isRunning)
		{
			point1=new Vector2(transform.position.x,transform.position.z);
			point2=Quaternion.Euler(0,0,-angle)*new Vector2(0,Abilities.ThorpedeParameters.minRange*Mathf.Abs(getAngleDst(angle,getAttackIconAngle())/Abilities.ThorpedeParameters.maxTurnAngle));
			point2+=point1;
			point4=new Vector2(attackIcon.transform.position.x,attackIcon.transform.position.z);
			Vector2 pointz = new Vector2(point4.x-point2.x,point4.y-point2.y)/2;
			point3 = new Vector2(pointz.y,-pointz.x)*getAngleDst(angle,getAttackIconAngle())/Abilities.ThorpedeParameters.maxTurnAngle;
			point3 = point3+point2+pointz;
			
			trackDots.Clear();
			float x,y,tt;
			float step = 0.001f;
			if(!enemy)
			{
				for(tt=0f;tt<=1;tt+=step)
				{
					x = Mathf.Pow((1-tt),3)*point1.x+3*(1-tt)*(1-tt)*tt*point2.x+3*(1-tt)*tt*tt*point3.x+tt*tt*tt*point4.x;
					y = Mathf.Pow((1-tt),3)*point1.y+3*(1-tt)*(1-tt)*tt*point2.y+3*(1-tt)*tt*tt*point3.y+tt*tt*tt*point4.y;
					trackDots.Add(new Vector2(x,y));
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
		GameStorage.getInstance().removeThorpedeUnit(this.gameObject);
		Destroy(this.gameObject);
	}
}
