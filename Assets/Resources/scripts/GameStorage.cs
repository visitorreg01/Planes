using UnityEngine;
using System.Collections;

public class GameStorage {

	private static GameStorage instance = null;
	public static GameStorage getInstance()
	{
		if(instance==null)
			instance=new GameStorage();
		return instance;
	}
	
	public GameObject currentSelectedFriendly=null;
	public int defaultDepth = GUI.depth;
	public bool isRunning=false;
	public bool isDebug=false;
	public float zoom=1.0f;
	
	public CameraBehaviour cam;
	
	public int totalHp=0;
	public int curHp=0;
	public int curLevel=0;
	private int currentFocusShip=0;
	private ArrayList friendlyGameObjectsList;
	private ArrayList enemyGameObjectsList;
	private ArrayList gasList;
	private ArrayList gasRemoveList;
	private ArrayList rocketList;
	private ArrayList rocketRemoveList;
	private ArrayList thorpedeList;
	private ArrayList thorpedeRemoveList;
	private ArrayList asteroidsList;
	private ArrayList minesList;
	
	public bool overlap=false;
	
	public bool allReady = false;
	
	private float time;
	
	public GameStorage()
	{
		friendlyGameObjectsList = new ArrayList();
		enemyGameObjectsList = new ArrayList();
		gasList=new ArrayList();
		gasRemoveList=new ArrayList();
		rocketList = new ArrayList();
		rocketRemoveList = new ArrayList();
		thorpedeList = new ArrayList();
		thorpedeRemoveList = new ArrayList();
		asteroidsList = new ArrayList();
		minesList = new ArrayList();
	}
	
	public void setAllShipsMaxTraec()
	{
		foreach(GameObject gg in friendlyGameObjectsList)
			gg.GetComponent<FriendlyShuttleBehaviour>().setMaxAttackIcon();
	}
	
	public void LoadLevel(Templates.LevelInfo lv)
	{
		totalHp=0;
		curHp=0;
		curLevel=lv.num;
		Application.LoadLevel(lv.file);
	}
	
	public void EndLevel()
	{
		friendlyGameObjectsList.Clear();
		enemyGameObjectsList.Clear();
		rocketList.Clear();
		rocketRemoveList.Clear();
		minesList.Clear();
		thorpedeList.Clear();
		thorpedeRemoveList.Clear();
		gasList.Clear();
		gasRemoveList.Clear();
		asteroidsList.Clear();
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
		currentFocusShip=0;
	}
	
	public void StepStop()
	{
		isRunning=false;
		foreach(GameObject f in GameStorage.getInstance().getFriendlyShuttles())
			f.GetComponent<FriendlyShuttleBehaviour>().StepEnd();
		foreach(GameObject f in GameStorage.getInstance().getEnemyShuttles())
			f.GetComponent<EnemyShuttleBehaviour>().StepEnd();
		gasRemoveList.Clear();
		foreach(GameObject f in GameStorage.getInstance().getGasUnits())
		{
			if(f.GetComponent<GasBehaviour>().updateStepCounter()==Abilities.GasParameters.lifeTimeRounds)
				gasRemoveList.Add(f);
		}
		
		foreach(GameObject f in gasRemoveList)
		{
			gasList.Remove(f);
			f.GetComponent<GasBehaviour>().Die();
		}
		rocketRemoveList.Clear();
		foreach(GameObject f in GameStorage.getInstance().getRocketUnits())
		{
			if(f.GetComponent<RocketBehaviour>().updateStepCounter()==Abilities.RocketParameters.lifeTimeRounds)
				rocketRemoveList.Add(f);
		}
		
		foreach(GameObject f in rocketRemoveList)
		{
			rocketList.Remove(f);
			f.GetComponent<RocketBehaviour>().Die();
		}
		
		thorpedeRemoveList.Clear();
		foreach(GameObject f in GameStorage.getInstance().getThorpedeUnits())
		{
			if(f.GetComponent<ThorpedeBehaviour>().updateStepCounter()==Abilities.ThorpedeParameters.lifeTimeRounds)
				thorpedeRemoveList.Add(f);
		}
		
		foreach(GameObject f in thorpedeRemoveList)
		{
			thorpedeList.Remove(f);
			f.GetComponent<ThorpedeBehaviour>().Die();
		}
		
		foreach(GameObject f in minesList)
			f.GetComponent<MineBehaviour>().StepEnd();
		
		
		if(getFriendlyShuttles().Length==0 && getEnemyShuttles().Length>0)
		{
			EndLevel();
			cam.GetComponent<CameraBehaviour>().nextLevelWindow(-1,curLevel+1);
		}
		else if(getFriendlyShuttles().Length>0 && getEnemyShuttles().Length==0)
		{
			foreach(GameObject go in friendlyGameObjectsList)
				curHp+=go.GetComponent<FriendlyShuttleBehaviour>().hp;
			
			float perc = ((float)curHp)/((float)totalHp)*100.0f;
			int stars=1;
			if(perc>=Templates.StarsSettings.oneStar && perc < Templates.StarsSettings.threeStar)
				stars=2;
			else if(perc>=Templates.StarsSettings.threeStar)
				stars=3;
			else
				stars=1;
			
			EndLevel();
			cam.GetComponent<CameraBehaviour>().nextLevelWindow(stars,curLevel+1);
		}
		else if(getFriendlyShuttles().Length==0 && getEnemyShuttles().Length==0)
		{
			EndLevel();
			cam.GetComponent<CameraBehaviour>().nextLevelWindow(0,curLevel+1);
		}
			
		
	}
	
	public void setThorpedesAndRocketsAbils()
	{
		foreach(GameObject g in friendlyGameObjectsList)
		{
			g.GetComponent<FriendlyShuttleBehaviour>().setActiveRocketAbil();
			g.GetComponent<FriendlyShuttleBehaviour>().setActiveThorpedeAbil();
		}
	}
	
	public void registerMine(GameObject o)
	{
		minesList.Add(o);
	}
	
	public void removeMine(GameObject o)
	{
		minesList.Remove(o);
	}
	
	public ArrayList getMines()
	{
		return minesList;
	}
	
	public void nextShipFocus()
	{
		currentFocusShip++;
		currentFocusShip%=friendlyGameObjectsList.Count;
		focusShip(currentFocusShip);
	}
	
	public void prevShipFocus()
	{
		currentFocusShip--;
		if(currentFocusShip<0) currentFocusShip+=friendlyGameObjectsList.Count;
		currentFocusShip%=friendlyGameObjectsList.Count;
		focusShip(currentFocusShip);
	}
	
	private void focusShip(int index)
	{
		GameObject gg = (GameObject) friendlyGameObjectsList[index];
		cam.transform.position=new Vector3(gg.transform.position.x,cam.transform.position.y,gg.transform.position.z);
		if(!isRunning)
			gg.GetComponent<FriendlyShuttleBehaviour>().selected=true;
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
	
	public void registerAsteroid(GameObject o)
	{
		asteroidsList.Add(o);
	}
	
	public ArrayList getAllAsteroids()
	{
		return asteroidsList;
	}
	
	public GameObject getNearestAsteroid(GameObject o)
	{
		Vector2 pos = new Vector2(o.transform.position.x,o.transform.position.z);
		float d=-1;
		GameObject ret = null;
		foreach(GameObject obj in getAllAsteroids())
		{
			if(d<0)
			{
				ret=obj;
				d=Vector2.Distance(pos,new Vector2(obj.transform.position.x,obj.transform.position.z));
			}
			else
			{
				if(Vector2.Distance(pos,new Vector2(obj.transform.position.x,obj.transform.position.z))<d)
				{
					d=Vector2.Distance(pos,new Vector2(obj.transform.position.x,obj.transform.position.z));
					ret=obj;
				}
			}
		}
		return ret;
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
	
	public float getAngleRelative(Vector2 a, Vector2 b)
	{
		Vector2 v1 = new Vector2(0,5);
		Vector2 v2 = new Vector2(b.x-a.x,b.y-a.y);
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
	
	public void registerGasUnit(GameObject o)
	{
		gasList.Add(o);
	}
	
	public void removeGasUnit(GameObject o)
	{
		gasList.Remove(o);
	}
	
	public ArrayList getGasUnits()
	{
		return gasList;
	}
	
	public void registerRocketUnit(GameObject o)
	{
		rocketList.Add(o);
	}
	
	public void removeRocketUnit(GameObject o)
	{
		rocketList.Remove(o);
	}
	
	public ArrayList getRocketUnits()
	{
		return rocketList;
	}
	
	public void registerThorpedeUnit(GameObject o)
	{
		thorpedeList.Add(o);
	}
	
	public void removeThorpedeUnit(GameObject o)
	{
		thorpedeList.Remove(o);
	}
	
	public ArrayList getThorpedeUnits()
	{
		return thorpedeList;
	}
	
	
	public GameObject getNearbyFriendly(GameObject attacker)
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
	
	public GameObject getNearbyEnemy(GameObject attacker)
	{
		GameObject ret=null;
		float dst=-1;
		Vector2 pos1,pos2;
		pos1 = new Vector2(attacker.transform.position.x,attacker.transform.position.z);
		foreach(GameObject target in getEnemyShuttles())
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
