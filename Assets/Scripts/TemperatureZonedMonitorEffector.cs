using System;
using System.Linq;
using UnityEngine;

public class TemperatureEffectLayerManagerEffector : IEffectLayerManagerEffector<Monitor>
{
    public string EffectLayer => "temperature";
    public int UpdateFrequency => 200;
    public bool IsReady => !(_lastTick != null && Environment.TickCount - _lastTick.GetValueOrDefault() < UpdateFrequency);

    private int? _lastTick = null;

    public void Update(Map map, Monitor[,] layer)
    {
        var startedAt = Environment.TickCount;
        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var curTemp = layer[x, y];
                if (!curTemp.IsAwake)
                {
                    continue;
                }

                var neighbours = curTemp.MonitorNeighbours.Where(x => x.Conductivity > 0).ToList();
                for (var n = 0; n < neighbours.Count; n++)
                {
                    var neighbour = neighbours[n];
                    var difference = curTemp.Temperature - neighbour.Temperature;
                    if (difference > 0)
                    {
                        var transfer = difference * neighbour.Conductivity;
                        neighbour.IncreaseTemp(transfer);
                        curTemp.DecreaseTemp(transfer);
                    }
                }
            }
        }

        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var curTemp = layer[x, y];
                if (!curTemp.IsAwake)
                {
                    continue;
                }

                curTemp.ApplyNextTemperature();
            }
        }

        _lastTick = Environment.TickCount;
    }
}
