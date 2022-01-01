using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class Monitor : IMonitor
{
    public float Temperature { get; private set; } = 0f;
    public float Conductivity { get; private set; } = 0f;
    public Point? Location { get; private set; }

    public IReadOnlyDictionary<NeighbourLocation, Monitor> Neighbours => _neighbours;
    public IReadOnlyList<Monitor> MonitorNeighbours => _neighbours.Values.ToList();
    public bool IsPendingUpdate { get; private set; } = false;

    private Dictionary<NeighbourLocation, Monitor> _neighbours = new Dictionary<NeighbourLocation, Monitor>();
    private bool _neighboursSet = false;
    
    public Monitor(Point location)
    {
        Location = location;
    }

    public void SetParameter(string name, object value)
    {
        switch(name.ToLower())
        {
            case "temperature":
                {
                    Temperature = (float)value;
                    break;
                }

            case "conductivity":
                {
                    Conductivity = (float)value;
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
    }

    public void IncreaseTemp(float value)
    {
        Temperature += value;
    }

    public void DecreaseTemp(float value)
    {
        Temperature -= value;
    }

    public void SetAllNeighbours(Map map, Monitor[,] layer)
    {
        if (_neighboursSet)
        {
            return;
        }

        int x = Location.GetValueOrDefault().X;
        int y = Location.GetValueOrDefault().Y;

        if (x > 0)
        {
            SetNeighbour(NeighbourLocation.East, layer[x - 1, y]);
        }

        if (x < (map.Width - 1))
        {
            SetNeighbour(NeighbourLocation.West, layer[x + 1, y]);
        }

        if (y > 0)
        {
            SetNeighbour(NeighbourLocation.South, layer[x, y - 1]);
        }

        if (y < (map.Height - 1))
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

    public TileType GetTileTypeFromMap(Map map)
    {
        return map.Terrain[Location.GetValueOrDefault().X, Location.GetValueOrDefault().Y];
    }
}
