using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Station
{
    [SerializeField] private StationsCode _code;
    [SerializeField] private List<StationsCode> _connections;

    public Station(StationsCode code, List<StationsCode> connections)
    {
        _code = code;
        _connections = connections;
    }

    public StationsCode GetCode()
    {
        return _code;
    }

    public List<StationsCode> GetConnections()
    {
        return new List<StationsCode>(_connections);
    }
}
