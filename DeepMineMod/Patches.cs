using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Voxel;
using HarmonyLib;

using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Objects.Items;
using System.Reflection.Emit;
using Assets.Scripts.GridSystem;
using HarmonyLib.Tools;

namespace DeepMineMod
{
    
    [HarmonyPatch(typeof(Asteroid), "SetMineable", new Type[] { typeof(Vector4), typeof(Mineables), typeof(HashSet < ChunkObject >), typeof(int)})]
    public class Asteroid_SetMineable
    {
        /**
         * Patch the Asteroid::SetMineable function (placing mineables on an asteroid, aka chunk)
         */

        static FieldInfo VeinSizeFieldInfo = AccessTools.Field(typeof(Mineables), "VeinSize");

        static float CalculateVeinSize(Grid3 worldGrid, Vector3 asteroidPosition)
        {
            Vector3 worldPosition = worldGrid.ToVector3Raw() * ChunkObject.VoxelSize + asteroidPosition;
            float veinSize = worldPosition.y * (-16.67f) + 166.67f;
            Debug.Log("Vein Y Position: " + worldPosition.y);
            veinSize = Math.Min(400, Math.Max(0, veinSize));
            //Debug.Log("Transpiler calculated " + veinSize);
            return 0;
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

    [HarmonyPatch(typeof(Mineables), MethodType.Constructor, new Type[] { typeof(Mineables), typeof(Vector3), typeof(Asteroid)})]
    public class Mineables_Constructor
    {
        static void Postfix(Mineables __instance, Mineables masterInstance, Vector3 position, Asteroid parentAsteroid)
        {

            Debug.Log("Mineable vein size before: " + __instance.VeinSize);
            //__instance.VeinSize = position.y*(-16.67f) + 166.67f;
            //__instance.VeinSize = Math.Min(10, Math.Max(0, __instance.VeinSize));
            //Debug.Log(__instance.Position + __instance.DisplayName);
        }
    }


    [HarmonyPatch(typeof(CursorVoxel), MethodType.Constructor)]
    public class CursorVoxel_Constructor
    {
        static void Postfix(CursorVoxel __instance)
        {
            Type typeBoxCollider = __instance.GameObject.GetComponent("BoxCollider").GetType();
            PropertyInfo prop = typeBoxCollider.GetProperty("size");
            prop.SetValue(__instance.GameObject.GetComponent("BoxCollider"), new Vector3(5, 5, 5), null);
        }
    }

    [HarmonyPatch(typeof(MiningDrill), "Awake")]
    public class MiningDrill_Awake
    {
        static void Prefix(MiningDrill __instance)
        {
            __instance.MineCompletionTime = 0.05f;
            __instance.MineAmount = 0.5f;
        }
    }

    //[HarmonyPatch(typeof(TestClass), MethodType.Constructor, new Type[] { typeof(int) })]

}
