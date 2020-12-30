using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(VA_PopupAttribute))]
public class VA_PopupDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attribute = (VA_PopupAttribute)attribute;
		var values    = new int[Attribute.Names.Length];
		var options   = System.Array.ConvertAll<string, GUIContent>(Attribute.Names, i => new GUIContent(i));
		
		for (var i = 0; i < values.Length; i++)
		{
			values[i] = i;
		}
		
		EditorGUI.IntPopup(position, property, options, values, label);
	}
}
#endif

public class VA_PopupAttribute : PropertyAttribute
{
	public string[] Names;
	
	public VA_PopupAttribute(params string[] newNames)
	{
		Names = newNames;
	}
}