using UnityEngine;
using System.Collections;
using System;

public class FriendlyShuttleBehaviour : MonoBehaviour {

	//scale
	const float scale = 10;
	
	
	//const
	const float shuttleH=5;
	const float attackIconH=6;
	const float attackIconDist=6f;
	const float maxTurnAngle = 55;
	
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
		return gameObject.transform.eulerAngles.y;
	}
	
	private void updateAttackIconPosition()
	{
		Vector2 vec =Quaternion.Euler(0,0,-angle)*(new Vector2(0,1)*attackIconDist);
		attackIcon.transform.position=new Vector3(vec.x+transform.position.x,6,vec.y+transform.position.z);
	}
	
	void Start()
	{
		attackIcon = Instantiate(Resources.Load("prefab/attackIcon") as GameObject);
		attackIcon.SetActive(false);
		LineRenderer lr = gameObject.AddComponent<LineRenderer>();
		lr.SetWidth(0.025f, 0.025f);
	}
	
	void Update()
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
		if(selected)
		{
			if(!GameStorage.getInstance().isRunning)
			{
				t=0;
				attackIcon.SetActive(true);
				DrawLine();
			}
			else
			{
				attackIcon.SetActive(false);
				clearLine();
			}
		}
		else
		{
			attackIcon.SetActive(false);
			clearLine();
		}
		
		if(GameStorage.getInstance().isRunning)
			Accelerate();
		
		if(attackIconCaptured)
			dragAttackIcon();
		
		transform.eulerAngles=new Vector3(0,angle-90,0);
	}
	
	void OnGUI()
	{
		
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
	
	private void Accelerate()
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			//Debug.Log(t);
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
			
			
		}
		else
		{
			updateAttackPosition();
			t=0;
			GameStorage.getInstance().isRunning=false;
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
		
			if(dst>=attackIconDist)
			{
				if(Mathf.Abs(ds)>=maxTurnAngle)
				{
					if(ds<0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+55,360))*new Vector2(0,1)*attackIconDist;
					else
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-55,360))*new Vector2(0,1)*attackIconDist;
				}
				else
					vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*attackIconDist;
				attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
			}
			else if(dst<=2)
			{
				if(Mathf.Abs(ds)>=maxTurnAngle)
				{
					if(ds<0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+55,360))*new Vector2(0,1)*2;
					else
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-55,360))*new Vector2(0,1)*2;
				}
				else
					vec = Quaternion.Euler(0,0,-getAttackAngle())*new Vector2(0,1)*2;
				attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
			}
			else
			{
				if(Mathf.Abs(ds)>=maxTurnAngle)
				{
					if(ds<0)
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle+55,360))*new Vector2(0,1)*dst;
					else
						vec = Quaternion.Euler(0,0,-Mathf.Repeat(angle-55,360))*new Vector2(0,1)*dst;
					attackIcon.transform.position=new Vector3(vec.x+transform.position.x,attackIconH,vec.y+transform.position.z);
				}
				else
				{
					vec = new Vector2(mouse.x,mouse.y);
					attackIcon.transform.position=new Vector3(vec.x,attackIconH,vec.y);
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
	
	public void ByeBye()
	{
		Destroy(this);
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
/*
	float attackIconH=6;
	float shuttleH = 5;
	
	GameObject pla;
	float angle=0;
	Vector3 direction;
	
	Sprite normalS,focusedS,selectedS;
	Sprite normalA,focusedA,selectedA;
	
	bool focused = false;
	bool attackIconFocused = false;
	bool selected = false;
	float prevAttackAngle = 0;
	bool showInfo = false;
	Vector3 infoPos;
	Vector2 rememberedCenter;
	Vector2 trackLast;
	Vector2 trackFirst;
	float rememberedRadius;
	
	bool attackIconCaptured = false;
	
	GameObject attackIcon;
	
	Vector3 pathPos;
	bool attackIconCaptureLost = false;
	Vector3 pz;
	
	float distance = 2.5f;
	float prevAttackDistance;
	
	int selection_mode=0;
	
	float acceleration=0;
	
	bool needUpdateAngle = false;
	bool needUpdatePosition = false;
	Vector2 posToUpdate;
	bool rememberTrack = false;
	
	float posX,posY;
	float width,height;
	
	public void RememberTrack()
	{
		rememberTrack=true;
	}
	
	public void setAngle(float angle)
	{
		this.angle=angle;
		
	}
	
	public void setPosition(Vector2 pos)
	{
		posToUpdate = pos;
		needUpdatePosition=true;
	}
	
	void Start () {
		prevAttackDistance=distance;
		attackIcon = (GameObject) GameObject.Instantiate(Resources.Load("prefab/attackIcon") as GameObject,new Vector3(0,attackIconH,0),Quaternion.identity);
		attackIcon.SetActive(false);
		pla = gameObject;
		//angle=0;
		normalS = Resources.Load<Sprite>("missle/shuttle");
		focusedS = Resources.Load<Sprite>("missle/shuttle_blur");
		selectedS = Resources.Load<Sprite>("missle/shuttle_selected");
		normalA = Resources.Load<Sprite>("attack_button/attack");
		focusedA = Resources.Load<Sprite>("attack_button/attack_blur");
		selectedA = Resources.Load<Sprite>("attack_button/attack_clicked");
		LineRenderer lr = pla.AddComponent<LineRenderer>();
		lr.SetWidth(0.025f, 0.025f);
		lr.SetVertexCount(2); 
		pathPos = new Vector2(pla.transform.position.x,pla.transform.position.y)+(new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad))*distance);
		attackIcon.transform.position=new Vector3(pathPos.x,pathPos.y,0);
	}
	
	
	void Update()
	{
		if(isMouseOver(attackIcon))
			attackIconFocused=true;
		else
		{
			attackIconFocused=false;
		}
		
		if(isMouseOver(pla))
			focused=true;
		else
			focused=false;
		
		if(Input.GetMouseButtonDown(0))
		{
			if(isMouseOver(attackIcon))
			{
				attackIconCaptured=true;
				//attackIcon.GetComponent<SpriteRenderer>().sprite=selectedA;
			}
			else
				attackIconCaptured=false;
		}
		
		if(Input.GetMouseButtonUp(0))
		{
			if(attackIconCaptured)
			{
				attackIconCaptured=!attackIconCaptured;
			}
			else
			{
				if(isMouseOver(pla) || isMouseOver(attackIcon))
					selected=true;
				else
				{
					
						selected=false;
				
				}
			}
			
			if(isMouseOver(attackIcon))
			{
				//attackIcon.GetComponent<SpriteRenderer>().sprite=normalA;
				attackIconCaptured=false;
			}
		}
		
		
		
		Mathf.Repeat(angle,360);
		Accelerate();
		
		//pla.GetComponent<SpriteRenderer>().sprite=normalS;
		if(focused) 
		{
			//pla.GetComponent<SpriteRenderer>().sprite=focusedS;
			infoPos = Camera.main.WorldToScreenPoint(pla.gameObject.transform.position);
			infoPos = new Vector3(infoPos.x,Screen.height-infoPos.y);
			showInfo=true;
		}
		else
			showInfo=false;
		
		
		Vector2 v1 = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad));
		Vector2 v2 = new Vector2(attackIcon.transform.position.x-pla.transform.position.x,attackIcon.transform.position.z-pla.transform.position.z);
	
		float sinPhi = (v1.x*v2.y - v1.y*v2.x); //---------------------------------------------- IMPORTANT
		if(selected)
		{
			//pla.GetComponent<SpriteRenderer>().sprite=selectedS;
			attackIcon.SetActive(true);
			if(attackIconCaptured)
			{
				
				if(Vector2.Distance(new Vector2(pla.transform.position.x,pla.transform.position.z),Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 2)
				{
					if(Vector2.Angle(v1,Camera.main.ScreenToWorldPoint(Input.mousePosition)-pla.transform.position)<=50)
					{
						Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition)-pla.transform.position;
						float len = Mathf.Sqrt(v.x*v.x+v.y*v.y);
						v=v/len;
						v=v*2;
						attackIcon.transform.position=new Vector3(v.x+pla.transform.position.x,v.y+pla.transform.position.y,0);
					}
				}
				else if(Vector2.Distance(pla.transform.position,Camera.main.ScreenToWorldPoint(Input.mousePosition)) > distance)
				{
					if(Vector2.Angle(v1,Camera.main.ScreenToWorldPoint(Input.mousePosition)-pla.transform.position)<=50)
					{
						Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition)-pla.transform.position;
						float len = Mathf.Sqrt(v.x*v.x+v.y*v.y);
						v=v/len;
						v=v*distance;
						attackIcon.transform.position=new Vector3(v.x+pla.transform.position.x,v.y+pla.transform.position.y,0);
					}
				}
				else
				{
					Debug.Log(Vector2.Angle(v1,Camera.main.ScreenToWorldPoint(Input.mousePosition)-pla.transform.position));
					Debug.Log(sinPhi);
					if(Vector2.Angle(v1,Camera.main.ScreenToWorldPoint(Input.mousePosition)-pla.transform.position)<=50)
					{
						Vector3 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
						attackIcon.transform.position=new Vector3(v.x,v.y,0);
					}
				}
			}
			DrawPath(pla,attackIcon,false);
			
			
		}
		else
		{
			attackIcon.SetActive(false);
			
			DrawPath(null,null,false);
		}
//		if(!attackIconCaptured)
//		{
//			Vector2 newVec;
//			if(sinPhi>0)
//				newVec = new Vector2(v1.x*Mathf.Cos(prevAttackAngle*Mathf.Deg2Rad)+v1.y*Mathf.Sin(prevAttackAngle*Mathf.Deg2Rad),v1.x*Mathf.Sin(prevAttackAngle*Mathf.Deg2Rad)+v1.y*Mathf.Cos(prevAttackAngle*Mathf.Deg2Rad));
//			else
//				newVec = new Vector2(v1.x*Mathf.Cos(prevAttackAngle*Mathf.Deg2Rad)-v1.y*Mathf.Sin(prevAttackAngle*Mathf.Deg2Rad),-v1.x*Mathf.Sin(prevAttackAngle*Mathf.Deg2Rad)+v1.y*Mathf.Cos(prevAttackAngle*Mathf.Deg2Rad));
//			
//			newVec*=prevAttackDistance+;
//			
//			attackIcon.transform.position=newVec;
//		}
		
		
	}
	
	void OnGUI()
	{
		if(showInfo)
		{
			GUI.Box(new Rect(infoPos.x,infoPos.y,80,60),"Info");
		}
	}
	
	private bool isMouseOver(GameObject o)
	{
		pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return (o.GetComponent<Renderer>().bounds.IntersectRay(new Ray(new Vector3(pz.x,pz.y,o.transform.position.z),new Vector3(pz.x,pz.y,o.transform.position.z))));
	}
	
	private void DrawPath(GameObject fr, GameObject to, bool clear)
	{
		if(clear)
		{
			LineRenderer lr = pla.GetComponent<LineRenderer>();
			lr.SetVertexCount(0);
		}
		else
		{
			Vector2 v1 = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad));
			Vector2 v2 = attackIcon.transform.position-pla.transform.position;
			float sinPhi = (v1.x*v2.y - v1.y*v2.x);
			float fangle = Vector2.Angle(v1,attackIcon.transform.position-pla.transform.position);
			Vector2 newVec = Vector2.zero;
			
			float step=0.1f;
			float thisSin = Mathf.Sin(fangle*Mathf.Deg2Rad);
			if(thisSin==0)
			{
				LineRenderer lr = pla.GetComponent<LineRenderer>();
				lr.SetVertexCount(2);
				lr.SetPosition(0,new Vector3(pla.transform.position.x,pla.transform.position.y,-1));
				lr.SetPosition(1,new Vector3(attackIcon.transform.position.x,attackIcon.transform.position.y,-1));
				float xFrom,xTo;
				float pos;
				if(rememberTrack)
				{
					if(attackIcon.transform.position.x<=pla.transform.position.x)
					{
						xFrom=attackIcon.transform.position.x;
						xTo=pla.transform.position.x;
					}
					else
					{
						xFrom=pla.transform.position.x;
						xTo=attackIcon.transform.position.x;
					}
					bool first = false;
					for(pos=xFrom;pos<xTo;pos+=step)
					{
						if(first)
						{
							trackFirst=new Vector2(pos,pla.transform.position.x);
							first=false;
						}
					}
					rememberedCenter=newVec;
					trackLast=new Vector2(pos-step,pla.transform.position.x);
					rememberTrack=false;
				}
			}
			else
			{
				if(sinPhi>0)
					newVec = -new Vector2(v2.y,-v2.x)*(v2.magnitude/2)/thisSin;
				else
					newVec = new Vector2(v2.y,-v2.x)*(v2.magnitude/2)/thisSin;
				
				Vector2 halfVector = new Vector2(attackIcon.transform.position.x-pla.transform.position.x,attackIcon.transform.position.y-pla.transform.position.y)/2;
				newVec=new Vector2(newVec.x+pla.transform.position.x,newVec.y+pla.transform.position.y)+halfVector; // center
				
				float radius = Vector2.Distance(newVec,pla.transform.position);
				float xFrom,xTo;
				
				if(attackIcon.transform.position.x<=pla.transform.position.x)
				{
					xFrom=attackIcon.transform.position.x;
					xTo=pla.transform.position.x;
				}
				else
				{
					xFrom=pla.transform.position.x;
					xTo=attackIcon.transform.position.x;
				}
	
				float pos=xFrom;
				float y=0;
				int i;
				ArrayList points = new ArrayList();
				float d1,d2;
				bool first=true;
				
				while(pos<xTo)
				{
					
					y=Mathf.Sqrt(radius*radius-(pos-newVec.x)*(pos-newVec.x));
					
					d1=Vector2.Distance(new Vector2(pos,y+newVec.y),pla.transform.position);
					d2=Vector2.Distance(new Vector2(pos,-y+newVec.y),pla.transform.position);
					if(d1<d2)
					{
						points.Add(new Vector2(pos,y+newVec.y));
						if(rememberTrack)
						{
							if(first)
							{
								trackFirst=new Vector2(pos,y+newVec.y);
								first=false;
							}
						}
					}
					else
					{
						points.Add(new Vector2(pos,-y+newVec.y));
						if(rememberTrack)
						{
							if(first)
							{
								trackFirst=new Vector2(pos,-y+newVec.y);
								first=false;
							}
						}
					}
					pos+=step;
				}
				
				if(rememberTrack)
				{
					d1=Vector2.Distance(new Vector2(pos,y+newVec.y),pla.transform.position);
					d2=Vector2.Distance(new Vector2(pos,-y+newVec.y),pla.transform.position);
					if(d1<d2)
						trackLast=new Vector2(pos-step,y+newVec.y);
					else
						trackLast=new Vector2(pos-step,-y+newVec.y);
					rememberTrack=false;
					rememberedRadius=radius;
					rememberedCenter=newVec;
				}
				
				
				
				
				LineRenderer lr = pla.GetComponent<LineRenderer>();
				lr.SetVertexCount(points.Count);
				for(i=0;i<points.Count;i++)
					lr.SetPosition(i,new Vector3(((Vector2) points[i]).x,((Vector2) points[i]).y,-1));
			}
			
		}
	}
	
	public void ByeBye()
	{
		Destroy(attackIcon);
		Destroy(this.gameObject);
	}
	
	void Accelerate()
	{
		if(GameStorage.getInstance().isRunning)
		{
			if(Time.time<=(GameStorage.getInstance().getFixedTime()+3))
			{
				Vector2 v1 = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad));
				Vector2 v2 = attackIcon.transform.position-pla.transform.position;
				float sinPhi = (v1.x*v2.y - v1.y*v2.x); //---------------------------------------------- IMPORTANT
				
				float rangel = Vector2.Angle(new Vector2(pla.transform.position.x-rememberedCenter.x,pla.transform.position.y-rememberedCenter.y),new Vector2(attackIcon.transform.position.x-rememberedCenter.x,attackIcon.transform.position.y-rememberedCenter.y))*Time.deltaTime;
				if(sinPhi>0)
					angle+=rangel;
				else
					angle-=rangel;
				
				Vector2 newVec;
				Vector2 v11 = new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad))*0.013f;
				pla.transform.position+=new Vector3(v11.x,v11.y,0);
			}
			else
			{
				GameStorage.getInstance().isRunning=false;
			}
		}
		pla.transform.eulerAngles=new Vector3(0,0,angle-90);
	}
}
*/