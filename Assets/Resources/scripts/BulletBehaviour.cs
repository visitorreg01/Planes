using UnityEngine;
using System.Collections;

public class BulletBehaviour : MonoBehaviour {

	public bool enemy=false;
	Vector2 startPoint;
	Vector2 targetPoint;
	float x,y;
	bool setuped=false;
	Templates.GunTemplate gos;
	float t=0,step=0.01f;
	
	void Update () 
	{
		if(!GameStorage.getInstance().isRunning)
			Destroy(gameObject);
		if(setuped)
		{
			t+=gos.bulletSpeed*Time.deltaTime*gos.bulletSpeed;
			
			x = (1-t)*startPoint.x+t*targetPoint.x;
			y = (1-t)*startPoint.y+t*targetPoint.y;
			transform.position=new Vector3(x,0,y);
			if(Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(startPoint.x,startPoint.y))>gos.attackRange)
				Destroy(gameObject);
		}
	}
	
	void OnCollisionEnter(Collision col)
	{
		GameObject target = col.gameObject;
		if(enemy)
		{
			if(target.GetComponent<FriendlyShuttleBehaviour>()!=null)
			{
				int defect=-1,i;
				float ch = UnityEngine.Random.Range(0.0f,100f);
				float lower=0.0f,upper;
				for(i=0;i<gos.defectsChance.Length;i++)
				{
					upper=gos.defectsChance[i]+lower;
					if(ch>=lower && ch<=upper)
					{
						defect=i;
						break;
					}
					else
						lower=upper;
				}
				target.GetComponent<FriendlyShuttleBehaviour>().Attacked(null,gos.damage,Defects.getDefect(defect));
				Destroy(gameObject);
			}
		}
		else
		{
			if(target.GetComponent<EnemyShuttleBehaviour>()!=null)
			{
				int defect=-1,i;
				float ch = UnityEngine.Random.Range(0.0f,100f);
				float lower=0.0f,upper;
				for(i=0;i<gos.defectsChance.Length;i++)
				{
					upper=gos.defectsChance[i]+lower;
					if(ch>=lower && ch<=upper)
					{
						defect=i;
						break;
					}
					else
						lower=upper;
				}
				target.GetComponent<EnemyShuttleBehaviour>().Attacked(null,gos.damage,Defects.getDefect(defect));
				Destroy(gameObject);
			}
		}
	}
	
	public void Launch(Vector2 targetPos, Vector2 startPos, Templates.GunOnShuttle gun)
	{
		gos=Templates.getInstance().getGunTemplate(gun.gunId);
		startPoint=startPos;
		targetPoint=new Vector2(targetPos.x+UnityEngine.Random.Range(-gos.bulletDispersion,gos.bulletDispersion),targetPos.y+UnityEngine.Random.Range(-gos.bulletDispersion,gos.bulletDispersion));
		targetPoint=new Vector2(targetPoint.x-startPoint.x,targetPoint.y-startPoint.y);
		targetPoint/=targetPoint.magnitude;
		targetPoint*=gos.attackRange;
		targetPoint=new Vector2(targetPoint.x+startPoint.x,targetPoint.y+startPoint.y);
		transform.eulerAngles=new Vector3(0,GameStorage.getInstance().getAngleRelative(startPoint,targetPoint),0);
		
		setuped=true;
	}
}
