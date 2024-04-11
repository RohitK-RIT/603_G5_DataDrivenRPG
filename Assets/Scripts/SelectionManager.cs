using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public delegate void SelectionEventHandler(List<Unit> units);
    public static event SelectionEventHandler OnUnitSelectionChanged;

    public RectTransform selectBox;

    List<Unit> newSelected, selected, pending;

    // Sets automatically handle dupes
    public static HashSet<Unit> allFriendlyUnits, allOtherUnits;

    Vector2 mouseStart;
    Camera cam;

    int unitMask = (1 << 6) | (1 << 7);

    private void Awake()
    {
        newSelected = new();
        selected = new();
        pending = new();
        allFriendlyUnits = new();
        allOtherUnits = new();
        cam = GetComponent<Camera>();
    }
    private void OnDestroy()
    {
        allFriendlyUnits.Clear();
        allOtherUnits.Clear();
        OnUnitSelectionChanged = null;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Begin the selection
        if (Input.GetMouseButtonDown(0))
        {
            selectBox.gameObject.SetActive(true);
            selectBox.sizeDelta = Vector3.zero;
            // Get bot start corner on near plane
            mouseStart = Input.mousePosition;
        }
        // Update selection
        else if (Input.GetMouseButton(0))
        {
            // Set select box dimensions
            Vector2 area = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - mouseStart;
            selectBox.anchoredPosition = mouseStart + area / 2f;
            selectBox.sizeDelta = new(Mathf.Abs(area.x), Mathf.Abs(area.y));

            // Get units in selection area
            Bounds bounds = new(selectBox.anchoredPosition, selectBox.sizeDelta);
            List<Unit> newPending = GetUnitsInSelectionBox(allFriendlyUnits, 1 << 6, bounds);
            foreach (Unit u in pending)
            {
                if (u) u.SetShowSelection(false);
            }
            pending = newPending;
            foreach (Unit u in pending)
                u.SetShowSelection(true);
        }
        // End selection; retrieve all units in the selection box
        else if (Input.GetMouseButtonUp(0))
        {
            pending.Clear();

            // Get units in selection area. If no friendlies, check if there's another type of unit to select.
            Bounds bounds = new(selectBox.anchoredPosition, selectBox.sizeDelta);
            newSelected = GetUnitsInSelectionBox(allFriendlyUnits, 1 << 6, bounds);
            if (newSelected.Count == 0)
                newSelected = GetUnitsInSelectionBox(allOtherUnits, (1 << 7) | (1 << 8), bounds, true);

            if (newSelected.Count > 0)
            {
                // Deselect all old units
                foreach (Unit u in selected)
                {
                    if (u)
                    {
                        u.Deselect();
                        u.OnKilled -= RemoveFromSelection;
                    }
                }

                selected = new(newSelected);
                newSelected.Clear();

                // Select the new units
                foreach (Unit u in selected)
                {
                    u.Select();
                    u.OnKilled += RemoveFromSelection;
                }

                // Notify selection listeners
                OnUnitSelectionChanged?.Invoke(selected);
            }

            selectBox.gameObject.SetActive(false);
        }

        // Handle moving units
        if (Input.GetMouseButtonDown(1) && selected.Count > 0 && selected[0].Hostility == Hostility.Friendly)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane;
            // Raycast against the environment or other units
            if (Physics.Raycast(transform.position, cam.ScreenToWorldPoint(mousePos) - transform.position, out RaycastHit hit, 500f, (1 << 0) | unitMask))
            {
                if (hit.collider.TryGetComponent(out Unit target))
                {
                    foreach (Unit u in selected)
                        u.Follow(target, true);
                }
                else
                {
                    foreach (Unit u in selected)
                        u.MoveTo(hit.point, true);
                }
            }
        }
    }

    /// <summary>
    /// Adds all units from the given set that lie within the given bounds to the selected units list
    /// </summary>
    /// <param name="unitsToCheck">The set of units to check for selection</param>
    /// <param name="bounds">The selection box bounds</param>
    /// <param name="oneUnitOnly">If true, only populates the selected list with the first unit that lies in the selection box.</param>
    List<Unit> GetUnitsInSelectionBox(HashSet<Unit> unitsToCheck, int unitLayer, Bounds bounds, bool oneUnitOnly = false)
    {
        List<Unit> selectedUnits = new();

        // For a single click and no drag, raycast against Unit layer
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.nearClipPlane;
        if (Physics.Raycast(transform.position, cam.ScreenToWorldPoint(mousePos) - transform.position, out RaycastHit hit, 500f, unitLayer))
        {
            selectedUnits.Add(hit.collider.GetComponent<Unit>());
            if (oneUnitOnly)
                return selectedUnits;
        }

        // Simple AABB test to see if the unit's inside the selection box
        foreach (Unit u in unitsToCheck)
        {
            Vector2 unitPos = cam.WorldToScreenPoint(u.transform.position);
            if (unitPos.x > bounds.min.x && unitPos.x < bounds.max.x && unitPos.y > bounds.min.y && unitPos.y < bounds.max.y)
            {
                selectedUnits.Add(u);
                u.Select();
                if (oneUnitOnly) break;
            }
        }
        return selectedUnits;
    }

    public void RemoveFromSelection(Unit u)
    {
        if (selected.Remove(u))
        {
            // Once again notify selection listeners
            OnUnitSelectionChanged?.Invoke(selected);
        }
    }
}
