using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum Hostility
{
    Friendly,
    Neutral,
    Hostile
}
public enum UnitState
{
    Idle,
    Moving
}

[RequireComponent(typeof(Collider), typeof(NavMeshAgent))]
public class Unit : MonoBehaviour
{
    // Event handlers
    public delegate void HealthChangedHandler(float newHP);
    public event HealthChangedHandler OnDamageTaken;
    public event HealthChangedHandler OnHealed;
    public delegate void KilledHandler(Unit destroyedUnit);
    public event KilledHandler OnKilled;
    public delegate void SelectedHandler(Unit selectedUnit);
    public event SelectedHandler OnSelected;
    public delegate void DeselectedHandler(Unit deselectedUnit);
    public event DeselectedHandler OnDeselected;
    public delegate void FocusedHandler(Unit focusedUnit);
    public event FocusedHandler OnFocused;
    public event FocusedHandler OnUnfocused;

    [Tooltip("The maximum HP of this unit. Set to 0 if indestructible.")]
    public float maxHP = 0f;
    [Tooltip("If this unit is immune to damage. If Max HP was set to 0, this does not matter.")]
    public bool immune = false;

    [SerializeField, Tooltip("Hostility of this unit.")]
    private Hostility hostility = Hostility.Friendly;
    public Hostility Hostility 
    { 
        get { return hostility; }
        set
        {
            if (hostility != value)
            {
                RemoveFromUnitList();
                hostility = value;
                AddToUnitList();
            }
        }
    }
    
    public string unitName = "";

    public float attackDmg = 0;
    public float attackRange = 0;

    public GameObject selectionPrefab;
    GameObject selectIcon;
    NavMeshAgent agent;

    [HideInInspector]
    public float currentHP;
    [HideInInspector]
    public UnitState unitState = UnitState.Idle;
    bool selected = false;
    float stopCD = 0.2f;
    float stopTmr = 0;
    protected Unit followUnit;

    UnitAbility nextAbility;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        name = unitName;

        // Add this unit to the list ofselectable units
        switch (hostility)
        {
            case Hostility.Friendly:
                SelectionManager.allFriendlyUnits.Add(this);
                break;
            default:
                SelectionManager.allOtherUnits.Add(this);
                break;
        }

        currentHP = maxHP;
        if (currentHP == 0f) immune = true;
        agent = GetComponent<NavMeshAgent>();

        Vector3 rayStart = transform.position;
        rayStart.y += 100f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 500f, 1 << 0))
        {
            transform.position = hit.point;
            if (selectionPrefab)
            {
                Vector3 extents = GetComponent<MeshRenderer>().localBounds.extents;
                selectIcon = Instantiate(selectionPrefab, transform.position - new Vector3(0, extents.y, 0), Quaternion.Euler(90, 0, 0));
                selectIcon.SetActive(false);
                selectIcon.name = $"{gameObject.name} Selection Icon";
                selectIcon.transform.parent = transform;

                float maxExtent = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
                selectIcon.transform.localScale = new(maxExtent, maxExtent, maxExtent);
                SetHostility(hostility);
            }
            else
            {
                Debug.LogWarning($"The Unit {gameObject.name} does not have a Selection Icon prefab set on its Unit component!");
            }
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        stopTmr += Time.deltaTime;

        if (stopTmr >= stopCD && agent.velocity.sqrMagnitude <= Mathf.Pow(agent.speed * 0.1f, 2))
        {
            agent.isStopped = true;
        }

        // If following a unit, keep updating the destination to move to
        if (followUnit)
        {
            // If the unit to follow should be attacked, stop moving and attack.
            if ((followUnit.transform.position - transform.position).sqrMagnitude <= attackRange * attackRange)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
                agent.destination = followUnit.transform.position;
            }
        }
    }

    public void Select()
    {
        if (!selected)
        {
            selected = true;
            SetShowSelection(true);
            OnSelected?.Invoke(this);
        }
    }
    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            SetShowSelection(false);
            OnDeselected?.Invoke(this);
        }
    }
    public void SetShowSelection(bool showSelection)
    {
        if (selectIcon)
            selectIcon.SetActive(showSelection);
    }
    
    public void SetHostility(Hostility hostility)
    {
        Hostility = hostility;

        if (selectIcon)
        {
            switch (hostility)
            {
                case Hostility.Friendly:
                    selectIcon.GetComponent<SpriteRenderer>().color = Color.green;
                    break;
                case Hostility.Neutral:
                    selectIcon.GetComponent<SpriteRenderer>().color = Color.yellow;
                    break;
                case Hostility.Hostile:
                    selectIcon.GetComponent<SpriteRenderer>().color = Color.red;
                    break;
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (immune) return;
        SetCurrentHP(currentHP - dmg);
    }

    public void SetCurrentHP(float hp)
    {
        if (hp == currentHP) return;

        float oldHP = currentHP;
        currentHP = Mathf.Clamp(hp, 0, maxHP);
        if (currentHP <= 0)
        {
            Destroy();
        }
        else if (currentHP < oldHP)
        {
            OnDamageTaken?.Invoke(currentHP);
        }
        else
        {
            OnHealed?.Invoke(currentHP);
        }
    }

    public void Destroy()
    {
        RemoveFromUnitList();
        OnKilled?.Invoke(this);
        Destroy(gameObject);
    }

    void AddToUnitList()
    {
        // Add this unit to the list ofselectable units
        switch (hostility)
        {
            case Hostility.Friendly:
                SelectionManager.allFriendlyUnits.Add(this);
                break;
            default:
                SelectionManager.allOtherUnits.Add(this);
                break;
        }
    }

    void RemoveFromUnitList()
    {
        if (hostility == Hostility.Friendly)
        {
            SelectionManager.allFriendlyUnits.Remove(this);
        }
        else
        {
            SelectionManager.allOtherUnits.Remove(this);
        }
    }

    public void Heal(float healAmt)
    {
        currentHP = Mathf.Clamp(currentHP + healAmt, 0, maxHP);
    }

    public void MoveTo(Vector3 destination, bool commanded = false)
    {
        unitState = UnitState.Moving;
        followUnit = null;
        stopTmr = 0f;
        agent.isStopped = false;
        agent.destination = destination;
    }

    /// <summary>
    /// Sets this unit to follow another unit indefinitely.
    /// If the unit's hostility does not match this one, it will attack it.
    /// </summary>
    /// <param name="other">The unit to follow or attack.</param>
    public void Follow(Unit other)
    {
        followUnit = other;
        if (!other) return;

        unitState = UnitState.Moving;
        stopTmr = 0f;
    }

    protected float FindDistance(Vector2 targetLocation)
    {
        return Vector2.Distance(transform.position, targetLocation);
    }

    public UnitAbility[] GetAllAbilities()
    {
        return GetComponents<UnitAbility>();
    }

    public UnitAbility GetAbility(string abilityName)
    {
        foreach (UnitAbility a in GetComponents<UnitAbility>())
        {
            if (a.abilityName == abilityName)
                return a;
        }
        return null;
    }

    public void SetQueuedAbility(string abilityName)
    {
        SetQueuedAbility(GetAbility(abilityName));

    }
    public void SetQueuedAbility(UnitAbility ability)
    {
        nextAbility = ability;
    }
    public UnitAbility GetQueuedAbility()
    {
        return nextAbility;
    }

    public void Focus()
    {
        selectIcon.GetComponent<Animator>().Play("UnitFocus");
        OnFocused?.Invoke(this);
    }
    public void Unfocus()
    {
        selectIcon.GetComponent<Animator>().StopPlayback();
        OnUnfocused?.Invoke(this);
    }
}
