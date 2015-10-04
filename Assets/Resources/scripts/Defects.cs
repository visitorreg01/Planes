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
	
	public interface Defect{};
	
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
	
	public class DisableTurnLeft : Defect{};
	
	public class DisableTurnRight : Defect{};
	
	public class DisableTurn : Defect{};
	
	public class EngineCrash : Defect
	{
		public float newRangeCoeff=0.5f;
	}
	
	public class DisableGuns : Defect{};
	
	public class Fired : Defect
	{
		public float reuse=0.5f; // in seconds!
		public int damage=20; // total damage calculates: step time(3s)/reuse*damage
	}
}
