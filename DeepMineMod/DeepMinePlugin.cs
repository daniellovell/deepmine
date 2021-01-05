using System;
using BepInEx;
using Assets.Scripts.Voxel;
using HarmonyLib;

namespace DeepMineMod
{
    [BepInPlugin("org.bepinex.plugins.deepmine", "Deep Mine Mod", "1.0.0.0")]
    public class DeepMinePlugin : BaseUnityPlugin
    {
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            UnityEngine.Debug.Log("Hello, world!");
            print(WorldManager.BedrockLevel.ToString());

            var harmony = new Harmony("com.dl.deepmine"); // rename "author" and "project"
            harmony.PatchAll();
        }

    }
}
