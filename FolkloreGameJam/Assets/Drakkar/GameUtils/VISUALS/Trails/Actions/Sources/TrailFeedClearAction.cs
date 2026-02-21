#if DRAKKAR_EVENTS
using Drakkar.Events;

namespace Drakkar.GameUtils
{
	public sealed class TrailFeedClearAction : DrakkarAction
	{
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override void Execute(DrakkarActionData data) => (data._obj as DrakkarTrail).Clear();

		public TrailFeedClearAction() => customType=typeof(DrakkarTrail);
	#if UNITY_EDITOR
		public override string ShowParameters(DrakkarActionData data) => string.Empty;
	#endif
	}
}
#endif