using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Voxel;
using HarmonyLib;

using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Assets.Scripts.Objects.Items;
using System.Reflection.Emit;
using Assets.Scripts.GridSystem;
using HarmonyLib.Tools;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;

namespace DeepMineMod
{

    /// <summary>
    /// Alter ore drop quantities based on their world position
    /// </summary>
    [HarmonyPatch(typeof(Mineables), MethodType.Constructor, new Type[] { typeof(Mineables), typeof(Vector3), typeof(Asteroid)})]
    public class Mineables_Constructor
    {
        static void Postfix(Mineables __instance, Mineables masterInstance, Vector3 position, Asteroid parentAsteroid)
        {

            if(position.y > -10)
            {
                __instance.MinDropQuantity = 0;
                __instance.MaxDropQuantity = 1;
            }
            else if(position.y > -30)
            {
                __instance.MinDropQuantity = 2;
                __instance.MaxDropQuantity = 7;
            }

            else if (position.y > -60)
            {
                __instance.MinDropQuantity = 10;
                __instance.MaxDropQuantity = 20;
            }
            else if (position.y > -90)
            {
                __instance.MinDropQuantity = 10;
                __instance.MaxDropQuantity = 30;
            }
            else
            {
                __instance.MinDropQuantity = 20;
                __instance.MaxDropQuantity = 40;
            }
            //Debug.Log("Position: " + position.y + "  new drop quantities: " + __instance.MinDropQuantity + " " + __instance.MaxDropQuantity);
            //__instance.VeinSize = 
            //__instance.VeinSize = Math.Min(10, Math.Max(0, __instance.VeinSize));
            //Debug.Log(__instance.Position + __instance.DisplayName);
        }
    }

    /// <summary>
    /// Alter ore drop quantities based on their world position
    /// </summary>
    [HarmonyPatch(typeof(Ore), "Start")]
    public class Ore_Start
    {
        static void Postfix(Ore __instance)
        {
            __instance.MaxQuantity = 100;
        }
    }

    /// <summary>
    /// Placeholder for larger mining tools
    /// </summary>
    [HarmonyPatch(typeof(CursorVoxel), MethodType.Constructor)]
    public class CursorVoxel_Constructor
    {
        static void Postfix(CursorVoxel __instance)
        {
            Type typeBoxCollider = __instance.GameObject.GetComponent("BoxCollider").GetType();
            PropertyInfo prop = typeBoxCollider.GetProperty("size");
            //prop.SetValue(__instance.GameObject.GetComponent("BoxCollider"), new Vector3(5, 5, 5), null);
        }
    }

    /// <summary>
    /// Prevents lava bedrock texture from spawning
    /// </summary>
    [HarmonyPatch(typeof(TerrainGeneration), "SetUpChunk", new Type[] { typeof(ChunkObject) })]
    public class TerrainGeneration_SetUpChunk
    {
        static void Postfix(TerrainGeneration __instance, ref ChunkObject chunk)
        {
            chunk.MeshRenderer.sharedMaterial.SetVector("_WorldOrigin", WorldManager.OriginPositionLoading + new Vector3(0, 150, 0));
        }
    }

    /// <summary>
    /// Increase the bedrock level
    /// </summary>
    [HarmonyPatch(typeof(TerrainGeneration), "BuildAsteroidsStream", new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
    public class WorldManager_SetWorldEnvironments
    {
        static FieldInfo SizeOfWorld = AccessTools.Field(typeof(WorldManager), "SizeOfWorld");
        static FieldInfo HalfSizeOfWorld = AccessTools.Field(typeof(WorldManager), "HalfSizeOfWorld");
        static FieldInfo BedrockLevel = AccessTools.Field(typeof(WorldManager), "BedrockLevel");

        static void Prefix(TerrainGeneration __instance)
        {
            WorldManager.BedrockLevel = DeepMinePlugin.BedrockDepth;
            //WorldManager.SizeOfWorld = -(int)DeepMinePlugin.BedrockDepth / 2;
            //WorldManager.HalfSizeOfWorld = -(int)DeepMinePlugin.BedrockDepth / 4;
            WorldManager.LavaLevel = DeepMinePlugin.BedrockDepth;
        }
    }

    /// <summary>
    /// Enforcing new bedrock level during the world creation process
    /// </summary>
    [HarmonyPatch(typeof(Asteroid), "GenerateChunk", new Type[] { typeof(IReadOnlyCollection<Vector4>), typeof(uint), typeof(bool) })]
    public class Asteroid_GenerateChunk
    {
        static void Prefix(Asteroid __instance)
        {
            WorldManager.BedrockLevel = DeepMinePlugin.BedrockDepth;
        }
    }

    /// <summary>
    /// Increasing drill speed
    /// </summary>
    [HarmonyPatch(typeof(MiningDrill), "Awake")]
    public class MiningDrill_Awake
    {
        static void Prefix(MiningDrill __instance)
        {
            __instance.MineCompletionTime = 0.05f;
            __instance.MineAmount = 0.5f;
        }
    }

    /// <summary>
    /// Increasing drill speed
    /// </summary>
    [HarmonyPatch(typeof(PortableGPR), "Awake")]
    public class PortableGPR_Awake
    {
        static void Prefix(PortableGPR __instance)
        {
            __instance.Resolution = DeepMinePlugin.GPRRange;
        }
    }

    /// <summary>
    /// Hooks the Quarry (Auto Miner) OnRegistered event for modification in the future
    /// e.g. altering the quarry direction to horizontal rather than vertica
    /// </summary>
    [HarmonyPatch(typeof(Quarry), "OnRegistered", new Type[] { typeof(Cell) })]
    public class Quarry_OnRegistered
    {

        static void Prefix(Quarry __instance)
        {
            /*
            FieldInfo QuarryArea = AccessTools.Field(typeof(Quarry), "QuarryArea");
            Type typeBoxCollider = QuarryArea.GetType();
            PropertyInfo prop = typeBoxCollider.GetProperty("size");
            if(QuarryArea.GetValue(__instance) != null)
            {
                Vector3 s = (Vector3)prop.GetValue(QuarryArea.GetValue(__instance));
                DeepMinePlugin.ModLog(s.ToString());
            }
            */
        }
    }

    // WIP for altering ore generation in the SetMineable function
    // VeinSize does not help really, its 0-1 and 1 is the maximum ore size
    // Best to alter VeinAttempts at lower locations

    /*
    [HarmonyPatch(typeof(Asteroid), "SetMineable", new Type[] { typeof(Vector4), typeof(Mineables), typeof(HashSet < ChunkObject >), typeof(int)})]
    public class Asteroid_SetMineable
    {
        /**
        // Patch the Asteroid::SetMineable function (placing mineables on an asteroid, aka chunk)
        ///

        static FieldInfo VeinSizeFieldInfo = AccessTools.Field(typeof(Mineables), "VeinSize");

        static float CalculateVeinSize(Grid3 worldGrid, Vector3 asteroidPosition)
        {
            Vector3 worldPosition = worldGrid.ToVector3Raw() * ChunkObject.VoxelSize + asteroidPosition;
            float veinSize = worldPosition.y * (-16.67f) + 166.67f;
            //Debug.Log("Vein Y Position: " + worldPosition.y);
            veinSize = Math.Min(400, Math.Max(0, veinSize));
            Debug.Log("Position: " + worldPosition + "  VeinSize: " + veinSize);
            return veinSize;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            HarmonyFileLog.Enabled = true;
            bool v0found = false;
            var instructionList = new List<CodeInstruction>(instructions);
            LocalVariableInfo lvi = null;

            int c = 0;
            foreach(CodeInstruction ci in instructionList)
            {
                FileLog.Log(c + ": " + ci.opcode + " " + ci.operand);
                c++;
            }

            int searchCounter = 0; // When searchCounter == 3, we have found where to take write three instructions and skip two.
            bool insertFlag = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!v0found && (instructionList[i].operand is LocalVariableInfo))
                {
                    LocalVariableInfo tmp = (LocalVariableInfo)instructionList[i].operand;
                    if (tmp.LocalIndex == 0)
                    {
                        Debug.Log("V_0 LocalVariableInfo found!");
                        lvi = tmp;
                        v0found = true;
                    }
                }

                if (instructionList[i].opcode == OpCodes.Ldc_R4)
                {
                    searchCounter++;
                    if(searchCounter == 2)
                    {
                        yield return instructionList[i];
                        yield return new CodeInstruction(OpCodes.Ldloc_S, lvi);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(Asteroid).GetField("Position", BindingFlags.Public | BindingFlags.Instance));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Asteroid_SetMineable), "CalculateVeinSize"));
                        insertFlag = true;
                    }
                }
                
                if(insertFlag)
                {
                    i += 2;
                    insertFlag = false;
                }
                else
                    yield return instructionList[i];
            }
        }
    }
    */


}
