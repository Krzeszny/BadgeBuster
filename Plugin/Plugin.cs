using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using Microsoft.VisualBasic;
using System.Reflection.Emit;

namespace BadgeRemover;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
#nullable disable
    internal static new ManualLogSource Log;
    internal static ConfigEntry<bool> DisableForEveryone;
    internal static ConfigEntry<bool> DisableLocally;
#nullable enable

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {PluginMetadata.GUID} is loaded!");
        DisableForEveryone = Config.Bind("General", "DisableForEveryone", true, new ConfigDescription("When true, this will make all users in sessions hosted by you not generate badges.\n\nChanging this option will require everyone to respawn to apply properly."));
        DisableLocally = Config.Bind("General", "DisableLocally", true, new ConfigDescription("When true, this will make so when you have default nameplates enabled, those will not have badges."));

        HarmonyInstance.PatchAll();
    }

    [HarmonyPatch(typeof(AvatarManager), nameof(AvatarManager.AddBadgeTemplate))]
    public class GlobalBadgeRemover
    {
        public static bool Prefix(Slot template)
        {
            if (!DisableForEveryone.Value) return true;

            template.Destroy();
            return false;
        }
    }

    [HarmonyPatch(typeof(UserRoot), "OnCommonUpdate")]
    public class LocalBadgeRemover
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
