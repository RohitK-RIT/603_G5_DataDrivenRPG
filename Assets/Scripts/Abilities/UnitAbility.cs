using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public abstract class UnitAbility : MonoBehaviour
{
    public delegate void AbilityHandler();

    public event AbilityHandler OnAbilityExecuted;
    public event AbilityHandler OnAbilityQueued;
    public event AbilityHandler OnAbilityCancelled;

    public string abilityName = "Ability";
    public string description = "Ability Description";
    public Sprite abilitySprite;

    protected float timer = 0f;

    const string defaultImgPath = "Assets/Art/Sprites/Ability_Default.png";

    KeyCode hotkey;

    public KeyCode Hotkey
    {
        get { return hotkey; }
        set { hotkey = value; }
    }

    private bool initialized = false;

    protected virtual void Awake()
    {
        if (!abilitySprite)
        {
            Texture2D imgTex = new Texture2D(128, 128);
            if (imgTex.LoadImage(File.ReadAllBytes(defaultImgPath)))
            {
                abilitySprite = Sprite.Create(imgTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
            }
        }
    }

    protected virtual void Start()
    {
        if (initialized)
            return;
        
        enabled = false;
        initialized = true;
    }

    protected virtual void Update()
    {
        if (Input.GetKey(hotkey))
            Queue();
    }

    /// <summary>
    /// Called when the queued ability is cast.
    /// Make sure to call base.Execute() AT THE END of your implementation
    /// Override this method in your specific ability scripts.
    /// </summary>
    public virtual void Execute()
    {
        OnAbilityExecuted?.Invoke();
    }

    /// <summary>
    /// Called when the player queues an ability to be cast when the unit's action timer fills.
    /// Make sure to call base.Queue() AT THE END of your implementation
    /// Override this method in your specific ability scripts.
    /// </summary>
    public virtual void Queue()
    {
        GetComponent<Unit>().SetQueuedAbility(this);
        OnAbilityQueued?.Invoke();
    }

    /// <summary>
    /// Called when the ability is cancelled, removing it from the unit's queued ability.
    /// A cancelled ability does not reset the unit's attack bar.
    /// </summary>
    /// <returns>
    /// If the ability was cancelled. This returns false if the ability was 
    /// never queued to begin with, or was already executed.
    /// </returns>
    public virtual bool Cancel()
    {
        Unit u = GetComponent<Unit>();
        if (u.GetQueuedAbility() == this)
        {
            u.SetQueuedAbility((UnitAbility)null);
            OnAbilityCancelled?.Invoke();
            return true;
        }

        return false;
    }
}