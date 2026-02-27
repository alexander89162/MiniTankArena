using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*Allocates/Deallocates memory, spawns units from JSON, and stores data about the current enemies such as enemiesRemaining*/
public class UnitManager : MonoBehaviour
{
    public enum WaveState
    {
        Allocating,
        BuildingQueue,
        WaveRunning,
        Deallocating,
        Paused
    }
    public int batchSize = 20; // how many units to allocate per frame during allocation state
    private List<UnitData> unitHandles; // list of all units in the current wave
    private WaveState currentState;
    private string currentWave = null;
    public int enemiesRemaining;
    private Queue<UnitConfig> spawnQueue;
    private bool busy = false; // set True when allocating or deallocating memory
    private string waveContentsPath;

    [System.Serializable]
    public struct UnitConfig // data to be used by SpawnQueue, ensuring we place units with proper configurations
    {
        public string prefabName;
        public float spawnDelay;
        public Vector3 position;
        public Vector3 rotation;
        public int team;
        public float damageMultiplier;
        public float healthMultiplier;
        public float movementSpeed;
    }

    void Awake()
    {
        unitHandles = new List<UnitData>();
    }

    void Update()
    /*Based on the state, we either allocate/ deallocate memory, pause, 
    or follow spawn queue for the current wave*/
    {
        switch (currentState)
        {
            case WaveState.Allocating:
                if (!busy)
                {
                    busy = true;
                    StartAllocating(); // Allocate units (resize unitHandles and fill with real, temporarily disabled GameObjects)
                }
                break;

            case WaveState.BuildingQueue:
                if (!busy)
                {
                    busy = true;
                    BuildSpawnQueue(); // Build the queue to read from during WaveRunning state
                }
                break;
            
            case WaveState.Deallocating:
                if (!busy)
                {
                    busy = true;
                    DeallocateWave();
                } // SpawnQueue empties itself in BuildSpawnQueue so no need to empty it here
                break;

            case WaveState.Paused:
                // do nothing (?)
                break;

            case WaveState.WaveRunning:
                UpdateWave(); // Uses pointers and queue to gradually spawn everything, then waits for all enemies (or player) to die to end wave
                break;
        }
    }

    private void SetWavePath(string mapName, string waveName)
    {
    waveContentsPath = System.IO.Path.Combine(
        Application.streamingAssetsPath,
        "Waves",
        mapName,
        waveName + ".json"
        );
    }

    private void StartAllocating()
    /*Start coroutine to allocate memory for next wave's units*/
    {
        SetCurrentWave(FindNextWave(currentWave)); // use current wave to find next wave
        StartCoroutine(AllocateUnits());
    }

    public void UpdateWave()
    /*Read from the SpawnQueue, spawning and popping until there are no more 
    valid units to spawn in the current frame*/
    {
        //
    }

    private IEnumerator AllocateUnits()
    /* Create all the GameObjects needed for the next wave. This is done in
    small batches to avoid freezing the UI while allocation is in progress*/
    {
        int waveSize = 25;
        for (int i = 0; i < waveSize; i++)
        {
            // GameObject unit = Instantiate(unitPrefab);
            // unit.SetActive(false);
            // unitHandles.Add(unitData);

            if ((i+1) % batchSize == 0)
                yield return null;
        }

        SetState(WaveState.WaveRunning);
        busy = false;
    }

    private void BuildSpawnQueue()
    /*Fill the spawn queue so it's ready for the wave to use*/
    {
        //
    }

    public void DeallocateWave()
    /*Destroy all the GameObjects in unitHandles, then clear*/
    {
        //
    }

    public string FindNextWave(string previous)
    {
        // TODO
        return "";
    }
    public void SetCurrentWave(string newWave)
    /*Increment the wave*/
    {
        // TODO
    }
    public void SetState(WaveState newState) {   currentState = newState;    }
}
