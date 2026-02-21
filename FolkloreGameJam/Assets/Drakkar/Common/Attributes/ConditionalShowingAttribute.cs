using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Drakkar
{
	[AttributeUsage(AttributeTargets.Field|AttributeTargets.Property|
		AttributeTargets.Class|AttributeTargets.Struct,Inherited=true)]
	public class ConditionalShowingAttribute : PropertyAttribute
	{
		public string ConditionalSourceField="";
		public int IntValue=-100;
		public bool HideInInspector=false;
		public bool boolValue;
		public bool Inverse;

	#if UNITY_EDITOR
		public static float VisibilityHeight(float height,SerializedProperty property,PropertyDrawer pd)
		{
			ConditionalShowingAttribute tb=(ConditionalShowingAttribute)pd.attribute;
			return IsVisibleProperty(property,pd) ? height : -EditorGUIUtility.standardVerticalSpacing;
		}

		public static bool IsVisibleProperty(SerializedProperty property,PropertyDrawer dr)
		{
			ConditionalShowingAttribute tb=(ConditionalShowingAttribute)dr.attribute;
			string propertyPath=property.propertyPath;
			string conditionPath=propertyPath.Replace(property.name,tb.ConditionalSourceField);
			SerializedProperty sourcePropertyValue=property.serializedObject.FindProperty(conditionPath);

			tb.HideInInspector=false;

			if (tb.IntValue!=-100)
			{
				if (property.serializedObject.FindProperty(tb.ConditionalSourceField).propertyType==SerializedPropertyType.Enum)
					tb.HideInInspector=!(property.serializedObject.FindProperty(tb.ConditionalSourceField).enumValueIndex==tb.IntValue);
				if (property.serializedObject.FindProperty(tb.ConditionalSourceField).propertyType==SerializedPropertyType.Integer)
					tb.HideInInspector=!(property.serializedObject.FindProperty(tb.ConditionalSourceField).intValue==tb.IntValue);
			}

			if (sourcePropertyValue!=null && !CheckPropertyType(sourcePropertyValue,tb))
				tb.HideInInspector=true;

			if (tb.Inverse)
				tb.HideInInspector=!tb.HideInInspector;

			return !tb.HideInInspector;
		}

		public static bool CheckPropertyType(SerializedProperty sourcePropertyValue,ConditionalShowingAttribute tb)
		{
			bool b;

			switch (sourcePropertyValue.propertyType)
			{
			case SerializedPropertyType.Generic:
				return sourcePropertyValue.arraySize>=tb.IntValue;
			case SerializedPropertyType.Boolean:
				return sourcePropertyValue.boolValue==tb.boolValue;
			case SerializedPropertyType.ObjectReference:
				b=sourcePropertyValue.objectReferenceValue!=null;
				if (!tb.boolValue)
					b=!b;
				return b;
			case SerializedPropertyType.Enum:
				return sourcePropertyValue.enumValueIndex==tb.IntValue;
			case SerializedPropertyType.Integer:
				return sourcePropertyValue.intValue==tb.IntValue;
			case SerializedPropertyType.String:
				return string.IsNullOrEmpty(sourcePropertyValue.stringValue);
			default:
				Debug.LogError("Data type of the property used for conditional hiding ["+sourcePropertyValue.propertyType+"] is currently not supported");
				return true;
			}
		}
	#endif
	}
}