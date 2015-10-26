using UnityEngine;
using System.Collections;

public static class Abilities 
{
	public enum AbilityType : int
	{
		none=-1,
		halfRoundTurn=0, // DONE pla, ai
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
		public const int gasDamage=20;
		public const float betweenDist=1f;
	}
	
	public static class RocketParameters
	{
		public const float minRange=2f;
		public const float maxRange=4f;
		public const float maxTurnAngle=55f;
		public const int damage=150;
		public const float startRange=8.0f;
	}
	
	public static class ThorpedeParameters
	{
		public const float minRange=3f;
		public const float maxRange=6f;
		public const float maxTurnAngle=35f;
		public const int damage=300;
		public const float startRange=8.0f;
	}
	
	public static class MinesParameters
	{
		public const float Range=0.5f;
		public const int Damage=20;
		public const float betweenDist=1f;
	}
}
