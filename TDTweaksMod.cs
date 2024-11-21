using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Threading;
using Harmony;
using ReikaKalseki.TDTweaks;
using ReikaKalseki.FortressCore;
using System.Xml;

namespace ReikaKalseki.TDTweaks
{
  public class TDTweaksMod : FCoreMod
  {
    public const string MOD_KEY = "ReikaKalseki.TDTweaks";
    public const string CUBE_KEY = "ReikaKalseki.TDTweaks_Key";
    
    private static Config<TDConfig.ConfigEntries> config;
    
    private static readonly AttackCurve attackTimeCurve = new AttackCurve(3600, 600);
    private static readonly AttackCurve heavySpawnCurve = new AttackCurve(0, 100);
    private static readonly AttackCurve bossSpawnCurve = new AttackCurve(0, 100);
    private static readonly AttackCurve empSpawnCurve = new AttackCurve(0, 100);
    
    private static readonly Dictionary<TD_WaspMob.eWaspType, int[]> defaultWaspHealths = new Dictionary<TD_WaspMob.eWaspType, int[]>{
    	{TD_WaspMob.eWaspType.Normal, new int[]{350, 350}},
    	{TD_WaspMob.eWaspType.Fast, new int[]{150, 150}},
    	{TD_WaspMob.eWaspType.EMP, new int[]{1750, 5750}},
    	{TD_WaspMob.eWaspType.Heavy, new int[]{5000, 33000}},
    	{TD_WaspMob.eWaspType.Boss, new int[]{312500, 612500}},
    	{TD_WaspMob.eWaspType.Robot, new int[]{750000, 750000}},
    };
    private static readonly Dictionary<TD_WaspMob.eWaspType, AttackCurve> waspHealthCurves = new Dictionary<TD_WaspMob.eWaspType, AttackCurve>();
    
    public TDTweaksMod() : base("TDTweaks") {
    	config = new Config<TDConfig.ConfigEntries>(this);
    }
    
    public static Config<TDConfig.ConfigEntries> getConfig() {
    	return config;
    }

    public override ModRegistrationData Register()
    {
        ModRegistrationData registrationData = new ModRegistrationData();      
                
        config.load();
        
        runHarmony();
        
        heavySpawnCurve.addPoint(config.getInt(TDConfig.ConfigEntries.HEAVY_THREAT), 0);
        bossSpawnCurve.addPoint(config.getInt(TDConfig.ConfigEntries.BOSS_THREAT), 0);
        
        loadAttackCurve(attackTimeCurve, "AttackTimeCurve");
        loadAttackCurve(heavySpawnCurve, "HeavySpawnCurve");
        loadAttackCurve(bossSpawnCurve, "BossSpawnCurve");
        loadAttackCurve(empSpawnCurve, "EMPSpawnCurve");
        
		XmlDocument doc = new XmlDocument();
    	string file = Path.Combine(Path.GetDirectoryName(modDLL.Location), "HealthCurves.xml");
        if (!File.Exists(file)) {
        	FUtil.log("Health curves do not exist, generating defaults.");
        	generateHealthDefaults(doc);
        	doc.Save(file);
        }
    	else {
    		doc.Load(file);
    	}
        
    	foreach (TD_WaspMob.eWaspType type in Enum.GetValues(typeof(TD_WaspMob.eWaspType)))
        	loadHealthCurve(type, doc.DocumentElement);
        
        return registrationData;
    }
    
    private void generateHealthDefaults(XmlDocument doc) {
    	XmlElement root = doc.CreateElement("Root");
    	doc.AppendChild(root);
    	foreach (TD_WaspMob.eWaspType type in Enum.GetValues(typeof(TD_WaspMob.eWaspType))) {
    		int[] def = defaultWaspHealths[type];
    		AttackCurve c = new AttackCurve(def[0], def[1]);
    		c.save(doc, root, type.ToString());
    	}
    }
    
    private void loadHealthCurve(TD_WaspMob.eWaspType type, XmlElement root) {
    	AttackCurve c = new AttackCurve(0, 0);
    	waspHealthCurves[type] = c;
    	FUtil.log("Loading health curve "+type.ToString());
		c.load(root, type.ToString());
    }
    
    private void loadAttackCurve(AttackCurve c, string name) {
    	string file = Path.Combine(Path.GetDirectoryName(modDLL.Location), name+".xml");
        if (!File.Exists(file)) {
        	FUtil.log("Attack curve does not exist, generating default.");
        	c.save(file);
        }
		FUtil.log("Loading attack curve "+name);
        c.load(file);
    }
    
    public static float getAgitatorStrength() {
    	return 0.1F*config.getFloat(TDConfig.ConfigEntries.AGITATOR_STRENGTH);
    }
    
    public static float getCalmerStrength() {
    	return 0.5F*config.getFloat(TDConfig.ConfigEntries.CALMER_STRENGTH);
    }
    
    public static float getEMPNukerThreatRatio() {
    	return config.getFloat(TDConfig.ConfigEntries.EMP_NUKER_RATIO)/100F; //since UI and thus config values are 100x internal
    }
    
    public static float getSubwaveSizeRatio() {
    	return config.getFloat(TDConfig.ConfigEntries.SUBWAVE_SCALE)*4F; //vanilla is 4
    }
    
    public static bool allowEMPNukers(bool orig) {
    	bool val = orig;
    	if (config.getBoolean(TDConfig.ConfigEntries.CONTROL_EMP_NUKERS))
    		val = config.getBoolean(TDConfig.ConfigEntries.ALLOW_EMP_NUKERS) && MobSpawnManager.mrSmoothedBaseThreat >= config.getInt(TDConfig.ConfigEntries.EMP_NUKER_THREAT);
    	return val;
    }	
    
    public static void configureSubWaveMakeup(MobSpawnManager mgr, System.Random rand, int[] spawns) {
		for (int i = 0; i < spawns.Length; i++)
			spawns[i] = 0;
		
		MobSpawnManager.mnSubWaveMobCount = (int)((MobSpawnManager.mnSubWaveLevel + 1) * getSubwaveSizeRatio());
		
		if (DifficultySettings.MobDifficulty == DifficultySettings.eMobDifficulty.Easy)
			MobSpawnManager.mnSubWaveMobCount /= 5;
		
		int num = SegmentUpdater.mnNumWaspAgitators * config.getInt(TDConfig.ConfigEntries.AGITATOR_BONUS);
		MobSpawnManager.mnSubWaveMobCount += num;
		for (int j = 0; j < MobSpawnManager.mnSubWaveMobCount; j++)
			addMobToSubWave(mgr, rand, spawns);
		
		int waspID = (int)MobType.TD_Wasp_Fast;		
		if (spawns[waspID] < 5)
			spawns[waspID] = 5;
		
		bool flag = false;
		if (DifficultySettings.mbAggressiveMobs || DifficultySettings.mbLucrativeMobs || DifficultySettings.mbImportantCPH)
			flag = true;
		else if (DifficultySettings.MobDifficulty == DifficultySettings.eMobDifficulty.Hard && (DLCOwnership.HasAdventuresPack() || DLCOwnership.HasPatreon()))
			flag = true;
		if (allowEMPNukers(flag))
			spawns[(int)MobType.TD_EMP] = getEMPSpawnCount();
		spawns[(int)MobType.TD_TunnelNuker] = 8; //why is this here?
		FUtil.log("Computed TD spawn counts @ threat "+(MobSpawnManager.mrSmoothedBaseThreat*100).ToString()+":\n[\n\t"+string.Join("\n\t", spawns.Where(amt => amt > 0).Select((amt, idx) => ((MobType)idx).ToString()+": "+amt).ToArray())+"\n]");
		
		MobSpawnManager.mnSubWaveMobCount = 0;
		for (int k = 0; k < spawns.Length; k++) {
			MobSpawnManager.mnSubWaveMobCount += spawns[k];
		}
	}
    
    private static int getEMPSpawnCount() {
    	if (config.getBoolean(TDConfig.ConfigEntries.EMP_NUKER_CURVE)) {
			int threat = MobSpawnManager.mnSubWaveLevel; //mnSubWaveLevel is threat
			return Mathf.CeilToInt(empSpawnCurve.getValue(threat));
    	}
    	return (int)(MobSpawnManager.mrSmoothedBaseThreat / getEMPNukerThreatRatio()) + 1;
    }
    
    public static void addMobToSubWave(MobSpawnManager mgr, System.Random rand, int[] spawnCounts) { //this is implemented weirdly and has what look like bugs but so is the vanilla one, and this mimics it
    	MobType mobType = MobType.TD_Wasp_Fast;
		int threat = MobSpawnManager.mnSubWaveLevel; //mnSubWaveLevel is threat
		float factor = config.getFloat(TDConfig.ConfigEntries.SUBWAVE_DIFFICULTY);
		if (config.getBoolean(TDConfig.ConfigEntries.HEAVYBOSS_CURVE)) {
			float heavyRate = heavySpawnCurve.getValue(threat)*factor/3F;
			float bossRate = bossSpawnCurve.getValue(threat)*factor/3F;
			int num = rand.Next(0, 100);
			if (num < bossRate) {
				mobType = MobType.TD_Wasp_Boss;
				MobSpawnManager.mnSubWaveMobCount -= config.getInt(TDConfig.ConfigEntries.BOSS_VALUE);
			}
			else if (num < heavyRate) {
				mobType = MobType.TD_Wasp_Heavy;
				MobSpawnManager.mnSubWaveMobCount -= config.getInt(TDConfig.ConfigEntries.HEAVY_VALUE);
			}
    	}
		else {
			int baseVal = 75;
			int heavyVal = (int)Math.Round(baseVal+config.getInt(TDConfig.ConfigEntries.HEAVY_THREAT)/100F*factor); // div by 100 since UI and thus config values are 100x internal
			int bossVal = (int)Math.Round(baseVal+config.getInt(TDConfig.ConfigEntries.BOSS_THREAT)/100F*factor);
			int num = baseVal;
			num += (int)(threat * factor);
			int num2 = rand.Next(0, num);
			if (num2 > baseVal) {
				mobType = MobType.TD_Wasp;
				MobSpawnManager.mnSubWaveMobCount--;
			}
			if (num2 > heavyVal) {
				if (spawnCounts[(int)MobType.TD_Wasp_Heavy] >= config.getInt(TDConfig.ConfigEntries.HEAVY_LIMIT)) {
					MobSpawnManager.mnSubWaveMobCount++;
					return;
				}
				mobType = MobType.TD_Wasp_Heavy;
				MobSpawnManager.mnSubWaveMobCount -= config.getInt(TDConfig.ConfigEntries.HEAVY_VALUE);
			}
			if (num2 > bossVal) {
				if (spawnCounts[(int)MobType.TD_Wasp_Boss] >= config.getInt(TDConfig.ConfigEntries.BOSS_LIMIT)) {
					MobSpawnManager.mnSubWaveMobCount++;
					return;
				}
				mobType = MobType.TD_Wasp_Boss;
				MobSpawnManager.mnSubWaveMobCount -= config.getInt(TDConfig.ConfigEntries.BOSS_VALUE);
			}
		}
		spawnCounts[(int)mobType]++;
    }
    
    public static void calcNextTDTime(MobSpawnManager mgr) {
    	if (config.getBoolean(TDConfig.ConfigEntries.CONTROL_ATTACK_INTERVAL)) {
    		MobSpawnManager.mrTDCountDown = attackTimeCurve.getValue((int)MobSpawnManager.mrSmoothedBaseThreat);
    	}
    	else {
	    	MobSpawnManager.mrTDCountDown = 3600f;
			MobSpawnManager.mrTDCountDown -= GameManager.mrPowerLastMin / 50f;
			MobSpawnManager.mrTDCountDown -= (float)(GameManager.mnBarsLastMin * 8);
			MobSpawnManager.mrTDCountDown -= (float)GameManager.mnOresLastMin / 50f;
			if (MobSpawnManager.mrTDCountDown < 600f)
				MobSpawnManager.mrTDCountDown = 600f;
			if (CentralPowerHub.Destroyed)
				MobSpawnManager.mrTDCountDown *= 2f;
			else
				MobSpawnManager.mrTDCountDown /= 4f;
			if (MobSpawnManager.mrTDCountDown > 3600f)
				MobSpawnManager.mrTDCountDown = 3600f;
    	}
    	if (CentralPowerHub.Destroyed)
    		MobSpawnManager.mrTDCountDown = Mathf.Max(MobSpawnManager.mrTDCountDown, config.getInt(TDConfig.ConfigEntries.LOST_ATTACK_DELAY));
		FUtil.log("Calculated TD gap time of " + MobSpawnManager.mrTDCountDown + " seconds");
    }
    
    public static int getTurretPriority(int orig, PopupTurretEntity turret, MobEntity e) {
    	if (!config.getBoolean(TDConfig.ConfigEntries.TURRET_TARGETING))
    		return orig;
    	if (e.mType == MobType.TD_Wasp) {
    		TD_WaspMob.eWaspType type = ((TD_WaspMob)e).mWaspType;
    		bool forceSmall = turret.mValue == 0;
    		bool preferSmall = turret.mValue <= 1;
    		bool preferNonBoss = turret.mValue <= 2;
    		switch(type) {
    			case TD_WaspMob.eWaspType.Normal:
    			case TD_WaspMob.eWaspType.Fast:
    				if (forceSmall)
    					return 101;
    				return preferSmall ? 90 : 10;
				case TD_WaspMob.eWaspType.Robot:
				case TD_WaspMob.eWaspType.EMP:
    				return preferSmall ? 75 : 25;
				case TD_WaspMob.eWaspType.Heavy:
    				if (forceSmall)
    					return -1;
    				return preferSmall ? 2 : 25;
				case TD_WaspMob.eWaspType.Boss:
    				if (forceSmall)
    					return -1;
    				return preferNonBoss ? 1 : 90;
    		}
    	}
    	return orig;
    }
    
    /*
    
    public static MobEntity computeTurretTarget(PopupTurretEntity turret) {
    	TurretPriorityList options = new TurretPriorityList(turret);
    	foreach (MobEntity e in MobManager.instance.mActiveMobs) {
    		if (e != null && isValidTarget(turret, e)) {
    			options.Add(e);
    		}
    	}
    	return options.Count == 0 ? null : options[0];
    }
    
	class TurretPriorityList : List<MobEntity>, IComparer<MobEntity> {
    	
    	private readonly PopupTurretEntity turret;
    	
    	internal TurretPriorityList(PopupTurretEntity e) {
    		turret = e;
    	}
    	
	    public new void Add(MobEntity entity) {
	        if (this.Count == 0) {
	            base.Add(entity);
	        }
	        else {
	            int index = this.BinarySearch(entity, this);
	            this.Insert(index < 0 ? ~index : index, entity);
	        }
	    }
    	
    	public int Compare(MobEntity e1, MobEntity e2) {
    		if (e1 is TD_WaspMob && e2 is TD_WaspMob) {
    			TD_WaspMob w1 = (TD_WaspMob)e1;
    			TD_WaspMob w2 = (TD_WaspMob)e2;
    			if (w1.mWaspType == w2.mWaspType) {
    				
    			}
    			else {
    				
    			}
    		}
    		else if (e1 is TD_WaspMob) {
    			return -1;
    		}
    		else if (e2 is TD_WaspMob) {
    			return 1;
    		}
    		else {
    			return 0;
    		}
    		return 0; //TODO
    	}
	}
    
    public static bool isValidTarget(PopupTurretEntity turret, MobEntity e) {
    	if (e.mnHealth <= 0)
    		return false;
    	if (e is TD_WaspMob && ((TD_WaspMob)e).mnRaycastLock > 0)
    		return false;
    	switch (e.mType) {
    		case MobType.TD_TunnelNuker:
    		case MobType.HiveConveyorMynock:
    		case MobType.TD_Wasp:
    			return true;
    		case MobType.OozeBlob:
    			return DifficultySettings.mbAggressiveMobs || e.mnHealth <= (float)e.mnStartHealth * 0.9F;
    		case MobType.CamoBot:
    			CamoBotMob cm = (CamoBotMob)e;
    			return cm.mState == CamoBotMob.State.SuckingPower || cm.AngerTime > 1f;
    		case MobType.WormBoss:
    		case MobType.WormBossLava:
    			return ((WormBoss)e).meState != WormBoss.eState.eHiding && ((WormBoss)e).meState != WormBoss.eState.eDead;
    	}
    	return true;
    }
    
    public static bool useCustomTurretLogic() {
    	return config.getBoolean(TDConfig.ConfigEntries.TURRET_TARGETING);
    }*/
    
    public static void adjustWaspHealth(TD_WaspMob mob) {
    	if (!config.getBoolean(TDConfig.ConfigEntries.CONTROL_HEALTH))
    		return;
    	AttackCurve curve;
    	if (waspHealthCurves.TryGetValue(mob.mWaspType, out curve)) {
    		mob.mnHealth = (int)curve.getValue((int)MobSpawnManager.mrSmoothedBaseThreat);
    	}
    }

  }
}
