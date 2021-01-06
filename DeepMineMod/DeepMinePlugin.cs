using System;
using BepInEx;
using Assets.Scripts;
using Assets.Scripts.Voxel;
using Assets.Scripts.Objects;
using HarmonyLib;

namespace DeepMineMod
{
    [BepInPlugin("org.bepinex.plugins.deepmine", "Deep Mine Mod", "1.0.0.0")]
    public class DeepMinePlugin : BaseUnityPlugin
    {

        void Log(string text)
        {
            UnityEngine.Debug.Log("[Deep Mine Mod] " + text);
        }
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            Log("Successfully loaded Deep Mine Mod");

            Log("Patching...");
            var harmony = new Harmony("com.dl.deepmine"); // rename "author" and "project"
            harmony.PatchAll();
            Log("Patched");
        }

    }
}
