using UnityEngine;
using System.Collections;

public class MineBehaviour : MonoBehaviour {

	public int cur;
	public int total;
	public bool ready=false;
	// Use this for initialization
	void Start () {
		
	}
	
	void Boom()
	{
		foreach(GameObject o in GameStorage.getInstance().getEnemyShuttles())
		{
			if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(o.transform.position.x,o.transform.position.z))<=Abilities.MinesParameters.Range)
				o.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.MinesParameters.Damage,null);
		}
		foreach(GameObject o in GameStorage.getInstance().getFriendlyShuttles())
		{
			if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(o.transform.position.x,o.transform.position.z))<=Abilities.MinesParameters.Range)
				o.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.MinesParameters.Damage,null);
		}
		Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if(ready && GameStorage.getInstance().isRunning)
		{
			if(Time.time>=(GameStorage.getInstance().getFixedTime()+3.0f/total*cur))
				Boom();
		}
	}
}
