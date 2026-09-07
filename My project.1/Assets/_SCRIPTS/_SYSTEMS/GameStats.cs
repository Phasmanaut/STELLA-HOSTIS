using System;
using System.Collections;
using JetBrains.Annotations;
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
    public float timer;
    public bool timerActive;

    //UI
    public TextMeshProUGUI healthUI;public TextMeshProUGUI timerUI;public TextMeshProUGUI levelUI;public TextMeshProUGUI chargeUI;public TextMeshProUGUI pointsUI;public TextMeshPro screenText;

    public AudioClip startSound;
    public AudioClip loseSound;
    public AudioClip deathSound;
    public AudioClip Music;
    private AudioSource musicSource;
    public GameObject player;
    private GameObject playerInstance;

    //pixel guy animations
    public GameObject pixelguy1;public GameObject pixelguy2;public GameObject pixelguy3;public GameObject pixelguy4;
    
    //Startcube
    public GameObject startCube;private GameObject startCubeInstance;public ParticleSystem cube_die;
    
    //Enemies
    public GameObject enemy_A; public GameObject enemy_Ab;



    //ENEMY SPAWNS
    public GameObject spawn0x1, spawn0x2, spawn0x3, spawn0x4, spawn0x5,
                      spawn1x1, spawn1x2, spawn1x3, spawn1x4, spawn1x5, spawn1x6, spawn1x7, spawn1x8, spawn1x9,
                      spawn2x1, spawn2x2, spawn2x3, spawn2x4, spawn2x5, spawn2x6, spawn2x7, spawn2x8, spawn2x9,
                      spawn3x1, spawn3x2, spawn3x3, spawn3x4, spawn3x5, spawn3x6, spawn3x7, spawn3x8, spawn3x9,
                      spawn4x1, spawn4x2, spawn4x3, spawn4x4, spawn4x5, spawn4x6, spawn4x7, spawn4x8, spawn4x9,
                      spawn5x1, spawn5x2, spawn5x3, spawn5x4, spawn5x5, spawn5x6, spawn5x7, spawn5x8, spawn5x9,
                      spawn6x1, spawn6x2, spawn6x3, spawn6x4, spawn6x5, spawn6x6, spawn6x7, spawn6x8, spawn6x9;

    public GameObject[,] spawn = new GameObject[7, 9];

    void Start()
    {
        spawn[0, 0] = spawn0x1; spawn[0, 1] = spawn0x2; spawn[0, 2] = spawn0x3; spawn[0, 3] = spawn0x4; spawn[0, 4] = spawn0x5;
        spawn[1, 0] = spawn1x1; spawn[1, 1] = spawn1x2; spawn[1, 2] = spawn1x3; spawn[1, 3] = spawn1x4; spawn[1, 4] = spawn1x5; spawn[1, 5] = spawn1x6; spawn[1, 6] = spawn1x7; spawn[1, 7] = spawn1x8; spawn[1, 8] = spawn1x9;
        spawn[2, 0] = spawn2x1; spawn[2, 1] = spawn2x2; spawn[2, 2] = spawn2x3; spawn[2, 3] = spawn2x4; spawn[2, 4] = spawn2x5; spawn[2, 5] = spawn2x6; spawn[2, 6] = spawn2x7; spawn[2, 7] = spawn2x8; spawn[2, 8] = spawn2x9;
        spawn[3, 0] = spawn3x1; spawn[3, 1] = spawn3x2; spawn[3, 2] = spawn3x3; spawn[3, 3] = spawn3x4; spawn[3, 4] = spawn3x5; spawn[3, 5] = spawn3x6; spawn[3, 6] = spawn3x7; spawn[3, 7] = spawn3x8; spawn[3, 8] = spawn3x9;
        spawn[4, 0] = spawn4x1; spawn[4, 1] = spawn4x2; spawn[4, 2] = spawn4x3; spawn[4, 3] = spawn4x4; spawn[4, 4] = spawn4x5; spawn[4, 5] = spawn4x6; spawn[4, 6] = spawn4x7; spawn[4, 7] = spawn4x8; spawn[4, 8] = spawn4x9;
        spawn[5, 0] = spawn5x1; spawn[5, 1] = spawn5x2; spawn[5, 2] = spawn5x3; spawn[5, 3] = spawn5x4; spawn[5, 4] = spawn5x5; spawn[5, 5] = spawn5x6; spawn[5, 6] = spawn5x7; spawn[5, 7] = spawn5x8; spawn[5, 8] = spawn5x9;
        spawn[6, 0] = spawn6x1; spawn[6, 1] = spawn6x2; spawn[6, 2] = spawn6x3; spawn[6, 3] = spawn6x4; spawn[6, 4] = spawn6x5; spawn[6, 5] = spawn6x6; spawn[6, 6] = spawn6x7; spawn[6, 7] = spawn6x8; spawn[6, 8] = spawn6x9;


        playerAlive = true;
        timerActive = false;
        playerHealth = 3;
        playerCharge = 0;
        level = 0;
        points = 0;
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

        if (level == 1)
        {
            Console.WriteLine($"LEVEL STARTED: {level}");
            levelEnemies = 6;
            Instantiate(enemy_A, spawn[2, 0].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[2, 2].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[2, 6].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[2, 8].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[3, 2].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[3, 6].transform.position, transform.rotation);

        }
        else if (level == 2)
        {
            Console.WriteLine($"LEVEL STARTED: {level}");
            levelEnemies = 9;
            
            Instantiate(enemy_A, spawn[1, 3].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[1, 2].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[1, 5].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[1, 6].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[2, 2].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[2, 4].transform.position, transform.rotation);
            Instantiate(enemy_A, spawn[2, 6].transform.position, transform.rotation);
            Instantiate(enemy_Ab, spawn[4, 6].transform.position, transform.rotation);
            Instantiate(enemy_Ab, spawn[4, 2].transform.position, transform.rotation);
        }

    }


    public void EnemyDown(int pointGain)
    {
        points += pointGain;
        levelEnemies--;
        if(levelEnemies <= 0)
        {
            LevelEnd();
        }
    }
    public void LevelEnd()
    {
        screenText.text = $"START LEVEL {level + 1} ";
        startCubeInstance = Instantiate(startCube);
        timerActive = false;
    }

    public void PlayerHit()
    {
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
        playerAlive = false;
        Destroy(playerInstance);
        musicSource.Stop();
        screenText.color = Color.red;
        screenText.fontSize = 40;
        screenText.text = $"GAME OVER";

    }

    public void UpdatePixelguy()
    {
            pixelguy1.SetActive(false);
            pixelguy2.SetActive(false);
            pixelguy3.SetActive(false);
            pixelguy4.SetActive(false);

            if(playerHealth ==3)
            {
                pixelguy1.SetActive(true);
            }
            if(playerHealth ==2)
            {
                pixelguy2.SetActive(true);
            }
            if(playerHealth ==1)
            {
                pixelguy3.SetActive(true);
            }
            if(playerHealth ==0)
            {
                pixelguy4.SetActive(true);
            }

    }



    //Camera Shaking below
    void CameraShake()
    {
        StartCoroutine(Shake(0.1f, 0.2f)); // duration, magnitude
    }

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

}
