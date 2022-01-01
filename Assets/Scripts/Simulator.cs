using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public double AverageCycleTime;

    public Map Map { get; private set; }

    public EffectLayerManager EffectLayerManager { get; private set; }

    private object _lock = new object();
    private TemperatureEffectLayerManagerEffector _temperatureEffectLayerManagerEffector;
    private Task _simulating;
    private List<int> _cycleTimes = new List<int>();

    void Start()
    {
        Map = GetComponent<Map>();
        Dictionary<string, object> initialParameters = new Dictionary<string, object>();
        initialParameters.Add("temperature", 0.0f);
        initialParameters.Add("conductivity", 0.25f);
        EffectLayerManager = new EffectLayerManager(Map);
        EffectLayerManager.CreateLayer<Monitor>("temperature", initialParameters);
        _temperatureEffectLayerManagerEffector = new TemperatureEffectLayerManagerEffector();
        SetAllMonitorNeighbours();
    }

    void Update()
    {
        lock (_lock)
        {
            if(_simulating != null)
            {
                return;
            }

            _simulating = Task.Run(DoSimulation);
        }
    }

    public void Test()
    {
        //!!! Need a better way of doing this
        int rockCount = 0;
        var layer = EffectLayerManager.GetLayer<Monitor>("temperature");
        int count = Map.Width * Map.Height;
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                if(Map.Terrain[x, y] == TileType.Rock)
                {
                    layer[x, y].SetParameter("conductivity", 0f);
                    rockCount += 1;
                }
            }
        }
        UnityEngine.Debug.Log($"Nodes in layer = {count}, {rockCount} rocks present.");
        layer[0, 0].IncreaseTemp(count * 100);
    }

    private void DoSimulation()
    {
        while(true)
        {
            var startedAt = System.Environment.TickCount;
            
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