using System;
using System.Collections.Generic;

using Elements.Core;

using FrooxEngine;

using OWOGame;

using UnityEngine;

public class OWO_New_Driver : MonoBehaviour, IInputDriver {
	private class HapticPointData {
		public readonly HapticPoint point;

		public readonly Muscle muscle;

		public bool HasHints => point.Hints.Count > 0;

		public float LastMaxIntensity { get; private set; }

		public float ActivationIntensity { get; private set; }

		public bool IsActivated => ActivationIntensity > 0f;

		public HapticPointData(HapticPoint point, Muscle muscle) {
			this.point = point;
			this.muscle = muscle;
		}

		public void Sample() {
			point.SampleSources(0f, IsSupportedHint);
			LastMaxIntensity = MathX.Max(point.Force, point.Pain, point.Vibration);
			if (LastMaxIntensity < ActivationIntensity * 0.5f) {
				ActivationIntensity = 0f;
			}
		}

		public void Activate() {
			ActivationIntensity = LastMaxIntensity;
		}

		private static bool IsSupportedHint(string hint) {
			return SensationMap.Contains(hint.ToLower());
		}
	}

	private static readonly string[] Sensations;

	private static readonly HashSet<string> SensationMap;

	private InputInterface input;

	private List<HapticPointData> _points = new List<HapticPointData>();

	public int UpdateOrder => 0;

	private static int GetSensationIndex(string name) {
		return Sensations.FindIndex((string s) => s != null && name.EndsWith(s, StringComparison.InvariantCultureIgnoreCase));
	}

	static OWO_New_Driver() {
		Sensations = new string[25]
		{
			"Ball", "Dart", "DaggerWound", null, "ShotWithExit", null, "Axe", "Punch", "Grip", "Shot",
			"Insects", "FreeFall", "LoadObject", "LoadHeavyObject", "FastDriving", "DriveSlow", "InsectBites", "MachineGunshots", "PushingSomethingHeavy", "PushSomething",
			"SevereAbdominalWound", "SlightBleedingWound", "Oppression", "StrangePresence", "Tickle"
		};
		SensationMap = new HashSet<string>();
		string[] sensations = Sensations;
		foreach (string text in sensations) {
			if (text != null) {
				SensationMap.Add("owo." + text.ToLower());
			}
		}
	}

	public void CollectDeviceInfos(DataTreeList list) {
		DataTreeDictionary dataTreeDictionary = new DataTreeDictionary();
		dataTreeDictionary.Add("Name", "OWO Suit");
		dataTreeDictionary.Add("Type", "Haptics");
		dataTreeDictionary.Add("Model", "OWO");
		list.Add(dataTreeDictionary);
	}

	public void RegisterInputs(InputInterface inputInterface) {
		input = inputInterface;
		UniLog.Log("Initializing OWO!");
		OWO.Configure(GameAuth.Create().WithId("74604770"));
		OWO.AutoConnect();

		CreateHapticPoint(0.1f, Muscle.Pectoral_L, new TorsoHapticPointPosition(-0.5f, 0.75f, TorsoSide.Front));
		CreateHapticPoint(0.1f, Muscle.Pectoral_R, new TorsoHapticPointPosition(0.5f, 0.75f, TorsoSide.Front));
		CreateHapticPoint(0.08f, Muscle.Dorsal_L, new TorsoHapticPointPosition(-0.5f, 0.75f, TorsoSide.Back));
		CreateHapticPoint(0.08f, Muscle.Dorsal_R, new TorsoHapticPointPosition(0.5f, 0.75f, TorsoSide.Back));
		CreateHapticPoint(0.085f, Muscle.Abdominal_L, new TorsoHapticPointPosition(-0.5f, 0.25f, TorsoSide.Front));
		CreateHapticPoint(0.085f, Muscle.Abdominal_R, new TorsoHapticPointPosition(0.5f, 0.25f, TorsoSide.Front));
		CreateHapticPoint(0.085f, Muscle.Lumbar_L, new TorsoHapticPointPosition(-0.5f, 0.25f, TorsoSide.Back));
		CreateHapticPoint(0.085f, Muscle.Lumbar_R, new TorsoHapticPointPosition(0.5f, 0.25f, TorsoSide.Back));
		CreateHapticPoint(0.05f, Muscle.Arm_L, new ArmHapticPosition(Chirality.Left, 0.2f, 0f));
		CreateHapticPoint(0.05f, Muscle.Arm_R, new ArmHapticPosition(Chirality.Right, 0.2f, 0f));
		foreach (HapticPointData point in _points) {
			input.RegisterHapticPoint(point.point);
		}
	}

	private void CreateHapticPoint(float radius, Muscle muscle, HapticPointPosition position) {
		_points.Add(new HapticPointData(new HapticPoint(input, radius, position), muscle));
	}

	public void UpdateInputs(float deltaTime) {
		HapticPointData hapticPointData = null;
		foreach (HapticPointData point in _points) {
			point.Sample();
			if ((point.HasHints || hapticPointData == null || !hapticPointData.HasHints) && (hapticPointData == null || point.LastMaxIntensity > hapticPointData.LastMaxIntensity || (point.HasHints && !hapticPointData.HasHints))) {
				hapticPointData = point;
			}
		}

		if (hapticPointData.IsActivated || hapticPointData.LastMaxIntensity == 0f) {
			return;
		}

		hapticPointData.Activate();
		if (hapticPointData.HasHints) {
			HapticHint hapticHint = default(HapticHint);
			foreach (HapticHint hint in hapticPointData.point.Hints) {
				if (hint.intensity > hapticHint.intensity) {
					hapticHint = hint;
				}
			}

			var sensation = SensationsFactory.Create(GetSensationIndex(hapticHint.hint), 0.1f, (int)hapticHint.intensity);
			OWO.Send(sensation, hapticPointData.muscle);
			return;
		}

		float lastMaxIntensity = hapticPointData.LastMaxIntensity;
		string text = null;
		if (lastMaxIntensity == hapticPointData.point.Force) {
			text = ((lastMaxIntensity < 0.25f) ? "Dart" : ((!(lastMaxIntensity < 0.5f)) ? "Punch" : "Ball"));
		} else if (lastMaxIntensity == hapticPointData.point.Pain) {
			text = ((lastMaxIntensity < 0.25f) ? "Shot" : ((lastMaxIntensity < 0.5f) ? "ShotWithExit" : ((!(lastMaxIntensity < 0.75f)) ? "Insects" : "SevereAbdominalWound")));
		} else if (lastMaxIntensity == hapticPointData.point.Vibration) {
			text = ((lastMaxIntensity < 0.25f) ? "FastDriving" : ((!(lastMaxIntensity < 0.5f)) ? "LoadObject" : "FreeFall"));
		}

		if (text != null) {
			var sensation = SensationsFactory.Create(50, 0.1f, GetSensationIndex(text));
			OWO.Send(sensation, hapticPointData.muscle);
		}
	}
}
