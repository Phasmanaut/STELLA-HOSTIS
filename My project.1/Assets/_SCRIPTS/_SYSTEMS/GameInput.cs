using UnityEngine;

// One place to read the player's controls: the keyboard on desktop, plus the on-screen joystick and
// fire button on phones and tablets (see MobileControls). Anything that moves, tilts, or fires reads from here.
public static class GameInput
{
    public static bool Right => Input.GetKey(KeyCode.D) || MobileControls.Stick.x > MobileControls.DeadZone;
    public static bool Left => Input.GetKey(KeyCode.A) || MobileControls.Stick.x < -MobileControls.DeadZone;
    public static bool Up => Input.GetKey(KeyCode.W) || MobileControls.Stick.y > MobileControls.DeadZone;
    public static bool Down => Input.GetKey(KeyCode.S) || MobileControls.Stick.y < -MobileControls.DeadZone;
    //Fire1 = space or left mouse. With touch controls on, the mouse is ignored: phones (and the Device Simulator)
    //can report a tap as a click, which would fire no matter where you touched, even on the joystick.
    public static bool Fire => MobileControls.Active
        ? MobileControls.Fire || Input.GetKey(KeyCode.Space)
        : Input.GetAxis("Fire1") > 0;
}
