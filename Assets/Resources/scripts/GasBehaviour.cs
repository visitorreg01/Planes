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
	
	void OnCollisionEnter (Collision col) 
	{
		GameObject target = col.gameObject;
		if(Time.time<=GameStorage.getInstance().getFixedTime()+3)
		{
			if(lastDamaged==0f)
			{
				if(enemy)
				{
					if(target.GetComponent<FriendlyShuttleBehaviour>()!=null)
						target.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
				}
				else
				{
					if(target.GetComponent<EnemyShuttleBehaviour>()!=null)
						target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
				}
				lastDamaged=Time.time;
			}
			else
			{
				if(Time.time>=lastDamaged+Abilities.GasParameters.gasReuse)
				{
					if(enemy)
					{
						if(target.GetComponent<FriendlyShuttleBehaviour>()!=null)
							target.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
					}
					else
					{
						if(target.GetComponent<EnemyShuttleBehaviour>()!=null)
							target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,Abilities.GasParameters.gasDamage,null);
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
