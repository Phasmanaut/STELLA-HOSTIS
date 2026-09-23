using UnityEngine;
using UnityEngine.InputSystem;

// One place to read the player's controls: the keyboard on desktop, a controller (see below), plus the
// on-screen joystick, fire and dodge buttons on phones and tablets (see MobileControls). Anything that
// moves, tilts, fires, or dodges reads from here.
//
// Controller: left stick or d-pad moves, A (Cross) or the right trigger fires, B (Circle) or either
// bumper dodges. Whichever controller was used last is the one that counts, and it can be plugged in mid-game.
public static class GameInput
{
    private const float PadDeadZone = 0.35f; //how far the stick has to move (out of 1) before it counts as a direction

    public static bool Right => Input.GetKey(KeyCode.D) || MobileControls.Stick.x > MobileControls.DeadZone || PadMove.x > PadDeadZone;
    public static bool Left => Input.GetKey(KeyCode.A) || MobileControls.Stick.x < -MobileControls.DeadZone || PadMove.x < -PadDeadZone;
    public static bool Up => Input.GetKey(KeyCode.W) || MobileControls.Stick.y > MobileControls.DeadZone || PadMove.y > PadDeadZone;
    public static bool Down => Input.GetKey(KeyCode.S) || MobileControls.Stick.y < -MobileControls.DeadZone || PadMove.y < -PadDeadZone;
    //Space or left mouse. With touch controls on, the mouse is ignored: phones (and the Device Simulator)
    //can report a tap as a click, which would fire no matter where you touched, even on the joystick.
    //(Not the old "Fire1" axis: it also listens to joystick button 0, which is Square on a PlayStation controller.)
    public static bool Fire => PadFire || (MobileControls.Active
        ? MobileControls.Fire || Input.GetKey(KeyCode.Space)
        : Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0));
    //Either shift key, the on-screen dodge button, or B / a bumper. Only true on the frame it's pressed, so holding it doesn't chain dodges
    public static bool DodgePressed => Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || MobileControls.DodgePressed || PadDodgePressed;

    //The stick and d-pad together, so either one steers
    private static Vector2 PadMove => Gamepad.current == null ? Vector2.zero : Gamepad.current.leftStick.ReadValue() + Gamepad.current.dpad.ReadValue();
    private static bool PadFire => Gamepad.current != null && (Gamepad.current.buttonSouth.isPressed || Gamepad.current.rightTrigger.isPressed);
    private static bool PadDodgePressed => Gamepad.current != null
        && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame);
}
