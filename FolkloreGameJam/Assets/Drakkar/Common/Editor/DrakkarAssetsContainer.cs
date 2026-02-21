#if UNITY_EDITOR
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Drakkar
{
	public enum DRAKKAR_MARKER_COLOR
	{
		RED,WHITE,BLUE,[InspectorName("LIGHT BLUE")]LIGHT_BLUE,GREEN,[InspectorName("DARK GREEN")]DARK_GREEN,GRAY,CYAN,ORANGE,PURPLE,YELLOW,PINK,
		[InspectorName("DEEP PURPLE (color)")]DEEP_PURPLE,
		[InspectorName("DARK YELLOW (color)")]DARK_YELLOW
	}

#if ODIN_INSPECTOR
	[HideMonoScript]
#endif
	public class DrakkarAssetsContainer : ScriptableObject
	{
		public static DrakkarAssetsContainer instance
		{
			get
			{
				if (_inst==null)
					Load();
				return _inst;
			}
		}
		public static bool loaded=false;

		private static DrakkarAssetsContainer _inst;

	#if ODIN_INSPECTOR
		[FoldoutGroup("Images")]
		[PreviewField]
	#endif
		public Texture DrakkarLogo,Transform,GUI,ArrowUp,ArrowDown,Play,Animation,Sound,Dialog,Palette,Spawn,Spawn2,Tracker,Trigger,Square,Corona,Steam,Trail,Decals,Sphere,VisibilitySphere,Eye,Instances,Mirror,
						Vfx,Flare,Rumble,Vibration,IK,Skin,Grass,Spline,Behaviour,HitPoint,Camera,Variable,Room,Portal,Occlusion,Occluder,Cylinder,Wall,Puzzle,Time,Frustum,Shock,
						Loot,Search,Map,MasterExploder,IA,Patch,VfxGraph;
	#if ODIN_INSPECTOR
		[FoldoutGroup("Mission Nodes")]
		[PreviewField]
	#endif
		public Texture2D MN_Map,MN_Cutscene,MN_Variable,MN_Time,MN_WorkIcon,MN_Dialogue,MN_Fight,MN_Checkpoint,MN_Switch;

	#if ODIN_INSPECTOR
		[FoldoutGroup("TimeLine Markers")]
		[PreviewField]
	#endif
		public Texture D_Red,D_White,D_Blue,D_LightBlue,D_Green,D_DarkGreen,D_Gray,D_Cyan,D_Orange,D_Purple,D_Yellow,D_Pink;
	#if ODIN_INSPECTOR
		[FoldoutGroup("Meshes")]
		[PreviewField]
	#endif
		public Mesh CapsuleMesh,LensFlare;

		private static void Load()
		{
	#if UNITY_EDITOR
			if (loaded)
				return;
			_inst=Resources.Load<DrakkarAssetsContainer>("DrakkarAssetsContainer");
			loaded=true;
	#endif
		}

	#if UNITY_EDITOR
		public static Color GetSchemaColor(DRAKKAR_MARKER_COLOR col) => col switch
		{
			DRAKKAR_MARKER_COLOR.RED        => Color.red,
			DRAKKAR_MARKER_COLOR.BLUE       => Color.blue,
			DRAKKAR_MARKER_COLOR.CYAN       => Color.cyan,
			DRAKKAR_MARKER_COLOR.DARK_GREEN => Color.green*0.6f,
			DRAKKAR_MARKER_COLOR.GRAY       => Color.gray,
			DRAKKAR_MARKER_COLOR.GREEN      => Color.green,
			DRAKKAR_MARKER_COLOR.LIGHT_BLUE => new Color(0.6f,0.6f,1),
			DRAKKAR_MARKER_COLOR.ORANGE     => new Color(1,0.6f,0),
			DRAKKAR_MARKER_COLOR.PINK       => new Color(1,0.7f,0.7f),
			DRAKKAR_MARKER_COLOR.PURPLE     => new Color(1,0,1),
			DRAKKAR_MARKER_COLOR.YELLOW     => new Color(1,1,0),
			DRAKKAR_MARKER_COLOR.DEEP_PURPLE=> new Color(.6f,0,.6f),
			DRAKKAR_MARKER_COLOR.DARK_YELLOW=> new Color(.6f,.6f,0),
			_                               => Color.white
		};

		public static Texture GetColorLogo(DRAKKAR_MARKER_COLOR col) => col switch
		{
			DRAKKAR_MARKER_COLOR.WHITE      => instance.D_White,
			DRAKKAR_MARKER_COLOR.YELLOW     => instance.D_Yellow,
			DRAKKAR_MARKER_COLOR.PURPLE     => instance.D_Purple,
			DRAKKAR_MARKER_COLOR.PINK       => instance.D_Pink,
			DRAKKAR_MARKER_COLOR.ORANGE     => instance.D_Orange,
			DRAKKAR_MARKER_COLOR.LIGHT_BLUE => instance.D_LightBlue,
			DRAKKAR_MARKER_COLOR.GREEN      => instance.D_Green,
			DRAKKAR_MARKER_COLOR.DARK_GREEN => instance.D_DarkGreen,
			DRAKKAR_MARKER_COLOR.CYAN       => instance.D_Cyan,
			DRAKKAR_MARKER_COLOR.BLUE       => instance.D_Blue,
			DRAKKAR_MARKER_COLOR.GRAY       => instance.D_Gray,
			_                               => instance.D_Red
		};

		public static Texture GetTexture(string pr)
		{
			Load();
			if (instance==null)
				Debug.Log("ERROR");
			var t=instance.GetType().GetField(pr);
			return t!=null ? (Texture)t.GetValue(instance) : null;
		}
	#endif
	}
}
#endif