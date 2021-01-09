using System;
using BepInEx;
using Assets.Scripts;
using Assets.Scripts.Voxel;
using Assets.Scripts.Objects;
using HarmonyLib;
using HarmonyLib.Tools;

namespace DeepMineMod
{
    [BepInPlugin("org.bepinex.plugins.deepmine", "Deep Mine Mod", "0.1.0.0")]
    public class DeepMinePlugin : BaseUnityPlugin
    {
        public static void ModLog(string text)
        {
            UnityEngine.Debug.Log("[Deep Mine Mod] " + text);
        }

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            ModLog("Successfully loaded Deep Mine Mod");

            ModLog("Patching...");
            var harmony = new Harmony("com.dl.deepmine");
            harmony.PatchAll();
            ModLog("Patched");
        }

    }
}
