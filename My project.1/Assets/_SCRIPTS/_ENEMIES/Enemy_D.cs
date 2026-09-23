using System.Collections;
using UnityEngine;

// Enemy D: a fast, armoured strafer. It sweeps its row as far left and as far right as it can -
// the ends of its run are set by whichever slots its neighbours have left empty, so it gets more and more
// room as the row is cleared. It fires short bursts of fast, oversized, very inaccurate shots.
//
// It takes two hits. Surviving the first one enrages it: the subtle idle shake turns into a
// violent vibration and it sweeps noticeably faster.
public class Enemy_D : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints;
    public AudioClip death;
    public AudioClip shoot;
    public AudioClip hit; //played when it survives a hit
    public GameObject Explosion;
    public GameObject gooPrefab;

    private int points = 150;
    public string explosionType = "EnemyAb"; //placeholder: there's no D shrapnel effect yet, so it borrows Ab's

    [Header("Toughness")]
    public int health = 2;
    private bool enraged; //set once it has taken a hit and lived

    [Header("Movement")]
    public float speed = 3f;                  // fast, compared to A's 0.5 and Ab's 1.5
    public float enragedSpeedMultiplier = 1.8f;
    public float edgePause = 0.15f;           // tiny beat at each end of the sweep, so the turn reads
    public float looseRoamWidth = 4f;         // fallback half-width when it was spawned outside the grid (the dev tester does that)
    private float leftLimit;
    private float rightLimit;
    private int direction = 1;
    private float edgePauseTimer;

    [Header("Shake")]
    public float shakeAmount = 0.02f;        // subtle while it's healthy
    public float enragedShakeAmount = 0.09f; // and rattling itself apart once it's hurt
    public float shakeSpeed = 14f;
    public float enragedShakeSpeed = 26f;
    private float shakeSeed;                 // offsets the noise per-axis so they don't sync up

    [Header("Firing")]
    public int burstCount = 4;
    public float burstInterval = 0.07f;        // gap between shots inside one burst
    public float burstProjectileSpeed = 9f;    // faster than the A/Ab shots (5), but slow enough to react to
    public float burstProjectileScale = 1.6f;  // drawn bigger than everyone else's shots, so they're easy to spot
    public float burstSpread = 18f;            // and much sloppier
    public float minFireInterval = 2.5f;       // shortest possible time between bursts
    public float maxFireInterval = 4.5f;
    private float fireTimer;
    private float nextFireTime;
    private bool canFire = true;

    [Header("Entrance")]
    public float entryHeight = 6f; // how far above its spawn slot it flies in from
    public float entryAngleFactor = 0.6f; // sideways offset per unit of the slot's x, so it glides in at an angle
    public float baseEntryDuration = 1f; // how long the fly-in takes
    public float entryDurationPerY = 0.08f; // longer for higher spawn slots, shorter for lower ones
    public float entryDurationVariance = 0.15f; // random +/- duration so the formation doesn't fly in perfectly uniform
    private Vector3 spawnPosition;
    private Vector3 entryStartPosition;
    private float entryDuration;
    private float entryElapsed = 0f;
    private bool isEntering = true;

    private Vector3 anchor; // where it's meant to be, before the shake is added on top
    private int row;
    private int column;
    private bool hasSlot;

    void Start()
    {
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        shakeSeed = Random.Range(0f, 1000f);

        nextFireTime = Random.Range(minFireInterval, maxFireInterval);

        spawnPosition = transform.position;
        anchor = spawnPosition;
        entryStartPosition = spawnPosition + Vector3.up * entryHeight + Vector3.right * (spawnPosition.x * entryAngleFactor);
        transform.position = entryStartPosition;
        entryDuration = Mathf.Max(0.2f, baseEntryDuration + entryDurationPerY * spawnPosition.y + Random.Range(-entryDurationVariance, entryDurationVariance));

        if (EnemySpawner.Instance != null)
        {
            hasSlot = EnemySpawner.Instance.TryFindSlotOf(gameObject, out row, out column);
        }
        MeasureRow();
    }

    void Update()
    {
        if (isEntering)
        {
            entryElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(entryElapsed / entryDuration);
            float eased = t * t * (3f - 2f * t); // smoothstep: eases in, then decelerates into a smooth landing
            anchor = Vector3.Lerp(entryStartPosition, spawnPosition, eased);
            transform.position = anchor; //no shake until it has landed
            if (t >= 1f) isEntering = false;
            return;
        }

        if (!gameStats.playerAlive) return; //everything freezes when the player dies, like the other enemies

        Patrol();
        UpdateFiring();

        transform.position = anchor + ShakeOffset();
    }

    Vector3 ShakeOffset()
    {
        float amount = enraged ? enragedShakeAmount : shakeAmount;
        float t = Time.time * (enraged ? enragedShakeSpeed : shakeSpeed);
        float x = (Mathf.PerlinNoise(t, shakeSeed) - 0.5f) * 2f * amount;
        float y = (Mathf.PerlinNoise(t, shakeSeed + 100f) - 0.5f) * 2f * amount;
        return new Vector3(x, y, 0f); //stays on the play plane, so it never shakes itself out of depth
    }

    void Patrol()
    {
        if (edgePauseTimer > 0f)
        {
            edgePauseTimer -= Time.deltaTime;
            return;
        }

        anchor.x += direction * speed * (enraged ? enragedSpeedMultiplier : 1f) * Time.deltaTime;

        if (direction > 0 && anchor.x >= rightLimit)
        {
            anchor.x = rightLimit;
            TurnAround();
        }
        else if (direction < 0 && anchor.x <= leftLimit)
        {
            anchor.x = leftLimit;
            TurnAround();
        }
    }

    void TurnAround()
    {
        direction = -direction;
        edgePauseTimer = edgePause;
        MeasureRow(); //neighbours may have died since the last sweep, which opens up more room
    }

    //Works out how far it can run in each direction: out from its own slot across the empty ones, stopping
    //a slot short of the next enemy (that one sways, and would slide into it), or at the end of the row.
    void MeasureRow()
    {
        if (!hasSlot || EnemySpawner.Instance == null)
        {
            //spawned outside the formation, so there are no neighbours to measure against
            leftLimit = spawnPosition.x - looseRoamWidth;
            rightLimit = spawnPosition.x + looseRoamWidth;
            return;
        }

        EnemySpawner spawner = EnemySpawner.Instance;

        int leftColumn = column;
        while (spawner.IsSlotFree(row, leftColumn - 1) && !spawner.IsSlotTaken(row, leftColumn - 2)) leftColumn--;

        int rightColumn = column;
        while (spawner.IsSlotFree(row, rightColumn + 1) && !spawner.IsSlotTaken(row, rightColumn + 2)) rightColumn++;

        leftLimit = spawner.SlotPosition(row, leftColumn).x;
        rightLimit = spawner.SlotPosition(row, rightColumn).x;
    }

    void UpdateFiring()
    {
        if (!canFire) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= nextFireTime)
        {
            fireTimer = 0f;
            nextFireTime = Random.Range(minFireInterval, maxFireInterval); // rolls a new random interval
            StartCoroutine(FireBurst());
        }
    }

    //A rapid string of shots, each one aimed at the player but thrown well off by its own random angle
    IEnumerator FireBurst()
    {
        canFire = false;

        for (int i = 0; i < burstCount; i++)
        {
            Projectile_A shot = Instantiate(projectile, transform.position + Vector3.down * 0.3f, transform.rotation).GetComponent<Projectile_A>();
            shot.projSpeed = burstProjectileSpeed;
            shot.spreadAngle = burstSpread;
            Enlarge(shot.transform);
            AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);

            if (i < burstCount - 1) yield return new WaitForSeconds(burstInterval);
        }

        canFire = true;
    }

    //D shares its laser prefab with A, Ab and C, so it scales up its own shots as they're fired.
    //The laser's flames use local particle scaling, which ignores the parent, so each one is scaled as well
    void Enlarge(Transform shot)
    {
        shot.localScale *= burstProjectileScale;
        foreach (ParticleSystem particles in shot.GetComponentsInChildren<ParticleSystem>())
        {
            particles.transform.localScale *= burstProjectileScale;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag != "Player Projectile") return;

        health--;

        if (health > 0)
        {
            //Took it and kept going: from here on it rattles hard and sweeps its row faster
            enraged = true;
            AudioSource.PlayClipAtPoint(hit, transform.position, 1.0f);
            return;
        }

        Instantiate(floatingPoints, transform.position, Quaternion.identity).GetComponent<FloatingPoints>().pointWorth = points;
        Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType;
        AudioSource.PlayClipAtPoint(death, transform.position, 1.0f);

        gameStats.EnemyDown(points);
        GooSpawner.SpawnGoo(gooPrefab, transform.position, points);
        Destroy(this.gameObject);
    }
}
