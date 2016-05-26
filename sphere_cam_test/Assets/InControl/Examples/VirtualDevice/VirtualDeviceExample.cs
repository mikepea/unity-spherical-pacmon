using System;
using UnityEngine;
using InControl;


namespace VirtualDeviceExample
{
	// This example illustrates how to make a custom virtual controller for
	// the purpose of feeding custom input into InControl.
	//
	// A virtual device is necessary because InControl is device centric internally and
	// this allows custom input to interact naturally with other devices, whether it be
	// joysticks, touch controls, or whatever else. Custom input cannot be "force fed"
	// into other existing devices. A device is considered authoritative over the input
	// it provides and cannot be directly overriden. However, by creating your own
	// virtual device, you can provide whatever input you desire and you gain all the
	// benefits of being a first class device within InControl.
	//
	// This example creates a single, simple virtual device that generates input
	// automatically. For more advanced situations you may want to have a device
	// manager to organize multiple devices. For an example of how to accomplish this,
	// see XInputDeviceManager and XInputDevice.
	//
	public class VirtualDeviceExample : MonoBehaviour
	{
		VirtualDevice virtualDevice;


		void OnEnable()
		{
			virtualDevice = new VirtualDevice();

			// We hook into the OnSetup callback to ensure the device is attached
			// after the InputManager has had a chance to initialize properly.
			InputManager.OnSetup += () => InputManager.AttachDevice( virtualDevice );
		}


		void OnDisable()
		{
			InputManager.DetachDevice( virtualDevice );
		}


		void Update()
		{
			// Use last device which provided input.
			var inputDevice = InputManager.ActiveDevice;

			// Rotate target object to reflect left stick angle.
			transform.rotation = Quaternion.AngleAxis( inputDevice.LeftStick.Angle, Vector3.back );

			// Get color based on action button pressed.
			var color = Color.white;
			if (inputDevice.Action1.IsPressed)
			{
				color = Color.green;
			}
			if (inputDevice.Action2.IsPressed)
			{
				color = Color.red;
			}
			if (inputDevice.Action3.IsPressed)
			{
				color = Color.blue;
			}
			if (inputDevice.Action4.IsPressed)
			{
				color = Color.yellow;
			}
			GetComponent<Renderer>().material.color = color;
		}
	}
}

