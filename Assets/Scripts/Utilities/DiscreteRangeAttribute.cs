using UnityEngine;
using UnityEditor;

public class DiscreteRangeAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public int steps;

    public DiscreteRangeAttribute(float min, float max, int steps)
    {
        this.min = min;
        this.max = max;
        this.steps = steps;
    }
}

[CustomPropertyDrawer(typeof(DiscreteRangeAttribute))]
public class DiscreteRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DiscreteRangeAttribute range = (DiscreteRangeAttribute)attribute;
        property.floatValue = EditorGUI.IntSlider(position, label, (int)(property.floatValue * range.steps), (int)(range.min * range.steps), (int)(range.max * range.steps)) / (float)range.steps;
    }
}