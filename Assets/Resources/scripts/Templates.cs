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
		EnemyDreadnaught=19,
		ShieldedFighter=20,
		RoundFighter=21,
		HeavyFighter=22,
		NewFighter=23,
		NewScout=24
	};
	

	
	public enum GunTemplates : int
	{
		default_gun=1,
		small_gun=2,
		medium_gun=3,
		large_gun=4,
		corvette_gun=5,
		battlecruiser_gun=6,
		fighter_gun=7
	};
	
	private ArrayList planeClasses;
	private ArrayList gunClasses;
	private ArrayList levelList;
	private ArrayList ranksList;
	private ArrayList campaignsList;
	private string planeTempFolder="xml/planes";
	private string gunTempFolder="xml/guns";
	private string levelsFolder = "levels";
	private string ranksFolder = "xml";
	
	public GUISkin button_level = null;
	public GUISkin button_level_selected = null;
	public GUISkin button_level_start = null;
	public GUISkin label_level_star = null;
	public GUISkin mainPopupRichtext = null;
	public GUISkin none_scroll_skin = null;
	//TEST
	public GUISkin playBIG=null;
	
	public string plotContext,helpContent;
	
	private GUISkin[] numbers;
	private GUISkin[] numbers_grey;
	
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
		campaignsList=new ArrayList();
		loadCampaigns();
		numbers=new GUISkin[10];
		numbers_grey=new GUISkin[10];
		loadNumbersIcons();
		loadAbilityIcons();
		loadContents();
		loadLevelsSkins();
		Loaded();
	}
	
	private void loadLevelsSkins()
	{
		button_level = (GUISkin) Resources.Load("gui/skins/button_level");
		button_level_selected = (GUISkin) Resources.Load("gui/skins/button_level_selected");
		button_level_start = (GUISkin) Resources.Load("gui/skins/button_level_start");
		label_level_star = (GUISkin) Resources.Load("gui/skins/label_level_star");
		mainPopupRichtext = (GUISkin) Resources.Load("gui/skins/main_popup_richtext");
		none_scroll_skin = (GUISkin) Resources.Load("gui/skins/none_scroll");
		playBIG=(GUISkin) Resources.Load("gui/skins/playBig");
	}
	
	public GUISkin[] getNumberIcons(int num, bool grey)
	{
		GUISkin first,second;
		if(grey)
		{
			first=numbers_grey[num/10];
			second=numbers_grey[num%10];
		}
		else
		{
			first=numbers[num/10];
			second=numbers[num%10];
		}
		return new GUISkin[] {first,second};
	}
	
	private void loadNumbersIcons()
	{
		for(int i=0;i<10;i++)
		{
			numbers[i]=(GUISkin) Resources.Load("gui/skins/numbers/"+i);
			numbers_grey[i]=(GUISkin) Resources.Load("gui/skins/numbers/"+i+"_grey");
		}
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
	
	private void loadContents()
	{
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(((TextAsset) Resources.Load("xml/gui_content/main_menu")).text);
		foreach(XmlNode x in doc.ChildNodes)
		{
			if(x.Name=="contents")
			{
				foreach(XmlNode m in x.ChildNodes)
				{
					if(m.Name=="content")
					{
						string name="",text="";
						foreach(XmlNode l in m.Attributes)
						{
							if(l.Name=="name")
								name=l.Value;
							else if(l.Name=="text")
								text=l.Value;
						}
						if(name=="plot")
							plotContext=text;
						else if(name=="help")
							helpContent=text;
					}
				}
			}
		}
	}
	
	private void loadCampaigns()
	{
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(((TextAsset) Resources.Load(levelsFolder+"/campaigns")).text);
		foreach(XmlNode x in doc.ChildNodes)
		{
			if(x.Name=="campaigns")
			{
				foreach(XmlNode m in x.ChildNodes)
				{
					if(m.Name=="campaign")
					{
						CampaignInfo li = new CampaignInfo();
						foreach(XmlNode l in m.Attributes)
						{
							if(l.Name=="id")
								li.id=int.Parse(l.Value);
							else if(l.Name=="name")
								li.name=l.Value;
						}
						
						foreach(XmlNode l in m.ChildNodes)
						{
							int levelNum=-1;
							if(l.Name=="level")
							{
								foreach(XmlNode b in l.Attributes)
								{
									if(b.Name=="id")
										levelNum=int.Parse(b.Value);
								}
							}
							li.levels.Add(levelNum);
						}
						
						campaignsList.Add(li);
					}
				}
			}
		}
		Debug.Log("Loaded "+campaignsList.Count+" campaigns.");
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
	
	public class CampaignInfo
	{
		public int id;
		public string name;
		public ArrayList levels = new ArrayList();
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
		public string levelName,file,description="NONE";
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
	
	public ArrayList getCampaigns()
	{
		return campaignsList;
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
							else if(l.Name=="description")
								li.description=l.Value;
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
