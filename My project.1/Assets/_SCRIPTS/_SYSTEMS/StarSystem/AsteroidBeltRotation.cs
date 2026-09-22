using UnityEngine;
public class AsteroidBeltRotation : MonoBehaviour
{
    private float rotationSpeed;

    void Start()
    {
        rotationSpeed = 2f;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}