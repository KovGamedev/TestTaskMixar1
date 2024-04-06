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
    private int _transfers = 0;

    public void CalculateShortestPath()
    {
        ResetCalculationData();
        CalculateStations();
        CalculatePath();
    }

    private void ResetCalculationData()
    {
        foreach (var pair in _stationsDictionary)
        {
            pair.Value.SetUnchecked();
        }
        _calculationQueue = new();
        _transfers = 0;
    }

    private void CalculateStations()
    {
        var stationFrom = _stationsDictionary[(StationsCode)_dropdownFrom.value];
        _calculationQueue.Enqueue(stationFrom);
        while (0 < _calculationQueue.Count)
        {
            var currentStation = _calculationQueue.Dequeue();
            if (currentStation.GetCode() == (StationsCode)_dropdownTo.value)
                break;

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

    private void CalculatePath()
    {
        var path = "";
        var checkedFrom = (StationsCode)_dropdownTo.value;
        var linesColors = new List<LinesColor>() { _stationsDictionary[checkedFrom].FromLine };
        while (checkedFrom != (StationsCode)_dropdownFrom.value)
        {
            path = $"-{checkedFrom}{path}";
            checkedFrom = _stationsDictionary[checkedFrom].CheckedFrom;
            if (!linesColors.Contains(_stationsDictionary[checkedFrom].FromLine))
                linesColors.Add(_stationsDictionary[checkedFrom].FromLine);
        }
        path = $"{checkedFrom}{path}";
        _shortestWayText.text = path;

        _transfersText.text = $"Transfers: {linesColors.Count - 1}";
    }

    private void Start()
    {
        AddAllStations(_dropdownFrom);
        AddAllStations(_dropdownTo);
        FillDictionary();
    }

    private void AddAllStations(TMP_Dropdown dropdown)
    {
        var stationCodes = System.Enum.GetNames(typeof(StationsCode));
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
