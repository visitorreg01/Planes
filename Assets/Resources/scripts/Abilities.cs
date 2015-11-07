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
	
	public static class GasParameters
	{
		public const float gasRange=0.5f;
		public const float gasReuse=0.5f;
		public const int gasDamage=50;
		public const float betweenDist=2f;
	}
	
	public static class RocketParameters
	{
		public const float minRange=8f;
		public const float maxRange=20f;
		public const float maxTurnAngle=60f;
		public const int damage=150;
		public const float startRange=8.0f;
		public const float damageRange=3.0f;
	}
	
	public static class ThorpedeParameters
	{
		public const float minRange=6f;
		public const float maxRange=10f;
		public const float maxTurnAngle=25f;
		public const int damage=600;
		public const float startRange=8.0f;
		public const float damageRange=3.0f;
	}
	
	public static class MinesParameters
	{
		public const float Range=2.0f;
		public const int Damage=300;
		public const float betweenDist=3f;
	}
}
