using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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

[RequireComponent(typeof(Collider))]
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



    //attributes
    public int strength = 1; 
    public int dexterity = 1;
    public int agility = 1;
    public int precision = 1;
    public int constitution = 1;

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

    UnitAbility queuedAbility;
    public float actionTime = 10f;

    //Added by Ty
    ActionBarController actionBarController;
    HealthBarController healthBarController;
    [SerializeField] private GameObject CharPortrait;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        name = unitName;

        //Added By Ty
        actionBarController = CharPortrait.GetComponent<ActionBarController>();
        healthBarController = CharPortrait.GetComponent<HealthBarController>();    

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

        //add constitution modifier to health
        if (constitution < 4)
        {
            maxHP *= (1f - (.1f * (4 - (constitution - 1))));
        }
        else
        {
            maxHP *= (1f + (.1f * constitution));
        }

        //action speed modifiers
        actionTime *= (1f + .04f * dexterity);

    

        currentHP = maxHP;
        if (currentHP == 0f) immune = true;

        //copy modifiers to the healthbar and actionbar
        healthBarController.maxHealthBar = maxHP;
        healthBarController.healthBar = currentHP;
        actionBarController.actionRegen = actionTime;


        agent = GetComponent<NavMeshAgent>();

        Vector3 rayStart = transform.position;
        rayStart.y += 100f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 500f, 1 << 0))
        {
            Vector3 extents = GetComponent<MeshRenderer>().localBounds.extents;
            Vector3 pos = hit.point;
            pos.y += extents.y;
            transform.position = pos;

            if (selectionPrefab)
            {
                selectIcon = Instantiate(selectionPrefab, transform.position - new Vector3(0, extents.y, 0), Quaternion.Euler(90, 0, 0));
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

        //Added and modified By Ty
        actionBarController.actionProgressUI.fillAmount = actionBarController.actionBar / actionBarController.maxActionBar;
        healthBarController.healthProgressUI.fillAmount = healthBarController.healthBar / healthBarController.maxHealthBar;
        actionBarController.actionBar += actionBarController.actionRegen * Time.deltaTime;

        // Execute the queued ability, if there is one, at the end of the timer
        if (actionBarController.actionBar >= actionBarController.maxActionBar) //old (actionTmr >= actionTime)
        {
            if (queuedAbility)
            {
                queuedAbility.Execute();
                actionBarController.actionBar = 0f;
                GetComponentInChildren<Image>().color = new(1, 1, 1, 0);
            }
            queuedAbility = null;

        }

        if (agent) // stationary units should not be using an agent
        {
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
    }

    public void Select()
    {
        if (!selected)
        {
            selected = true;
            ShowSelection(true);
            OnSelected?.Invoke(this);
        }
    }
    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            ShowSelection(false);
            OnDeselected?.Invoke(this);
        }
    }

    /// <summary>
    /// Displays a faded selection circle on this unit, indicating it is covered by the player's drag-selection
    /// </summary>
    /// <param name="showSelection">If the faded circle should be displayed or not.</param>
    public void ShowSoftSelection(bool showSelection)
    {
        if (selectIcon)
        {
            SpriteRenderer s = selectIcon.GetComponent<SpriteRenderer>();
            Color c = s.color;
            c.a = showSelection ? 0.33f : (selected ? 1f : 0f);
            s.color = c;
        }
    }
    
    /// <summary>
    /// Sets the hostility of this unit.
    /// - Friendly units can be controlled and have abilities queued by the player.
    /// - Enemy units cannot be controlled at all, but can still be selected to show their stats.
    /// </summary>
    /// <param name="hostility">The new hostility of the unit.</param>
    public void SetHostility(Hostility hostility)
    {
        Hostility = hostility;

        if (selectIcon)
        {
            SpriteRenderer s = selectIcon.GetComponent<SpriteRenderer>();
            Color c;
            switch (hostility)
            {
                case Hostility.Friendly:
                    c = Color.green;
                    break;
                case Hostility.Neutral:
                    c = Color.yellow;
                    break;
                case Hostility.Hostile:
                    c = Color.red;
                    break;
                default:
                    c = Color.white;
                    break;
            }
            c.a = 0f;
            s.color = c;
        }
    }

    /// <summary>
    /// Deals damage to this unit. 
    /// The unit is killed if their resulting HP <= 0.
    /// </summary>
    /// <param name="dmg">The amount of damage to deal.</param>
    public void TakeDamage(float dmg)
    {
        if (immune) return;
        SetCurrentHP(currentHP - dmg);

        // Added By Ty
        healthBarController.healthBar -= dmg;
    }

    /// <summary>
    /// Sets the unit's HP to an amount.
    /// The provided value to set to is clamped between 0 and the unit's max HP.
    /// If the reuslting HP <= 0, the unit dies.
    /// </summary>
    /// <param name="hp">The HP to set to. Automatically clamped between 0 and the unit's Max HP</param>
    public void SetCurrentHP(float hp)
    {
        if (hp == currentHP) return;

        float oldHP = currentHP;
        currentHP = Mathf.Clamp(hp, 0, maxHP);

        if (currentHP <= 0)
            Destroy();
        else if (currentHP < oldHP)
            OnDamageTaken?.Invoke(currentHP);
        else
            OnHealed?.Invoke(currentHP);
    }

    /// <summary>
    /// Kills this unit.
    /// </summary>
    public void Destroy()
    {
        RemoveFromUnitList();
        OnKilled?.Invoke(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Heals this unit by a particular amount.
    /// Can only heal up to its max HP.
    /// </summary>
    /// <param name="healAmt">The amount to heal.</param>
    public void Heal(float healAmt)
    {
        currentHP = Mathf.Clamp(currentHP + healAmt, 0, maxHP);
    }

    /// <summary>
    /// Sets this unit to move to the given position.
    /// </summary>
    /// <param name="destination">The position to move to</param>
    public void MoveTo(Vector3 destination)
    {
        if (!agent) return;

        unitState = UnitState.Moving;
        followUnit = null;
        stopTmr = 0f;
        agent.isStopped = false;
        agent.destination = destination;
    }

    /// <summary>
    /// Sets this unit to follow another unit indefinitely.
    /// </summary>
    /// <param name="other">The unit to follow.</param>
    public void Follow(Unit other)
    {
        followUnit = other;
        if (!other) return;

        unitState = UnitState.Moving;
        stopTmr = 0f;
    }

    /// <returns>All abilities on this unit.</returns>
    public UnitAbility[] GetAllAbilities()
    {
        return GetComponents<UnitAbility>();
    }

    /// <param name="abilityName">The name of the ability on this unit</param>
    /// <returns>The ability on this unit whose name matches the given ability name. Null otherwise.</returns>
    public UnitAbility GetAbility(string abilityName)
    {
        foreach (UnitAbility a in GetComponents<UnitAbility>())
        {
            if (a.abilityName == abilityName)
                return a;
        }
        return null;
    }

    /// <summary>
    /// Sets the ability the unit will execute when their action timer fills.
    /// </summary>
    /// <param name="abilityName">The name of the ability to set.</param>
    public void SetQueuedAbility(string abilityName)
    {
        SetQueuedAbility(GetAbility(abilityName));

    }

    /// <summary>
    /// Sets the ability the unit will execute when their action timer fills.
    /// </summary>
    /// <param name="ability">The ability to set. This should be a component on this unit.</param>
    public void SetQueuedAbility(UnitAbility ability)
    {
        Image abilityImg = GetComponentInChildren<Image>();
        queuedAbility = ability;
        abilityImg.sprite = ability.abilitySprite;
        abilityImg.color = new(1, 1, 1, 1);
    }

    /// <returns>The ability currently queued on this unit that will execute when its action bar fills.</returns>
    public UnitAbility GetQueuedAbility()
    {
        return queuedAbility;
    }

    /// <summary>
    /// "Focuses" on this unit, animating its selection circle and listening for ability hotkey presses.
    /// </summary>
    public void Focus()
    {
        //Update the ability hotkey and enable listening for button press
        UnitAbility[] abilities = GetAllAbilities();
        for (int i = 0; i < abilities.Length; i++)
        {
            abilities[i].Hotkey = (KeyCode)(i + 49);
            abilities[i].enabled = true;
        }

        selectIcon.GetComponent<Animator>().Play("UnitFocus");
        OnFocused?.Invoke(this);
    }

    /// <summary>
    /// "Unfocuses" this unit, stopping its selection animation and listening for its hotkey presses
    /// </summary>
    public void Unfocus()
    {
        // disable listening for ability hotkey presses
        foreach (UnitAbility a in GetAllAbilities())
            a.enabled = false;

        selectIcon.GetComponent<Animator>().Rebind();
        OnUnfocused?.Invoke(this);
    }

    /// <summary>
    /// Sets the unit's selection circle to 100% or 0% opacity, depending on showSelection.
    /// </summary>
    /// <param name="showSelection">If the unit's selection circle should be displayed.</param>
    void ShowSelection(bool showSelection)
    {
        if (selectIcon)
        {
            SpriteRenderer s = selectIcon.GetComponent<SpriteRenderer>();
            Color c = s.color;
            c.a = showSelection ? 1f : 0f;
            s.color = c;
        }
    }

    /// <summary>
    /// Adds this unit to the list of selectable units corresponding to its hostility.
    /// </summary>
    void AddToUnitList()
    {
        
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

    /// <summary>
    /// Removes this unit from the list of selectable units, meaning it can no longer be selected
    /// or interacted with.
    /// </summary>
    void RemoveFromUnitList()
    {
        if (hostility == Hostility.Friendly)
            SelectionManager.allFriendlyUnits.Remove(this);
        else
            SelectionManager.allOtherUnits.Remove(this);
    }
}
