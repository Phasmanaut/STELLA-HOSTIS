using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStats : MonoBehaviour
{
    public int playerHealth;
    public int playerCharge;
    public bool playerAlive;

    public int level;
    public int levelEnemies;
    public int points;
    public int goo;
    public float timer;
    public bool timerActive;

    //UI
    public TextMeshProUGUI healthUI; public TextMeshProUGUI timerUI; public TextMeshProUGUI levelUI; public TextMeshProUGUI chargeUI; public TextMeshProUGUI pointsUI; public TextMeshPro screenText;

    //Goo meter
    public int maxGoo = 50;

    //Extra life (full goo bar) feedback
    public AudioClip extraLifeSound;
    public GameObject floatingPoints; //the same popup enemies use for their points, reused to float "EXTRA LIFE!" off the player

    public AudioClip startSound;
    public AudioClip loseSound;
    public AudioClip deathSound;
    public AudioClip Music;
    private AudioSource musicSource;
    public GameObject player;
    private GameObject playerInstance;

    //pixel guy animations
    public GameObject pixelguy1; public GameObject pixelguy2; public GameObject pixelguy3; public GameObject pixelguy4;

    //Startcube
    public GameObject startCube; private GameObject startCubeInstance; public ParticleSystem cube_die;

    //Between-level countdown
    public float levelCountdownDuration = 3f;

    public int demoFinalLevel = 9;

    //Time bonus: clear a level faster than its par time to earn points for every second left over
    private const float parBaseSeconds = 3f; //covers enemies flying in
    private const float parSecondsPerEnemy = 2f; //you can fire once a second, so par allows about every other shot to miss
    private const int bonusPointsPerSecond = 10;
    private float levelStartTime; //the run timer's value when the current level started
    private float levelParTime;

    //Enemies - level layouts and spawn points live on the EnemySpawner object
    private EnemySpawner enemySpawner;

    //Highscores: when the run ends, HighscoreEntry lets the player send their score to the website
    private bool runOver; //set once the run ends (death or clearing the last level), so nothing can end it twice or carry on after it
    public bool devModeUsed; //set by DebugTester; runs that used dev mode aren't sent to the highscore list

    void Start()
    {
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        playerAlive = true;
        timerActive = false;
        playerHealth = 3;
        playerCharge = 0;
        level = 0;
        points = 0;
        goo = 0;
        screenText.text = $"SHOOT THE CUBE TO START!";

        playerInstance = Instantiate(player);//instantiates the pref and assigns it so just the instance can be destroyed
        startCubeInstance = Instantiate(startCube);

        //Setup music loop
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.volume = 0.3f;
        musicSource.clip = Music;
        musicSource.loop = true;
        musicSource.Play();
        UpdatePixelguy();
    }


    void Update()
    {
        healthUI.text = $"{playerHealth} ";
        levelUI.text = $"Level: \n{level} ";
        chargeUI.text = $" {playerCharge} ";
        pointsUI.text = $" {points} ";

        if (playerAlive) //stops timer if player dies
        {
            if (timerActive)//makes sure time stops between levels
            {
                timerUI.text = $"Time: {Convert.ToInt32(timer)} ";
                timer += Time.deltaTime;
            }
        }


    }

    //Handles the very first level start, triggered by shooting the start cube
    public void StartLevelHit()
    {
        cube_die.Play();
        screenText.text = $" ";
        level++;
        LevelStart(level);
        AudioSource.PlayClipAtPoint(startSound, transform.position, 5.0f);
        Destroy(startCubeInstance);

    }


    public void LevelStart(int level)
    {
        timerActive = true;

        Console.WriteLine($"LEVEL STARTED: {level}");
        levelEnemies = enemySpawner.SpawnLevel(level);

        levelStartTime = timer;
        levelParTime = parBaseSeconds + parSecondsPerEnemy * levelEnemies;
    }


    public void EnemyDown(int pointGain)
    {
        points += pointGain;
        levelEnemies--;
        if (levelEnemies <= 0)
        {
            LevelEnd();
        }
    }

    public void CollectGoo(int amount)
    {
        goo += amount;

        //A full goo bar gives an extra life (for now - goo will get another use later)
        if (goo >= maxGoo)
        {
            goo -= maxGoo;
            playerHealth++;
            UpdatePixelguy();

            AudioSource.PlayClipAtPoint(extraLifeSound, transform.position, 1.0f);
            Instantiate(floatingPoints, playerInstance.transform.position, Quaternion.identity).GetComponent<FloatingPoints>().message = "EXTRA LIFE!";
        }
    }

    //Between-level transition countdown
    public void LevelEnd()
    {
        if (runOver) return; //a bullet still in flight finished off the level after the player died

        timerActive = false;

        int timeBonus = TimeBonus();
        points += timeBonus;

        if (level >= EnemySpawner.LastLevel)
        {
            WinGame(timeBonus);
            return;
        }
        StartCoroutine(LevelCountdown(timeBonus));
    }

    //Clearing the last level ends the run with a win
    void WinGame(int timeBonus)
    {
        runOver = true;
        string bonusText = timeBonus > 0 ? $"TIME BONUS +{timeBonus}\n" : "";
        screenText.text = $"{bonusText}YOU WIN!";
        StartCoroutine(OfferHighscore());
    }

    //Leaves the end screen up for a moment, then asks for initials to put the score on the website's highscore list
    IEnumerator OfferHighscore()
    {
        yield return new WaitForSeconds(3f);

        if (points <= 0) yield break; //nothing worth listing
        if (devModeUsed)
        {
            Debug.Log("[GameStats] Dev mode was used this run, so the score isn't sent to the highscore list.");
            yield break;
        }
        gameObject.AddComponent<HighscoreEntry>().Begin(screenText, points, level);
    }

    //Points for every whole second the level was cleared under its par time (0 if it took longer)
    int TimeBonus()
    {
        float secondsUnderPar = levelParTime - (timer - levelStartTime);
        return Mathf.Max(0, Mathf.RoundToInt(secondsUnderPar)) * bonusPointsPerSecond;
    }

    

    public void PlayerHit()
    {
        if (runOver) return; //stray bullets after the run ended (e.g. after winning) don't count

        CameraShake();

        if (playerHealth > 1)
        {
            playerHealth--;
            UpdatePixelguy();
        }
        else
        {
            playerHealth = 0;
            EndGame();
            UpdatePixelguy();
            AudioSource.PlayClipAtPoint(loseSound, transform.position, 1.0f);
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.0f);
        }
    }

    public void EndGame()
    {
        runOver = true;
        playerAlive = false;
        Destroy(playerInstance);
        musicSource.Stop();
        screenText.color = Color.red;
        screenText.fontSize = 40;
        screenText.text = $"GAME OVER";
        StartCoroutine(OfferHighscore());
    }

    public void UpdatePixelguy()
    {
        pixelguy1.SetActive(false);
        pixelguy2.SetActive(false);
        pixelguy3.SetActive(false);
        pixelguy4.SetActive(false);

        if (playerHealth >= 3) //extra lives from goo can take health past 3, so he stays at his healthiest look
        {
            pixelguy1.SetActive(true);
        }
        if (playerHealth == 2)
        {
            pixelguy2.SetActive(true);
        }
        if (playerHealth == 1)
        {
            pixelguy3.SetActive(true);
        }
        if (playerHealth == 0)
        {
            pixelguy4.SetActive(true);
        }

    }



    void CameraShake()
    {
        StartCoroutine(Shake(0.1f, 0.2f)); // duration, magnitude
    }


    // Coroutine: a function that can pause execution and resume later without freezing the game.
    // IEnumerator is the return type used by coroutines to track where execution should continue.
    ///////////////////////////////////////////////////////
    IEnumerator Shake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            cam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.localPosition = originalPos;
    }
    IEnumerator LevelCountdown(int timeBonus)
    {
        string bonusText = timeBonus > 0 ? $"TIME BONUS +{timeBonus}\n" : ""; //only mention the bonus if one was earned

        if (level == demoFinalLevel)
        {
            screenText.text = $"{bonusText}THANK YOU FOR PLAYING\nDEMO 0.2!";
            yield return new WaitForSeconds(5f);
        }
        else
        {
            int secondsLeft = Mathf.CeilToInt(levelCountdownDuration);
            while (secondsLeft > 0)
            {
                if (runOver) yield break; //the player died during the countdown
                screenText.text = $"{bonusText}LEVEL {level + 1} \nSTARTS IN {secondsLeft}";
                yield return new WaitForSeconds(1f);
                secondsLeft--;
            }
        }

        if (runOver) yield break; //the player died while waiting, so don't start the next level over the end screen

        screenText.text = $" ";
        level++;
        LevelStart(level);
        AudioSource.PlayClipAtPoint(startSound, transform.position, 5.0f);
    }

}