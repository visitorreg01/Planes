using UnityEngine;
using System.Collections;

public static class Defects {

	public enum DefectType : int
	{
		disableTurnLeft=0,
		disableTurnRight=1,
		disableTurn=2,
		engineCrash=3,
		disableGuns=4,
		fired=5
	}
	
	public interface Defect{
		string getName();
	};
	
	public static Defect getDefect(int id)
	{
		DefectType ids = (DefectType) id;
		switch(ids)
		{
			case DefectType.disableTurnLeft:
				return new DisableTurnLeft();
			case DefectType.disableTurnRight:
				return new DisableTurnRight();
			case DefectType.disableTurn:
				return new DisableTurn();
			case DefectType.engineCrash:
				return new EngineCrash();
			case DefectType.disableGuns:
				return new DisableGuns();
			case DefectType.fired:
				return new Fired();
			default:
				return null;
		}
	}
	
	public class DisableTurnLeft : Defect{
		public string getName()
		{
			return "Disabled turn left";
		}
	};
	
	public class DisableTurnRight : Defect{
		public string getName()
		{
			return "Disabled turn right";
		}
	};
	
	public class DisableTurn : Defect{
		public string getName()
		{
			return "Disabled turn";
		}
	};
	
	public class EngineCrash : Defect
	{
		public float newRangeCoeff=0.5f;
		public string getName()
		{
			return "Engines off";
		}
	}
	
	public class DisableGuns : Defect{
		public string getName()
		{
			return "Weapons systems off";
		}
	};
	
	public class Fired : Defect
	{
		public float reuse=0.5f; // in seconds!
		public int damage=20; // total damage calculates: step time(3s)/reuse*damage
		public string getName()
		{
			return "Internal fire";
		}
	}
}
