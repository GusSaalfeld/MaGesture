using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDropdown : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private RectTransform panels;
#pragma warning restore 0649

    private Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponentInChildren<Dropdown>();

        if (dropdown == null)
        {
            Debug.LogError("MenuDropdown must have a Dropdown among its children.");
        }

        if (panels == null)
        {
            Debug.LogError("MenuDropdown is missing a panels reference.  Please set it in the inspector.");
        }

        InitializeDropdown();
    }

    private void Start()
    {
        dropdown.value = 0;
        dropdown.onValueChanged.Invoke(0);
    }

    private void InitializeDropdown()
    {
        dropdown.ClearOptions();

        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();
        foreach (RectTransform panel in panels)
        {
            Dropdown.OptionData newOption = new Dropdown.OptionData();
            newOption.text = panel.gameObject.name;
            newOptions.Add(newOption);
        }

        dropdown.AddOptions(newOptions);
        dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(int optionIndex)
    {
        for (int i = 0; i < panels.childCount; i++)
        {
            panels.GetChild(i).gameObject.SetActive(i == optionIndex);
        }
    }
}
