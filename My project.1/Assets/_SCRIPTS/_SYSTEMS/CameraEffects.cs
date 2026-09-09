using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    public float rollAmount = 2f;
    public float pitchAmount = 2f;
    public float tiltSmoothing = 3f;

    private float currentRoll = 0f;
    private float currentPitch = 0f;

    void Update()
    {
        bool right = Input.GetKey(KeyCode.D);
        bool left = Input.GetKey(KeyCode.A);
        bool up = Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.S);

        float targetRoll = 0f;
        if (right && !left) targetRoll = -rollAmount;
        else if (left && !right) targetRoll = rollAmount;

        float targetPitch = 0f;
        if (up && !down) targetPitch = -pitchAmount;
        else if (down && !up) targetPitch = pitchAmount;

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * tiltSmoothing);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * tiltSmoothing);

        transform.localRotation = Quaternion.Euler(currentPitch, 0f, currentRoll);
    }
}
