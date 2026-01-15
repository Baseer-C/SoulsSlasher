using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(TerrainCollider))]
public class ArenaGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 256;
    public int depth = 512;
    public int height = 25; // High walls to trap player
    public float scale = 20f;

    [Header("Path Settings")]
    public float pathWidth = 15f;
    public float pathWander = 30f;
    public float pathFrequency = 0.02f;
    [Range(0f, 1f)]
    public float pathDensity = 0.1f;

    [Header("Boss Arena Settings")]
    [Range(0.5f, 0.9f)]
    public float arenaZPosition = 0.85f;
    public float arenaRadius = 40f;
    public float arenaWallSteepness = 20f;

    [Header("Enemy Spawner")]
    public GameObject bossPrefab;
    public GameObject[] minionPrefabs;

    [Tooltip("Number of enemies to spawn strictly on the main path")]
    public int minionsOnPath = 20;

    [Tooltip("Number of enemies to spawn deep in the woods")]
    public int minionsInForest = 5;

    [Tooltip("How wide they spread from the center of the path (Path Minions only)")]
    public float minionScatter = 8f;
    public float spawnHeightOffset = 0.1f;

    [Header("Minion Tuner")]
    public float minionMoveSpeed = 3.5f;
    public int minionMaxHealth = 100;

    [Header("UI References")]
    [Tooltip("Drag the RED FILL image of the health bar here, not the parent.")]
    public Image bossHealthBarImage;
    public GameObject winScreenObject;

    [Header("Pickup Settings")]
    public GameObject healthPickupPrefab;
    public GameObject damagePickupPrefab;
    public int forestPickupCount = 15;

    [Header("Environment Settings")]
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    public int treeCount = 1000;
    public int rockCount = 200;

    [HideInInspector]
    public List<GameObject> spawnedObjects = new List<GameObject>();

    [ContextMenu("Generate Adventure Map")]
    public void GenerateArena()
    {
        if (gameObject.name == "EnvironmentContainer" || gameObject.name == "EnemyContainer")
        {
            Debug.LogError("? Attach this to the TERRAIN, not a container.");
            return;
        }

        Terrain terrain = GetComponent<Terrain>();

        ClearObjects();

        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        transform.position = new Vector3(-width / 2, 0, 0);

        PlaceEnvironment(terrain);
        SpawnEnemies(terrain);
        SpawnPickups(terrain);

        Debug.Log("? Adventure Map & Pickups Generated!");
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, depth);

        float[,] heights = new float[width, depth];
        Vector2 bossCenter = new Vector2(width / 2f, depth * arenaZPosition);

        // --- CALCULATION VARIABLES ---
        float pathEndZ = depth * arenaZPosition;
        float backWallThickness = 15f;

        // --- HEIGHT TUNING ---
        float wallHeight = 1.0f;
        float forestFloorHeight = 0.08f;
        float arenaFloorHeight = 0.0f;
        float rampLength = 50f;

        float rampStartZ = (depth * arenaZPosition) - arenaRadius - rampLength;

        for (int z = 0; z < depth; z++)
        {
            // 1. Path Wiggle Math
            float progressToBoss = Mathf.Clamp01(z / (depth * arenaZPosition));
            float currentWanderMultiplier = 1f;
            if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
            float pathCenterX = (width / 2f) + (Mathf.Sin(z * pathFrequency) * pathWander * currentWanderMultiplier);

            // 2. Determine "Floor Level"
            float currentPathFloor = forestFloorHeight;
            if (z > rampStartZ && z < pathEndZ)
            {
                float rampProgress = (z - rampStartZ) / (pathEndZ - rampStartZ);
                currentPathFloor = Mathf.SmoothStep(forestFloorHeight, arenaFloorHeight, rampProgress);
            }
            else if (z >= pathEndZ)
            {
                currentPathFloor = arenaFloorHeight;
            }

            for (int x = 0; x < width; x++)
            {
                // --- A. Base Noisy Walls ---
                float xCoord = (float)x / width * scale;
                float zCoord = (float)z / depth * scale;
                float baseNoise = Mathf.PerlinNoise(xCoord, zCoord);
                float environmentHeight = 0.6f + (baseNoise * 0.4f);

                if (x < 10 || x > width - 10 || z < 10) environmentHeight = 1.0f;

                // --- B. Digging The Hollows ---
                float finalHeight = environmentHeight;

                // 1. Arena Digging
                float arenaFactor = 1.0f;

                if (z <= bossCenter.y)
                {
                    float distToBoss = Vector2.Distance(new Vector2(x, z), bossCenter);
                    if (distToBoss < arenaRadius) arenaFactor = arenaFloorHeight;
                    else if (distToBoss < arenaRadius + arenaWallSteepness)
                    {
                        float t = (distToBoss - arenaRadius) / arenaWallSteepness;
                        arenaFactor = Mathf.Lerp(arenaFloorHeight, wallHeight, t);
                    }
                }
                else
                {
                    float distToSpine = Mathf.Abs(x - (width / 2f));
                    float distToBackWall = depth - z;
                    bool insideWidth = distToSpine < arenaRadius;
                    bool beforeBackWall = distToBackWall > backWallThickness;

                    if (insideWidth && beforeBackWall) arenaFactor = arenaFloorHeight;
                    else
                    {
                        float sideRamp = (distToSpine - arenaRadius) / arenaWallSteepness;
                        float backRamp = (backWallThickness - distToBackWall) / (backWallThickness * 0.5f);

                        float t = 0f;
                        if (!insideWidth && !beforeBackWall) t = Mathf.Max(sideRamp, backRamp);
                        else if (!insideWidth) t = sideRamp;
                        else t = backRamp;

                        arenaFactor = Mathf.Lerp(arenaFloorHeight, wallHeight, t);
                    }
                }

                // 2. Path Digging
                float pathFactor = 1.0f;
                if (z <= pathEndZ)
                {
                    float distToPath = Mathf.Abs(x - pathCenterX);
                    if (distToPath < pathWidth)
                    {
                        pathFactor = currentPathFloor;
                    }
                    else if (distToPath < pathWidth + 15f)
                    {
                        float t = (distToPath - pathWidth) / 15f;
                        pathFactor = Mathf.Lerp(currentPathFloor, wallHeight, t);
                    }
                }

                // --- C. Merge ---
                float hollowHeight = Mathf.Min(arenaFactor, pathFactor);
                finalHeight = Mathf.Min(environmentHeight, hollowHeight);

                heights[z, x] = finalHeight;
            }
        }

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    void SpawnPickups(Terrain terrain)
    {
        GameObject container = new GameObject("PickupContainer");
        container.transform.parent = this.transform;
        container.transform.localPosition = Vector3.zero;

        if (healthPickupPrefab == null || damagePickupPrefab == null) return;

        SpawnSpecificPickup(terrain, container, healthPickupPrefab, 0, 15f);
        SpawnSpecificPickup(terrain, container, healthPickupPrefab, 3, 15f);
        SpawnSpecificPickup(terrain, container, damagePickupPrefab, -3, 15f);

        float arenaEntranceZ = (depth * arenaZPosition) - arenaRadius - 5f;
        SpawnSpecificPickup(terrain, container, healthPickupPrefab, 4, arenaEntranceZ);
        SpawnSpecificPickup(terrain, container, healthPickupPrefab, -4, arenaEntranceZ);
        SpawnSpecificPickup(terrain, container, damagePickupPrefab, 0, arenaEntranceZ);

        int placed = 0;
        int attempts = 0;
        float maxZ = (depth * arenaZPosition) - arenaRadius - 20f;

        while (placed < forestPickupCount && attempts < forestPickupCount * 20)
        {
            attempts++;
            float z = Random.Range(40f, maxZ);
            float progressToBoss = Mathf.Clamp01(z / maxZ);
            float currentWanderMultiplier = 1f;
            if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
            float pathCenterX = (width / 2f) + (Mathf.Sin(z * pathFrequency) * pathWander * currentWanderMultiplier);

            float xOffset = Random.Range(-25f, 25f);
            float x = pathCenterX + xOffset;

            if (x > 5 && x < width - 5)
            {
                GameObject prefab = (Random.value > 0.3f) ? healthPickupPrefab : damagePickupPrefab;
                float worldX = x + transform.position.x;
                float worldZ = z + transform.position.z;
                float y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                Vector3 pos = new Vector3(worldX, y + transform.position.y + 1.0f, worldZ);
                GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
                obj.transform.parent = container.transform;
                placed++;
            }
        }
    }

    void SpawnSpecificPickup(Terrain terrain, GameObject container, GameObject prefab, float xOffsetFromCenter, float zPos)
    {
        float arenaStartZ = (depth * arenaZPosition) - arenaRadius;
        float progressToBoss = Mathf.Clamp01(zPos / arenaStartZ);
        float currentWanderMultiplier = 1f;
        if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
        float pathCenterX = (width / 2f) + (Mathf.Sin(zPos * pathFrequency) * pathWander * currentWanderMultiplier);
        float finalX = pathCenterX + xOffsetFromCenter;
        float worldX = finalX + transform.position.x;
        float worldZ = zPos + transform.position.z;
        float y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
        Vector3 pos = new Vector3(worldX, y + transform.position.y + 1.0f, worldZ);
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        obj.transform.parent = container.transform;
    }

    void SpawnEnemies(Terrain terrain)
    {
        GameObject container = new GameObject("EnemyContainer");
        container.transform.parent = this.transform;
        container.transform.localPosition = Vector3.zero;

        // --- BOSS SPAWNING ---
        if (bossPrefab != null)
        {
            float bossX = width / 2f;
            float bossZ = depth * arenaZPosition;
            float worldX = bossX + transform.position.x;
            float worldZ = bossZ + transform.position.z;
            float y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
            Vector3 bossPos = new Vector3(worldX, y + transform.position.y + spawnHeightOffset, worldZ);

            GameObject boss = Instantiate(bossPrefab, bossPos, Quaternion.identity);
            boss.transform.parent = container.transform;
            boss.transform.rotation = Quaternion.Euler(0, 180, 0);

            // --- FIX: MORE ROBUST UI ASSIGNMENT ---
            // 1. Try to find the script on the root
            BossAI bossAI = boss.GetComponent<BossAI>();

            // 2. If not found, look in children (in case it's on a child mesh)
            if (bossAI == null) bossAI = boss.GetComponentInChildren<BossAI>();

            // 3. Assign
            if (bossAI != null)
            {
                if (bossHealthBarImage != null)
                {
                    bossAI.healthBar = bossHealthBarImage;
                    bossAI.healthBar.fillAmount = 1f; // Force it full visually immediately
                    Debug.Log("? UI: Health Bar assigned to Boss successfully.");
                }
                else
                {
                    Debug.LogError("? UI ERROR: 'Boss Health Bar Image' is EMPTY in the ArenaGenerator Inspector!");
                }
            }
            else
            {
                Debug.LogError("? BOSS ERROR: Could not find 'BossAI' script on the Boss Prefab!");
            }

            // --- Win Screen Logic (Same Robust Logic) ---
            CharacterStats bossStats = boss.GetComponent<CharacterStats>();
            if (bossStats == null) bossStats = boss.GetComponentInChildren<CharacterStats>();

            if (bossStats != null && winScreenObject != null)
            {
                bossStats.deathScreen = winScreenObject;
            }
        }

        // --- MINION SPAWNING ---
        if (minionPrefabs != null && minionPrefabs.Length > 0)
        {
            float arenaStartZ = (depth * arenaZPosition) - arenaRadius - 20f;

            // 1. SPAWN PATH MINIONS (Majority)
            int placedOnPath = 0;
            int attemptsPath = 0;
            float effectiveScatter = Mathf.Min(minionScatter, pathWidth * 0.8f);

            while (placedOnPath < minionsOnPath && attemptsPath < minionsOnPath * 20)
            {
                attemptsPath++;
                float z = Random.Range(40f, arenaStartZ);

                float progressToBoss = Mathf.Clamp01(z / ((depth * arenaZPosition) - arenaRadius));
                float currentWanderMultiplier = 1f;
                if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
                float pathCenterX = (width / 2f) + (Mathf.Sin(z * pathFrequency) * pathWander * currentWanderMultiplier);

                float xOffset = Random.Range(-effectiveScatter, effectiveScatter);
                float x = pathCenterX + xOffset;

                SpawnSingleMinion(x, z, terrain, container);
                placedOnPath++;
            }

            // 2. SPAWN FOREST MINIONS (Minority)
            int placedInForest = 0;
            int attemptsForest = 0;

            while (placedInForest < minionsInForest && attemptsForest < minionsInForest * 30)
            {
                attemptsForest++;
                float z = Random.Range(20f, arenaStartZ);
                float x = Random.Range(5f, width - 5f);

                float progressToBoss = Mathf.Clamp01(z / ((depth * arenaZPosition) - arenaRadius));
                float currentWanderMultiplier = 1f;
                if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
                float pathCenterX = (width / 2f) + (Mathf.Sin(z * pathFrequency) * pathWander * currentWanderMultiplier);

                float distToPath = Mathf.Abs(x - pathCenterX);

                if (distToPath > pathWidth + 5f)
                {
                    SpawnSingleMinion(x, z, terrain, container);
                    placedInForest++;
                }
            }
        }
    }

    // Helper function to handle the actual instantiation and tuning
    void SpawnSingleMinion(float x, float z, Terrain terrain, GameObject container)
    {
        float worldX = x + transform.position.x;
        float worldZ = z + transform.position.z;
        float y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

        GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
        Vector3 spawnPos = new Vector3(worldX, y + transform.position.y + spawnHeightOffset, worldZ);
        GameObject minion = Instantiate(prefab, spawnPos, Quaternion.identity);
        minion.transform.parent = container.transform;
        minion.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        NavMeshAgent agent = minion.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = minionMoveSpeed;
        }

        CharacterStats stats = minion.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.maxHealth = minionMaxHealth;
            stats.currentHealth = minionMaxHealth;
        }
    }

    void PlaceEnvironment(Terrain terrain)
    {
        GameObject container = new GameObject("EnvironmentContainer");
        container.transform.parent = this.transform;
        container.transform.localPosition = Vector3.zero;

        SpawnPathObstacles(terrain, container, treePrefabs);
        SpawnForestObjects(terrain, container, treePrefabs, treeCount);
        SpawnForestObjects(terrain, container, rockPrefabs, rockCount);
    }

    void SpawnPathObstacles(Terrain terrain, GameObject container, GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0) return;
        if (pathDensity <= 0.01f) return;

        float arenaStartZ = (depth * arenaZPosition) - arenaRadius;

        for (float z = 10f; z < arenaStartZ; z += 2f)
        {
            if (Random.value < pathDensity)
            {
                float progressToBoss = Mathf.Clamp01(z / arenaStartZ);
                float currentWanderMultiplier = 1f;
                if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
                float pathCenterX = (width / 2f) + (Mathf.Sin(z * pathFrequency) * pathWander * currentWanderMultiplier);

                float xOffset = Random.Range(-pathWidth / 2f, pathWidth / 2f);
                float x = pathCenterX + xOffset;

                float worldX = x + transform.position.x;
                float worldZ = z + transform.position.z;
                float y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.transform.position = new Vector3(worldX, y + transform.position.y - 0.1f, worldZ);
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    float s = Random.Range(0.8f, 1.4f);
                    obj.transform.localScale *= s;
                    obj.transform.parent = container.transform;
                }
            }
        }
    }

    void SpawnForestObjects(Terrain terrain, GameObject container, GameObject[] prefabs, int count)
    {
        if (prefabs == null || prefabs.Length == 0) return;
        int placed = 0;
        int attempts = 0;
        Vector2 bossCenter = new Vector2(width / 2f, depth * arenaZPosition);
        float arenaStartZ = (depth * arenaZPosition) - arenaRadius;

        while (placed < count && attempts < count * 20)
        {
            attempts++;
            float x = Random.Range(0, width);
            float z = Random.Range(0, depth);
            float progressToBoss = Mathf.Clamp01(z / arenaStartZ);
            float currentWanderMultiplier = 1f;
            if (progressToBoss > 0.7f) currentWanderMultiplier = 1f - ((progressToBoss - 0.7f) / 0.3f);
            float pathCenterX = (width / 2f) + (Mathf.Sin(z * pathFrequency) * pathWander * currentWanderMultiplier);
            float distToPath = Mathf.Abs(x - pathCenterX);
            float distToBoss = Vector2.Distance(new Vector2(x, z), bossCenter);

            bool shouldSpawn = true;
            if (distToBoss < arenaRadius + 5f) shouldSpawn = false;
            else if (distToPath < pathWidth + 6f) shouldSpawn = false;

            if (shouldSpawn)
            {
                float worldX = x + transform.position.x;
                float worldZ = z + transform.position.z;
                float y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.transform.position = new Vector3(worldX, y + transform.position.y - 0.1f, worldZ);
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    float s = Random.Range(0.8f, 1.4f);
                    obj.transform.localScale *= s;
                    obj.transform.parent = container.transform;
                    placed++;
                }
            }
        }
    }

    void ClearObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name == "EnvironmentContainer" || child.name == "EnemyContainer" || child.name == "PickupContainer")
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}