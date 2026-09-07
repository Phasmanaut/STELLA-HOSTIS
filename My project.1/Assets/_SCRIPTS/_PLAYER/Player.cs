using System;
using UnityEngine;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
public class Player : MonoBehaviour
{
    private float invincibilityTime = 0;
    public float invincibilityDuration = 0.3f;
    public float speed = 5f;//not a very good var name
    public float rotation = .05f;
    public string explosionType = "Player";
    

    public GameObject Explosion;
    private GameStats gameStats;
    

    public AudioClip hurt1;public AudioClip hurt2;public AudioClip hurt3;



    void Start()
    {
        transform.position = new Vector3(0, 0.5f, 0);
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();// gets the script from the object
    }
    public float maxRotationAngle = 20f; // Max rotation in degrees
    public float rotationSpeed = 100f; // Rotation speed
    public float returnSpeed = 200f; // Speed to return to neutral
    private float targetRotationY = 0f; // Target rotation around Y-axis

    public float acceleration = 30f; // How quickly the ship ramps up to full speed
    public float deceleration = 20f; // How quickly the ship slows down when you let go
    private float currentSpeed = 0f; // Current actual speed, eased toward the target each frame

    public float maxBankAngle = 15f; // Max roll/bank angle in degrees
    public float bankSpeed = 100f; // How quickly it banks into a turn
    public float bankReturnSpeed = 150f; // How quickly it levels back out when stopping
    private float targetRotationZ = 0f; // Target roll around Z-axis (the bank)

    void Update()
    {    // Check input keys
        bool right = Input.GetKey(KeyCode.D);
        bool left = Input.GetKey(KeyCode.A);

        bool up = Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.S);

        Vector3 pos = transform.position;

        if(invincibilityTime > 0) { invincibilityTime -= Time.deltaTime; }
        
        // Adjust target rotation and speed based on input, easing everything instead of snapping
        if (right && !left && pos.x <= 5.75)
        {
            targetRotationY = Mathf.Clamp(targetRotationY - rotationSpeed * Time.deltaTime, -maxRotationAngle, maxRotationAngle);
            targetRotationZ = Mathf.Clamp(targetRotationZ - bankSpeed * Time.deltaTime, -maxBankAngle, maxBankAngle);
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
        }
        else if (left && !right && pos.x >= -5.75)
        {
            targetRotationY = Mathf.Clamp(targetRotationY + rotationSpeed * Time.deltaTime, -maxRotationAngle, maxRotationAngle);
            targetRotationZ = Mathf.Clamp(targetRotationZ + bankSpeed * Time.deltaTime, -maxBankAngle, maxBankAngle);
            currentSpeed = Mathf.MoveTowards(currentSpeed, -speed, acceleration * Time.deltaTime);
        }
        else
        {
            // Return to neutral smoothly
            targetRotationY = Mathf.MoveTowards(targetRotationY, 0f, returnSpeed * Time.deltaTime);
            targetRotationZ = Mathf.MoveTowards(targetRotationZ, 0f, bankReturnSpeed * Time.deltaTime);
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }



        //vertical movement
        if (up && !down && pos.y <= 2)
        {
            pos.y += (speed*.75f)*Time.deltaTime;
        }
        else if (down && !up && pos.y >= .25)
        {
            pos.y -= (speed * .75f) * Time.deltaTime;
        }





        pos.x += currentSpeed * Time.deltaTime;

        // Apply rotation (yaw + bank) & movement to the player
        Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, targetRotationZ);
        transform.rotation = targetRotation;
        transform.position = pos;
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("EnemyProjectile") && invincibilityTime <= 0)
        {
            gameStats.PlayerHit();
            Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType;
            AudioClip[] hurtSounds = { hurt1, hurt2, hurt3 };
            AudioClip randomHurt = hurtSounds[UnityEngine.Random.Range(0, hurtSounds.Length)];
            AudioSource.PlayClipAtPoint(randomHurt, gameStats.transform.position, 1.0f);
            invincibilityTime = invincibilityDuration;
        }
    }
}