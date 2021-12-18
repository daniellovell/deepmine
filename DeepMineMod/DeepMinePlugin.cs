using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using System;
using UnityEngine;

namespace DeepMineMod
{
    [BepInPlugin("com.dl.deepmine", "Deep Mine Mod", "0.2.2.0")]
    public class DeepMinePlugin : BaseUnityPlugin
    {
        private ConfigEntry<float> configBedrockDepth;
        private ConfigEntry<int> configGPRRange;
        private ConfigEntry<int> configOreStackSize;
        private ConfigEntry<float> configMineCompletionTime;
        private ConfigEntry<float> configMineAmount;

        public static float BedrockDepth;
        public static int GPRRange;
        public static int OreStackSize;
        public static float MineCompletionTime;
        public static float MineAmount;

        public static float[] DepthCurve;
        public static float[] DistanceCurve;

        public static void ModLog(string text)
        {
            UnityEngine.Debug.Log("[Deep Mine Mod] " + text);
        }

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            HandleConfig();

            ModLog("Successfully loaded Deep Mine Mod");
            Logger.LogInfo("Precalculating distributuion curves");
            ModLog("Patching...");
            var harmony = new Harmony("com.dl.deepmine");
            harmony.PatchAll();
            ModLog("Patched");
        }

        void PrecaluculateDistribution()
        {
            var maxDepth = (int)Math.Abs(BedrockDepth);
            var depthCurve = new float[maxDepth];
            for (int i = 0; i < maxDepth; i++) {
                depthCurve[i] = Utils.BezierInterp(0f, 1f, 10f, 25f, (float)i / maxDepth);
            }

            var maxDistance = 1024 * 32;
            var distanceCurve = new float[maxDistance];
            for (int i = 0; i < maxDistance; i++)
            {
                distanceCurve[i] = Utils.BezierInterp(0f, 0f, 25f, 25f, i / (1024f * 32f)); ;
            }
            DepthCurve = depthCurve;
            DistanceCurve = distanceCurve;
        }

        void HandleConfig()
        {
            configBedrockDepth = Config.Bind("General",   // The section under which the option is shown
                                     "BedrockDepth",  // The key of the configuration option in the configuration file
                                     -160f, // The default value
                                     "The depth of the bedrock layer, vanilla is -40"); // Description of the option to show in the config file

            BedrockDepth = configBedrockDepth.Value;

            configGPRRange = Config.Bind("General",   // The section under which the option is shown
                                     "GPRRange",  // The key of the configuration option in the configuration file
                                     40, // The default value
                                     "The range of the portable ground penetrating radar (GPR), vanilla is 20 "); // Description of the option to show in the config file

            GPRRange = configGPRRange.Value;

            configOreStackSize = Config.Bind("General",   // The section under which the option is shown
                                     "OreStackSize",  // The key of the configuration option in the configuration file
                                     100, // The default value
                                     "The maximum amount of ore to be stacked in the inventory, vanilla is 50"); // Description of the option to show in the config file

            OreStackSize = configOreStackSize.Value;


            configMineCompletionTime = Config.Bind("Mining Tool",   // The section under which the option is shown
                                     "MineCompletionTime",  // The key of the configuration option in the configuration file
                                     0.05f, // The default value
                                     "Time to complete mining when using the tool. Smaller is faster drilling. Vanilla is 0.12"); // Description of the option to show in the config file

            MineCompletionTime = configMineCompletionTime.Value;

            configMineAmount = Config.Bind("Mining Tool",   // The section under which the option is shown 
                         "MineAmount",  // The key of the configuration option in the configuration file
                         0.5f, // The default value
                         "How much of the voxel to mine at a time. Larger is faster drilling. Vanilla is 0.2"); // Description of the option to show in the config file

            MineAmount = configMineAmount.Value;
        }

    }
}
