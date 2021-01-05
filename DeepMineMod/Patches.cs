using System;
using Assets.Scripts;
using Assets.Scripts.Voxel;
using HarmonyLib;

using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Serialization;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.Rendering;


namespace DeepMineMod
{
    [HarmonyPatch(typeof(Asteroid), "SetMineable", new Type[] { typeof(Vector4), typeof(Mineables), typeof(HashSet < ChunkObject >), typeof(int)})]
    public class Asteroid_SetMineable
    {
        static bool Prefix(Asteroid __instance, ref IReadOnlyCollection<ChunkObject> __result,
            Vector4 localPosition, Mineables mineable, HashSet<ChunkObject> chunks, int attempts )
        {
            System.Random _mineableRandom = (System.Random)__instance.GetType().GetField("_mineableRandom", BindingFlags.NonPublic
                | BindingFlags.Instance).GetValue(__instance);
            //UnityEngine.Debug.Log("Found _mineableRandom: " + _mineableRandom);
            bool flag = chunks == null;
            if (flag)
            {

                //m
                var _chunksInfo = __instance.GetType().GetField("_chunks", BindingFlags.NonPublic | BindingFlags.Instance);
                var _chunksVal = _chunksInfo.GetValue(__instance);
                var _chunksType = _chunksInfo.FieldType;
                var _chunksClearMethod = _chunksType.GetMethod("Clear");

              //  UnityEngine.Debug.Log("Invoking _chunks.Clear(): " );
                _chunksClearMethod.Invoke(_chunksVal, new object[] { });
                //UnityEngine.Debug.Log("Invoked _chunks.Clear()");
                chunks = (HashSet < ChunkObject > )_chunksVal;
            }

            //m
            //UnityEngine.Debug.Log("Invoking localPosition,ToGridRaw()" );
            Grid3 grid = new Grid3((int)localPosition.x, (int)localPosition.y, (int)localPosition.z);
            //UnityEngine.Debug.Log("Invoked localPosition,ToGridRaw()");
            //em
            mineable = (mineable ?? MiningManager.GenerateRandomMineableType(_mineableRandom));
            while (attempts < mineable.MaxVeinAttempts)
            {
                attempts++;
                bool flag2 = !VoxelGrid.IsPositionValid(grid, __instance.ChunkSize);
                if (flag2)
                {
                    Asteroid asteroid = __instance.ChunkController.GetChunk(__instance.LocalPosition + grid.ToVector3Raw()) as Asteroid;
                    bool flag3 = asteroid == null;
                    IReadOnlyCollection<ChunkObject> result;
                    if (flag3)
                    {
                        result = chunks;
                    }
                    else
                    {
                        
                        grid.ModuloCorrect(__instance.ChunkSize);
                        //UnityEngine.Debug.Log("Attempting voodoo");
                        Prefix(__instance, ref __result, localPosition, mineable, chunks, attempts);
                        //UnityEngine.Debug.Log("Voodoo success");
                        result = chunks;
                    }
                    __result = result;
                    return false;
                }
                chunks.Add(__instance);
                __instance.SetVoxel((byte)mineable.VoxelType, 1f, (short)grid.x, (short)grid.y, (short)grid.z, true);
                __instance.PopulateMineable(grid.ToVector3Raw(), mineable.VoxelType);
                bool flag4 = _mineableRandom.NextDouble() >= (double)(1f - mineable.VeinSize);
                if (flag4)
                {
                    while (attempts < mineable.MaxVeinAttempts)
                    {
                        Grid3 grid2;
                        do
                        {
                            grid2.x = _mineableRandom.Next(-1, 2);
                            grid2.y = _mineableRandom.Next(-1, 2);
                            grid2.z = _mineableRandom.Next(-1, 2);
                        }
                        while (grid2.x == 0 && grid2.y == 0 && grid2.z == 0);
                        Grid3 grid3 = grid + grid2;
                        Vector3 worldPosition = __instance.LocalPosition + grid3.ToVector3Raw();
                        ChunkObject chunk = ChunkController.World.GetChunk(worldPosition);
                        bool flag5 = chunk != null;
                        if (flag5)
                        {
                            Voxel voxelWorld = __instance.ChunkController.GetVoxelWorld(worldPosition);
                            bool flag6 = voxelWorld.GetDensityAsFloat() >= __instance.IsoLevel && voxelWorld.Type != byte.MaxValue;
                            if (flag6)
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
                    bool flag7 = attempts < mineable.MinVeinAttempts;
                    if (!flag7)
                    {
                        break;
                    }
                }
            }
            __result = chunks;
            return false;
        }
    }
}
