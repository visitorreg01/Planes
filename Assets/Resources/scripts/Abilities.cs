﻿using UnityEngine;
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
				return false;
			case AbilityType.halfRoundTurn:
				return false;
			case AbilityType.homingMissle:
				return false;
			case AbilityType.homingThorpede:
				return false;
			case AbilityType.mines:
				return false;
			case AbilityType.shield:
				return false;
			case AbilityType.turnAround:
				return false;
			default:
				return true;
		}
	}
	
	public static class GasParameters
	{
		public const float gasRange=0.5f;
		public const float gasReuse=0.5f;
		public const int gasDamage=50;
		public const float betweenDist=2f;
		public const int lifeTimeRounds=3;
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
		public const float Range=2.0f;
		public const int Damage=300;
		public const float betweenDist=3f;
	}
}
