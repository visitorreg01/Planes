using UnityEngine;
using System.Collections;
using System.Xml;
using System;

public class Templates {
	private static Templates instance = null;
	public static Templates getInstance()
	{
		if(instance==null)
			instance=new Templates();
		return instance;
	}
	
	public enum PlaneTemplates : int
	{
		default_class=1
	};
	
	public enum GunTemplates : int
	{
		default_gun=1
	};
	
	private ArrayList planeClasses;
	private ArrayList gunClasses;
	private string planeTempFolder="xml/planes";
	private string gunTempFolder="xml/guns";
	
	public Templates()
	{
		planeClasses=new ArrayList();
		LoadPlaneClasses();
		gunClasses=new ArrayList();
		LoadGunClasses();
	}
	
	public class GunOnShuttle
	{
		public int gunId;
		public Vector2 pos;
		public float turnAngle;
		public bool ready=true;
		public float shotTime=0;
	}
	
	public class PlaneTemplate
	{
		public int id;
		public string classname;
		public int hp;
		public float minRange,maxRange,maxTurnAngle;
		public string description;
		public ArrayList guns = new ArrayList();
		public ArrayList abilities = new ArrayList();
	}
	
	public class GunTemplate
	{
		public int id;
		public string gunName;
		public int damage;
		public float reuse;
		public float bulletSpeed;
		public float bulletDispersion;
		public float attackAngle,attackRange;
		public string bulletMesh;
		public float[] defectsChance = new float[Enum.GetNames(typeof(Defects.DefectType)).Length];
	}
	
	public PlaneTemplate getPlaneTemplate(PlaneTemplates id)
	{
		foreach(PlaneTemplate p in planeClasses)
		{
			if(p.id==(int)id)
			{
				PlaneTemplate t = new PlaneTemplate();
				t.classname=p.classname;
				t.description=p.description;
				t.hp=p.hp;
				t.id=p.id;
				t.maxRange=p.maxRange;
				t.maxTurnAngle=p.maxTurnAngle;
				t.minRange=p.minRange;
				foreach(GunOnShuttle f in p.guns)
				{
					GunOnShuttle z = new GunOnShuttle();
					z.gunId=f.gunId;
					z.pos=f.pos;
					z.turnAngle=f.turnAngle;
					t.guns.Add(z);
				}
				
				foreach(int abilId in p.abilities)
					t.abilities.Add(abilId);
				return t;
			}
		}
		return null;
	}
	
	public GunTemplate getGunTemplate(GunTemplates id)
	{
		foreach(GunTemplate p in gunClasses)
		{
			if(p.id==(int)id)
				return p;
		}
		return null;
	}
	
	public PlaneTemplate getPlaneTemplate(int id)
	{
		foreach(PlaneTemplate p in planeClasses)
		{
			if(p.id==(int)id)
			{
				PlaneTemplate t = new PlaneTemplate();
				t.classname=p.classname;
				t.description=p.description;
				t.hp=p.hp;
				t.id=p.id;
				t.maxRange=p.maxRange;
				t.maxTurnAngle=p.maxTurnAngle;
				t.minRange=p.minRange;
				foreach(GunOnShuttle f in p.guns)
				{
					GunOnShuttle z = new GunOnShuttle();
					z.gunId=f.gunId;
					z.pos=f.pos;
					z.turnAngle=f.turnAngle;
					t.guns.Add(z);
				}
				
				foreach(int abilId in p.abilities)
					t.abilities.Add(abilId);
				return t;
			}
		}
		return null;
	}
	
	public GunTemplate getGunTemplate(int id)
	{
		foreach(GunTemplate p in gunClasses)
		{
			if(p.id==(int)id)
				return p;
		}
		return null;
	}
	
	private void LoadGunClasses()
	{
		string name;
		foreach(int Class in Enum.GetValues(typeof(GunTemplates)))
		{
			name = Enum.GetName(typeof(GunTemplates),Class);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(((TextAsset) Resources.Load(gunTempFolder+"/"+name)).text);
			foreach(XmlNode x in doc.ChildNodes)
			{
				if(x.Name=="gun")
				{
					GunTemplate p = new GunTemplate();
					foreach(XmlNode m in x.Attributes)
					{
						if(m.Name=="id")
							p.id=int.Parse(m.Value);
						else if(m.Name=="gunName")
							p.gunName=m.Value;
						else if(m.Name=="damage")
							p.damage=int.Parse(m.Value);
						else if(m.Name=="reuse")
							p.reuse=float.Parse(m.Value);
						else if(m.Name=="attackAngle")
							p.attackAngle=float.Parse(m.Value);
						else if(m.Name=="attackRange")
							p.attackRange=float.Parse(m.Value);
						else if(m.Name=="bulletSpeed")
							p.bulletSpeed=float.Parse(m.Value);
						else if(m.Name=="bulletDispersion")
							p.bulletDispersion=float.Parse(m.Value);
						else if(m.Name=="bulletMesh")
							p.bulletMesh=m.Value;
					}
					
					foreach(XmlNode m in x.ChildNodes)
					{
						if(m.Name=="defectChance")
						{
							int id=-1;
							float chance=-1;
							foreach(XmlNode l in m.Attributes)
							{
								if(l.Name=="id")
									id=int.Parse(l.Value);
								else if(l.Name=="chance")
									chance=float.Parse(l.Value);
							}
							p.defectsChance[id]=chance;
						}
					}
					
					gunClasses.Add(p);
				}
			}
		}
		Debug.Log("Loaded: "+gunClasses.Count+" gun classes.");
	}
	
	private void LoadPlaneClasses()
	{
		string name;
		foreach(int Class in Enum.GetValues(typeof(PlaneTemplates)))
		{
			name = Enum.GetName(typeof(PlaneTemplates),Class);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(((TextAsset) Resources.Load(planeTempFolder+"/"+name)).text);
			foreach(XmlNode x in doc.ChildNodes)
			{
				if(x.Name=="plane")
				{
					PlaneTemplate p = new PlaneTemplate();
					foreach(XmlNode m in x.Attributes)
					{
						if(m.Name=="id")
							p.id=int.Parse(m.Value);
						else if(m.Name=="hp")
							p.hp=int.Parse(m.Value);
						else if(m.Name=="classname")
							p.classname=m.Value;
						else if(m.Name=="minRange")
							p.minRange=float.Parse(m.Value);
						else if(m.Name=="maxRange")
							p.maxRange=float.Parse(m.Value);
						else if(m.Name=="maxTurnAngle")
							p.maxTurnAngle=float.Parse(m.Value);
						else if(m.Name=="description")
							p.description=m.Value;
					}
					
					foreach(XmlNode m in x.ChildNodes)
					{
						if(m.Name=="gun")
						{
							GunOnShuttle gos = new GunOnShuttle();
							foreach(XmlNode l in m.Attributes)
							{
								if(l.Name=="id")
									gos.gunId=int.Parse(l.Value);
								else if(l.Name=="pos")
								{
									float xx,yy;
									xx=float.Parse(l.Value.Split(',')[0]);
									yy=float.Parse(l.Value.Split(',')[1]);
									gos.pos=new Vector2(xx,yy);
								}
								else if(l.Name=="turnAngle")
									gos.turnAngle=float.Parse(l.Value);
							}
							p.guns.Add(gos);
						}
						else if(m.Name=="ability")
						{
							int id=-1;
							foreach(XmlNode l in m.Attributes)
							{
								if(l.Name=="id")
									id=int.Parse(l.Value);
							}
							p.abilities.Add(id);
						}
					}
					planeClasses.Add(p);
				}
			}
		}
		Debug.Log("Loaded: "+planeClasses.Count+" plane classes.");
	}
}
