using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using CodeX;
using System;
using System.Collections.Generic;



namespace HandsToYourself;
public class HandsToYourself : NeosMod
{
    public override string Author => "Cyro";
    public override string Name => "HandsToYourself";
    public override string Version => "1.0.0";

    public static ModConfiguration? Config;

    [AutoRegisterConfigKey]
    private static ModConfigurationKey<bool> GrabEnabled = new ModConfigurationKey<bool>("Grabbable", "Enable or disable the ability for other players to grab onto you.", () => true);
    public override void OnEngineInit()
    {
        Config = GetConfiguration();
        Config!.Save(true);
        Harmony harmony = new Harmony("net.Cyro.HandsToYourself");
        harmony.PatchAll();
        Config!.OnThisConfigurationChanged += (ConfigurationChangedEvent e) => ConfigChange(e.Config, e.Key, e.Label);
    }

    public static void ConfigChange(ModConfiguration Config, ModConfigurationKey Key, string? Label)
    {
        World CurrentWorld = Engine.Current.WorldManager.FocusedWorld;
        CurrentWorld.RunSynchronously(() => {
            Slot UserRootSlot = CurrentWorld.LocalUser.Root.Slot;
            LocomotionGrip grip = UserRootSlot.GetComponent<LocomotionGrip>();
            if (grip == null)
                grip = UserRootSlot.AttachComponent<LocomotionGrip>();
            
            NeosMod.Msg($"Key changed: {Config.GetValue(Key)}, Key Label: {Key.Name}");
            if (Config.GetValue(Key).GetType() == typeof(bool) && Key.Name == "Grabbable")
                grip.Enabled = (bool)Config.GetValue(Key);
        });
    }


    [HarmonyPatch(typeof(UserRoot), "OnAwake")]
    public static class UserRootPatch
    {
        public static void Postfix(UserRoot __instance)
        {
            __instance.RunInUpdates(3, () => {
                if (__instance.ActiveUser == __instance.LocalUser)
                {
                    LocomotionGrip grip = __instance.Slot.AttachComponent<LocomotionGrip>();
                    grip.Enabled = Config!.GetValue<bool>(GrabEnabled);    
                }
            });
            
        }
    }
}
