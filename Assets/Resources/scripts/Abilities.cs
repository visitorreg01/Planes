using UnityEngine;
using System.Collections;

public static class Abilities 
{
	public enum AbilityType : int
	{
		none=-1,
		halfRoundTurn=0, // DONE pla, ai, ico
		turnAround=1, // DONE pla, ai
		doubleThrottle=2, // DONE pla, ai
		gas=3, // DONE pla, ai
		homingMissle=4, // DONE pla, ai
		shield=5, //DONE pla
		homingThorpede=6, // DONE pla, ai
		mines=7
	}
	
	public static int aiUseAbilityChance=100;
	public static float aiUse180360abilitiesRange=20f;
	
	public static bool getLockGun(AbilityType abil)
	{
		switch(abil)
		{
			case AbilityType.doubleThrottle:
				return false;
			case AbilityType.gas:
				return true;
			case AbilityType.halfRoundTurn:
				return false;
			case AbilityType.homingMissle:
				return true;
			case AbilityType.homingThorpede:
				return true;
			case AbilityType.mines:
				return false;
			case AbilityType.shield:
				return true;
			case AbilityType.turnAround:
				return false;
			default:
				return false;
		}
	}
	
	public static class GasParameters
	{
		public const float gasRange=4.0f;
		public const float gasReuse=0.5f;
		public const int gasDamage=50;
		public const float betweenDist=2f;
		public const int lifeTimeRounds=5;
	}
	
	public static class RocketParameters
	{
		public const float minRange=8f;
		public const float maxRange=25f;
		public const float maxTurnAngle=70f;
		public const int damage=150;
		public const float startRange=5.0f;
		public const float damageRange=4.0f;
		public const float lowerSmooth = 1f;
		public const float upperSmooth = 1f;
		public const int lifeTimeRounds=6;
	}
	
	public static class ThorpedeParameters
	{
		public const float minRange=6f;
		public const float maxRange=20f;
		public const float maxTurnAngle=55f;
		public const int damage=600;
		public const float startRange=3.0f;
		public const float damageRange=5.0f;
		public const float lowerSmooth = 1f;
		public const float upperSmooth = 1f;
		public const int lifeTimeRounds=6;
	}
	
	public static class MinesParameters
	{
		public const float Range=5.0f;
		public const int Damage=300;
		public const float betweenDist=3f;
		public const float boomSpeed=2;
		public const int lifeTimeRounds=3;
	}
}
