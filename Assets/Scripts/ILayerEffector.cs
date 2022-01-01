using System.Collections.Generic;

internal interface ILayerEffector<T>
{
    string EffectLayer { get; }
    void Update(Map map, T[,] layer);
    void Setup(Map map, Monitor monitor, T[,] layer);
}