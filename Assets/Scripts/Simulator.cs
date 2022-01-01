using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public float AmbientTemperature = 100.0f;
    public double AverageCycleTime;

    public Map Map { get; private set; }

    public EffectLayerManager EffectLayerManager { get; private set; }

    private object _lock = new object();
    private TemperatureLayerEffector _temperatureEffectLayerManagerEffector;
    private Task _simulating;
    private List<int> _cycleTimes = new List<int>();
    private bool _ready = false;

    void Start()
    {
        Map = GetComponent<Map>();
        Dictionary<string, object> initialParameters = new Dictionary<string, object>();
        EffectLayerManager = new EffectLayerManager(Map);
        _temperatureEffectLayerManagerEffector = new TemperatureLayerEffector();
        EffectLayerManager.CreateLayer<Monitor>("temperature", _temperatureEffectLayerManagerEffector.Setup);
        _temperatureEffectLayerManagerEffector.AmbientTemperature = AmbientTemperature;
        SetAllMonitorNeighbours();
    }

    void Update()
    {
        lock (_lock)
        {
            if(!_ready || _simulating != null)
            {
                return;
            }

            _simulating = Task.Run(DoSimulation);
        }
    }

    public void StartSimulation()
    {
        _ready = true;
    }

    private void Setup(Monitor monitor)
    {
        var location = monitor.Location.GetValueOrDefault();
        if (Map.Terrain[location.X, location.Y] == TileType.Rock)
        {
            var enclosed = monitor.Neighbours.Values.All(x => x.GetTileTypeFromMap(Map) == TileType.Rock);
            monitor.SetParameter("conductivity", enclosed ? 0f : 0.0001f);
        }
    }

    private void DoSimulation()
    {
        while(true)
        {
            var startedAt = System.Environment.TickCount;
            if(AmbientTemperature != _temperatureEffectLayerManagerEffector.AmbientTemperature)
            {
                _temperatureEffectLayerManagerEffector.AmbientTemperature = AmbientTemperature;
            }
            
            if (Map == null || _temperatureEffectLayerManagerEffector == null)
            {
                continue;
            }

            if (_temperatureEffectLayerManagerEffector.IsReady)
            {
                var tempLayer = EffectLayerManager.GetLayer<Monitor>("temperature");
                _temperatureEffectLayerManagerEffector.Update(Map, tempLayer);
            }

            _cycleTimes.Add(System.Environment.TickCount - startedAt);
            if(_cycleTimes.Count > 10)
            {
                _cycleTimes.RemoveAt(0);
                AverageCycleTime = _cycleTimes.Average();
            }
        }
    }

    private void SetAllMonitorNeighbours()
    {
        var layer = EffectLayerManager.GetLayer<Monitor>("temperature");
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                layer[x, y].SetAllNeighbours(Map, layer);
            }
        }
    }
}