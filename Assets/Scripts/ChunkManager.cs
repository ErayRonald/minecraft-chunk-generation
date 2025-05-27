using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int maxLoadedChunks = 100;

    private readonly Dictionary<Vector2Int, Chunk> _loadedChunks = new Dictionary<Vector2Int, Chunk>();
    private readonly List<Chunk> _loadedChunksList = new List<Chunk>();
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        GenerateChunks();
    }

    private void Update()
    {
        UpdateChunkLoading();
    }


    private void GenerateChunks()
    {
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(maxLoadedChunks));
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                LoadChunk(chunkCoord);
            }
        }
    }

    private float CalculateChunkPriority(Vector2Int chunkCoord)
    {
        Vector3 chunkPosition = new Vector3(chunkCoord.x * Chunk.ChunkSize, 0, chunkCoord.y * Chunk.ChunkSize);
        float distance = Vector3.Distance(chunkPosition, _mainCamera.transform.position);
        float priority = 1f / (distance + 1f);
        return priority;
    }

    private void UpdateChunkLoading()
    {
        Vector3 cameraPosition = _mainCamera.transform.position;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);

        Vector2Int cameraChunkCoord = new Vector2Int(
            Mathf.RoundToInt(cameraPosition.x / Chunk.ChunkSize),
            Mathf.RoundToInt(cameraPosition.z / Chunk.ChunkSize)
        );

        int loadRange = Mathf.CeilToInt(Mathf.Sqrt(maxLoadedChunks));

        _loadedChunksList.Clear();
        List<KeyValuePair<Vector2Int, float>> chunkPriorities = new List<KeyValuePair<Vector2Int, float>>();

        for (int x = cameraChunkCoord.x - loadRange; x <= cameraChunkCoord.x + loadRange; x++)
        {
            for (int y = cameraChunkCoord.y - loadRange; y <= cameraChunkCoord.y + loadRange; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                Vector3 chunkPosition = new Vector3(chunkCoord.x * Chunk.ChunkSize, 0, chunkCoord.y * Chunk.ChunkSize);
                Bounds chunkBounds = new Bounds(chunkPosition + Vector3.one * (Chunk.ChunkSize * 0.5f),
                    Vector3.one * Chunk.ChunkSize);

                if (GeometryUtility.TestPlanesAABB(frustumPlanes, chunkBounds))
                {
                    float priority = CalculateChunkPriority(chunkCoord);
                    chunkPriorities.Add(new KeyValuePair<Vector2Int, float>(chunkCoord, priority));
                }
            }
        }

        chunkPriorities.Sort((a, b) => b.Value.CompareTo(a.Value));

        int chunksToLoad = Mathf.Min(maxLoadedChunks, chunkPriorities.Count);
        for (int i = 0; i < chunksToLoad; i++)
        {
            Vector2Int chunkCoord = chunkPriorities[i].Key;
            if (!_loadedChunks.TryGetValue(chunkCoord, out Chunk chunk))
            {
                LoadChunk(chunkCoord);
                chunk = _loadedChunks[chunkCoord];
            }

            _loadedChunksList.Add(chunk);
        }

        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, Chunk> kvp in _loadedChunks)
        {
            if (!_loadedChunksList.Contains(kvp.Value))
            {
                chunksToUnload.Add(kvp.Key);
            }
        }

        foreach (Vector2Int chunkCoord in chunksToUnload)
        {
            UnloadChunk(chunkCoord);
        }
    }

    private void LoadChunk(Vector2Int chunkCoord)
    {
        if (!_loadedChunks.ContainsKey(chunkCoord))
        {
            GameObject chunkObject = Instantiate(chunkPrefab, transform);
            chunkObject.name = $"Chunk_{chunkCoord.x}_{chunkCoord.y}";

            Chunk chunk = chunkObject.GetComponent<Chunk>();
            chunk.chunkCoord = chunkCoord;
            _loadedChunks.Add(chunkCoord, chunk);
        }
    }

    private void UnloadChunk(Vector2Int chunkCoord)
    {
        if (_loadedChunks.ContainsKey(chunkCoord))
        {
            Chunk chunk = _loadedChunks[chunkCoord];
            _loadedChunks.Remove(chunkCoord);
            Destroy(chunk.gameObject);
        }
    }
}