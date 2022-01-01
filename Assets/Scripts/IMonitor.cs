using System.Drawing;

public interface IMonitor
{
    Point? Location { get; }

    void SetParameter(string name, object value);
}
