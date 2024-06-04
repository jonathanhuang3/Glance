using UnityEngine;

public class SingleScotoma : Scotoma
{
    protected override void ModulateOcclusion(bool correct, float occlusionAmount)
    {
        // Modulate the occlusion of the peripheral or central scotoma
        transform.localScale *= correct ? occlusionAmount : occlusionAmount - 0.1f * occlusionAmount;
    }
}