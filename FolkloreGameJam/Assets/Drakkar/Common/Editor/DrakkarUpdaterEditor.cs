#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Drakkar
{
	[CustomEditor(typeof(DrakkarUpdater))]
	public class DrakkarUpdaterEditor : Editor
	{
		private DrakkarUpdater upd;
		private SerializedObject Serialized;

		void OnEnable() => Serialized=new SerializedObject((DrakkarUpdater)target);

		public override void OnInspectorGUI()
		{
			DrakkarUpdater du=(DrakkarUpdater)target;
			upd=du;
			DrakkarEditor.TextureLogo(du);
			Undo.RecordObject(du,"updater");

			Serialized.Update();
			EditorGUI.BeginChangeCheck();
		#if DRAKKAR
			DrakkarEditor.Toggle(ref du.WaitForColdstart,"Wait for Coldstart");
		#endif
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"PreUpdateSlots");
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"NormalUpdateSlots");
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"PostUpdateSlots");
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"PreLateUpdateSlots");
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"LateUpdateSlots");
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"PostLateUpdateSlots");
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			DrakkarEditor.DrakkarPropertyInspector(Serialized,"FixedUpdateSlots");
			if (EditorGUI.EndChangeCheck())
				Serialized.ApplyModifiedProperties();

			if (Application.isPlaying)
			{
				string res=string.Empty;
				GUI.contentColor=Color.yellow;
				if (du.preUpdate!=null && du.preUpdate.Length>0)
					res+="Pre-Updates="+du.preUpdate.Length.ToString()+"       ";
				if (du.normalUpdate!=null && du.normalUpdate.Length>0)
					res+="Updates="+du.normalUpdate.Length.ToString()+"       ";
				if (du.postUpdate!=null && du.postUpdate.Length>0)
					res+="Post-Updates="+du.postUpdate.Length.ToString();
				EditorGUILayout.Space();
				if (!string.IsNullOrEmpty(res))
					EditorGUILayout.LabelField(res,DrakkarEditor.DrakkarBoldLabel);

				res=string.Empty;
				if (du.preLateUpdate!=null && du.preLateUpdate.Length>0)
					res+="PreLate="+du.preLateUpdate.Length.ToString()+"       ";
				if (du.lateUpdate!=null && du.lateUpdate.Length>0)
					res+="Late="+du.lateUpdate.Length.ToString()+"       ";
				if (du.postLateUpdate!=null && du.postLateUpdate.Length>0)
					res+="PostLate="+du.postLateUpdate.Length.ToString();
				if (!string.IsNullOrEmpty(res))
					EditorGUILayout.LabelField(res,DrakkarEditor.DrakkarBoldLabel);

				EditorGUILayout.Space();
				if (du.fixedUpdate!=null && du.fixedUpdate.Length>0)
					EditorGUILayout.LabelField("Fixed Update="+du.fixedUpdate.Length.ToString(),DrakkarEditor.DrakkarBoldLabel);
				GUI.contentColor=Color.white;
			}
		}
	}

	
	[CustomEditor(typeof(DrakkarUpdaterPre))]
	public class DrakkarUpdaterPreEditor : Editor
	{
		public override void OnInspectorGUI() { target.hideFlags=UnityEngine.HideFlags.None; }
	}

	[CustomEditor(typeof(DrakkarUpdaterPost))]
	public class DrakkarUpdaterPostEditor : Editor
	{
		public override void OnInspectorGUI() { target.hideFlags=UnityEngine.HideFlags.None; }
	}
}
#endif