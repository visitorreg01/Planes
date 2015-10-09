using UnityEngine;
using System.Collections;

public class GasBehaviour : MonoBehaviour {

	public bool enemy=false;
	private int stepCount=0;
	float lastDamaged=0f;
	
	void Start()
	{
		GameStorage.getInstance().registerGasUnit(this.gameObject);
	}
	
	void Update () 
	{
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			if(lastDamaged==0f)
			{
				if(enemy)
				{
					foreach(GameObject target in GameStorage.getInstance().getFriendlyShuttles())
					{
						if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(target.transform.position.x,target.transform.position.z))<=Abilities.GasParameters.gasRange)
							target.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
					}
				}
				else
				{
					foreach(GameObject target in GameStorage.getInstance().getEnemyShuttles())
					{
						if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(target.transform.position.x,target.transform.position.z))<=Abilities.GasParameters.gasRange)
							target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
					}
				}
				lastDamaged=Time.time;
			}
			else
			{
				if(Time.time>=lastDamaged+Abilities.GasParameters.gasReuse)
				{
					if(enemy)
					{
						foreach(GameObject target in GameStorage.getInstance().getFriendlyShuttles())
						{
							if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(target.transform.position.x,target.transform.position.z))<=Abilities.GasParameters.gasRange)
								target.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
						}
					}
					else
					{
						foreach(GameObject target in GameStorage.getInstance().getEnemyShuttles())
						{
							if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(target.transform.position.x,target.transform.position.z))<=Abilities.GasParameters.gasRange)
								target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
						}
					}
					lastDamaged=Time.time;
				}
			}
		}
	}
	
	public int updateStepCounter()
	{
		return stepCount++;
	}
	
	public void Die()
	{
		Destroy(this.gameObject);
	}
}
