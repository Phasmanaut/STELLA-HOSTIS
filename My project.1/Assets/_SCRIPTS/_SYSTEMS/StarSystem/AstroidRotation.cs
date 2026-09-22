using UnityEngine;

public class AsteroidRotate : MonoBehaviour
{
    private float rotationSpeed;
    private Vector3 rotationAxis;

    void Start()
    {
        rotationSpeed = Random.Range(2f, 15f);      // random speed
        rotationAxis = Random.onUnitSphere;         // random 3D direction
    }

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
