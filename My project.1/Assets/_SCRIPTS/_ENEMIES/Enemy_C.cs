using System.Collections;
using UnityEngine;

// Enemy C: a jittery gunner that refuses to sit still. It hovers in its formation slot with a
// constant nervous shake, fires a five-shot fan with random spread, and every few seconds it
// breaks formation - flying backwards out of the play area before swooping back down into
// whichever slot happens to be empty by then.
//
// It drives a separate "anchor" position instead of moving the transform directly, so the shake
// can be layered on top every frame without the two fighting each other.
public class Enemy_C : MonoBehaviour
{
    private GameStats gameStats;
    public GameObject projectile;
    public GameObject floatingPoints;
    public AudioClip death;
    public AudioClip shoot;
    public AudioClip retreat; //played when it breaks formation
    public GameObject Explosion;
    public GameObject gooPrefab;

    private int points = 100;
    public string explosionType = "EnemyB"; //placeholder: there's no C shrapnel effect yet, so it borrows B's

    [Header("Firing")]
    public int projectilesPerShot = 5;
    public float shotSpread = 25f;     // degrees of random aim error, rolled separately for each projectile
    public float minFireInterval = 2f; // shortest possible time between fans
    public float maxFireInterval = 3.5f;

    [Header("Shake")]
    public float shakeAmount = 0.06f; // how far it wobbles
    public float shakeSpeed = 9f;     // how fast the wobble cycles
    private float shakeSeed;          // offsets the noise per-axis so they don't sync up

    [Header("Repositioning")]
    public float holdDuration = 6f;        // how long it stays put before breaking formation
    public float holdVariance = 1.5f;      // random +/- so a row of them doesn't leave in lockstep
    public float retreatDistance = 9f;     // how far up it flies when it pulls out (far enough to clear the top of the screen)
    public float retreatBackDistance = 2f; // and how far into the background it drifts on the way
    public float retreatDuration = 0.9f;
    public float waitDuration = 0.6f;      // beat spent off screen before it comes back
    public float returnDuration = 1.1f;

    [Header("Entrance")]
    public float entryHeight = 6f; // how far above its spawn slot it flies in from
    public float entryAngleFactor = 0.6f; // sideways offset per unit of the slot's x, so it glides in at an angle
    public float baseEntryDuration = 1f; // how long the fly-in takes
    public float entryDurationPerY = 0.08f; // longer for higher spawn slots, shorter for lower ones
    public float entryDurationVariance = 0.15f; // random +/- duration so the formation doesn't fly in perfectly uniform

    private Vector3 anchor; // where it is meant to be, before the shake is added on top

    void Start()
    {
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        shakeSeed = Random.Range(0f, 1000f);

        anchor = transform.position; //the slot the spawner dropped it into, which it already holds

        StartCoroutine(Behaviour());
    }

    void Update()
    {
        if (!gameStats.playerAlive) return; //everything freezes when the player dies, like the other enemies

        transform.position = anchor + ShakeOffset();
    }

    Vector3 ShakeOffset()
    {
        float t = Time.time * shakeSpeed;
        float x = (Mathf.PerlinNoise(t, shakeSeed) - 0.5f) * 2f * shakeAmount;
        float y = (Mathf.PerlinNoise(t, shakeSeed + 100f) - 0.5f) * 2f * shakeAmount;
        return new Vector3(x, y, 0f); //stays on the play plane, so it never shakes itself out of depth
    }

    //The whole life of the enemy: fly in once, then loop hold -> retreat -> come back somewhere else
    IEnumerator Behaviour()
    {
        Vector3 entryStart = anchor + Vector3.up * entryHeight + Vector3.right * (anchor.x * entryAngleFactor);
        float entryDuration = Mathf.Max(0.2f, baseEntryDuration + entryDurationPerY * anchor.y + Random.Range(-entryDurationVariance, entryDurationVariance));
        yield return MoveOver(entryStart, anchor, entryDuration);

        while (true)
        {
            yield return Hold();
            yield return Retreat();
            yield return WaitAlive(waitDuration);
            yield return ReturnToFreeSlot();
        }
    }

    //Sits in its slot shooting until it is time to move on
    IEnumerator Hold()
    {
        float holdTime = Mathf.Max(0.5f, holdDuration + Random.Range(-holdVariance, holdVariance));
        float nextFireTime = Random.Range(minFireInterval, maxFireInterval);
        float elapsed = 0f;
        float fireTimer = 0f;

        while (elapsed < holdTime)
        {
            if (gameStats.playerAlive)
            {
                elapsed += Time.deltaTime;
                fireTimer += Time.deltaTime;

                if (fireTimer >= nextFireTime)
                {
                    fireTimer = 0f;
                    nextFireTime = Random.Range(minFireInterval, maxFireInterval); // rolls a new random interval
                    Fire();
                }
            }
            yield return null;
        }
    }

    //A fan of shots that all leave at once, each one aimed at the player but thrown off by its own random angle
    void Fire()
    {
        for (int i = 0; i < projectilesPerShot; i++)
        {
            Projectile_A shot = Instantiate(projectile, transform.position + Vector3.down * 0.3f, transform.rotation).GetComponent<Projectile_A>();
            shot.spreadAngle = shotSpread;
        }

        AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
    }

    //Breaks formation: gives up its slot and flies backwards off the top of the screen
    IEnumerator Retreat()
    {
        AudioSource.PlayClipAtPoint(retreat, transform.position, 1.0f);
        if (EnemySpawner.Instance != null) EnemySpawner.Instance.ReleaseSlot(gameObject); //its slot is up for grabs while it is away

        Vector3 exit = anchor + Vector3.up * retreatDistance + Vector3.forward * retreatBackDistance;
        yield return MoveOver(anchor, exit, retreatDuration);
    }

    //Drops back into whichever slot is empty now, or back where it left from if the formation is full
    IEnumerator ReturnToFreeSlot()
    {
        Vector3 target = anchor - Vector3.up * retreatDistance - Vector3.forward * retreatBackDistance; //undo the retreat, as a fallback

        if (EnemySpawner.Instance != null && EnemySpawner.Instance.TryFindFreeSlot(out int freeRow, out int freeColumn))
        {
            EnemySpawner.Instance.ClaimSlot(gameObject, freeRow, freeColumn); //claimed straight away so another C cannot pick the same one
            target = EnemySpawner.Instance.SlotPosition(freeRow, freeColumn);
        }

        //Line up above the new slot and glide down into it. This jump happens off screen, behind the top edge
        Vector3 start = target + Vector3.up * entryHeight + Vector3.right * (target.x * entryAngleFactor);
        anchor = start;
        yield return MoveOver(start, target, returnDuration);
    }

    //Eases the anchor from one point to another. Time only advances while the player is alive, so a
    //death mid-flight leaves it hanging there instead of finishing the move over the game over screen
    IEnumerator MoveOver(Vector3 from, Vector3 to, float duration)
    {
        anchor = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (gameStats.playerAlive) elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            anchor = Vector3.Lerp(from, to, t * t * (3f - 2f * t)); // smoothstep: eases in, then decelerates into a smooth landing
            yield return null;
        }

        anchor = to;
    }

    IEnumerator WaitAlive(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (gameStats.playerAlive) elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player Projectile")
        {
            Instantiate(floatingPoints, transform.position, Quaternion.identity).GetComponent<FloatingPoints>().pointWorth = points;
            Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType;
            AudioSource.PlayClipAtPoint(death, transform.position, 1.0f);

            gameStats.EnemyDown(points);
            GooSpawner.SpawnGoo(gooPrefab, transform.position, points);
            Destroy(this.gameObject);
        }
    }
}
