using System;
using UnityEngine;
using InControl;


namespace VirtualDeviceExample
{
	public class VirtualDevice : InputDevice
	{
		public VirtualDevice()
			: base( "Virtual Controller" )
		{
			// We need to add the controls we want to emulate here.
			// For this example we'll only have a left analog stick and four action buttons.
			AddControl( InputControlType.LeftStickLeft, "Left Stick Left" );
			AddControl( InputControlType.LeftStickRight, "Left Stick Right" );
			AddControl( InputControlType.LeftStickUp, "Left Stick Up" );
			AddControl( InputControlType.LeftStickDown, "Left Stick Down" );
			AddControl( InputControlType.Action1, "A" );
			AddControl( InputControlType.Action2, "B" );
			AddControl( InputControlType.Action3, "X" );
			AddControl( InputControlType.Action4, "Y" );
		}


		// This method will be called by the input manager every update tick.
		public override void Update( ulong updateTick, float deltaTime )
		{
			// Generate a vector to feed into the left stick.
			// This is just as an example, but could come from whatever source you want.
			var vector = GenerateRotatingVector();
			UpdateLeftStickWithValue( vector, updateTick, deltaTime );

			// Generate button presses to feed into action buttons.
			// This is just as an example, but could come from whatever source you want.
			var button = GenerateSequentialButtonPresses();
			UpdateWithState( InputControlType.Action1, button == 0, updateTick, deltaTime );
			UpdateWithState( InputControlType.Action2, button == 1, updateTick, deltaTime );
			UpdateWithState( InputControlType.Action3, button == 2, updateTick, deltaTime );
			UpdateWithState( InputControlType.Action4, button == 3, updateTick, deltaTime );

			// Commit the current state of all controls.
			// This may only be done once per update tick. Updates submissions (like those above)
			// can be done multiple times per tick (for example, to aggregate multiple sources) 
			// but must be followed by a single commit to submit the final value to the control.
			Commit( updateTick, deltaTime );
		}


		Vector2 GenerateRotatingVector()
		{
			var angle = Time.time;
			return new Vector2( Mathf.Cos( angle ), -Mathf.Sin( angle ) ).normalized;
		}


		int GenerateSequentialButtonPresses()
		{
			var slowTime = Time.time * 0.5f;
			// Get only the fractional component of the slowed time value.
			var fraction = slowTime - Mathf.Floor( slowTime );
			// Multiply by four to get a value in the range 0..4 and floor to an int.
			return Mathf.FloorToInt( fraction * 4.0f );
		}
	}
}

