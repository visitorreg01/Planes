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
	
	private GUISkin[] abilitySkins = new GUISkin[9];
	private GUISkin[] abilityDisabledSkins = new GUISkin[9];
	
	public static class StarsSettings {
		public static int oneStar=60;
		public static int threeStar=80;
	}
	
	public enum PlaneTemplates : int
	{
		default_class=1,
		AllyFighter=2,
		AllyScout=3,
		AllyAssault=4,
		AllyGas=5,
		AllyFrigate=6,
		AllyCruiser=7,
		AllyBattleship=8,
		AllyDreadnaught=9,
		AllyCorvette=10,
		AllyBattlecruiser=11,
		EnemyFighter=12,
		EnemyScout=13,
		EnemyAssault=14,
		EnemyGas=15,
		EnemyFrigate=16,
		EnemyCruiser=17,
		EnemyBattleship=18,
		EnemyDreadnaught=19

	};
	
	public enum GunTemplates : int
	{
		default_gun=1,
		small_gun=2,
		medium_gun=3,
		large_gun=4,
		corvette_gun=5,
		battlecruiser_gun=6
	};
	
	private ArrayList planeClasses;
	private ArrayList gunClasses;
	private ArrayList levelList;
	private ArrayList ranksList;
	private string planeTempFolder="xml/planes";
	private string gunTempFolder="xml/guns";
	private string levelsFolder = "levels";
	private string ranksFolder = "xml";
	
	public Templates()
	{
		planeClasses=new ArrayList();
		LoadPlaneClasses();
		ranksList=new ArrayList();
		loadRanks();
		gunClasses=new ArrayList();
		LoadGunClasses();
		levelList=new ArrayList();
		loadLevels();
		loadAbilityIcons();
		Loaded();
	}
	
	private void loadAbilityIcons()
	{
		abilitySkins[0]=(GUISkin) Resources.Load("gui/skins/ability_common");
		abilitySkins[1]=(GUISkin) Resources.Load("gui/skins/ability_180");
		abilitySkins[2]=(GUISkin) Resources.Load("gui/skins/ability_360");
		abilitySkins[3]=(GUISkin) Resources.Load("gui/skins/ability_dt");
		abilitySkins[4]=(GUISkin) Resources.Load("gui/skins/ability_gas");
		abilitySkins[5]=(GUISkin) Resources.Load("gui/skins/ability_rocket");
		abilitySkins[6]=(GUISkin) Resources.Load("gui/skins/ability_shield");
		abilitySkins[7]=(GUISkin) Resources.Load("gui/skins/ability_thorpede");
		abilitySkins[8]=(GUISkin) Resources.Load("gui/skins/ability_mines");
		
		abilityDisabledSkins[0]=(GUISkin) Resources.Load("gui/skins/ability_common");
		abilityDisabledSkins[1]=(GUISkin) Resources.Load("gui/skins/ability_180_grey");
		abilityDisabledSkins[2]=(GUISkin) Resources.Load("gui/skins/ability_360_grey");
		abilityDisabledSkins[3]=(GUISkin) Resources.Load("gui/skins/ability_dt_grey");
		abilityDisabledSkins[4]=(GUISkin) Resources.Load("gui/skins/ability_gas_grey");
		abilityDisabledSkins[5]=(GUISkin) Resources.Load("gui/skins/ability_rocket_grey");
		abilityDisabledSkins[6]=(GUISkin) Resources.Load("gui/skins/ability_shield_grey");
		abilityDisabledSkins[7]=(GUISkin) Resources.Load("gui/skins/ability_thorpede_grey");
		abilityDisabledSkins[8]=(GUISkin) Resources.Load("gui/skins/ability_mines_grey");
	}
	
	public GUISkin getAbilityIcon(int id)
	{
		return abilitySkins[id+1];
	}
	
	public GUISkin getAbilityIcon(Abilities.AbilityType id)
	{
		return abilitySkins[((int)id)+1];
	}
	
	public GUISkin getAbilityIconGrey(int id)
	{
		return abilityDisabledSkins[id+1];
	}
	
	public GUISkin getAbilityIconGrey(Abilities.AbilityType id)
	{
		return abilityDisabledSkins[((int)id)+1];
	}
	
	void Loaded()
	{
		GameStorage.getInstance().allReady=true;
	}
	
	public class Rank
	{
		public int id;
		public string name;
		// maybe ico
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
		public float upperSmooth=1f;
		public float lowerSmooth=1f;
		public string description;
		public ArrayList guns = new ArrayList();
		public ArrayList abilities = new ArrayList();
	}
	
	public class LevelInfo
	{
		public int num;
		public string levelName,file;
		public int rankReached=-1;
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
				t.lowerSmooth=p.lowerSmooth;
				t.upperSmooth=p.upperSmooth;
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
	
	public Rank getRank(int id)
	{
		foreach(Rank p in ranksList)
		{
			if(p.id==(int)id)
				return p;
		}
		return null;
	}
	
	public LevelInfo getLevel(int num)
	{
		foreach(LevelInfo l in levelList)
		{
			if(l.num==num)
				return l;
		}
		return null;
	}
	
	public ArrayList getAllLevels()
	{
		return levelList;
	}
	
	private void loadLevels()
	{
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(((TextAsset) Resources.Load(levelsFolder+"/levels")).text);
		foreach(XmlNode x in doc.ChildNodes)
		{
			if(x.Name=="levels")
			{
				foreach(XmlNode m in x.ChildNodes)
				{
					if(m.Name=="level")
					{
						LevelInfo li = new LevelInfo();
						foreach(XmlNode l in m.Attributes)
						{
							if(l.Name=="num")
								li.num=int.Parse(l.Value);
							else if(l.Name=="levelName")
								li.levelName=l.Value;
							else if(l.Name=="file")
								li.file=l.Value;
							else if(l.Name=="rankReached")
								li.rankReached=int.Parse(l.Value);
						}
						levelList.Add(li);
					}
				}
			}
		}
		Debug.Log("Loaded "+levelList.Count+" levels.");
	}
	
	private void loadRanks()
	{
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(((TextAsset) Resources.Load(ranksFolder+"/ranks")).text);
		foreach(XmlNode x in doc.ChildNodes)
		{
			if(x.Name=="ranks")
			{
				foreach(XmlNode m in x.ChildNodes)
				{
					if(m.Name=="rank")
					{
						Rank li = new Rank();
						foreach(XmlNode l in m.Attributes)
						{
							if(l.Name=="id")
								li.id=int.Parse(l.Value);
							else if(l.Name=="name")
								li.name=l.Value;
						}
						ranksList.Add(li);
					}
				}
			}
		}
		Debug.Log("Loaded "+ranksList.Count+" ranks.");
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
				t.lowerSmooth=p.lowerSmooth;
				t.upperSmooth=p.upperSmooth;
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
						else if(m.Name=="upperSmooth")
							p.upperSmooth=float.Parse(m.Value);
						else if(m.Name=="lowerSmooth")
							p.lowerSmooth=float.Parse(m.Value);
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
