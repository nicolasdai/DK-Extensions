using UnityEngine;
using DK.Attributes;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var minMaxAttribute = (MinMaxSliderAttribute)attribute;
        var propertyType = property.propertyType;

        label.tooltip = minMaxAttribute.min.ToString("F2") + " to " + minMaxAttribute.max.ToString("F2");

        //PrefixLabel returns the rect of the right part of the control. It leaves out the label section. We don't have to worry about it. Nice!
        var controlRect = EditorGUI.PrefixLabel(position, label);

        var splitRect = SplitRect(controlRect, 3);

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (propertyType)
        {
            case SerializedPropertyType.Vector2:
            {
                EditorGUI.BeginChangeCheck();

                var vector = property.vector2Value;
                var minVal = vector.x;
                var maxVal = vector.y;

                //F2 limits the float to two decimal places (0.00).
                minVal = EditorGUI.FloatField(splitRect[0], float.Parse(minVal.ToString("F2")));
                maxVal = EditorGUI.FloatField(splitRect[2], float.Parse(maxVal.ToString("F2")));

                EditorGUI.MinMaxSlider(splitRect[1], ref minVal, ref maxVal,
                    minMaxAttribute.min, minMaxAttribute.max);

                if (minVal < minMaxAttribute.min)
                {
                    minVal = minMaxAttribute.min;
                }

                if (maxVal > minMaxAttribute.max)
                {
                    maxVal = minMaxAttribute.max;
                }

                vector = new Vector2(minVal > maxVal ? maxVal : minVal, maxVal);

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2Value = vector;
                }

                break;
            }
            case SerializedPropertyType.Vector2Int:
            {
                EditorGUI.BeginChangeCheck();

                var vector = property.vector2IntValue;
                float minVal = vector.x;
                float maxVal = vector.y;

                minVal = EditorGUI.FloatField(splitRect[0], minVal);
                maxVal = EditorGUI.FloatField(splitRect[2], maxVal);

                EditorGUI.MinMaxSlider(splitRect[1], ref minVal, ref maxVal,
                    minMaxAttribute.min, minMaxAttribute.max);

                if (minVal < minMaxAttribute.min)
                {
                    maxVal = minMaxAttribute.min;
                }

                if (minVal > minMaxAttribute.max)
                {
                    maxVal = minMaxAttribute.max;
                }

                vector = new Vector2Int(Mathf.FloorToInt(minVal > maxVal ? maxVal : minVal), Mathf.FloorToInt(maxVal));

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2IntValue = vector;
                }

                break;
            }
        }
    }

    private static Rect[] SplitRect(Rect rectToSplit, int n)
    {
        var rects = new Rect[n];

        for (var i = 0; i < n; i++)
        {
            rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
        }

        var padding = (int)rects[0].width - 40;
        var space = 5;

        rects[0].width -= padding + space;
        rects[2].width -= padding + space;

        rects[1].x -= padding;
        rects[1].width += padding * 2;

        rects[2].x += padding + space;

        return rects;

    }

}