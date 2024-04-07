using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Underground : MonoBehaviour
{
    [SerializeField] private List<Station> _stations = new();
    [SerializeField] private TMP_Dropdown _dropdownFrom;
    [SerializeField] private TMP_Dropdown _dropdownTo;
    [SerializeField] private TextMeshProUGUI _shortestWayText;
    [SerializeField] private TextMeshProUGUI _transfersText;

    private Dictionary<StationsCode, Station> _stationsDictionary = new();
    private Queue<Station> _calculationQueue = new();

    public void CalculateShortestPath()
    {
        ResetCalculationData();
        CalculateStations();
        CalculateAllPaths();
    }

    private void ResetCalculationData()
    {
        foreach (var pair in _stationsDictionary)
        {
            pair.Value.SetUnchecked();
        }
        _calculationQueue = new();
    }

    private void CalculateStations()
    {
        var stationFrom = _stationsDictionary[(StationsCode)_dropdownFrom.value];
        _calculationQueue.Enqueue(stationFrom);
        while (0 < _calculationQueue.Count)
        {
            var currentStation = _calculationQueue.Dequeue();
            if (currentStation.GetCode() == (StationsCode)_dropdownTo.value)
                continue;

            foreach (var connection in currentStation.GetConnections())
            {
                if (_stationsDictionary[connection].Calculated)
                    continue;

                var possibleLinesColor = new List<LinesColor>(
                    currentStation.GetLinesColor().Intersect(_stationsDictionary[connection].GetLinesColor())
                );
                var lineColor = 0 < possibleLinesColor.Count() ? possibleLinesColor[0] : currentStation.GetLinesColor()[0];

                _stationsDictionary[connection].SetCheckedFrom(currentStation.GetCode(), lineColor);
                _calculationQueue.Enqueue(_stationsDictionary[connection]);
            }
        }
    }

    private void CalculateAllPaths()
    {
        var allPaths = new List<string>();
        var allTransfers = new List<int>();
        foreach (var connection in _stationsDictionary[(StationsCode)_dropdownTo.value].GetConnections())
        {
            CalculatePath(connection, out var newPath, out var transfersNumber);
            if (newPath[0].ToString() == ((StationsCode)_dropdownFrom.value).ToString())
            {
                allPaths.Add(newPath);
                allTransfers.Add(transfersNumber);
            }
        }
        if (0 < allTransfers.Count)
        {
            var targetIndex = allTransfers.IndexOf(allTransfers.Min());
            _shortestWayText.text = allPaths[targetIndex];
            _transfersText.text = $"Transfers: {allTransfers[targetIndex]}";
        }
        else
        {
            _shortestWayText.text = ((StationsCode)_dropdownTo.value).ToString();
            _transfersText.text = "Transfers: 0";
        }
    }

    private void CalculatePath(StationsCode targetStationConnectionCode, out string path, out int transfersNumber)
    {
        var stationTo = _stationsDictionary[(StationsCode)_dropdownTo.value];
        var checkedFrom = targetStationConnectionCode;
        var linesColors = new List<LinesColor>() { _stationsDictionary[checkedFrom].FromLine };
        path = $"{stationTo.GetCode()}";
        while (checkedFrom != (StationsCode)_dropdownFrom.value && checkedFrom != _stationsDictionary[checkedFrom].CheckedFrom)
        {
            path = $"{checkedFrom}-{path}";
            checkedFrom = _stationsDictionary[checkedFrom].CheckedFrom;
            if (!linesColors.Contains(_stationsDictionary[checkedFrom].FromLine))
                linesColors.Add(_stationsDictionary[checkedFrom].FromLine);
        }
        path = $"{checkedFrom}-{path}";
        if (!linesColors.Contains(_stationsDictionary[(StationsCode)_dropdownTo.value].FromLine))
            linesColors.Add(_stationsDictionary[(StationsCode)_dropdownTo.value].FromLine);
        transfersNumber = linesColors.Count - 1;
    }

    private void Start()
    {
        AddAllStations(_dropdownFrom);
        AddAllStations(_dropdownTo);
        FillDictionary();
    }

    private void AddAllStations(TMP_Dropdown dropdown)
    {
        var stationCodes = Enum.GetNames(typeof(StationsCode));
        dropdown.AddOptions(stationCodes.ToList<string>());
    }

    private void FillDictionary()
    {
        foreach (var station in _stations)
        {
            _stationsDictionary.Add(station.GetCode(), station);
        }
    }
}
