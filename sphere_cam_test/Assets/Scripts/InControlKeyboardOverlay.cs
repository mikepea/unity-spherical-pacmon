using System;
using System.Collections;
using UnityEngine;
using InControl;

public class InControlKeyboardOverlayProfile : UnityInputDeviceProfile
{
  public InControlKeyboardOverlayProfile() 
  {
    Name = "Keyboard Overlay";
    Meta = "Allow use of keyboard to control players";

    SupportedPlatforms = new[] {
      "Windows",
      "Mac",
      "Linux",
    };

    Sensitivity = 1.0F;
    LowerDeadZone = 0.0F;
    UpperDeadZone = 1.0F;
    
    ButtonMappings = new[]
    {
      new InputControlMapping {
        Handle = "DPadLeft alt",
        Target = InputControlType.DPadLeft,
        Source = KeyCodeButton( KeyCode.LeftArrow )
      },

      new InputControlMapping {
        Handle = "DPadRight alt",
        Target = InputControlType.DPadRight,
        Source = KeyCodeButton( KeyCode.RightArrow )
      },

      new InputControlMapping {
        Handle = "DPadUp alt",
        Target = InputControlType.DPadUp,
        Source = KeyCodeButton( KeyCode.UpArrow )
      },

      new InputControlMapping {
        Handle = "DPadDown alt",
        Target = InputControlType.DPadDown,
        Source = KeyCodeButton( KeyCode.DownArrow )
      },

    };


  }
}
