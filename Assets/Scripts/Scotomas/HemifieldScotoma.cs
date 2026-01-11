using UnityEngine;
using Tobii.XR;

// Simple hemifield scotoma for superior/inferior visual field defects (glaucoma-like).
// Creates a black rectangular occluder covering the superior or inferior hemifield.
// Add a GameObject with this script to the `Scotomas` list in ScotomaHandler with name "SuperiorHemifield" or "InferiorHemifield".

public class HemifieldScotoma : Scotoma
{
    // Determine which hemifield to occlude based on the GameObject name
    private bool isSuperior = true;
    private GameObject maskQuad;
    private Material maskMaterial;

    protected override void OnEnable()
    {
        // Determine hemifield from this gameObject's name
        isSuperior = this.gameObject.name.Contains("Superior");

        base.OnEnable();

        CreateMask();
        
        UpdateMaskTransform();
        
        if (maskQuad != null) maskQuad.SetActive(true);
    
    }

    protected override void OnDisable()
    {
        if (maskQuad != null)
        {
            maskQuad.SetActive(false);
        }
        base.OnDisable();
    }

    void CreateMask()
    {
        if (maskQuad != null) return;

        // Create a simple quad with the same background color as DotsSpawner
        maskQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        maskQuad.name = "HemifieldMask";

        // Set position of maskQuad relative to the parent scotoma object
        maskQuad.transform.SetParent(this.transform, false);
        
        // Remove collider to avoid physics conflicts with dots. Not actually sure if I need this
        var coll = maskQuad.GetComponent<Collider>();
        if (coll != null) DestroyImmediate(coll);        

        // Using the same low contrast color as DotsSpawner.cs (Color32(60,60,60,0))
        Color backgroundColor = new Color32(60, 60, 60, 0);

        // Create material using the unlit shader to avoid lighting effects
        Shader unlit = Shader.Find("Unlit/Color");
        if (unlit != null)
        {
            maskMaterial = new Material(unlit);
            maskMaterial.SetColor("_Color", backgroundColor);
            maskQuad.GetComponent<Renderer>().sharedMaterial = maskMaterial;
        }
    }

    void UpdateMaskTransform()
    {
        if (maskQuad == null) return;

        // DotsSpawner.cs uses spawnRadius = 7f, so y ranges from -7 to +7
        // Mask height = 7 to cover exactly one hemifield
        float height = 7f;
        maskQuad.transform.localScale = new Vector3(40f, height, 1f);

        // Position so the mask's bottom/top edge aligns with y=0
        maskQuad.transform.localPosition = new Vector3(0, isSuperior ? height / 2f : -height / 2f, 2f);
    }

    protected override void ModulateOcclusion(bool correct, float occlusionAmount)
    {
        // Hemifield scotoma doesn't need to fade in/out; keep for now
    }

    protected override void CycleOcclusion(int min, int max, float timeframe, float pauseDuration, int repetitions)
    {
        // Hemifield scotoma doesn't blink on/off; keep for now
    }
}
