using ResoniteModLoader;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using Microsoft.VisualBasic;
using System.Reflection.Emit;

namespace BadgeBuster;

public class BadgeBuster: ResoniteMod
{
    // TOOD: Correct Meta info
        internal const string VERSION_CONSTANT = "1.0.0";
        public override string Name => "BadgeBuster";
        public override string Author => "Dandrid";
        public override string Version => VERSION_CONSTANT;
        public override string Link => "https://github.com/Krzeszny/BadgeBuster";

        public static ModConfiguration Config;

	[AutoRegisterConfigKey]
	public static readonly ModConfigurationKey<bool> DisableForEveryone = new("Disable For Everyone", "When true, this will make all users in sessions hosted by you not generate badges.\n\nChanging this option will require everyone to respawn to apply properly.", () => true);
	[AutoRegisterConfigKey]
	public static readonly ModConfigurationKey<bool> DisableLocally = new("Disable Locally", "When true, this will make so when you have default nameplates enabled, those will not have badges.", () =>  true);


    public override void OnEngineInit() {
        Config = GetConfiguration();
        Config.Save(true);

        Harmony harmony = new Harmony("com.github.Krzeszny.BadgeBuster");
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(AvatarManager), nameof(AvatarManager.AddBadgeTemplate))]
    public class GlobalBadgeBuster
    {
        public static bool Prefix(Slot template)
        {
            if (!DisableForEveryone.Value) return true;

            template.Destroy();
            return false;
        }
    }

    [HarmonyPatch(typeof(UserRoot), "OnCommonUpdate")]
    public class LocalBadgeBuster
    {
        public static void Postfix(Slot ____localNameplate)
        {
            if(____localNameplate != null)
            {
                ____localNameplate.ForeachComponentInChildren<AvatarBadgeManager>(x =>
                {
                    x.Slot.ActiveSelf = !DisableLocally.Value;
                });
            }
        }
    }
}
