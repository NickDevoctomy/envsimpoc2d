using System;
using System.Collections.Generic;
public class EffectLayerManager
{
    private Dictionary<string, object> _layers = new Dictionary<string, object>();
    private Map _map;

    public EffectLayerManager(Map map)
    {
        _map = map;
    }

    public T[,] GetLayer<T>(string name) where T : IMonitor
    {
        if(!_layers.ContainsKey(name))
        {
            return null;
        }

        return (T[,])_layers[name];
    }

    public void CreateLayer<T>(
        string name,
        Action<Map, T, T[,]> setup) where T : IMonitor
    {
        if(_layers.ContainsKey(name))
        {
            return;
        }

        var layer = new T[_map.Width, _map.Height];
        
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                layer[x, y] = (T)Activator.CreateInstance(typeof(T), new System.Drawing.Point(x, y));
            }
        }

        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                setup(_map, layer[x, y], layer);
            }
        }

        _layers.Add(name, layer);
    }
}
