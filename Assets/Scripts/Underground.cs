using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Underground : MonoBehaviour
{
    [SerializeField] private List<Station> _stations = new();
    [SerializeField] private TMP_Dropdown _dropdownFrom;
    [SerializeField] private TMP_Dropdown _dropdownTo;

    private void Start()
    {
        AddAllStations(_dropdownFrom);
        AddAllStations(_dropdownTo);
    }

    private void AddAllStations(TMP_Dropdown dropdown)
    {
        var stationCodes = System.Enum.GetNames(typeof(StationsCode));
        dropdown.AddOptions(stationCodes.ToList<string>());
    }
}
