using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScotomaHandler : MonoBehaviour
{
    public MetaStimulus.OKRDriver driver;
    public float speed = 0.56f;
    public enum ScotomaTypes { None, Cone, Central, Peripheral, Grid, Fixation };
    public ScotomaTypes scotoma = ScotomaTypes.None;
    public List<GameObject> scotomas = new List<GameObject>(); // For User to provide scotoma gameobjects in Unity editor
    private GameObject currentScotoma;
    public GameObject CurrentScotoma
    {
        get { return currentScotoma; }
    }

    void OnEnable()
    {
        Initialize();
    }

    void OnDisable()
    {
        if (currentScotoma != null)
        {
            currentScotoma.SetActive(false);
        }
    }

    public void Initialize()
    {
        currentScotoma = null;
        scotomas.ForEach(s => s.SetActive(false));

        if (scotoma != ScotomaTypes.None)
        {
            currentScotoma = scotomas.Where(s => s.name.Contains(scotoma.ToString())).FirstOrDefault();
            currentScotoma.GetComponent<Scotoma>().driver = driver;
            // Debug.Log($"currentScotoma (inside scotoma handler): {currentScotoma.name}");
            currentScotoma.SetActive(true); // Disable if using rotation calibration.
        }
    }
}