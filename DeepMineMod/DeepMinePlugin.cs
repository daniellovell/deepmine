using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace DeepMineMod
{
    [BepInPlugin("com.dl.deepmine", "Deep Mine Mod", "0.2.2.0")]
    public class DeepMinePlugin : BaseUnityPlugin
    {
        private ConfigEntry<float> configBedrockDepth;
        private ConfigEntry<int> configGPRRange;
        private ConfigEntry<int> configOreStackSize;

        public static float BedrockDepth;
        public static int GPRRange;
        public static int OreStackSize;

        public static void ModLog(string text)
        {
            UnityEngine.Debug.Log("[Deep Mine Mod] " + text);
        }

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            HandleConfig();

            ModLog("Successfully loaded Deep Mine Mod");
            ModLog("Patching...");
            var harmony = new Harmony("com.dl.deepmine");
            harmony.PatchAll();
            ModLog("Patched");
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
        }

    }
}
