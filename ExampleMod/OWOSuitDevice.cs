using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;
using OWOGame;

namespace OWOSuitMod;

/*
		constantTouch = SensationsFactory.Create(30, 0.5f, 100, 0, 0, 0);
		var signal = constantTouch.WithMuscles(leftAbs, rightAbs, leftDorsal, rightDorsal, leftPectoral, rightPectoral);
		OWO.Send(constantTouch);
*/

public class OWOSuitDevice : IInputDriver {
	private InputInterface _input;
	private HapticPoint _point;

	public int UpdateOrder => 0;

	public void CollectDeviceInfos(DataTreeList list) {
		DataTreeDictionary dataTreeDictionary = new DataTreeDictionary();
		dataTreeDictionary.Add<string>("Name", "OWO");
		dataTreeDictionary.Add<string>("Type", "Haptics");
		dataTreeDictionary.Add<string>("Model", "OWO Suit");
		list.Add(dataTreeDictionary);
	}

	private async Task FindDevices() {
		UniLog.Log("OWO Suit: Creating connection", false);

		OWO.Configure(GameAuth.Create().WithId("74604770"));
		await OWO.AutoConnect();
	}

	public void RegisterInputs(InputInterface inputInterface) {
		_input = inputInterface;
		var leftAbs = Muscle.Abdominal_L;
		var rightAbs = Muscle.Abdominal_R;
		var leftArm = Muscle.Arm_L;
		var rightArm = Muscle.Arm_R;
		var leftDorsal = Muscle.Dorsal_L;
		var rightDorsal = Muscle.Dorsal_R;
		var leftLumbar = Muscle.Lumbar_L;
		var rightLumbar = Muscle.Lumbar_R;
		var leftPectoral = Muscle.Pectoral_L;
		var rightPectoral = Muscle.Pectoral_R;

		Task.Run(() => FindDevices());
	}

	public void UpdateInputs(float deltaTime) {
		var connectionState = OWO.ConnectionState;
		if (connectionState == ConnectionState.Connected) {
			// TODO
		}
	}
}
