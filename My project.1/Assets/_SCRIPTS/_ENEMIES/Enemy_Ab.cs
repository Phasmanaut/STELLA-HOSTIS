using UnityEngine;
public class Enemy_Ab : MonoBehaviour
{

    private GameStats gameStats;public GameObject projectile;public GameObject floatingPoints;public AudioClip death;public AudioClip shoot;public GameObject Explosion;

    private int points = 75;//more points
    private float speed = 1.5f; // faster than Enemy_A's 0.5f
    private float duration = 1.5f; // shorter than Enemy_A's 3f = fires more often

    private float timeElapsed = 0f;
    private bool moveRight = true;
    public string explosionType = "EnemyA";

    void Start()
    {
        Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType; //spawn Explosion
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
        timeElapsed += duration / 2; //head start to keep enemies centered

    }
    void Update() //idle movemets
    {
        if (gameStats.playerAlive) //stops all if player is dead
        {
            timeElapsed += Time.deltaTime;
            if (moveRight)
            {
                transform.Translate(speed * Time.deltaTime, 0, 0);
            }
            else
            {
                transform.Translate(-speed * Time.deltaTime, 0, 0);
            }
            if (timeElapsed >= duration)
            {
                moveRight = !moveRight; // Swap direction
                timeElapsed = 0f; // Reset timer     
                Instantiate(projectile, this.transform);
                AudioSource.PlayClipAtPoint(shoot, transform.position, 1.0f);
            }
        }
    }
    void OnTriggerEnter(Collider col)  //killed on hit
    {
        if (col.gameObject.tag == "Player Projectile")
        {
            Instantiate(floatingPoints, transform.position, Quaternion.identity).GetComponent<FloatingPoints>().pointWorth = points; //spawn points graphic
            Instantiate(Explosion, transform.position, Quaternion.identity).GetComponent<Explosion_Effect>().explosionType = explosionType; //spawn Explosion
            AudioSource.PlayClipAtPoint(death, transform.position, 1.0f);
            gameStats.EnemyDown(points);// pass points to gamestats
            Destroy(this.gameObject);
        }
    }
}
