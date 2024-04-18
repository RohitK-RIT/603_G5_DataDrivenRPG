using System.Collections.Generic;
using UnityEngine;

public class CharacterPortrait : MonoBehaviour
{
    private Unit _owner;

    public void SetOwner(Unit owner)
    {
        _owner = owner;
    }

    public void OnCharacterPortraitClicked()
    {
        SelectionManager.SetSelectedUnits(new List<Unit> { _owner });
    }
}