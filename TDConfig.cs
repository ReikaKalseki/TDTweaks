using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.TDTweaks
{
	public class TDConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("Override Attack Interval Calculations With Threat-Based Curve In AttackTimeCurve.xml", false)]CONTROL_ATTACK_INTERVAL,
			[ConfigEntry("Minimum Attack Delay After CPH Loss", typeof(int), 900, 0, 86400, 0)]LOST_ATTACK_DELAY,
			[ConfigEntry("Subwave Size Multiplier", typeof(float), 1, 0.01F, 100, 1)]SUBWAVE_SCALE,
			[ConfigEntry("Subwave Mob Difficulty Ratio (Higher is harder per threat)", typeof(float), 3, 1, 10, 3)]SUBWAVE_DIFFICULTY,
			[ConfigEntry("Threat Reducer Effectivity", typeof(float), 1, 0, 100, 1)]CALMER_STRENGTH,
			[ConfigEntry("Threat Agitator Effectivity", typeof(float), 1, 0, 100, 1)]AGITATOR_STRENGTH,
			[ConfigEntry("Threat Agitator Mob Bonus", typeof(int), 5, 0, 1000, 5)]AGITATOR_BONUS,
			[ConfigEntry("Override EMP Nuker Spawn Conditions", true)]CONTROL_EMP_NUKERS,
			[ConfigEntry("Allow EMP nukers to spawn in all game modes", true)]ALLOW_EMP_NUKERS,
			[ConfigEntry("EMP nuker threat threshold", typeof(int), 1, 0, 20000, 1)]EMP_NUKER_THREAT,
			[ConfigEntry("Threat Per Bonus EMP Nuker", typeof(float), 2000, 1, 20000, 2000)]EMP_NUKER_RATIO,
			[ConfigEntry("Heavy Threat Threshold", typeof(int), 1667, 0, 20000, 1667)]HEAVY_THREAT,
			[ConfigEntry("Heavy Subwave Limit", typeof(int), 50, 1, 1000, 50)]HEAVY_LIMIT,
			[ConfigEntry("Heavy Mob Value", typeof(int), 5, 1, 1000, 5)]HEAVY_VALUE,
			[ConfigEntry("Boss Threat Threshold", typeof(int), 5833, 0, 20000, 5833)]BOSS_THREAT,
			[ConfigEntry("Boss Subwave Limit", typeof(int), 10, 1, 1000, 10)]BOSS_LIMIT,
			[ConfigEntry("Boss Mob Value", typeof(int), 29, 1, 1000, 29)]BOSS_VALUE,
			[ConfigEntry("Override Heavy And Boss Spawn Rate With Threat-Based Curve In HeavySpawnCurve.xml and BossSpawnCurve.xml; Overrides Threat/Limit Options", false)]HEAVYBOSS_CURVE,
			[ConfigEntry("Override EMP Nuker Spawn Count Per Subwave With Threat-Based Curve In EMPSpawnCurve.xml; Overrides Threat Options", false)]EMP_NUKER_CURVE,
			[ConfigEntry("Use Smarter Turret Target Selection", true)]TURRET_TARGETING,
			[ConfigEntry("Override TD Mob Health Calculations With Threat-Based Curve In HealthCurves.xml", false)]CONTROL_HEALTH,
		}
	}
}
