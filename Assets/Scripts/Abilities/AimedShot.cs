using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimedShot : Attack
{
    public float chargeTime = 1.5f;
    public float accuracyMultiplier = 1.2f;

    protected override void Start()
    {
        base.Start();

        abilityName = "Aimed Shot";
        description = $"Fire a precise sniper round after {chargeTime} seconds, dealing {equippedWeapon.damage_per_shot} damage.";
        accuracy *= accuracyMultiplier;
    }

    public override void Execute()
    {
        Line.startColor = Color.yellow;
        Line.endColor = Color.yellow;
        StartCoroutine(ShotTimer());
    }

    IEnumerator ShotTimer()
    {
        for (float x = 0; x <= chargeTime; x += Time.deltaTime)
        {
            Line.widthMultiplier = Mathf.Lerp(0.1f, 0.5f, x / chargeTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        base.Execute();
    }
}
