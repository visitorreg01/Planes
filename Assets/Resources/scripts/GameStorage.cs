using UnityEngine;
using System.Collections;
using System.IO;

public class GameStorage {

	private static GameStorage instance = null;
	public static GameStorage getInstance()
	{
		if(instance==null)
			instance=new GameStorage();
		return instance;
	}
	
	public bool isRunning=false;
	
	private ArrayList friendlyGameObjectsList;
	private ArrayList enemyGameObjectsList;
	
	private GameObject friendlyUnitPrefab;
	
	private float time;
	
	public GameStorage()
	{
		friendlyGameObjectsList = new ArrayList();
		enemyGameObjectsList = new ArrayList();
	}
	
	public void StepStart()
	{
		fixTime();
		isRunning=true;
		foreach(GameObject f in GameStorage.getInstance().getFriendlyShuttles())
		{
			f.GetComponent<FriendlyShuttleBehaviour>().StepStart();
		}
		foreach(GameObject f in GameStorage.getInstance().getEnemyShuttles())
		{
			f.GetComponent<EnemyShuttleBehaviour>().StepStart();
		}
	}
	
	public void StepStop()
	{
		isRunning=false;
		foreach(GameObject f in GameStorage.getInstance().getFriendlyShuttles())
		{
			f.GetComponent<FriendlyShuttleBehaviour>().StepEnd();
		}
		foreach(GameObject f in GameStorage.getInstance().getEnemyShuttles())
		{
			f.GetComponent<EnemyShuttleBehaviour>().StepEnd();
		}
	}
	
	public void createFriendlyShuttle()
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/friendlyShuttlePrefab") as GameObject);
		friendlyGameObjectsList.Add(c);
	}
	
	public void createFriendlyShuttle(Vector2 pos, float angle)
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/friendlyShuttlePrefab") as GameObject);
		c.GetComponent<FriendlyShuttleBehaviour>().setPosition(pos);
		c.GetComponent<FriendlyShuttleBehaviour>().setAngle(angle);
		friendlyGameObjectsList.Add(c);
	}
	
	public void createFriendlyShuttle(float angle)
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/friendlyShuttlePrefab") as GameObject);
		c.GetComponent<FriendlyShuttleBehaviour>().setAngle(angle);
		friendlyGameObjectsList.Add(c);
	}
	
	public void createFriendlyShuttle(Vector2 pos)
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/friendlyShuttlePrefab") as GameObject);
		c.GetComponent<FriendlyShuttleBehaviour>().setPosition(pos);
		friendlyGameObjectsList.Add(c);
	}
	
	public void addFriendlyShuttle(GameObject s)
	{
		if(!friendlyGameObjectsList.Contains(s))
			friendlyGameObjectsList.Add(s);
	}
	
	public void removeFriendlyShuttle(GameObject s)
	{
		friendlyGameObjectsList.Remove(s);
		s.GetComponent<FriendlyShuttleBehaviour>().ByeBye();
	}
	
	public GameObject[] getFriendlyShuttles()
	{
		GameObject[] lst = new GameObject[friendlyGameObjectsList.Count];
		int i;
		for(i=0;i<lst.Length;i++)
			lst[i]=(GameObject)friendlyGameObjectsList[i];
		return lst;
	}
	
	public float getAngleDst(float fr, float to)
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
	
	//--------------------------------------------------------
	public void createEnemyShuttle()
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/enemyShuttlePrefab") as GameObject);
		enemyGameObjectsList.Add(c);
	}
	
	public void createEnemyShuttle(Vector2 pos, float angle)
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/enemyShuttlePrefab") as GameObject);
		c.GetComponent<EnemyShuttleBehaviour>().setPosition(pos);
		c.GetComponent<EnemyShuttleBehaviour>().setAngle(angle);
		enemyGameObjectsList.Add(c);
	}
	
	public void createEnemyShuttle(float angle)
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/enemyShuttlePrefab") as GameObject);
		c.GetComponent<EnemyShuttleBehaviour>().setAngle(angle);
		enemyGameObjectsList.Add(c);
	}
	
	public void createEnemyShuttle(Vector2 pos)
	{
		GameObject c = (GameObject) GameObject.Instantiate(Resources.Load("prefab/enemyShuttlePrefab") as GameObject);
		c.GetComponent<EnemyShuttleBehaviour>().setPosition(pos);
		enemyGameObjectsList.Add(c);
	}
	
	public void addEnemyShuttle(GameObject s)
	{
		if(!enemyGameObjectsList.Contains(s))
			enemyGameObjectsList.Add(s);
	}
	
	public void removeEnemyShuttle(GameObject s)
	{
		enemyGameObjectsList.Remove(s);
		s.GetComponent<EnemyShuttleBehaviour>().ByeBye();
	}
	
	public float getAngleRelative(GameObject a, GameObject b)
	{
		Vector2 v1 = new Vector2(0,5);
		Vector2 v2 = new Vector2(b.transform.position.x-a.transform.position.x,b.transform.position.z-a.transform.position.z);
		float mySinPhi = (v1.x*v2.y - v1.y*v2.x);
		float mangle = Vector2.Angle(v1,v2);
		if(mySinPhi<=0)
			return mangle;
		else
			return (180-mangle)+180;
	}
	
	public GameObject getEnemyInFireZone(GameObject friendlyShuttle, Templates.GunOnShuttle gun)
	{
		GameObject ret = null;
		float dist=0;
		float mindist=-1;
		Templates.GunTemplate gunTemp = Templates.getInstance().getGunTemplate(gun.gunId);
		Vector2 gunPos = new Vector2(friendlyShuttle.transform.position.x+gun.pos.x,friendlyShuttle.transform.position.z+gun.pos.y);
		float gunAngle = Mathf.Repeat(friendlyShuttle.GetComponent<FriendlyShuttleBehaviour>().getAngle()+gun.turnAngle,360);
		Vector2 pos2;
		foreach(GameObject enemy in getEnemyShuttles())
		{
			pos2=new Vector2(enemy.transform.position.x,enemy.transform.position.z);
			if((dist=Vector2.Distance(gunPos,pos2))<=gunTemp.attackRange && Mathf.Abs(getAngleDst(gunAngle,getAngleRelative(friendlyShuttle,enemy)))<=gunTemp.attackAngle)
			{
				if(mindist<0)
				{
					ret=enemy;
					mindist=dist;
				}
				else
				{
					if(dist<mindist)
					{
						ret=enemy;
						mindist=dist;
					}
				}
			}
		}
		return ret;
	}
	
	public GameObject getFriendlyInFireZone(GameObject enemyShuttle, Templates.GunOnShuttle gun)
	{
		GameObject ret = null;
		float dist=0;
		float mindist=-1;
		Templates.GunTemplate gunTemp = Templates.getInstance().getGunTemplate(gun.gunId);
		Vector2 gunPos = new Vector2(enemyShuttle.transform.position.x+gun.pos.x,enemyShuttle.transform.position.z+gun.pos.y);
		float gunAngle = Mathf.Repeat(enemyShuttle.GetComponent<EnemyShuttleBehaviour>().getAngle()+gun.turnAngle,360);
		Vector2 pos2;
		foreach(GameObject friendly in getFriendlyShuttles())
		{
			pos2=new Vector2(friendly.transform.position.x,friendly.transform.position.z);
			if((dist=Vector2.Distance(gunPos,pos2))<=gunTemp.attackRange && Mathf.Abs(getAngleDst(gunAngle,getAngleRelative(enemyShuttle,friendly)))<=gunTemp.attackAngle)
			{
				if(mindist<0)
				{
					ret=friendly;
					mindist=dist;
				}
				else
				{
					if(dist<mindist)
					{
						ret=friendly;
						mindist=dist;
					}
				}
			}
		}
		return ret;
	}
	
	public GameObject getNearbyTarget(GameObject attacker)
	{
		GameObject ret=null;
		float dst=-1;
		Vector2 pos1,pos2;
		pos1 = new Vector2(attacker.transform.position.x,attacker.transform.position.z);
		foreach(GameObject target in getFriendlyShuttles())
		{
			pos2 = new Vector2(target.transform.position.x,target.transform.position.z);
			if(dst<0)
			{
				ret=target;
				dst = Vector2.Distance(pos1,pos2);
			}
			else
			{
				if(dst>Vector2.Distance(pos1,pos2))
				{
					ret=target;
					dst=Vector2.Distance(pos1,pos2);
				}
			}
		}
		return ret;
	}
	
	public GameObject[] getEnemyShuttles()
	{
		GameObject[] lst = new GameObject[enemyGameObjectsList.Count];
		int i;
		for(i=0;i<lst.Length;i++)
			lst[i]=(GameObject)enemyGameObjectsList[i];
		return lst;
	}
	
	public void fixTime()
	{
		time=Time.time;
	}
	
	public float getFixedTime()
	{
		return time;
	}
}
