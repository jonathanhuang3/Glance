using UnityEngine;

public class SingleScotoma : Scotoma
{
    public float occlusionAmount = 0.5f;
    protected override void ModulateOcclusion(bool correct)
    {
        // Modulate the occlusion of the peripheral or central scotoma
        transform.localScale *= correct ? occlusionAmount : occlusionAmount - 0.1f * occlusionAmount;
    }
}