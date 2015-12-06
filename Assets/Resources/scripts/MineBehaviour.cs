using UnityEngine;
using System.Collections;

public class MineBehaviour : MonoBehaviour {

	public int cur;
	public int total;
	private int stepCount=0;
	public bool ready=false;
	public bool enemy=false;
	// Use this for initialization
	void Start () {
		GameStorage.getInstance().registerMine(this.gameObject);
	}
	
	void Boom()
	{
		if(enemy)
		{
			foreach(GameObject o in GameStorage.getInstance().getFriendlyShuttles())
			{
				if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(o.transform.position.x,o.transform.position.z))<=Abilities.MinesParameters.Range)
					o.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.MinesParameters.Damage,null);
			}
		}
		else
		{
			foreach(GameObject o in GameStorage.getInstance().getEnemyShuttles())
			{
				if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(o.transform.position.x,o.transform.position.z))<=Abilities.MinesParameters.Range)
					o.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.MinesParameters.Damage,null);
			}
		}
		GameStorage.getInstance().removeMine(this.gameObject);
		Destroy(this.gameObject);
	}
	
	public int updateStepCounter()
	{
		return stepCount++;
	}
	
	public void Die()
	{
		Boom();
	}
	
	void Update()
	{
		GameObject target;
		if(enemy)
		{
			target = GameStorage.getInstance().getNearbyFriendly(gameObject);
			if(target!=null)
				if(Vector2.Distance(new Vector2(target.transform.position.x,target.transform.position.z),new Vector2(transform.position.x,transform.position.z))<=Abilities.MinesParameters.Range)
					Boom();
		}
		else
		{
			target = GameStorage.getInstance().getNearbyEnemy(gameObject);
			if(target!=null)
				if(Vector2.Distance(new Vector2(target.transform.position.x,target.transform.position.z),new Vector2(transform.position.x,transform.position.z))<=Abilities.MinesParameters.Range)
					Boom();
		}
	}
}
