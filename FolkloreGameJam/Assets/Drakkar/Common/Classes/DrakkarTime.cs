using UnityEngine;
using System;
using System.Runtime.CompilerServices;

namespace Drakkar
{
	public enum TimeSlices
	{
		TIME,
		[InspectorName("DRAKKAR TIME")]DRAKKAR_TIME,
		[InspectorName("GUI TIME")]GUI_TIME
	}
	public static class DrakkarTime
	{
		public static float TimeScale=1.0f;
		public static float GUIdeltaTime,timeSinceLevelLoad;
		public static float deltaTime,realTime,time;
		public static double realTimeDouble;
		public static double timeSinceLevelLoadDouble;
		public static bool Pause=false;
		private static float oldTimeScale=1;
		private static float delta;

	#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]	// to avoid Domain reload
		static void editor_init()
		{
			TimeScale=1.0f;
			Pause=false;
			oldTimeScale=1;
		}
	#endif

		public static void SetPause(bool pause)
		{
			if (Pause==pause)
				return;
			if (pause)
			{
				oldTimeScale=Time.timeScale;
				Time.timeScale=0;
			}
			else
				Time.timeScale=oldTimeScale;
			Pause=pause;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SetDrakkarTimeScale(float scale) => TimeScale=scale;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DeltaTimeClamped()
		{
		#if UNITY_EDITOR
			if (!DrakkarUpdater.online)
				Debug.LogError("<color=red>DrakkarUpdater is note present!</color>");
		#endif
			if (Pause) return 0;
			delta=deltaTime*TimeScale;
			return (delta>0.2f)?0.2f:delta;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetTimeSlice(TimeSlices slice)
		{
		#if UNITY_EDITOR
			if (!DrakkarUpdater.online)
				Debug.LogError("<color=red>DrakkarUpdater is note present!</color>");
		#endif
			return slice switch
			{
				TimeSlices.DRAKKAR_TIME => DeltaTimeClamped(),
				TimeSlices.GUI_TIME     => GUIdeltaTime,
				_                       => deltaTime
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Update_GUI_DeltaTime() => GUIdeltaTime=Time.unscaledDeltaTime;

		public static long GetTimeStamp()
		{
			long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
			return ticks/10000000;
		}
	}
}