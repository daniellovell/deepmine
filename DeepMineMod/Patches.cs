using System;
using Assets.Scripts;
using Assets.Scripts.Voxel;
using HarmonyLib;

using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects;

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
        }
    }

    /// <summary>
    /// Alter ore max stack size
    /// </summary>
    [HarmonyPatch(typeof(Thing), "LoadPrefabs")]
    public class Thing_LoadPrefabs
    {
        static void Postfix()
        {
            foreach (var orePrefab in Ore.AllOrePrefabs) {
                orePrefab.MaxQuantity = DeepMinePlugin.OreStackSize;
            }
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
        }
    }

    /// <summary>
    /// Prevents lava bedrock texture from spawning, potentially has insidious effect but unclear from initial tests
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
            __instance.MineCompletionTime = DeepMinePlugin.MineCompletionTime;
            __instance.MineAmount = DeepMinePlugin.MineAmount;
        }
    }

    /// <summary>
    /// Alter GPR Range
    /// </summary>
    [HarmonyPatch(typeof(PortableGPR), "Awake")]
    public class PortableGPR_Awake
    {
        static void Prefix(PortableGPR __instance)
        {
            __instance.Resolution = DeepMinePlugin.GPRRange;
        }
    }
}
