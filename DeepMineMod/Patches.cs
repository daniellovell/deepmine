using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Util;
using Assets.Scripts.Voxel;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace DeepMineMod
{

    /// <summary>
    /// Alter ore drop quantities based on their world position
    /// </summary>
    [HarmonyPatch(typeof(Ore), "Start")]
    public class Ore_Start
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

    [HarmonyPatch(typeof(Asteroid), "SetMineable", new Type[] { typeof(Vector4), typeof(Mineables), typeof(HashSet<ChunkObject>), typeof(int) })]
    public static class Asteroid_SetMineable
    {
        static (int, int) CalculateMaxVeinAttempts(Mineables mineable, Grid3 worldGrid, Vector3 asteroidPosition)
        {
            Vector3 worldPosition = worldGrid.ToVector3Raw() * ChunkObject.VoxelSize + asteroidPosition;
            var distanceFromSpawn = Vector3.Distance(worldPosition, new Vector3(0f, worldPosition.y, 0f));
            float depth = 0f - worldPosition.y;
            depth = depth > 0f ? depth : 0f;
            float depthWeight = 0.1f;
            float distanceWeight = 0.01f;
            float depthMult = DeepMinePlugin.DepthCurve[(int)depth];
            float distanceMult = DeepMinePlugin.DistanceCurve[(int)distanceFromSpawn];
            float multiplier = 1f + (depthMult * depthWeight) + (distanceMult * distanceWeight);
            // Debug.Log($"Pos: {worldPosition}, Depth: {depth}, Dist: {distanceFromSpawn}, depth_mult: {depthMult}, dist_mult: {distanceMult}, total: {multiplier}");
            int min = (int)Math.Ceiling(mineable.MinVeinAttempts * multiplier);
            int max = (int)Math.Ceiling(mineable.MaxVeinAttempts * multiplier);
            return (min, max);
        }

        static Mineables GetRandomMineableType(System.Random random, float depth)
        {
            depth = Math.Abs(depth);
            int depth_int;
            if (depth > 30)
            {
                depth_int = 1;
            }
            else if (depth > 60)
            {
                depth_int = 2;
            }
            else if (depth >= 90)
            {
                depth_int = 3;
            }
            else
            {
                depth_int = 0;
            }
            if (DeepMinePlugin.MineralDistributions[depth_int] == null)
            {
                DeepMinePlugin.ModLog("Calculating mineable distribution");
                var mineables = MiningManager.MineableTypes.Values;
                for (int i = 0; i < DeepMinePlugin.DISTRIBUTION_LEVELS; i++)
                {
                    Func<Mineables, float> getWeight = (m) =>
                    {
                        var rarity = m.Rarity;
                        if (rarity < 25)
                        {
                            rarity *= i;
                        }
                        //DeepMinePlugin.ModLog($"{m.DisplayName}.Rarity: {rarity}");
                        return rarity;
                    };
                    DeepMinePlugin.MineralDistributions[i] = Utils.WalkerMethod<Mineables>.Ctor(mineables, getWeight);
                }
            }

            return DeepMinePlugin.MineralDistributions[depth_int].Random(random);
        }

        public static bool Prefix(Asteroid __instance, Vector4 localPosition, ref IReadOnlyCollection<ChunkObject> __result, Mineables mineable = null, HashSet<ChunkObject> chunks = null, int attempts = 0)
        {
            if (chunks == null)
            {
                __instance._chunks.Clear();
                chunks = __instance._chunks;
            }
            Grid3 grid = ((Vector3)localPosition).ToGridRaw();
            mineable = (mineable ?? GetRandomMineableType(__instance._mineableRandom, grid.y));
            var (minVeinAttempts, maxVeinAttempts) = CalculateMaxVeinAttempts(mineable, grid, __instance.Position);
            while (attempts < maxVeinAttempts)
            {
                attempts++;
                if (!VoxelGrid.IsPositionValid(grid, __instance.ChunkSize))
                {
                    Asteroid asteroid = __instance.ChunkController.GetChunk(__instance.LocalPosition + grid.ToVector3Raw()) as Asteroid;
                    if (asteroid != null)
                    {
                        grid.ModuloCorrect(__instance.ChunkSize);
                        asteroid.SetMineable(new Vector4((float)grid.x, (float)grid.y, (float)grid.z, localPosition.w), mineable, chunks, attempts);
                    }
                    __result = chunks;
                    return false;
                }
                chunks.Add(__instance);
                __instance.SetVoxel((byte)mineable.VoxelType, 1f, (short)grid.x, (short)grid.y, (short)grid.z, true);
                __instance.PopulateMineable(grid.ToVector3Raw(), mineable.VoxelType);
                if (__instance._mineableRandom.NextDouble() >= (double)(1f - mineable.VeinSize))
                {
                    while (attempts < maxVeinAttempts)
                    {
                        Grid3 grid2;
                        do
                        {
                            grid2.x = __instance._mineableRandom.Next(-1, 2);
                            grid2.y = __instance._mineableRandom.Next(-1, 2);
                            grid2.z = __instance._mineableRandom.Next(-1, 2);
                        }
                        while (grid2.x == 0 && grid2.y == 0 && grid2.z == 0);
                        Grid3 grid3 = grid + grid2;
                        Vector3 worldPosition = __instance.LocalPosition + grid3.ToVector3Raw();
                        ChunkObject chunk = ChunkController.World.GetChunk(worldPosition);
                        if (chunk != null)
                        {
                            Voxel voxelWorld = __instance.ChunkController.GetVoxelWorld(worldPosition);
                            if (voxelWorld.GetDensityAsFloat() >= __instance.IsoLevel && voxelWorld.Type != byte.MaxValue)
                            {
                                grid = grid3;
                                break;
                            }
                        }
                        attempts++;
                    }
                }
                else
                {
                    if (attempts >= minVeinAttempts)
                        break;
                }
            }
            __result = chunks;
            return false;
        }
    }
}
