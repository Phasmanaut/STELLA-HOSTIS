using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
public class Player : MonoBehaviour
{
    public AudioClip hurt1; public AudioClip hurt2; public AudioClip hurt3;
    public GameObject Explosion;
    private GameStats gameStats;

    private float invincibilityTime = 0;
    public float invincibilityDuration = 0.3f;
    public float speed = 5f;//not a very good var name
    public float rotation = .05f;
    public string explosionType = "Player";

    [Header("Flight Shake")]
    public Transform shipModel;
    public float shakeAmount = 0.03f;   // how far it wobbles
    public float shakeSpeed = 8f;       // how fast the wobble cycles
    private float shakeSeed;            // offsets noise per-axis so they don't sync up

    [Header("Rotation and banking")]
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

    [Header("Dodge")]
    public float dodgeDistance = 1.5f;       // how much further the dodge throws you, on top of your normal movement
    public float dodgeDuration = 0.35f;      // how long the burst of movement lasts
    public float rollDuration = 0.5f;        // how long the barrel roll takes - a bit longer than the burst, so the spin is easy to follow
    public float dodgeInvincibility = 0.315f; // how long you can't be hurt, counted from the moment you dodge
    public float dodgeCooldown = 1.3f;       // shortest time between dodges, counted from the start of the last one
    private bool dodging;
    private float dodgeElapsed;
    private bool rolling;
    private float rollElapsed;
    private float dodgeCooldownTimer;
    private Vector2 dodgeDirection;          // zero if you dodged while standing still, which just rolls in place
    private float rollDirection;             // -1 rolls right, +1 rolls left, the same way the ship banks
    private Quaternion shipModelRestRotation;

    //The play area the ship is kept inside
    private const float EdgeX = 5.75f;
    private const float MinY = 0.25f;
    private const float MaxY = 2f;



    void Start()
    {
        shakeSeed = UnityEngine.Random.Range(0f, 1000f);
        if (shipModel != null) shipModelRestRotation = shipModel.localRotation; //the barrel roll spins it away from this and back

        transform.position = new Vector3(0, 0.5f, 0);
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();// gets the script from the object
    }


    void Update()
    {    // Check input keys
        bool right = GameInput.Right;
        bool left = GameInput.Left;

        bool up = GameInput.Up;
        bool down = GameInput.Down;

        Vector3 pos = transform.position;

        if(invincibilityTime > 0) { invincibilityTime -= Time.deltaTime; }
        if(dodgeCooldownTimer > 0) { dodgeCooldownTimer -= Time.deltaTime; }

        if (GameInput.DodgePressed && !dodging && dodgeCooldownTimer <= 0) { StartDodge(right, left, up, down); }

        // Adjust target rotation and speed based on input, easing everything instead of snapping
        if (right && !left && pos.x <= EdgeX)
        {
            targetRotationY = Mathf.Clamp(targetRotationY - rotationSpeed * Time.deltaTime, -maxRotationAngle, maxRotationAngle);
            targetRotationZ = Mathf.Clamp(targetRotationZ - bankSpeed * Time.deltaTime, -maxBankAngle, maxBankAngle);
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
        }
        else if (left && !right && pos.x >= -EdgeX)
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
        if (up && !down && pos.y <= MaxY)
        {
            pos.y += (speed*.75f)*Time.deltaTime;
        }
        else if (down && !up && pos.y >= MinY)
        {
            pos.y -= (speed * .75f) * Time.deltaTime;
        }

        pos.x += currentSpeed * Time.deltaTime;

        if (dodging)
        {
            Vector2 step = DodgeStep();
            //the dodge can't carry you out of the play area, but it doesn't snap you back in if you'd already drifted past the edge
            pos.x = Mathf.Clamp(pos.x + step.x, Mathf.Min(-EdgeX, pos.x), Mathf.Max(EdgeX, pos.x));
            pos.y = Mathf.Clamp(pos.y + step.y, Mathf.Min(MinY, pos.y), Mathf.Max(MaxY, pos.y));
        }

        // Apply rotation (yaw + bank) & movement to the player
        Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, targetRotationZ);
        transform.rotation = targetRotation;
        transform.position = pos;

        ApplyFlightShake();
        ApplyBarrelRoll();
    }

    //Throws the ship the way it was already heading, with a barrel roll and a moment of invincibility
    void StartDodge(bool right, bool left, bool up, bool down)
    {
        Vector2 direction = new Vector2(
            right && !left ? 1f : left && !right ? -1f : 0f,
            up && !down ? 1f : down && !up ? -1f : 0f);
        if (direction.x == 0f && Mathf.Abs(currentSpeed) > 0.1f) direction.x = Mathf.Sign(currentSpeed); //let go of the controls but still gliding

        dodgeDirection = direction.normalized;
        rollDirection = dodgeDirection.x > 0f ? -1f : 1f;
        dodging = true;
        dodgeElapsed = 0f;
        rolling = true;
        rollElapsed = 0f;
        dodgeCooldownTimer = dodgeCooldown;
        invincibilityTime = Mathf.Max(invincibilityTime, dodgeInvincibility); //shares the timer that stops you getting hit twice in a row
    }

    //How far the dodge moves you this frame
    Vector2 DodgeStep()
    {
        float before = DodgeProgress();
        dodgeElapsed += Time.deltaTime;
        if (dodgeElapsed >= dodgeDuration) dodging = false;
        return dodgeDirection * dodgeDistance * (DodgeProgress() - before);
    }

    //0 to 1 through the dodge: it bursts away fast, then settles back into normal flight
    float DodgeProgress()
    {
        return EaseOut(dodgeElapsed / dodgeDuration);
    }

    //Fast start, gentle finish
    static float EaseOut(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t) * (1f - t);
    }

    //Spins the ship model a full turn around its nose after a dodge. Only the model spins, so the collider and the gun aren't affected
    void ApplyBarrelRoll()
    {
        if (shipModel == null) return;

        float angle = 0f;
        if (rolling)
        {
            rollElapsed += Time.deltaTime;
            angle = rollDirection * 360f * EaseOut(rollElapsed / rollDuration);
            if (rollElapsed >= rollDuration) rolling = false;
        }
        shipModel.localRotation = Quaternion.AngleAxis(angle, Vector3.up) * shipModelRestRotation;
    }

    void ApplyFlightShake()
    {
        float t = Time.time * shakeSpeed;
        float x = (Mathf.PerlinNoise(t, shakeSeed) - 0.5f) * 2f * shakeAmount;
        float y = (Mathf.PerlinNoise(t, shakeSeed + 100f) - 0.5f) * 2f * shakeAmount;
        float z = (Mathf.PerlinNoise(t, shakeSeed + 200f) - 0.5f) * 2f * shakeAmount * 0.5f;

        if (shipModel != null)
        {
            shipModel.localPosition = new Vector3(x, y, z);
        }
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