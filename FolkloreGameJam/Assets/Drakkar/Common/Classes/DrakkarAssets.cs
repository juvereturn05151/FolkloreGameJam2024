#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Drakkar
{
	public static class DrakkarAssets
	{
		[MenuItem("Tools/Drakkar/Other Drakkar Assets")]
		internal static void drakkraAssets() => Application.OpenURL("https://assetstore.unity.com/publishers/108531");
	}
}
#endif