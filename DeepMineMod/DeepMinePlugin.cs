using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace DeepMineMod
{
    [BepInPlugin("com.dl.deepmine", "Deep Mine Mod", "0.1.0.0")]
    public class DeepMinePlugin : BaseUnityPlugin
    {
        private ConfigEntry<float> configBedrockDepth;

        public static float BedrockDepth;

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
                                     "The depth of the bedrock layer"); // Description of the option to show in the config file

            BedrockDepth = configBedrockDepth.Value;
        }

    }
}
