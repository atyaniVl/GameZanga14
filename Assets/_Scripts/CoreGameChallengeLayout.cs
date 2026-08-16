using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Builds the authored challenge route for the Core Game scene.</summary>
public class CoreGameChallengeLayout : MonoBehaviour
{
    [Header("Level Geometry")]
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap backgroundTilemap;

    [Header("Challenge Prefabs")]
    [SerializeField] private GameObject cannonLeftPrefab;
    [SerializeField] private GameObject cannonRightPrefab;
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject patrolEnemyPrefab;

    [Header("Pickups")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject rocksPrefab;
    [SerializeField] private GameObject keyPrefab;

    [Header("Prefab Bottom Offsets")]
    [Tooltip("Local Y coordinate of the cannon's lowest point relative to its prefab pivot.")]
    [SerializeField] private float cannonBottomOffset = -0.5f;
    [Tooltip("Local Y coordinate of the mine's lowest point relative to its prefab pivot.")]
    [SerializeField] private float mineBottomOffset = -0.25f;
    [Tooltip("Local Y coordinate of the patrol enemy's lowest point relative to its prefab pivot.")]
    [SerializeField] private float patrolEnemyBottomOffset = -0.5f;
    [Tooltip("Local Y coordinate of the heart's lowest point relative to its prefab pivot.")]
    [SerializeField] private float heartBottomOffset = -0.4f;
    [Tooltip("Local Y coordinate of the rocks' lowest point relative to its prefab pivot.")]
    [SerializeField] private float rocksBottomOffset = -0.35f;
    [Tooltip("Local Y coordinate of the key's lowest point relative to its prefab pivot.")]
    [SerializeField] private float keyBottomOffset = -0.45f;

    private void Awake()
    {
        if (!HasRequiredPrefabs() || terrainTilemap == null || backgroundTilemap == null)
        {
            Debug.LogError("CoreGameChallengeLayout is missing its BG/Tiles references or one or more prefab references.", this);
            return;
        }

        RemoveLegacyKeys();
        var layoutRoot = new GameObject("Core Game Challenge Layout").transform;
        SpawnPickups(layoutRoot);
        SpawnKeys(layoutRoot);
        SpawnHazards(layoutRoot);
    }

    private bool HasRequiredPrefabs() =>
        cannonLeftPrefab != null && cannonRightPrefab != null &&
        minePrefab != null && patrolEnemyPrefab != null &&
        heartPrefab != null && rocksPrefab != null && keyPrefab != null;

    private static void RemoveLegacyKeys()
    {
        foreach (var key in GameObject.FindGameObjectsWithTag("Key"))
            Destroy(key);
    }

    private void SpawnPickups(Transform parent)
    {
        Spawn(rocksPrefab, new(8f, -2f), parent, "Rocks - Start");
        Spawn(rocksPrefab, new(55f, 5f), parent, "Rocks - Mid Level");
        Spawn(rocksPrefab, new(145f, 13f), parent, "Rocks - BG Entrance");
        Spawn(rocksPrefab, new(230f, 23f), parent, "Rocks - BG Middle");
        Spawn(rocksPrefab, new(310f, 30f), parent, "Rocks - BG Exit");
        Spawn(rocksPrefab, new(350f, 35f), parent, "Rocks - Final Route");
        Spawn(heartPrefab, new(32f, 2f), parent, "Heart - First Reward");
        Spawn(heartPrefab, new(85f, 8f), parent, "Heart - Mid Reward");
        Spawn(heartPrefab, new(185f, 18f), parent, "Heart - BG Reward");
        Spawn(heartPrefab, new(280f, 28f), parent, "Heart - BG Exit Reward");
        Spawn(heartPrefab, new(370f, 39f), parent, "Heart - Final Route Reward");
        Spawn(heartPrefab, new(410f, 44f), parent, "Heart - Final Reward");
    }

    private void SpawnKeys(Transform parent)
    {
        // The twelve keys lead the player across the whole Tiles map.
        Vector2[] keyPositions =
        {
            new(-4f, -2f), new(32f, 2f), new(70f, 6f), new(105f, 10f),
            new(140f, 13f), new(170f, 17f), new(200f, 20f), new(230f, 23f),
            new(260f, 26f), new(290f, 29f), new(350f, 36f), new(405f, 43f)
        };

        for (int i = 0; i < keyPositions.Length; i++)
            Spawn(keyPrefab, keyPositions[i], parent, $"Key {i + 1:00}");
    }

    private void SpawnHazards(Transform parent)
    {
        Spawn(minePrefab, new(55f, 5f), parent, "Mine - Lower Route");
        Spawn(minePrefab, new(100f, 10f), parent, "Mine - Mid Route");
        Spawn(minePrefab, new(125f, 13f), parent, "Mine - First Summit");
        Spawn(minePrefab, new(165f, 16f), parent, "Mine - BG Route");
        Spawn(minePrefab, new(245f, 24f), parent, "Mine - BG Middle");
        Spawn(minePrefab, new(305f, 30f), parent, "Mine - BG Exit");
        Spawn(minePrefab, new(345f, 34f), parent, "Mine - Final Route");
        Spawn(patrolEnemyPrefab, new(75f, 7f), parent, "Patrol Enemy - Mid Level");
        Spawn(patrolEnemyPrefab, new(115f, 12f), parent, "Patrol Enemy - First Summit");
        Spawn(patrolEnemyPrefab, new(195f, 19f), parent, "Patrol Enemy - BG Route");
        Spawn(patrolEnemyPrefab, new(275f, 28f), parent, "Patrol Enemy - BG Exit");
        Spawn(patrolEnemyPrefab, new(370f, 39f), parent, "Patrol Enemy - Final Route");
        Spawn(patrolEnemyPrefab, new(405f, 43f), parent, "Patrol Enemy - Final Summit");
        Spawn(cannonLeftPrefab, new(90f, 9f), parent, "Cannon Left - Mid Route");
        Spawn(cannonRightPrefab, new(130f, 14f), parent, "Cannon Right - First Summit");
        Spawn(cannonLeftPrefab, new(220f, 22f), parent, "Cannon Left - BG Middle");
        Spawn(cannonRightPrefab, new(295f, 29f), parent, "Cannon Right - BG Exit");
        Spawn(cannonLeftPrefab, new(390f, 41f), parent, "Cannon Left - Final Route");
    }

    private void Spawn(GameObject prefab, Vector2 position, Transform parent, string objectName)
    {
        var instance = Instantiate(prefab, FindSpawnPoint(position, GetBottomOffset(prefab)), Quaternion.identity, parent);
        instance.name = objectName;
    }

    private float GetBottomOffset(GameObject prefab)
    {
        if (prefab == cannonLeftPrefab || prefab == cannonRightPrefab)
            return cannonBottomOffset;
        if (prefab == minePrefab)
            return mineBottomOffset;
        if (prefab == patrolEnemyPrefab)
            return patrolEnemyBottomOffset;
        if (prefab == heartPrefab)
            return heartBottomOffset;
        if (prefab == rocksPrefab)
            return rocksBottomOffset;
        return keyBottomOffset;
    }

    private Vector3 FindSpawnPoint(Vector2 requestedPosition, float bottomOffset)
    {
        Tilemap bestTilemap = terrainTilemap;
        Vector3Int bestCell = terrainTilemap.WorldToCell(requestedPosition);
        float bestScore = float.MaxValue;

        foreach (Tilemap tilemap in new[] { terrainTilemap, backgroundTilemap })
        {
            Vector3Int requestedCell = tilemap.WorldToCell(requestedPosition);
            for (int xOffset = -12; xOffset <= 12; xOffset++)
            {
                for (int yOffset = -12; yOffset <= 12; yOffset++)
                {
                    Vector3Int cell = requestedCell + new Vector3Int(xOffset, yOffset, 0);
                    if (!tilemap.HasTile(cell) || tilemap.HasTile(cell + Vector3Int.up))
                        continue;

                    float score = Mathf.Abs(xOffset) * 1.5f + Mathf.Abs(yOffset);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestCell = cell;
                        bestTilemap = tilemap;
                    }
                }
            }
        }

        // The tile center is half a cell below its walkable top edge. Move the
        // prefab pivot so its configured local bottom rests on that edge.
        Vector3 tileTop = bestTilemap.GetCellCenterWorld(bestCell) + Vector3.up * 0.5f;
        return tileTop - Vector3.up * bottomOffset;
    }
}
