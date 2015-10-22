using UnityEngine;
using System.Collections;

public class AsteroidBehaviour : MonoBehaviour {

	void Start()
	{
		GameStorage.getInstance().registerAsteroid(this.gameObject);
	}
	
	void OnCollisionEnter(Collision col)
	{
		GameObject target = col.gameObject;
		if(target.GetComponent<FriendlyShuttleBehaviour>()!=null)
			target.GetComponent<FriendlyShuttleBehaviour>().Die();
		if(target.GetComponent<EnemyShuttleBehaviour>()!=null)
			target.GetComponent<EnemyShuttleBehaviour>().Die();
		if(target.GetComponent<ThorpedeBehaviour>()!=null)
			target.GetComponent<ThorpedeBehaviour>().Die();
		if(target.GetComponent<RocketBehaviour>()!=null)
			target.GetComponent<RocketBehaviour>().Die();
	}
}
