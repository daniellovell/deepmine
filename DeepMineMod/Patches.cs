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

namespace DeepMineMod
{
    
    [HarmonyPatch(typeof(Asteroid), "SetMineable", new Type[] { typeof(Vector4), typeof(Mineables), typeof(HashSet < ChunkObject >), typeof(int)})]
    public class Asteroid_SetMineable
    {
        /**
         * Patch the Asteroid::SetMineable function (placing mineables on an asteroid, aka chunk)
         */

        static FieldInfo VeinSizeFieldInfo = AccessTools.Field(typeof(Mineables), "VeinSize");
        static MethodInfo m_MyExtraMethod = SymbolExtensions.GetMethodInfo((Vector3 x) => CalculateVeinSize(x));

        static float CalculateVeinSize(Vector3 localPos)
        {
            float veinSize = localPos.y * (-16.67f) + 166.67f;
            veinSize = Math.Min(10, Math.Max(0, veinSize));
            Debug.Log("Transpiler calculated " + veinSize);
            return veinSize;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var foundMassUsageMethod = false;

            var instructionList = new List<CodeInstruction>(instructions);
            
            int startIndex;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].LoadsField(VeinSizeFieldInfo))
                {
                    startIndex = i - 1; // Back to nop@ IL012B
                    // Load V_0 (Grid3 world pos) onto the stack
                    instructionList[startIndex] = new CodeInstruction(OpCodes.Ldarg_1);
                    instructionList[startIndex + 1] = new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
                    
                }
            }

            return instructionList.AsEnumerable();
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
