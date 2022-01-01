using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public static Map Instance => _instance;

    [Range(2, 1000)] public int Width = 200;
    [Range(2, 1000)] public int Height = 200;
    public int Seed;
    public int SandSeedOffset = 200;
    public int GrassSeedOffset = 300;
    public int TreeSeedOffset = 400;
    public TileBase WaterTile;
    public TileBase LandTile;
    public TileBase SandTile;
    public TileBase GrassTile;
    public TileBase FlowersTile;
    public TileBase RockTile;
    public TileBase TreeTile;
    public Canvas Canvas;
    public float ChanceOfFlower = 0.5f;
    public float ChanceOfTree = 0.5f;

    public TileType[,] Terrain => _terrain;

    private static Map _instance;
    private GameObject _tilesRoot;
    private GameObject _gridRoot;
    private Tilemap _tilemap;

    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator;
    private float[,] _terrainNoiseLayer;
    private float[,] _sandNoiseLayer;
    private float[,] _grassNoiseLayer;
    private TileType[,] _terrain;
    private System.Random _random;

    public Map()
    {
        _instance = this;
        _random = new System.Random(Seed);
        _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    }

    void Start()
    {
        var canvas = GameObject.FindWithTag("UICanvas");
        if(canvas != null)
        {
            var rectTransform = canvas.GetComponent<RectTransform>();
            rectTransform.position = new Vector3(Width / 2, Height / 2, 0);
            rectTransform.sizeDelta = new Vector2(Width, Height);
        }

        Prepare();
        CreateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int tilemapPos = _tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if((tilemapPos.x >= 0 && tilemapPos.x < Width) &&
                (tilemapPos.y >=0 && tilemapPos.y < Height))
            {
                var square = Canvas.transform.Find("Square");
                square.transform.position = new Vector3(tilemapPos.x + 0.5f, tilemapPos.y + 0.5f, 0);

                var simulator = GetComponent<Simulator>();
                if(simulator != null)
                {
                    var monitor = simulator.EffectLayerManager.GetLayer<Monitor>("temperature")[tilemapPos.x, tilemapPos.y];
                    Debug.Log($"{Terrain[tilemapPos.x,tilemapPos.y].ToString()} - {tilemapPos.x},{tilemapPos.y} = Temp = {monitor.Temperature}, Condictivity = {monitor.Conductivity}");
                }
            }
        }
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
        _terrain = new TileType[Width, Height];
        _terrainNoiseLayer = _perlinNoiseMapGenerator.Generate(Seed, Width, Height);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var tileType = GetTileTypeFromHeight(_terrainNoiseLayer[x, y]);
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
                                LandTile);

                            _tilemap.SetTile(
                                new Vector3Int(x, y, 6),
                                tileBase);
                            break;
                        }
                }

                _terrain[x,y] = tileType;
            }
        }

        _sandNoiseLayer = AddLandLayer(
            2,
            SandSeedOffset,
            SandTile,
            0.475f,
            0.6f,
            true,
            TileType.Sand,
            new List<TileType> { TileType.Land, TileType.Rock },
            1.0f);

        _grassNoiseLayer = AddLandLayer(
            3,
            GrassSeedOffset,
            GrassTile,
            0.475f,
            0.75f,
            true,
            TileType.Grass,
            new List<TileType> { TileType.Land, TileType.Sand, TileType.Rock },
            1.0f);

        AddLandLayer(
            4,
            _grassNoiseLayer,
            FlowersTile,
            0.55f,
            0.75f,
            false,
            null,
            new List<TileType> { TileType.Grass },
            ChanceOfFlower);

        AddLandLayer(
            5,
            TreeSeedOffset,
            TreeTile,
            0.40f,
            0.80f,
            false,
            null,
            new List<TileType> { TileType.Grass, TileType.Land },
            ChanceOfTree);
    }

    private float[,] AddLandLayer(
        int zIndex,
        int seedOffset,
        TileBase tile,
        float minHeight,
        float maxHeight,
        bool setTerrain,
        TileType? tileType,
        List<TileType> ontop,
        float chance)
    {
        var noiseLayer = _perlinNoiseMapGenerator.Generate(Seed + seedOffset, Width, Height);
        AddLandLayer(zIndex, noiseLayer, tile, minHeight, maxHeight, setTerrain, tileType, ontop, chance);
        return noiseLayer;
    }

    private void AddLandLayer(
        int zIndex,
        float[,] noiseLayer,
        TileBase tile,
        float minHeight,
        float maxHeight,
        bool setTerrain,
        TileType? tileType,
        List<TileType> ontop,
        float chance)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var height = noiseLayer[x, y];
                if (height > minHeight && height < maxHeight)
                {
                    var rnd = _random.NextDouble();
                    bool set = chance == 1f || rnd <= chance;
                    if (set && ontop.Contains(_terrain[x, y]))
                    {
                        _tilemap.SetTile(
                            new Vector3Int(x, y, zIndex),
                            tile);

                        bool isRock = _terrain[x, y] == TileType.Rock;
                        if (setTerrain && tileType.HasValue && !isRock)
                        {
                            _terrain[x, y] = tileType.GetValueOrDefault();
                        }
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
                    return LandTile;
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
