using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Chunk : MonoBehaviour
{
    #region Fields

    public Vector2Int chunkCoord;

    private readonly BlockType[,,] _blocks = new BlockType[32, 128, 32];
    public const int ChunkSize = 32;
    public Material BedrockMaterial;
    public Material StoneMaterial;
    public Material dirtMaterial;
    public Material grassBlockMaterial;
    private Dictionary<BlockType, Material> _blockMaterials;
    private Dictionary<BlockType, Vector2[]> _blockUVs;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        _blockMaterials = new Dictionary<BlockType, Material>
        {
            { BlockType.Bedrock, BedrockMaterial },
            { BlockType.Stone, StoneMaterial },
            { BlockType.Dirt, dirtMaterial },
            { BlockType.Grass, grassBlockMaterial }
        };

        _blockUVs = new Dictionary<BlockType, Vector2[]>
        {
            {
                BlockType.Bedrock,
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) }
            },
            {
                BlockType.Stone,
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) }
            },
            {
                BlockType.Dirt,
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) }
            },
            {
                BlockType.Grass,
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) }
            }
        };

        GenerateChunkMesh();
        SetPosition();
    }

    public void SetPosition()
    {
        Vector3 position = new Vector3(chunkCoord.x * 32, 0, chunkCoord.y * 32);
        // Debug.Log($"Chunk {chunkCoord} position: {position}");
        transform.position = position;
    }

    // Update is called once per frame
    // void Update()
    // {
    //
    // }


    private (float stoneHeight, float dirtHeight) GetStoneHeight(int x, int z)
    {
        float noiseValue = Mathf.PerlinNoise((chunkCoord.x * _blocks.GetLength(0) + x) * 0.05f,
            (chunkCoord.y * _blocks.GetLength(2) + z) * 0.05f);
        float stoneHeight = Mathf.Lerp(5f, 10f, noiseValue);

        float dirtNoiseValue = Mathf.PerlinNoise((chunkCoord.x * _blocks.GetLength(0) + x) * 0.1f,
            (chunkCoord.y * _blocks.GetLength(2) + z) * 0.1f);
        float dirtThickness = Mathf.Lerp(0f, 5f, dirtNoiseValue);
        float dirtHeight = stoneHeight + dirtThickness;

        return (stoneHeight, dirtHeight);
    }

    private void GenerateChunk()
    {
        for (int z = 0; z < _blocks.GetLength(2); z++)
        {
            for (int x = 0; x < _blocks.GetLength(0); x++)
            {
                int bedrockHeight = Random.Range(1, 4);
                for (int y = 0; y < bedrockHeight; y++)
                {
                    _blocks[x, y, z] = BlockType.Bedrock;
                }

                (float stoneHeight, float dirtHeight) = GetStoneHeight(x, z);
                int stoneHeightInt = Mathf.RoundToInt(stoneHeight);
                int dirtHeightInt = Mathf.RoundToInt(dirtHeight);

                for (int y = bedrockHeight; y < stoneHeightInt && y < _blocks.GetLength(1); y++)
                {
                    if (_blocks[x, y, z] == BlockType.Empty)
                    {
                        _blocks[x, y, z] = BlockType.Stone;
                    }
                }

                for (int y = stoneHeightInt; y < dirtHeightInt && y < _blocks.GetLength(1); y++)
                {
                    if (_blocks[x, y, z] == BlockType.Empty)
                    {
                        _blocks[x, y, z] = BlockType.Dirt;
                    }
                }
            }
        }

        GenerateCaves();
        //replace dirt to grass if at top
        for (int z = 0; z < _blocks.GetLength(2); z++)
        {
            for (int x = 0; x < _blocks.GetLength(0); x++)
            {
                for (int y = _blocks.GetLength(1) - 1; y >= 0; y--)
                {
                    if (_blocks[x, y, z] == BlockType.Dirt)
                    {
                        if (y == _blocks.GetLength(1) - 1 || _blocks[x, y + 1, z] == BlockType.Empty)
                        {
                            _blocks[x, y, z] = BlockType.Grass;
                        }

                        break;
                    }
                }
            }
        }
    }

    private void GenerateChunkMesh()
    {
        GenerateChunk();
        Dictionary<BlockType, List<Vector3>> verticesByBlockType = new Dictionary<BlockType, List<Vector3>>();
        Dictionary<BlockType, List<int>> trianglesByBlockType = new Dictionary<BlockType, List<int>>();
        Dictionary<BlockType, List<Vector2>> uvsByBlockType = new Dictionary<BlockType, List<Vector2>>();


        foreach (BlockType blockType in System.Enum.GetValues(typeof(BlockType)))
        {
            if (blockType != BlockType.Empty)
            {
                uvsByBlockType[blockType] = new List<Vector2>();
            }
        }

        for (int z = 0; z < _blocks.GetLength(2); z++)
        {
            for (int y = 0; y < _blocks.GetLength(1); y++)
            {
                for (int x = 0; x < _blocks.GetLength(0); x++)
                {
                    BlockType blockType = _blocks[x, y, z];
                    if (blockType != BlockType.Empty)
                    {
                        if (!verticesByBlockType.ContainsKey(blockType))
                        {
                            verticesByBlockType[blockType] = new List<Vector3>();
                            trianglesByBlockType[blockType] = new List<int>();
                        }

                        GenerateBlockMesh(x, y, z, verticesByBlockType[blockType], trianglesByBlockType[blockType],
                            uvsByBlockType[blockType]);
                    }
                }
            }

            foreach (BlockType blockType in verticesByBlockType.Keys)
            {
                Mesh chunkMesh = new Mesh
                {
                    vertices = verticesByBlockType[blockType].ToArray(),
                    triangles = trianglesByBlockType[blockType].ToArray(),
                    uv = uvsByBlockType[blockType].ToArray()
                };
                chunkMesh.RecalculateNormals();

                GameObject meshObject = new GameObject($"Mesh_{blockType}");
                meshObject.transform.SetParent(transform);

                MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
                meshFilter.mesh = chunkMesh;

                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.material = _blockMaterials[blockType];
            }
        }
    }

    private void GenerateBlockMesh(int x, int y, int z, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        int vertexOffset = vertices.Count;

        // Check neighboring blocks
        bool blockBelow = y > 0 && _blocks[x, y - 1, z] != BlockType.Empty;
        bool blockAbove = y < _blocks.GetLength(1) - 1 && _blocks[x, y + 1, z] != BlockType.Empty;
        bool blockLeft = x > 0 && _blocks[x - 1, y, z] != BlockType.Empty;
        bool blockRight = x < _blocks.GetLength(0) - 1 && _blocks[x + 1, y, z] != BlockType.Empty;
        bool blockBack = z < _blocks.GetLength(2) - 1 && _blocks[x, y, z + 1] != BlockType.Empty;
        bool blockFront = z > 0 && _blocks[x, y, z - 1] != BlockType.Empty;


        #region vertices

        // so now u make the vertices only for the visible faces
        if (!blockBelow)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 3);
            triangles.Add(vertexOffset + 2);
            vertexOffset += 4;
            uvs.AddRange(_blockUVs[_blocks[x, y, z]]);
        }

        if (!blockAbove)
        {
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 3);
            vertexOffset += 4;
            uvs.AddRange(_blockUVs[_blocks[x, y, z]]);
        }

        if (!blockFront)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 3);
            vertexOffset += 4;
            uvs.AddRange(_blockUVs[_blocks[x, y, z]]);
        }


        if (!blockBack)
        {
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 3);
            vertexOffset += 4;
            uvs.AddRange(_blockUVs[_blocks[x, y, z]]);
        }

        if (!blockLeft)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 3);
            triangles.Add(vertexOffset + 2);
            vertexOffset += 4;
            uvs.AddRange(_blockUVs[_blocks[x, y, z]]);
        }


        if (!blockRight)
        {
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));

            triangles.Add(vertexOffset + 0);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 1);
            triangles.Add(vertexOffset + 2);
            triangles.Add(vertexOffset + 3);
            uvs.AddRange(_blockUVs[_blocks[x, y, z]]);
        }

        #endregion
    }


    private void GenerateCaves()
    {
        int numCaves = Random.Range(10, 20);

        for (int i = 0; i < numCaves; i++)
        {
            int caveX = Random.Range(0, _blocks.GetLength(0));
            int caveY = Random.Range(10, _blocks.GetLength(1) - 10);
            int caveZ = Random.Range(0, _blocks.GetLength(2));
            int caveRadius = Random.Range(3, 8);

            for (int x = caveX - caveRadius; x <= caveX + caveRadius; x++)
            {
                for (int y = caveY - caveRadius; y <= caveY + caveRadius; y++)
                {
                    for (int z = caveZ - caveRadius; z <= caveZ + caveRadius; z++)
                    {
                        if (IsInsideChunk(x, y, z))
                        {
                            float distance = Vector3.Distance(new Vector3(x, y, z), new Vector3(caveX, caveY, caveZ));
                            if (distance <= caveRadius)
                            {
                                _blocks[x, y, z] = BlockType.Empty;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsInsideChunk(int x, int y, int z)
    {
        return x >= 0 && x < _blocks.GetLength(0) &&
               y >= 0 && y < _blocks.GetLength(1) &&
               z >= 0 && z < _blocks.GetLength(2);
    }
}