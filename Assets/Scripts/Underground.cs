using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    private bool _isCalculatoinReversed = false;

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
        _isCalculatoinReversed = false;
    }

    private void CalculateStations()
    {
        var stationFrom = _stationsDictionary[(StationsCode)_dropdownFrom.value];
        var stationTo = _stationsDictionary[(StationsCode)_dropdownTo.value];
        _isCalculatoinReversed = stationTo.GetConnections().Count() < stationFrom.GetConnections().Count();
        if (_isCalculatoinReversed)
        {
            var temp = stationFrom;
            stationFrom = stationTo;
            stationTo = temp;
        }

        _calculationQueue.Enqueue(stationFrom);
        while (0 < _calculationQueue.Count)
        {
            var currentStation = _calculationQueue.Dequeue();
            if (currentStation.GetCode() == stationTo.GetCode())
                continue;

            foreach (var connection in currentStation.GetConnections())
            {
                if (_stationsDictionary[connection].Calculated)
                    continue;

                _stationsDictionary[connection].SetCheckedFrom(currentStation.GetCode());
                _calculationQueue.Enqueue(_stationsDictionary[connection]);
            }
        }
    }

    private void CalculateAllPaths()
    {
        var allPaths = new List<string>();
        var allTransfers = new List<int>();
        var firstStation = _isCalculatoinReversed ?
            _stationsDictionary[(StationsCode)_dropdownFrom.value] :
            _stationsDictionary[(StationsCode)_dropdownTo.value];
        foreach (var connection in firstStation.GetConnections())
        {
            CalculatePath(connection, out var newPath, out var transfersNumber);
            if (
                newPath[0].ToString() == ((StationsCode)_dropdownFrom.value).ToString() &&
                newPath[newPath.Count() - 1].ToString() == ((StationsCode)_dropdownTo.value).ToString()
            )
            {
                allPaths.Add(newPath);
                allTransfers.Add(transfersNumber);
            }
        }
        if (0 < allTransfers.Count)
        {
            var shortestPathStringLength = int.MaxValue;
            var targetIndexes = new List<int>();
            var targetTransfers = new List<int>();
            for (var i = 0; i < allPaths.Count; i++)
            {
                if (allPaths[i].Length < shortestPathStringLength)
                {
                    targetIndexes = new();
                    targetTransfers = new();
                    shortestPathStringLength = allPaths[i].Length;
                }
                if (allPaths[i].Length == shortestPathStringLength) {
                    targetIndexes.Add(i);
                    targetTransfers.Add(allTransfers[i]);
                }
            }
            var targetIndex = targetTransfers.IndexOf(allTransfers.Min());
            _shortestWayText.text = allPaths[targetIndexes[targetIndex]];
            _transfersText.text = $"Transfers: {allTransfers[targetIndexes[targetIndex]]}";
        }
        else
        {
            _shortestWayText.text = ((StationsCode)_dropdownTo.value).ToString();
            _transfersText.text = "Transfers: 0";
        }
        //_shortestWayText.text = "";
        //foreach (var path in allPaths)
        //{
        //    _shortestWayText.text += path + Environment.NewLine;
        //}
        //_transfersText.text = "";
        //foreach (var transfer in allTransfers)
        //{
        //    _transfersText.text += transfer + Environment.NewLine;
        //}
    }

    private void CalculatePath(StationsCode targetStationConnectionCode, out string path, out int transfersNumber)
    {
        var stationTo = _isCalculatoinReversed ?
            _stationsDictionary[(StationsCode)_dropdownFrom.value] :
            _stationsDictionary[(StationsCode)_dropdownTo.value];
        var codeFrom = _isCalculatoinReversed ? (StationsCode)_dropdownTo.value : (StationsCode)_dropdownFrom.value;
        var checkedFrom = targetStationConnectionCode;
        var linesColors = new List<List<LinesColor>>();
        linesColors.Add(stationTo.GetLinesColor());
        path = $"{stationTo.GetCode()}";
        while (checkedFrom != codeFrom && checkedFrom != _stationsDictionary[checkedFrom].CheckedFrom)
        {
            path = $"{checkedFrom}-{path}";
            linesColors.Add(_stationsDictionary[checkedFrom].GetLinesColor());
            checkedFrom = _stationsDictionary[checkedFrom].CheckedFrom;
        }
        path = $"{checkedFrom}-{path}";
        linesColors.Add(_stationsDictionary[checkedFrom].GetLinesColor());
        if (_isCalculatoinReversed)
        {
            StringBuilder reversed = new();
            for (int i = path.Length - 1; i >= 0; i--)
            {
                reversed.Append(path[i]);
            }
            path = reversed.ToString();
        }
        var intersections = new List<LinesColor>();
        for (int i = 1; i < linesColors.Count; i += 2)
        {
            var oneIntersection = new List<LinesColor>(linesColors[i - 1].Intersect(linesColors[i]));
            intersections.Add(oneIntersection[0]);
        }
        transfersNumber = intersections.Distinct().ToList().Count() - 1;
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
