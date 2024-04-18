using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortrait : MonoBehaviour
{
    private Unit _owner;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnCharacterPortraitClicked);
    }

    public void SetOwner(Unit owner)
    {
        _owner = owner;
    }

    private void OnCharacterPortraitClicked()
    {
        SelectionManager.SetSelectedUnits(new List<Unit> { _owner });
    }
}