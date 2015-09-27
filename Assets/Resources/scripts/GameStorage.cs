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
	
	public void updateIcons()
	{
		foreach(GameObject f in GameStorage.getInstance().getFiendlyShuttles())
		{
			f.GetComponent<FriendlyShuttleBehaviour>().updateAttackPosition();
			f.GetComponent<FriendlyShuttleBehaviour>().t=0;
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
	
	public GameObject[] getFiendlyShuttles()
	{
		GameObject[] lst = new GameObject[friendlyGameObjectsList.Count];
		int i;
		for(i=0;i<lst.Length;i++)
			lst[i]=(GameObject)friendlyGameObjectsList[i];
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
