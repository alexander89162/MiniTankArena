using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.Networking;

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
    [SerializeField] private List<GameObject> prefabList;
    private Dictionary<string, GameObject> prefabLookup;
    private List<UnitData> unitHandles; // list of all units in the current wave
    private WaveState currentState;
    private string currentWave { get; set; }
    private string waveMode;
    private float waveTimer = 0;
    public int enemiesRemaining;
    private Queue<UnitData> spawnQueue;
    private bool busy = false; // set True while allocating or deallocating memory
    public string currentMap;
    private string waveContentsPath;
    private Dictionary<string, string> waveTransitions;

    [System.Serializable]
    public class WaveConfigFile
    {
        public string waveMode;
        public Transition[] transitions;
    }

    [System.Serializable]
    public class Transition
    {
        public string from;
        public string to;
    }

    [System.Serializable]
    public struct UnitConfig // data to be used by SpawnQueue, ensuring we place units with proper configurations
    {
        public string prefabName;
        public float spawnDelay;
        public Vector3 spawnPosition;
        public Vector3 spawnRotation;
        public int team;
        public float damageMultiplier;
        public float healthMultiplier;
        public float movementSpeed;
    }

    [System.Serializable]
    public struct WaveDefinition
    {
        public string spawnMethod;
        public UnitConfig[] unitConfigs;
    }

    void Awake()
    {
        unitHandles = new List<UnitData>();
        prefabLookup = prefabList.ToDictionary(p => p.name, p => p);
        waveTransitions = new Dictionary<string, string>(); // this is filled inside of LoadWaveConfigurations()
        SetWaveContentsPath(currentMap);
        LoadWaveConfigurations();
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
                    StartCoroutine(StartAllocating());
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
                }
                break;

            case WaveState.Paused:
                // do nothing (?)
                break;

            case WaveState.WaveRunning:
                UpdateWave(); // Uses queue to gradually spawn everything, then waits for all enemies (or player) to die to end wave
                break;
        }
    }

    private void SetWaveContentsPath(string mapName)
    {
    waveContentsPath = System.IO.Path.Combine(
        Application.streamingAssetsPath,
        "Waves",
        mapName
        );
    }

    public void UpdateWave()
    /*Read from the SpawnQueue, spawning and popping until there are no more 
    valid units to spawn in the current frame*/
    {
        waveTimer += Time.deltaTime;
        while (spawnQueue.Count > 0 && spawnQueue.Peek().config.spawnDelay < waveTimer)
        {
            SpawnUnit(); // unit is popped in SpawnUnit()
        }

        if (spawnQueue.Count == 0 && enemiesRemaining == 0)
        {
            SetState(WaveState.Deallocating);
        }
    }

    public void SpawnUnit()
    {
        UnitData data = spawnQueue.Dequeue();
        UnitConfig config = data.config;

        data.gameObject.transform.position = config.spawnPosition;
        data.gameObject.transform.rotation = Quaternion.Euler(config.spawnRotation);
        data.gameObject.SetActive(true);
    }

    private IEnumerator StartAllocating()
    /*Start coroutine to allocate memory for next wave's units*/
    {
        currentWave = FindNextWave(currentWave); // set next wave - it can be sequential or procedurally chosen
        yield return StartCoroutine(LoadWave()); // Before allocation, unitHandles will be up-to-date with wave info from the current wave JSON
        yield return StartCoroutine(AllocateUnits()); // here we actually create the next wave's units
    }

    private IEnumerator LoadWave()
    {
        string path = System.IO.Path.Combine(waveContentsPath, currentWave);

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load wave JSON: " + request.error);
            yield break;
        }

        string jsonString = request.downloadHandler.text;

        WaveDefinition wave =
            JsonUtility.FromJson<WaveDefinition>(jsonString);

        FillUnitHandles(wave);
    }

    private IEnumerator LoadWaveConfigurations()
    /*Load the waveconfig.json and set WaveMode. Set wavetransitions to null if 
    sequential and fill the mappings based on the JSON if procedural*/
    {
        string path = System.IO.Path.Combine(
            waveContentsPath,
            "waveconfig.json"
        );

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load wave config: " + request.error);
            yield break;
        }

        WaveConfigFile config =
            JsonUtility.FromJson<WaveConfigFile>(
                request.downloadHandler.text
            );

        waveTransitions.Clear();

        if (config.waveMode.ToLower() == "procedural")
        {
            foreach (var t in config.transitions)
            {
                waveTransitions[t.from] = t.to;
            }
        }
    }

    public void FillUnitHandles(WaveDefinition wave)
    /*Fill unitHandles based on JSON file of current wave*/
    {
        // Fill unitHandles with new data
        unitHandles.Clear();
        for (int i = 0; i < wave.unitConfigs.Length; i++)
        {
            UnitData handle = new UnitData
            {
                config = wave.unitConfigs[i]
            };

            unitHandles.Add(handle);
        }
    }

    private IEnumerator AllocateUnits()
    /* Create all the GameObjects needed for the next wave. This is done in
    small batches to avoid freezing the UI while allocation is in progress*/
    {
        int waveSize = unitHandles.Count;
        for (int i = 0; i < waveSize; i++)
        {
            UnitConfig config = unitHandles[i].config;
            GameObject unitPrefab = prefabLookup[config.prefabName];
            GameObject unit = Instantiate(unitPrefab);
            unitHandles[i].gameObject = unit;
            unit.SetActive(false);

            if ((i+1) % batchSize == 0)
                yield return null;
        }

        // Sort unitHandles by spawnDelay so it's ready for building the spawn queue
        unitHandles.Sort((a, b) =>
            a.config.spawnDelay.CompareTo(b.config.spawnDelay)
        );

        SetState(WaveState.WaveRunning);
        busy = false;
    }

    private void BuildSpawnQueue()
    /*Fill the spawn queue so it's ready for the wave to use*/
    {
        //
    }

    public void DeallocateWave()
    /*Destroy all the GameObjects in unitHandles, then clear list*/
    {
        //
    }

    public string FindNextWave(string previous)
    {
        switch (waveMode.ToLower())
        {
            case "sequential":
                string lastWaveNum = previous.Substring(4);
                return String.Concat("wave", (lastWaveNum+1));
            case "procedural":
                if (waveTransitions != null && waveTransitions.TryGetValue(previous, out string next))
                    return next;
                Debug.LogWarning("No procedural transition found, defaulting to wave1");
                return "wave1";
            default:
                Debug.Log("Error: Spawn method was not specified");
                return "wave1";
        }
    }
    
    public void SetState(WaveState newState) { currentState = newState; }
}
