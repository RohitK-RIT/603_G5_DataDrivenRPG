using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class UnitAbility : MonoBehaviour
{
    public delegate void AbilityExecutedHandler();
    public event AbilityExecutedHandler OnAbilityExecuted;

    public string abilityName = "Ability";
    public string description = "Ability Description";
    public Sprite abilitySprite;

    protected float timer = 0f;

    const string defaultImgPath = "Assets/Art/Sprites/Ability_Default.png";

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

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called when the player clicks on the corresponding Unit Ability button.
    /// Make sure to call base.Execute() AT THE END of your implementation
    /// Override this method in your specific ability scripts.
    /// </summary>
    public virtual void Execute()
    {
        OnAbilityExecuted?.Invoke();
    }
}
