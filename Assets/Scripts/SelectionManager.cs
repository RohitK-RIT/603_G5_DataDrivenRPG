using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    enum SelectionState
    {
        Normal,
        TargetFriendlyUnit,
        TargetEnemyUnit,
        TargetPosition
    }

    // Events
    public delegate void SelectionEventHandler(List<Unit> units);
    public static event SelectionEventHandler OnUnitSelectionChanged;

    public delegate void TargetPosCastHandler(Vector3 position);
    public static event TargetPosCastHandler OnTargetPositionRequested;
    public delegate void TargetUnitCastHandler(Unit unit);
    public static event TargetUnitCastHandler OnTargetUnitRequested;

    static SelectionManager Instance;

    // Sets automatically handle dupes
    public static HashSet<Unit> allFriendlyUnits, allOtherUnits;

    public RectTransform selectBox;
    List<Unit> selected, pending;
    Vector2 mouseStart;
    Camera cam;
    bool selecting;

    //Added by Ty
    [SerializeField] Texture2D NormalCursor;
    [SerializeField] Texture2D SelectionCursor;

    int unitMask = (1 << 6) | (1 << 7);
    SelectionState selectState;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;

            selecting = false;
            selectState = SelectionState.Normal;
            selected = new();
            pending = new();
            allFriendlyUnits = new();
            allOtherUnits = new();
            cam = GetComponent<Camera>();
        }
        else
        {
            Debug.LogWarning("Detected multiple Selection Managers present. Only the first one found will be used.");
        }
    }
    private void OnDestroy()
    {
        // clear all unit lists
        allFriendlyUnits.Clear();
        allOtherUnits.Clear();

        // nullify all event listeners
        OnUnitSelectionChanged = null;
        OnTargetPositionRequested = null;
        OnTargetUnitRequested = null;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch (selectState)
        {
            // -- NORMAL SELECTION FUNCTIONALITY -- \\

            case SelectionState.Normal:

                // Begin the selection
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.SetCursor(NormalCursor, Vector2.zero, CursorMode.Auto);
                    selecting = true;
                    selectBox.gameObject.SetActive(true);
                    selectBox.sizeDelta = Vector3.zero;
                    // Get bot start corner on near plane
                    mouseStart = Input.mousePosition;
                }
                // End selection; retrieve all units in the selection box
                else if (Input.GetMouseButtonUp(0))
                {
                    selecting = false;
                    if (pending.Count > 0)
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

                        selected = new(pending);
                        pending.Clear();

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

                // Update selection
                if (selecting)
                {
                    // Set select box dimensions
                    Vector2 area = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - mouseStart;
                    selectBox.anchoredPosition = mouseStart + area / 2f;
                    selectBox.sizeDelta = new(Mathf.Abs(area.x), Mathf.Abs(area.y));

                    // Get units in selection area
                    Bounds bounds = new(selectBox.anchoredPosition, selectBox.sizeDelta);
                    List<Unit> newPending = GetUnitsInSelectionBox(allFriendlyUnits, 1 << 6, bounds);
                    if (newPending.Count == 0)
                        newPending = GetUnitsInSelectionBox(allOtherUnits, 1 << 7, bounds);

                    foreach (Unit u in pending)
                    {
                        if (u) u.ShowSoftSelection(false);
                    }
                    pending = newPending;
                    foreach (Unit u in pending)
                        u.ShowSoftSelection(true);
                }
                // Handle moving units
                else if (Input.GetMouseButtonDown(1) && selected.Count > 0 && selected[0].Hostility == Hostility.Friendly)
                {
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = cam.nearClipPlane;
                    // Raycast against the environment or other units
                    if (Physics.Raycast(transform.position, cam.ScreenToWorldPoint(mousePos) - transform.position, out RaycastHit hit, 500f, (1 << 0) | unitMask))
                    {
                        if (hit.collider.TryGetComponent(out Unit target))
                        {
                            foreach (Unit u in selected)
                                u.Follow(target);
                        }
                        else
                        {
                            foreach (Unit u in selected)
                                u.MoveTo(hit.point);
                        }
                    }
                }
                break;

            // -- SELECTING A TARGET POSITION ON THE MAP -- \\

            case SelectionState.TargetPosition:
                if (Input.GetMouseButtonDown(0))
                {
                    if (RaycastMouse(out RaycastHit posHit))
                    {
                        OnTargetPositionRequested?.Invoke(posHit.point);
                        OnTargetPositionRequested = null;
                        selectState = SelectionState.Normal;
                    }
                    else
                    {
                        HUDController.ShowError("Invalid selection. Please select a valid position.");
                    }
                }
                else if (Input.GetMouseButtonDown(1)) // cancel
                    selectState = SelectionState.Normal;
                break;

            // -- SELECTING A TARGET FRIENDLY UNIT ON THE MAP -- \\

            case SelectionState.TargetFriendlyUnit:
                if (Input.GetMouseButtonDown(0))
                {
                    if (RaycastMouse(out RaycastHit unitHit, 1 << 6))
                    {
                        if (unitHit.collider.TryGetComponent(out Unit u))
                        {
                            OnTargetUnitRequested?.Invoke(u);
                            OnTargetUnitRequested = null;
                            selectState = SelectionState.Normal;
                        }
                    }
                    else
                    {
                        HUDController.ShowError("Invalid selection. Please select a friendly unit.");
                    }
                }
                else if (Input.GetMouseButtonDown(1)) // cancel
                    selectState = SelectionState.Normal;
                break;

            // -- SELECTING A TARGET ENEMY UNIT ON THE MAP -- \\

            case SelectionState.TargetEnemyUnit:
                if (Input.GetMouseButtonDown(0))
                {
                    if (RaycastMouse(out RaycastHit unitHit, 1 << 7))
                    {
                        if (unitHit.collider.TryGetComponent(out Unit u))
                        {
                            OnTargetUnitRequested?.Invoke(u);
                            OnTargetUnitRequested = null;
                            selectState = SelectionState.Normal;
                        }
                    }
                    else
                    {
                        HUDController.ShowError("Invalid selection. Please select an enemy unit.");
                    }
                }
                else if (Input.GetMouseButtonDown(1)) // cancel
                    selectState = SelectionState.Normal;
                break;
        }
    }

    /// <summary>
    ///     Adds all units from the given set that lie within the given bounds to the selected units list
    /// </summary>
    /// <param name="unitsToCheck">The set of units to check for selection</param>
    /// <param name="bounds">The selection box bounds</param>
    /// <param name="oneUnitOnly">If true, only populates the selected list with the first unit that lies in the selection box.</param>
    List<Unit> GetUnitsInSelectionBox(HashSet<Unit> unitsToCheck, int unitLayer, Bounds bounds, bool oneUnitOnly = false)
    {
        List<Unit> selectedUnits = new();

        // For a single click and no drag, raycast against Unit layer
        if (RaycastMouse(out RaycastHit hit, unitLayer))
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
                if (oneUnitOnly) break;
            }
        }
        return selectedUnits;
    }

    /// <summary>
    ///     Helper function for Getting the cursor position in the world
    /// </summary>
    /// <param name="hit">The Raycast hit result</param>
    /// <param name="layers">The layers to raycast against</param>
    /// <returns>If raycasting was successfull or not</returns>
    bool RaycastMouse(out RaycastHit hit, int layers = Physics.AllLayers)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.nearClipPlane;
        return Physics.Raycast(transform.position, cam.ScreenToWorldPoint(mousePos) - transform.position, out hit, 500f, layers);
    }

    /// <summary>
    ///     Removes the specified unit from the list of selected units
    /// </summary>
    /// <param name="u">
    ///     The unit to remove. The corresponding 
    ///     event is not called if removal was unsuccessful.
    /// </param>
    public void RemoveFromSelection(Unit u)
    {
        if (selected.Remove(u))
        {
            // Once again notify selection listeners
            OnUnitSelectionChanged?.Invoke(selected);
        }
    }

    /// <summary>
    ///     Tells the Selection Manager to switch from "unit selection" mode to "ability cast" mode,
    ///     where the player is prompted to left-click a position in the level. This position is then returned
    ///     for use in the provided callback function.
    /// </summary>
    /// <param name="callback">The callback to invoke upon selecting a position in the level</param>
    public static void RequestCastPosition(TargetPosCastHandler callback)
    {
        Instance.selectState = SelectionState.TargetPosition;
        Instance.selecting = false;
        OnTargetPositionRequested += callback;

        Cursor.SetCursor(Instance.SelectionCursor, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    ///     Tells the Selection Manager to switch from "unit selection" mode to "ability cast" mode,
    ///     where the player is prompted to left-click a unit in the level. This unit is then returned
    ///     for use in the provided callback function.
    /// </summary>
    /// <param name="callback">The callback to invoke upon selecting a unit in the level</param>
    /// <param name="unitHostility">The hostility of the units that can be selected for casting.</param>
    public static void RequestCastUnit(Hostility unitHosility, TargetUnitCastHandler callback)
    {
        Instance.selectState = unitHosility == Hostility.Friendly ? SelectionState.TargetFriendlyUnit : SelectionState.TargetEnemyUnit;
        Instance.selecting = false;
        OnTargetUnitRequested += callback;

        Cursor.SetCursor(Instance.SelectionCursor, Vector2.zero, CursorMode.Auto);
    }
}
