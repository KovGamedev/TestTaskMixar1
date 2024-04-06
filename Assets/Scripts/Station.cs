using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Station
{
    public bool Calculated { get; private set; }
    public StationsCode CheckedFrom { get; private set; }
    public LinesColor FromLine { get; private set; }

    [SerializeField] private StationsCode _code;
    [SerializeField] private List<LinesColor> _linesColor;
    [SerializeField] private List<StationsCode> _connections;

    public Station(StationsCode code, List<StationsCode> connections)
    {
        _code = code;
        _connections = connections;
        CheckedFrom = code;
    }

    public void SetUnchecked()
    {
        Calculated = false;
        CheckedFrom = _code;
        FromLine = _linesColor[0];
    }

    public void SetCheckedFrom(StationsCode code, LinesColor linesColor)
    {
        Calculated = true;
        CheckedFrom = code;
        FromLine = linesColor;
    }

    public StationsCode GetCode()
    {
        return _code;
    }

    public List<StationsCode> GetConnections()
    {
        return new List<StationsCode>(_connections);
    }

    public List<LinesColor> GetLinesColor()
    {
        return new List<LinesColor>(_linesColor);
    }
}
