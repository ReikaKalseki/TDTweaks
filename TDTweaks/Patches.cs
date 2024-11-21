/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 04/11/2019
 * Time: 11:28 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.TDTweaks {
	
	[HarmonyPatch(typeof(MobSpawnManager))]
	[HarmonyPatch("GetBaseThreat")]
	public static class CalmerAgitatorMagnitudes {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldsfld, typeof(SegmentUpdater), "mnNumWaspAgitators");
				codes[idx+2].operand = InstructionHandlers.convertMethodOperand(typeof(TDTweaksMod), "getAgitatorStrength", false, new Type[0]);
				idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldsfld, typeof(SegmentUpdater), "mnNumWaspCalmers");
				codes[idx+2].operand = InstructionHandlers.convertMethodOperand(typeof(TDTweaksMod), "getCalmerStrength", false, new Type[0]);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	/* not necessary because the subwave redirect can bypass the vanilla one
	[HarmonyPatch(typeof(MobSpawnManager))]
	[HarmonyPatch("AddMobToSpawnWave")]
	public static class MobTypeThresholdsHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(MobSpawnManager), "mRand")));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(MobSpawnManager), "maMobsToSpawnThisSubWave")));
				codes.Add(InstructionHandlers.createMethodCall(typeof(TDTweaksMod), "addMobToSubWave", false, new Type[]{typeof(MobSpawnManager), typeof(System.Random), typeof(int).MakeArrayType()}));
				codes.Add(new CodeInstruction(OpCodes.Ret));				
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	*/
	[HarmonyPatch(typeof(MobSpawnManager))]
	[HarmonyPatch("ConfigureSubWaveMakeup")]
	public static class SubwaveRewrite {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(MobSpawnManager), "mRand")));
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(MobSpawnManager), "maMobsToSpawnThisSubWave")));
				codes.Add(InstructionHandlers.createMethodCall(typeof(TDTweaksMod), "configureSubWaveMakeup", false, new Type[]{typeof(MobSpawnManager), typeof(System.Random), typeof(int).MakeArrayType()}));
				codes.Add(new CodeInstruction(OpCodes.Ret));	
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(MobSpawnManager))]
	[HarmonyPatch("CalcNextTDTime")]
	public static class AttackIntervalHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				codes.Add(InstructionHandlers.createMethodCall(typeof(TDTweaksMod), "calcNextTDTime", false, new Type[]{typeof(MobSpawnManager)}));
				codes.Add(new CodeInstruction(OpCodes.Ret));	
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PopupTurretEntity))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class TurretTargetRewrite {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions/*, ILGenerator il*/) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				/*
				int start = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldsfld, typeof(MobManager), "instance");
				int end = InstructionHandlers.getInstruction(codes, start, 0, OpCodes.Stfld, typeof(PopupTurretEntity), "TargetsConsidered");
				end = InstructionHandlers.getInstruction(codes, end, 0, OpCodes.Ldloc_0)+1;
				FileLog.Log("Running targeting patch, which found anchors "+InstructionHandlers.toString(codes, start)+" and "+InstructionHandlers.toString(codes, end));
				codes.RemoveRange(start, end-start+1);
				FileLog.Log("Deletion of range successful, injecting new instructions");
				List<CodeInstruction> inject = new List<CodeInstruction>();
				Label lbl = il.DefineLabel();
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));  
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));
				inject.Add(InstructionHandlers.createMethodCall(typeof(TDTweaksMod), "computeTurretTarget", false, typeof(PopupTurretEntity)));
				inject.Add(new CodeInstruction(OpCodes.Dup));
				inject.Add(new CodeInstruction(OpCodes.Ldnull));
				inject.Add(new CodeInstruction(OpCodes.Ceq));
				inject.Add(new CodeInstruction(OpCodes.Brtrue_S, lbl));
				//consume the initial ldarg0 and dup here
				inject.Add(new CodeInstruction(OpCodes.Stfld, InstructionHandlers.convertFieldOperand(typeof(PopupTurretEntity), "Target"))); 
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));
				inject.Add(new CodeInstruction(OpCodes.Ldc_R4, 0));
				inject.Add(new CodeInstruction(OpCodes.Stfld, InstructionHandlers.convertFieldOperand(typeof(PopupTurretEntity), "ShootEffectTimer")));
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0)); 
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0)); 
				inject.Add(new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(PopupTurretEntity), "GapBetweenShots"))); 
				inject.Add(new CodeInstruction(OpCodes.Stfld, InstructionHandlers.convertFieldOperand(typeof(PopupTurretEntity), "SwingTimer"))); 
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));
				inject.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
				inject.Add(new CodeInstruction(OpCodes.Stfld, InstructionHandlers.convertFieldOperand(typeof(PopupTurretEntity), "mbDoShootEffect"))); 
				inject.Add(new CodeInstruction(OpCodes.Pop)); //clean up stack
				inject[inject.Count-1].labels.Add(lbl);

				FileLog.Log("Injecting "+inject.Count+" instructions: "+InstructionHandlers.toString(inject));
				codes.InsertRange(start, inject);
				*/
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, typeof(System.Random), "Next", true, new Type[]{typeof(int), typeof(int)});
				List<CodeInstruction> inject = new List<CodeInstruction>();
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));  
				inject.Add(new CodeInstruction(OpCodes.Ldloc_2));  
				inject.Add(InstructionHandlers.createMethodCall(typeof(TDTweaksMod), "getTurretPriority", false, new Type[]{typeof(int), typeof(PopupTurretEntity), typeof(MobEntity)}));
				codes.InsertRange(idx+2, inject); //after load V_4
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(TD_WaspMob))]
	[HarmonyPatch("InitHealth")]
	public static class WaspHealthHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, typeof(TD_WaspMob), "mnSwoopOffset")-2;
				//List<Label> from = codes[idx].labels;
				codes.InsertRange(idx, new List<CodeInstruction>(){
					new CodeInstruction(OpCodes.Ldarg_0),
					InstructionHandlers.createMethodCall(typeof(TDTweaksMod), "adjustWaspHealth", false, new Type[]{typeof(TD_WaspMob)}),
				});
				//codes[idx].labels.AddRange(from);
				//from.Clear();
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	static class Lib {
		
	}
}
