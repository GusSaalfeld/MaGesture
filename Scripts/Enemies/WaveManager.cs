using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
#pragma warning disable 0649

    [SerializeField] private Collider[] spawnAreas;

    [SerializeField] private List<GameObject> obelisks = new List<GameObject>();
    [SerializeField] private GameObject forcefield;
    [SerializeField] private GameObject player_loc;
    [SerializeField] private List<Wave> waves = new List<Wave>();

    [SerializeField] private Enemy grunt_prefab;
    [SerializeField] private Enemy mage_prefab;
    [SerializeField] private Enemy bomber_prefab;
    [SerializeField] private Enemy big_prefab;
    [SerializeField] private Enemy small_prefab;

    [SerializeField] private LevelManager level_manager;

#pragma warning restore 0649

    //For use in waves: stores an enemy type and how many to spawn
    struct EnemyTypeCounter {
        public Enemy enemyType;
        public int spawnAmount;
        public EnemyTypeCounter(Enemy enemyType, int spawnAmount) {
            this.enemyType = enemyType;
            this.spawnAmount = spawnAmount;
        }
    }

    struct Wave {
        public int waveID;
        public List<EnemyTypeCounter> enemiesToSpawn;
        public bool ifSpecialStart;
        public Enemy[] specialStartGO;
        public bool ifSpecialEnd;
        public Enemy[] specialEndGO;
    
        //Constructor
        public Wave(int waveID, List<EnemyTypeCounter> enemiesToSpawn, bool ifSpecialStart, Enemy[] specialStartGO, bool ifSpecialEnd, Enemy[] specialEndGO) {
            this.waveID = waveID;
            this.enemiesToSpawn = enemiesToSpawn;
            this.ifSpecialStart = ifSpecialStart;
            this.specialStartGO = specialStartGO;
            this.ifSpecialEnd = ifSpecialEnd;
            this.specialEndGO = specialEndGO;
        }
    }

    #region Wave Management
    private GameObject[] attackableLocations;
    private bool noAttackableLocations => attackableLocations.Length == 0;

    private readonly ISet<Enemy> currentWave = new HashSet<Enemy>();

    //WARNING: Inefficient implementation as its the last week of development 
    // and I didn't know Unity allows you to edit custom structs in the inspector
    private void InitializeWaves() {
        
        //Wave 1:
        //Create enemy types + amounts featured in wave
        List<EnemyTypeCounter> eList1 = new List<EnemyTypeCounter>();

        EnemyTypeCounter eGrunt1 = new EnemyTypeCounter(grunt_prefab, 10);
        
        //Add those types + amounts to the list of enemiesTypes.
        eList1.Add(eGrunt1);

        //Create wave and add it to the list of waves
        Wave wave1 = new Wave(1, eList1, false, null, false, null);
        waves.Add(wave1);

        //Wave 2:
        List<EnemyTypeCounter> eList2 = new List<EnemyTypeCounter>();

        EnemyTypeCounter eGrunt2 = new EnemyTypeCounter(grunt_prefab, 10);
        EnemyTypeCounter eMage2 = new EnemyTypeCounter(mage_prefab, 5);
        EnemyTypeCounter eSmall2 = new EnemyTypeCounter(small_prefab, 3);

        eList2.Add(eGrunt2);
        eList2.Add(eMage2);
        eList2.Add(eSmall2);
        Wave wave2 = new Wave(2, eList2, false, null, false, null);
        waves.Add(wave2);

        //Wave 3:
        List<EnemyTypeCounter> eList3 = new List<EnemyTypeCounter>();

        Enemy[] wave3Start = { grunt_prefab, grunt_prefab, grunt_prefab, grunt_prefab, grunt_prefab};
        EnemyTypeCounter eGrunt3 = new EnemyTypeCounter(grunt_prefab, 6);
        EnemyTypeCounter eBomber3 = new EnemyTypeCounter(bomber_prefab, 7);
        EnemyTypeCounter eMage3 = new EnemyTypeCounter(mage_prefab, 2);
        Enemy[] wave3End = { big_prefab };

        eList3.Add(eGrunt3);
        eList3.Add(eBomber3);
        eList3.Add(eMage3);
        Wave wave3 = new Wave(3, eList3, true, wave3Start, true, wave3End);
        waves.Add(wave3);

        //Wave 4:
        List<EnemyTypeCounter> eList4 = new List<EnemyTypeCounter>();

        Enemy[] wave4Start = { big_prefab };
        EnemyTypeCounter eGrunt4 = new EnemyTypeCounter(grunt_prefab, 12);
        EnemyTypeCounter eMage4 = new EnemyTypeCounter(mage_prefab, 6);
        EnemyTypeCounter eBomber4 = new EnemyTypeCounter(bomber_prefab, 4);
        EnemyTypeCounter eSmall4 = new EnemyTypeCounter(small_prefab, 2);
        Enemy[] wave4End = { big_prefab };

        eList4.Add(eGrunt4);
        eList4.Add(eMage4);
        eList4.Add(eBomber4);
        eList4.Add(eSmall4);
        Wave wave4 = new Wave(4, eList4, true, wave4Start, true, wave4End);
        waves.Add(wave4);
    }

    //Handles spawning waves
    private IEnumerator WaveRoutine()
    {
        Wave currWave;
        for (int waveNumber = 0; waveNumber < waves.Count; waveNumber++)
        {
            Debug.Log("Starting wave " + waveNumber);
            currWave = waves[waveNumber];

            //Handles any special start of wave spawns
            if(currWave.ifSpecialStart) {
                for (int i = 0; i < currWave.specialStartGO.Length; i++)
                {
                    Enemy enemyPrefab = currWave.specialStartGO[i];
                    SpawnEnemy(enemyPrefab);
                    yield return new WaitForSeconds(Random.Range(1f, 2f));
                }
                currWave.ifSpecialStart = false;
            }

            //Spawn a new wave
            while(currWave.enemiesToSpawn.Count != 0) {
                Enemy enemyPrefab = FindEnemyToSpawn(currWave);
                SpawnEnemy(enemyPrefab);
                yield return new WaitForSeconds(Random.Range(1f, 2.5f));
            }

            //Handles any special end of wave spawn cases
            if (currWave.ifSpecialEnd)
            {
                for (int i = 0; i < currWave.specialEndGO.Length; i++)
                {
                    Enemy enemyPrefab = currWave.specialEndGO[i];
                    SpawnEnemy(enemyPrefab);
                    yield return new WaitForSeconds(Random.Range(1f, 2f));
                }

                currWave.ifSpecialEnd = false;
            }

            Debug.Log("Wave " + waveNumber + " waiting to be completed");

            //Wait until current wave is complete
            yield return new WaitUntil(WaveComplete);
            currentWave.Clear();

            yield return AfterWaveRoutine();
        }
    }
    
    private IEnumerator AfterWaveRoutine()
    {
        level_manager.Progress();
        yield break;
    }

    private bool WaveComplete()
    {
        foreach (Enemy e in currentWave)
        {
            if (e == null) continue;
            if (e.IsAlive)
            {
                return false;
            }
        }
        return true;
    }    
    #endregion

    #region Spawning Enemy
    //Chooses random enemy from list of available enemies
    private Enemy FindEnemyToSpawn(Wave currWave) {
        int enemyIndex = Random.Range(0, currWave.enemiesToSpawn.Count);
        EnemyTypeCounter eCounter = currWave.enemiesToSpawn[enemyIndex];
        eCounter.spawnAmount--;
        currWave.enemiesToSpawn[enemyIndex] = eCounter;
        
        //If wave has spawn its allotted number of an enemy type, remove it as an option 
        if(eCounter.spawnAmount <= 0) {
            currWave.enemiesToSpawn.Remove(eCounter);
        }
        return eCounter.enemyType;
    }

    //Handles spawning an enemy
    private void SpawnEnemy(Enemy enemyPrefab) {
        Enemy newEnemy = Instantiate(enemyPrefab, RandomSpawnPoint(), Quaternion.identity, transform.parent);
        newEnemy.SetDestination(FindLocation());        
        currentWave.Add(newEnemy);
    }

    //Find a random point within designated spawn area to spawn enemy
    private Vector3 RandomSpawnPoint()
    {
        Bounds spawnAreaBounds = spawnAreas[Random.Range(0, spawnAreas.Length)].bounds;
        return new Vector3(
            Random.Range(spawnAreaBounds.min.x, spawnAreaBounds.max.x),
            Random.Range(spawnAreaBounds.min.y, spawnAreaBounds.max.y),
            Random.Range(spawnAreaBounds.min.z, spawnAreaBounds.max.z)
        );
    }

    //Find a NavMesh destination (a living player defense) for enemy
    private GameObject FindLocation()
    {

        if(obelisks.Count > 0) {
            GameObject obTransform = obelisks[Random.Range(0, obelisks.Count)];
            if(obTransform == null) {
                Debug.Log("ERROR: transform destination is null");
            } else {
                return obTransform;
            }
        } 
            return forcefield;
    }

    #endregion

    private void RemoveDestroyedObelisks() {
        for(int i = 0; i < obelisks.Count; i++) {
            if(!obelisks[i].GetComponent<Obelisk>().IsAlive) {
                obelisks.Remove(obelisks[i]);
            }
        }
    }

    private void UpdateEnemyDestination()
    {
           RemoveDestroyedObelisks();
           foreach (Enemy enemy in currentWave) {
                enemy.UpdateDestination(FindLocation());
            }
    }
    
    #region Unity Events
    private void Awake()
    {
        InitializeWaves();
        if (spawnAreas.Length == 0)
        {
            Debug.LogError("No spawn areas for enemy waves specified.  Destroying self.");
            Destroy(this);
        }

    }

    private void Start()
    {
        //Once an obelisk is destroyed, enemies update their navmesh destination to different target
        foreach (GameObject obelisk in obelisks)
        {
            obelisk.GetComponent<Obelisk>().AddObeliskDeathEffect(UpdateEnemyDestination);
        }
        StartCoroutine(WaveRoutine());
    }

    //On keyboard input p, skip wave
    private void Update()
    {
        switch (Input.inputString)
        {
            case "p":
                SkipAllEnemies();
                break;
            default:
                break;
        }
    }
    #endregion
    
    //For debugging purposes: skips wave
    private void SkipAllEnemies()
    {
        foreach (Enemy e in currentWave)
        {
            if (e != null)
            {
                Destroy(e.gameObject);
            }
        }
        currentWave.Clear();
    }
}
