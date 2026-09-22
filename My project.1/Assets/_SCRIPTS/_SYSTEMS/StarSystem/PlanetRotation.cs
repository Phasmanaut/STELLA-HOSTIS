using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    private float rotationSpeed;

    void Start()
    {
        rotationSpeed = 4f;
    }

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
