#if DRAKKAR_EVENTS
using Drakkar.Events;

namespace Drakkar.GameUtils
{
	public sealed class TrailFeedAction : DrakkarAction
	{
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override void Execute(DrakkarActionData data)
		{
			if (data._bool)
				(data._obj as DrakkarTrail).Begin();
			else
				(data._obj as DrakkarTrail).End();
		}

		public TrailFeedAction() => customType=typeof(DrakkarTrail);
	#if UNITY_EDITOR
		public override string ShowParameters(DrakkarActionData data) => data._obj==null ? string.Empty : data._bool ? "BEGIN" : "END";
	#endif
	}
}
#endif