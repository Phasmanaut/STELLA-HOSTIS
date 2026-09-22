using UnityEngine;

// Holds the enemy prefabs, the spawn point grid, and every level's enemy layout.
// GameStats calls SpawnLevel() when a level starts and uses the returned count
// to know when the level has been cleared.
public class EnemySpawner : MonoBehaviour
{
    //Enemies
    public GameObject enemy_A; public GameObject enemy_Ab; public GameObject enemy_B; public GameObject enemy_C; public GameObject enemy_D;



    //ENEMY SPAWNS
    public GameObject spawn0x1, spawn0x2, spawn0x3, spawn0x4, spawn0x5,
                      spawn1x1, spawn1x2, spawn1x3, spawn1x4, spawn1x5, spawn1x6, spawn1x7, spawn1x8, spawn1x9,
                      spawn2x1, spawn2x2, spawn2x3, spawn2x4, spawn2x5, spawn2x6, spawn2x7, spawn2x8, spawn2x9,
                      spawn3x1, spawn3x2, spawn3x3, spawn3x4, spawn3x5, spawn3x6, spawn3x7, spawn3x8, spawn3x9,
                      spawn4x1, spawn4x2, spawn4x3, spawn4x4, spawn4x5, spawn4x6, spawn4x7, spawn4x8, spawn4x9,
                      spawn5x1, spawn5x2, spawn5x3, spawn5x4, spawn5x5, spawn5x6, spawn5x7, spawn5x8, spawn5x9,
                      spawn6x1, spawn6x2, spawn6x3, spawn6x4, spawn6x5, spawn6x6, spawn6x7, spawn6x8, spawn6x9;

    public const int Rows = 7;
    public const int Columns = 9;
    public const int FirstEnemyRow = 1; //row 0 is the item row, so enemies never use it

    public GameObject[,] spawn = new GameObject[Rows, Columns];

    private int spawnedCount; //how many enemies the current level has spawned so far

    //Who is sitting in each grid slot. Destroyed enemies read back as null on their own, so a slot
    //frees itself up as soon as its occupant dies - Enemy_C and Enemy_D use this to find room to move into.
    private readonly GameObject[,] occupant = new GameObject[Rows, Columns];

    //The one spawner in the scene, so enemies can ask about slots without hunting for it every time
    public static EnemySpawner Instance { get; private set; }

    //Awake runs before any Start, so the grid is ready before GameStats (or DebugTester) can start a level
    void Awake()
    {
        Instance = this;

        spawn[0, 0] = spawn0x1; spawn[0, 1] = spawn0x2; spawn[0, 2] = spawn0x3; spawn[0, 3] = spawn0x4; spawn[0, 4] = spawn0x5;
        spawn[1, 0] = spawn1x1; spawn[1, 1] = spawn1x2; spawn[1, 2] = spawn1x3; spawn[1, 3] = spawn1x4; spawn[1, 4] = spawn1x5; spawn[1, 5] = spawn1x6; spawn[1, 6] = spawn1x7; spawn[1, 7] = spawn1x8; spawn[1, 8] = spawn1x9;
        spawn[2, 0] = spawn2x1; spawn[2, 1] = spawn2x2; spawn[2, 2] = spawn2x3; spawn[2, 3] = spawn2x4; spawn[2, 4] = spawn2x5; spawn[2, 5] = spawn2x6; spawn[2, 6] = spawn2x7; spawn[2, 7] = spawn2x8; spawn[2, 8] = spawn2x9;
        spawn[3, 0] = spawn3x1; spawn[3, 1] = spawn3x2; spawn[3, 2] = spawn3x3; spawn[3, 3] = spawn3x4; spawn[3, 4] = spawn3x5; spawn[3, 5] = spawn3x6; spawn[3, 6] = spawn3x7; spawn[3, 7] = spawn3x8; spawn[3, 8] = spawn3x9;
        spawn[4, 0] = spawn4x1; spawn[4, 1] = spawn4x2; spawn[4, 2] = spawn4x3; spawn[4, 3] = spawn4x4; spawn[4, 4] = spawn4x5; spawn[4, 5] = spawn4x6; spawn[4, 6] = spawn4x7; spawn[4, 7] = spawn4x8; spawn[4, 8] = spawn4x9;
        spawn[5, 0] = spawn5x1; spawn[5, 1] = spawn5x2; spawn[5, 2] = spawn5x3; spawn[5, 3] = spawn5x4; spawn[5, 4] = spawn5x5; spawn[5, 5] = spawn5x6; spawn[5, 6] = spawn5x7; spawn[5, 7] = spawn5x8; spawn[5, 8] = spawn5x9;
        spawn[6, 0] = spawn6x1; spawn[6, 1] = spawn6x2; spawn[6, 2] = spawn6x3; spawn[6, 3] = spawn6x4; spawn[6, 4] = spawn6x5; spawn[6, 5] = spawn6x6; spawn[6, 6] = spawn6x7; spawn[6, 7] = spawn6x8; spawn[6, 8] = spawn6x9;
    }


    public const int LastLevel = 10; //the highest level with a layout below; clearing it wins the game

    //Spawns the enemy layout for the given level and returns how many enemies it spawned
    //Grid: rows 1-6 go top to bottom (row 0 is the item row), columns 0-8 go left to right.
    //Enemies sway side to side (A ~0.75, Ab ~1.1, B ~0.5), so different types in the same row
    //are kept at least 2 columns apart so they don't slide through each other.
    public int SpawnLevel(int level)
    {
        spawnedCount = 0;
        System.Array.Clear(occupant, 0, occupant.Length); //last level's enemies are gone, so every slot is free again

        if (level == 1)
        {
            Spawn(enemy_A, 2, 0);
            Spawn(enemy_A, 2, 2);
            Spawn(enemy_A, 2, 6);
            Spawn(enemy_A, 2, 8);
            Spawn(enemy_A, 3, 2);
            Spawn(enemy_A, 3, 6);
        }
        else if (level == 2)
        {
            Spawn(enemy_A, 1, 3);
            Spawn(enemy_A, 1, 2);
            Spawn(enemy_A, 1, 5);
            Spawn(enemy_A, 1, 6);
            Spawn(enemy_A, 2, 2);
            Spawn(enemy_A, 2, 4);
            Spawn(enemy_A, 2, 6);
            Spawn(enemy_Ab, 4, 6);
            Spawn(enemy_Ab, 4, 2);
        }
        else if (level == 3)
        {
            Spawn(enemy_A, 1, 3);
            Spawn(enemy_A, 1, 2);
            Spawn(enemy_A, 1, 5);
            Spawn(enemy_A, 1, 6);
            Spawn(enemy_Ab, 2, 2);
            Spawn(enemy_Ab, 2, 4);
            Spawn(enemy_Ab, 2, 6);
            Spawn(enemy_Ab, 3, 6);
            Spawn(enemy_Ab, 3, 2);
        }
        else if (level == 4)
        {
            Spawn(enemy_B, 1, 0);
            Spawn(enemy_A, 1, 2);
            Spawn(enemy_B, 1, 8);
            Spawn(enemy_Ab, 1, 6);
            Spawn(enemy_Ab, 2, 2);
            Spawn(enemy_Ab, 2, 4);
            Spawn(enemy_Ab, 2, 6);
            Spawn(enemy_A, 4, 6);
            Spawn(enemy_A, 4, 2);
        }
        else if (level == 5) //ARROWHEAD: a V pointing at the player, B gunners on the wing tips
        {
            Spawn(enemy_B, 1, 0);
            Spawn(enemy_B, 1, 8);
            Spawn(enemy_Ab, 2, 1);
            Spawn(enemy_A, 2, 4);
            Spawn(enemy_Ab, 2, 7);
            Spawn(enemy_Ab, 3, 2);
            Spawn(enemy_A, 3, 4);
            Spawn(enemy_Ab, 3, 6);
            Spawn(enemy_A, 4, 3);
            Spawn(enemy_A, 4, 5);
            Spawn(enemy_A, 5, 4);
        }
        else if (level == 6) //SHIELD WALL: a row of A's soaks up bullets while the B's charge behind it
        {
            Spawn(enemy_B, 1, 1);
            Spawn(enemy_B, 1, 4);
            Spawn(enemy_B, 1, 7);
            Spawn(enemy_Ab, 2, 0);
            Spawn(enemy_Ab, 2, 4);
            Spawn(enemy_Ab, 2, 8);
            Spawn(enemy_A, 4, 1);
            Spawn(enemy_A, 4, 2);
            Spawn(enemy_A, 4, 3);
            Spawn(enemy_A, 4, 4);
            Spawn(enemy_A, 4, 5);
            Spawn(enemy_A, 4, 6);
            Spawn(enemy_A, 4, 7);
        }
        else if (level == 7) //CHECKERBOARD: an Ab swarm - fewer B's, but a lot more fast shooters
        {
            Spawn(enemy_B, 1, 1);
            Spawn(enemy_B, 1, 7);
            Spawn(enemy_Ab, 2, 0);
            Spawn(enemy_Ab, 2, 2);
            Spawn(enemy_Ab, 2, 4);
            Spawn(enemy_Ab, 2, 6);
            Spawn(enemy_Ab, 2, 8);
            Spawn(enemy_A, 3, 1);
            Spawn(enemy_A, 3, 3);
            Spawn(enemy_A, 3, 5);
            Spawn(enemy_A, 3, 7);
            Spawn(enemy_Ab, 4, 2);
            Spawn(enemy_Ab, 4, 6);
        }
        else if (level == 8) //PINCER: two claws reach down to the lowest row, closest to the player
        {
            Spawn(enemy_B, 1, 1);
            Spawn(enemy_B, 1, 4);
            Spawn(enemy_B, 1, 7);
            Spawn(enemy_Ab, 2, 0);
            Spawn(enemy_Ab, 2, 8);
            Spawn(enemy_Ab, 3, 0);
            Spawn(enemy_A, 3, 4);
            Spawn(enemy_Ab, 3, 8);
            Spawn(enemy_A, 4, 0);
            Spawn(enemy_A, 4, 4);
            Spawn(enemy_A, 4, 8);
            Spawn(enemy_A, 5, 1);
            Spawn(enemy_A, 5, 7);
            Spawn(enemy_A, 6, 2);
            Spawn(enemy_A, 6, 6);
        }
        else if (level == 9) //FORTRESS: B's hold all four corners of a box with an Ab core inside
        {
            Spawn(enemy_B, 1, 0);
            Spawn(enemy_Ab, 1, 2);
            Spawn(enemy_Ab, 1, 4);
            Spawn(enemy_Ab, 1, 6);
            Spawn(enemy_B, 1, 8);
            Spawn(enemy_A, 2, 0);
            Spawn(enemy_Ab, 2, 4);
            Spawn(enemy_A, 2, 8);
            Spawn(enemy_A, 3, 0);
            Spawn(enemy_Ab, 3, 4);
            Spawn(enemy_A, 3, 8);
            Spawn(enemy_B, 4, 0);
            Spawn(enemy_A, 4, 2);
            Spawn(enemy_A, 4, 3);
            Spawn(enemy_A, 4, 4);
            Spawn(enemy_A, 4, 5);
            Spawn(enemy_A, 4, 6);
            Spawn(enemy_B, 4, 8);
        }
        else if (level == 10) //SWARM MODE: starts after the demo's thank-you screen
        {
            for (int row = 1; row <= 6; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    Spawn(enemy_A, row, column);
                }
            }
        }

        return spawnedCount;
    }


    //Spawns one enemy at a grid spot and counts it, so the level's enemy total can't get out of sync with the layout
    void Spawn(GameObject enemy, int row, int column)
    {
        occupant[row, column] = Instantiate(enemy, spawn[row, column].transform.position, transform.rotation);
        spawnedCount++;
    }


    //// Slot lookups, used by the enemies that move around the formation ////////////////////////

    //Row 0 only has 5 spawn points, and anything off the grid has none, so not every row/column pair is a real slot
    public bool SlotExists(int row, int column)
    {
        return row >= 0 && row < Rows && column >= 0 && column < Columns && spawn[row, column] != null;
    }

    //Free means the slot is real and whoever was there is gone (Unity reports destroyed objects as null)
    public bool IsSlotFree(int row, int column)
    {
        return SlotExists(row, column) && occupant[row, column] == null;
    }

    public Vector3 SlotPosition(int row, int column)
    {
        return spawn[row, column].transform.position;
    }

    //Which slot an enemy was spawned into. False if it wasn't spawned through the grid at all (the dev tester does that)
    public bool TryFindSlotOf(GameObject enemy, out int row, out int column)
    {
        for (row = 0; row < Rows; row++)
        {
            for (column = 0; column < Columns; column++)
            {
                if (occupant[row, column] == enemy) return true;
            }
        }

        row = -1;
        column = -1;
        return false;
    }

    public void ClaimSlot(GameObject enemy, int row, int column)
    {
        if (SlotExists(row, column)) occupant[row, column] = enemy;
    }

    //Gives up whatever slot this enemy holds, so others can move into it while it's away
    public void ReleaseSlot(GameObject enemy)
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (occupant[row, column] == enemy) occupant[row, column] = null;
            }
        }
    }

    //Picks one empty slot at random, with every free slot equally likely (reservoir sampling, so nothing is allocated)
    public bool TryFindFreeSlot(out int row, out int column)
    {
        row = -1;
        column = -1;
        int found = 0;

        for (int r = FirstEnemyRow; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (!IsSlotFree(r, c)) continue;

                found++;
                if (Random.Range(0, found) == 0)
                {
                    row = r;
                    column = c;
                }
            }
        }

        return found > 0;
    }
}
