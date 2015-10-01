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
		return gameObject.transform.eulerAngles.y;
	}
	
	public void Attacked(GameObject attacker, int damage)
	{
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
		
		if(isMouseOver(gameObject))
			focused=true;
		else
			focused=false;
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
