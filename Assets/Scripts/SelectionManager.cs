using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject posSelector;
    List<Unit> selected, pending;
    Vector2 mouseStart;
    Camera cam;
    bool selecting;

    //Added by Ty
    [SerializeField] Texture2D NormalCursor;

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
            cam = GetComponentInChildren<Camera>();

            // instantiate the position selector for abilities
            posSelector = Instantiate(posSelector);
            posSelector.SetActive(false);
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
        StopTargetSelection();

        
        
    }

    // Start is called before the first frame update
    void Start() { }

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
                    if (SetSelectedUnits(pending))
                    {
                        pending.Clear();
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
                        newPending = GetUnitsInSelectionBox(allOtherUnits, 1 << 7, bounds, true);

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
                    if (Physics.Raycast(cam.transform.position, cam.ScreenToWorldPoint(mousePos) - cam.transform.position, out RaycastHit hit, 500f, (1 << 0) | unitMask))
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
                if (RaycastMouse(out RaycastHit posHit, 1))
                {
                    Vector3 pos = posHit.point;
                    pos.y += 0.01f;
                    posSelector.transform.position = pos;
                    if (Input.GetMouseButtonDown(0))
                    {
                        OnTargetPositionRequested?.Invoke(posHit.point);
                        StopTargetSelection();
                    }
                }
                else if(RaycastMouse(out posHit, 1 << 10))
                {
                    posSelector.transform.position = posHit.point;
                    if (Input.GetMouseButtonDown(0))
                        HUDController.ShowError("Invalid selection. Please select a valid position.");
                }
                    
                if (Input.GetMouseButtonDown(1)) // cancel
                    StopTargetSelection();
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
                            StopTargetSelection();
                        }
                    }
                    else
                    {
                        HUDController.ShowError("Invalid selection. Please select a friendly unit.");
                    }
                }
                else if (Input.GetMouseButtonDown(1)) // cancel
                    StopTargetSelection();

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
                            StopTargetSelection();
                        }
                    }
                    else
                    {
                        HUDController.ShowError("Invalid selection. Please select an enemy unit.");
                    }
                }
                else if (Input.GetMouseButtonDown(1)) // cancel
                    StopTargetSelection();

                break;
        }
    }

    void StopTargetSelection()
    {
        posSelector.SetActive(false);
        Cursor.visible = true;
        Cursor.SetCursor(NormalCursor, Vector2.zero, CursorMode.Auto);
        OnTargetPositionRequested = null;
        OnTargetUnitRequested = null;
        selectState = SelectionState.Normal;
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
            if (hit.collider.TryGetComponent(out Unit u))
                selectedUnits.Add(u);
            else
                Debug.LogWarning($"WARNING: Attempted to select GameObject {hit.collider.gameObject.name} on a Unit collision layer, but it does not have a Unit component! " +
                    $"This could have unintended side affects, either change this object's collision layer or add a Unit component to it.");
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
        return Physics.Raycast(cam.transform.position, cam.ScreenToWorldPoint(mousePos) - cam.transform.position, out hit, 500f, layers);
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
    public static void RequestCastPosition(Texture2D aoeVisual, float radius, TargetPosCastHandler callback)
    {
        Instance.posSelector.SetActive(true);
        Instance.posSelector.GetComponent<SpriteRenderer>().sprite = Sprite.Create(aoeVisual, new(0, 0, aoeVisual.width, aoeVisual.height), new(0.5f, 0.5f), aoeVisual.width / radius);

        Instance.selectState = SelectionState.TargetPosition;
        Instance.selecting = false;
        OnTargetPositionRequested += callback;

        Cursor.visible = false;
    }

    /// <summary>
    ///     Tells the Selection Manager to switch from "unit selection" mode to "ability cast" mode,
    ///     where the player is prompted to left-click a unit in the level. This unit is then returned
    ///     for use in the provided callback function.
    /// </summary>
    /// <param name="callback">The callback to invoke upon selecting a unit in the level</param>
    /// <param name="unitHostility">The hostility of the units that can be selected for casting.</param>
    public static void RequestCastUnit(Texture2D cursorVisual, Hostility unitHostility, TargetUnitCastHandler callback)
    {
        Instance.selectState = unitHostility == Hostility.Friendly ? SelectionState.TargetFriendlyUnit : SelectionState.TargetEnemyUnit;
        Instance.selecting = false;
        OnTargetUnitRequested += callback;

        Cursor.visible = true;
        Cursor.SetCursor(cursorVisual, Vector2.zero, CursorMode.Auto);
    }

    public static bool SetSelectedUnits(List<Unit> units)
    {
        // If no units are provided, return false.
        if (units.Count <= 0)
            return false;

        // Deselect all old units
        foreach (var u in Instance.selected.Where(u => u))
        {
            u.Deselect();
            u.OnKilled -= Instance.RemoveFromSelection;
        }

        Instance.selected = new List<Unit>(units);

        // Select the new units
        foreach (var u in Instance.selected)
        {
            u.Select();
            u.OnKilled += Instance.RemoveFromSelection;
        }

        // Notify selection listeners
        OnUnitSelectionChanged?.Invoke(Instance.selected);

        return true;
    }
}