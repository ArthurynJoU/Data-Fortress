using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
/// <summary>
/// Standard Unity PropertyDrawer implementation to visualize FloatRange as a MinMax slider.
/// Complies with standard UnityEditor API guidelines.
/// </summary>
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // We find our variables by their exact names in the FloatRange class
        SerializedProperty minProp = property.FindPropertyRelative("_min");
        SerializedProperty maxProp = property.FindPropertyRelative("_max");

        float minValue = minProp.floatValue;
        float maxValue = maxProp.floatValue;

        FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;

        if ( limit != null )
        {
            // Unity's built-in double slider
            EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);
        }

        minProp.floatValue = minValue;
        maxProp.floatValue = maxValue;

        EditorGUI.EndProperty();
    }
}