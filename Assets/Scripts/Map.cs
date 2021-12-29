using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public static Map Instance => _instance;

    [Range(2, 400)] public int Width = 200;
    [Range(2, 400)] public int Height = 200;
    public int Seed;

    public TileBase WaterTile;
    public TileBase GrassTile;
    public TileBase RockTile;

    private static Map _instance;
    private GameObject _tilesRoot;
    private GameObject _gridRoot;
    private Tilemap _tilemap;

    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator;

    public Map()
    {
        _instance = this;
        _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    }

    void Start()
    {
        Prepare();
        CreateMap();
    }

    void Update()
    {
        
    }

    private void Prepare()
    {
        _tilesRoot = transform.AssureEmptyGameObjectChild("Tiles");
        _tilesRoot.AddComponent<Grid>();

        _gridRoot = _tilesRoot.transform.AssureEmptyGameObjectChild("Grid");
        _tilemap = _gridRoot.AddComponent<Tilemap>();
        _gridRoot.AddComponent<TilemapRenderer>();
    }

    private void CreateMap()
    {
        var terrain = _perlinNoiseMapGenerator.Generate(Seed, Width, Height);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var tileType = GetTileTypeFromHeight(terrain[x, y]);
                var tileBase = GetTileBaseFromTileType(tileType);

                _tilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    WaterTile);

                switch (tileType)
                {
                    case TileType.Land:
                        {
                            _tilemap.SetTile(
                                new Vector3Int(x, y, 1),
                                tileBase);
                            break;
                        }

                    case TileType.Rock:
                        {
                            _tilemap.SetTile(
                                new Vector3Int(x, y, 1),
                                GrassTile);

                            _tilemap.SetTile(
                                new Vector3Int(x, y, 2),
                                tileBase);
                            break;
                        }
                }
            }
        }
    }

    private TileType GetTileTypeFromHeight(float height)
    {
        if (height < 0.5f)
        {
            return TileType.Water;
        }
        else if (height < 0.75f)
        {
            return TileType.Land;
        }
        else
        {
            return TileType.Rock;
        }
    }

    private TileBase GetTileBaseFromTileType(TileType tileType)
    {
        switch(tileType)
        {
            case TileType.Water:
                {
                    return WaterTile;
                }

            case TileType.Land:
                {
                    return GrassTile;
                }

            case TileType.Rock:
                {
                    return RockTile;
                }

            default:
                {
                    throw new System.NotSupportedException($"Tile type {tileType} is not supported by GetTileBaseFromTileType.");
                }
        }
    }
}
