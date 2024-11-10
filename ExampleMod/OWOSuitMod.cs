using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System;
using Elements.Core;

namespace OWOSuitMod;

public class OWOSuitMod : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0";
	public override string Name => "OWOSuit";
	public override string Author => "felikcat";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/felikcat/owosuit-resonite";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true);
	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<int> pectoral_r = new("100", "Right Pectoral -> Intensity");

	private static ModConfiguration Config;

	public override void OnEngineInit() {
		Config = GetConfiguration();
		Config.OnThisConfigurationChanged += OnConfigurationChanged;

		Harmony harmony = new Harmony("com.felikcat.OWOSuit");
		harmony.PatchAll();
	}

	private void OnConfigurationChanged(ConfigurationChangedEvent @event) {
		// TODO
	}
	
	[HarmonyPatch(typeof(InputInterface), MethodType.Constructor)]
	[HarmonyPatch(new[] { typeof(Engine) })]
	public class InputInterfaceCtorPatch {
		public static void Postfix(InputInterface __instance) {
			try {
				OWOSuitDevice suit = new();
				__instance.RegisterInputDriver(suit);
				UniLog.Log("OWOSuit registered: " + suit.ToString(), false);
			}
			catch (Exception e) {
				UniLog.Warning("OWOSuit failed to register!", false);
				UniLog.Warning(e.ToString());
			}
		}
	}
}
