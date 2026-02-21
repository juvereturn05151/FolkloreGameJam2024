#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Drakkar
{
	public class ToggleBool : ConditionalShowingAttribute
	{
		public bool full=false;

		public ToggleBool() { }

		public ToggleBool(bool fullLine) => full=fullLine;

		public ToggleBool(string conditionalSourceField) => ConditionalSourceField=conditionalSourceField;

		public ToggleBool(string conditionalSourceField,bool boolvalue,bool inverse=false)
		{
			ConditionalSourceField=conditionalSourceField;
			boolValue=boolvalue;
			Inverse=inverse;
		}

		public ToggleBool(string conditionalSourceField,int showInInspector,bool inverse=false)
		{
			ConditionalSourceField=conditionalSourceField;
			IntValue=showInInspector;
			Inverse=inverse;
		}
	}
}