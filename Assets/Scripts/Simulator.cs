﻿using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Simulator : MonoBehaviour
{
    public Map Map { get; private set; }

    private EffectLayerManager _effectLayerManager;
    private TemperatureEffectLayerManagerEffector _temperatureEffectLayerManagerEffector;

    void Start()
    {
        Map = GetComponent<Map>();
        _effectLayerManager = new EffectLayerManager(Map);
        _effectLayerManager.CreateLayer<Monitor>("temperature");
        _temperatureEffectLayerManagerEffector = new TemperatureEffectLayerManagerEffector();
        SetAllMonitorNeighbours();
    }

    void Update()
    {
        if (Map == null || _temperatureEffectLayerManagerEffector == null)
        {
            return;
        }

        if (_temperatureEffectLayerManagerEffector.IsReady)
        {
            var tempLayer = _effectLayerManager.GetLayer<Monitor>("temperature");
            _temperatureEffectLayerManagerEffector.Update(Map, tempLayer);
        }
    }

    public void Test()
    {
        var layer = _effectLayerManager.GetLayer<Monitor>("temperature");
        int count = Map.Width * Map.Height;
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                if(Map.Terrain[x, y] != TileType.Rock)
                {
                    layer[x, y].Conductivity = 0.25f;
                }
            }
        }
        UnityEngine.Debug.Log($"Nodes in layer = {count}");
        layer[0, 0].IncreaseTemp(count * 100);
    }

    private void SetAllMonitorNeighbours()
    {
        var layer = _effectLayerManager.GetLayer<Monitor>("temperature");
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                if (layer[x, y] != null)
                {
                    layer[x, y].SetAllNeighbours(Map, layer);
                }
            }
        }
    }
}