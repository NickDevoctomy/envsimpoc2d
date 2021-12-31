using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Monitor
{
    public float Temperature = 0f;
    public float Conductivity = 0f;
    public Point? Location;

    public IReadOnlyDictionary<NeighbourLocation, Monitor> Neighbours => _neighbours;
    public IReadOnlyList<Monitor> MonitorNeighbours => _neighbours.Values.ToList();
    public bool IsAwake { get; private set; } = false;
    public bool IsPendingUpdate { get; private set; } = false;

    private Dictionary<NeighbourLocation, Monitor> _neighbours = new Dictionary<NeighbourLocation, Monitor>();
    private bool _neighboursSet = false;
    private float _nextTemperature = 0f;

    public void SetParameter(string name, float value)
    {
        switch(name.ToLower())
        {
            case "temperature":
                {
                    Temperature = value;
                    break;
                }

            case "conductivity":
                {
                    Conductivity = value;
                    break;
                }

            default:
                {
                    throw new System.NotSupportedException($"Parameter name '{name}' is not supported.");
                }
        }
    }

    public void SetConductivity(float value)
    {
        Conductivity = value;
        IsAwake = true;
    }

    public void IncreaseTemp(float value)
    {
        _nextTemperature += value;
        IsPendingUpdate = true;
        IsAwake = true;
    }

    public void DecreaseTemp(float value)
    {
        _nextTemperature -= value;
        IsPendingUpdate = true;
        IsAwake = true;
    }

    public void ApplyNextTemperature()
    {
        if (Mathf.Abs(Temperature - _nextTemperature) < 0.00001f)
        {
            IsAwake = false;
            return;
        }

        Temperature = _nextTemperature;
        IsPendingUpdate = false;
    }

    public void SetAllNeighbours(Map map, Monitor[,] layer)
    {
        if (_neighboursSet)
        {
            return;
        }

        int x = Location.GetValueOrDefault().X;
        int y = Location.GetValueOrDefault().Y;

        if (x > 0 && layer[x - 1, y] != null)
        {
            SetNeighbour(NeighbourLocation.East, layer[x - 1, y]);
        }

        if (x < (map.Width - 1) && layer[x + 1, y] != null)
        {
            SetNeighbour(NeighbourLocation.West, layer[x + 1, y]);
        }

        if (y > 0 && layer[x, y - 1] != null)
        {
            SetNeighbour(NeighbourLocation.South, layer[x, y - 1]);
        }

        if (y < (map.Height - 1) && layer[x, y + 1] != null)
        {
            SetNeighbour(NeighbourLocation.North, layer[x, y + 1]);
        }

        _neighboursSet = true;
    }

    public void SetNeighbour(NeighbourLocation neighbour, Monitor monitor)
    {
        if (_neighbours.ContainsKey(neighbour))
        {
            _neighbours.Remove(neighbour);
        }

        _neighbours.Add(neighbour, monitor);
    }
}
