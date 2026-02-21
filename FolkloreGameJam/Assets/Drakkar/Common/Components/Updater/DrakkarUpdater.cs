using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
#if MEC
using MEC;
using System.Collections.Generic;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Drakkar
{
	#region INTERFACES
	public interface IPreUpdatable
	{
		void OnPreUpdate();
	}
	public interface IUpdatable
	{
		void OnUpdate();
	}
	public interface IPostUpdatable
	{
		void OnPostUpdate();
	}
	public interface IPreLateUpdatable
	{
		void OnPreLateUpdate();
	}
	public interface ILateUpdatable
	{
		void OnLateUpdate();
	}
	public interface IPostLateUpdatable
	{
		void OnPostLateUpdate();
	}
	public interface IFixedUpdatable
	{
		void OnFixedUpdate();
	}
	#endregion

	[RequireComponent(typeof(DrakkarUpdaterPre))]
	[RequireComponent(typeof(DrakkarUpdaterPost))]
	public class DrakkarUpdater : MonoBehaviour
	{
		#region STATICS
		public static DrakkarUpdater instance;
		public static bool PreUpdates=true,Updates=true,PostUpdates=true,PreLateUpdates=true,LateUpdates=true,PostLateUpdates=true,FixedUpdates=true;
		public static bool online;
		#endregion
		#region PUBLICS
	#if DRAKKAR
		[ToggleBool]
		public bool WaitForColdstart;
	#endif
		public int PreUpdateSlots=100;
		public int NormalUpdateSlots=1000,PostUpdateSlots=100;
		public int PreLateUpdateSlots=30;
		public int LateUpdateSlots=400,PostLateUpdateSlots=30;
		public int FixedUpdateSlots=10;
		#endregion
		#region INTERNALS
		internal bool started;
		internal IndexedStaticList<IPreUpdatable> preUpdate;
		internal IndexedStaticList<IUpdatable> normalUpdate;
		internal IndexedStaticList<IPostUpdatable> postUpdate;
		internal IndexedStaticList<IPreLateUpdatable> preLateUpdate;
		internal IndexedStaticList<ILateUpdatable> lateUpdate;
		internal IndexedStaticList<IPostLateUpdatable> postLateUpdate;
		internal IndexedStaticList<IFixedUpdatable> fixedUpdate;
		#endregion
		#region PRIVATES
		private int length;
		private bool editorPaused,initialized;
		#endregion

		#region ADD / REMOVE
		#region PRE
		public static void AddPre(IPreUpdatable obj)
		{
		#if UNITY_EDITOR
			if (instance==null)
			{
				Debug.LogError("<color=red>There is no DRAKKAR UPDATER in the scene!</color>");
				return;
			}
			if (instance.preUpdate==null || instance.preUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to PreUpdate but there's no room! ("+instance.PreUpdateSlots+")</color>");
		#endif
			instance.preUpdate.Add(obj);
		}
		public static void RemovePre(IPreUpdatable obj) => instance.preUpdate.BookRemove(obj);
		#endregion
		#region NORMAL
		public static void Clear() => instance.normalUpdate.RemoveBooked();
		public static void Add(IUpdatable obj)
		{
		#if UNITY_EDITOR
			if (instance==null)
			{
				DrakkarEditor.NameAndReasonError("DRAKKAR UPDATER","is not present in the scene");
				return;
			}
			if (instance.normalUpdate==null || instance.normalUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to NormalUpdate but there's no room! ("+instance.NormalUpdateSlots+")</color>");
		#endif
		#if UPDATER_DEBUG
			DrakkarConsole.Log("Updater Normal Add: "+obj);
		#endif
			instance.normalUpdate.Add(obj);
		}
	#if MEC
		public static void AddSafe(IUpdatable obj)
		{
			if (online)
			{
			#if UNITY_EDITOR
				if (instance.normalUpdate==null || instance.normalUpdate.Room<=0)
					Debug.LogError("<color=red>"+obj+" is trying to register to NormalUpdate but there's no room! ("+instance.NormalUpdateSlots+")</color>");
			#endif
			#if UPDATER_DEBUG
				DrakkarConsole.Log("Updater Normal Add: "+obj);
			#endif
				instance.normalUpdate.Add(obj);
			}
			else
				Timing.RunCoroutine(addDeferred(obj));
		}
		static IEnumerator<float> addDeferred(IUpdatable o)
		{
			yield return Timing.WaitForOneFrame;
			Add(o);
		}
		public static void RemoveSafe(IUpdatable obj)
		{
		#if UPDATER_DEBUG
			DrakkarConsole.Log("Updater Normal REMOVE: "+obj);
		#endif
			Timing.RunCoroutine(removeDeferred(obj));
		}
		static IEnumerator<float> removeDeferred(IUpdatable o)
		{
			yield return Timing.WaitForOneFrame;
			instance.normalUpdate.BookRemove(o);
		}
	#endif
		public static void Remove(IUpdatable obj)
		{
		#if UPDATER_DEBUG
			DrakkarConsole.Log("Updater Normal REMOVE: "+obj);
		#endif
			instance.normalUpdate.BookRemove(obj);
		}
	#endregion
		#region POST
		public static void AddPost(IPostUpdatable obj)
		{
#if UNITY_EDITOR
			if (instance==null)
			{
				Debug.LogError("<color=red>There is no DRAKKAR UPDATER in the scene!</color>");
				return;
			}
			if (instance.postUpdate==null || instance.postUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to PostUpdate but there's no room! ("+instance.PostUpdateSlots+")</color>");
#endif
			instance.postUpdate.Add(obj);
		}
		public static void RemovePost(IPostUpdatable obj) => instance.postUpdate.BookRemove(obj);
		#endregion
		#region LATE
		public static void ClearPreLate() => instance.preLateUpdate.RemoveBooked();
		public static void ClearLate() => instance.lateUpdate.RemoveBooked();
		public static void ClearPostLate() => instance.postLateUpdate.RemoveBooked();
		public static void AddPreLate(IPreLateUpdatable obj)
		{
		#if UNITY_EDITOR
			if (instance==null)
			{
				Debug.LogError("<color=red>There is no DRAKKAR UPDATER in the scene!</color>");
				return;
			}
			if (instance.preLateUpdate==null || instance.preLateUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to PreLateUpdate but there's no room! ("+instance.PreLateUpdateSlots+")</color>");
		#endif
			instance.preLateUpdate.Add(obj);
		}
		public static void RemovePreLate(IPreLateUpdatable obj) => instance.preLateUpdate.BookRemove(obj);
		
		public static void TryAddLate(ILateUpdatable obj)
		{
			if (!instance.lateUpdate.Contains(obj))
				instance.lateUpdate.Add(obj);
		}
		public static void AddLate(ILateUpdatable obj)
		{
		#if UNITY_EDITOR
			if (instance==null)
			{
				Debug.LogError("<color=red>There is no DRAKKAR UPDATER in the scene!</color>");
				return;
			}
			if (instance.lateUpdate==null || instance.lateUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to LateUpdate but there's no room! ("+instance.LateUpdateSlots+")</color>");
		#endif
			instance.lateUpdate.Add(obj);
		}
		public static void TryRemoveLate(ILateUpdatable obj)
		{
			if (instance.lateUpdate.Contains(obj))
				instance.lateUpdate.BookRemove(obj);
		}
		public static void RemoveLate(ILateUpdatable obj) => instance.lateUpdate.BookRemove(obj);
		public static void AddPostLate(IPostLateUpdatable obj)
		{
		#if UNITY_EDITOR
			if (instance==null)
			{
				Debug.LogError("<color=red>There is no DRAKKAR UPDATER in the scene!</color>");
				return;
			}
			if (instance.postLateUpdate==null || instance.postLateUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to PostLateUpdate but there's no room! ("+instance.PostLateUpdateSlots+")</color>");
		#endif
			instance.postLateUpdate.Add(obj);
		}
		public static void RemovePostLate(IPostLateUpdatable obj) => instance.postLateUpdate.BookRemove(obj);
		#endregion
		#region FIXED
		public static void ClearFixed() => instance.fixedUpdate.RemoveBooked();
		public static void AddFixed(IFixedUpdatable obj)
		{
		#if UNITY_EDITOR
			if (instance==null)
			{
				Debug.LogError("<color=red>There is no DRAKKAR UPDATER in the scene!</color>");
				return;
			}
			if (instance.fixedUpdate==null||instance.fixedUpdate.Room<=0)
				Debug.LogError("<color=red>"+obj+" is trying to register to FixedUpdate but there's no room! ("+instance.FixedUpdateSlots+")</color>");
		#endif
			instance.fixedUpdate.Add(obj);
		}
		public static void RemoveFixed(IFixedUpdatable obj) => instance.fixedUpdate.BookRemove(obj);
		#endregion
	#endregion

	#if UNITY_EDITOR
		void HandleOnPauseChanged(PauseState state) => editorPaused=state==PauseState.Paused;
	#endif

		#region UNITY STUFF
		private void OnEnable()
		{
			if (initialized)
				return;
			initialized=true;
			instance=this;
			if (PreUpdateSlots>0)
				preUpdate      =new(PreUpdateSlots,PreUpdateSlots);
			if (NormalUpdateSlots>0)
				normalUpdate   =new(NormalUpdateSlots,NormalUpdateSlots);
			if (PostUpdateSlots>0)
				postUpdate     =new(PostUpdateSlots,PostUpdateSlots);
			if (PreLateUpdateSlots>0)
				preLateUpdate  =new(PreLateUpdateSlots,PreLateUpdateSlots);
			if (LateUpdateSlots>0)
				lateUpdate     =new(LateUpdateSlots,LateUpdateSlots);
			if (PostLateUpdateSlots>0)
				postLateUpdate =new(PostLateUpdateSlots,PostLateUpdateSlots);
			if (FixedUpdateSlots>0)
				fixedUpdate    =new(FixedUpdateSlots,FixedUpdateSlots);
		#if UNITY_EDITOR
			EditorApplication.pauseStateChanged+=HandleOnPauseChanged;
		#endif
			online=true;
		}

		private void Update()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.UPDATE");
		#endif
		#if DRAKKAR
			if (WaitForColdstart && !started && ColdStart.working)
			{
			#if UNITY_EDITOR
				UnityEngine.Profiling.Profiler.EndSample();
			#endif
				return;
			}
		#endif
			if (Updates && NormalUpdateSlots>0)
				executeList(normalUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}

		private void LateUpdate()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.LATE UPDATE");
		#endif
		#if DRAKKAR
			if (WaitForColdstart && !started && ColdStart.working)
			{
			#if UNITY_EDITOR
				UnityEngine.Profiling.Profiler.EndSample();
			#endif
				return;
			}
		#endif
			if (LateUpdates && LateUpdateSlots>0)
				executeLateList(lateUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}

		private void FixedUpdate()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.FIXED UPDATE");
		#endif
		#if DRAKKAR
			if (WaitForColdstart && !started && ColdStart.working)
			{
			#if UNITY_EDITOR
				UnityEngine.Profiling.Profiler.EndSample();
			#endif
				return;
			}
		#endif
			if (FixedUpdates && FixedUpdateSlots>0)
				executeFixedList(fixedUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}

		private void OnDestroy()
		{
			online=false;
			//if (Thread0Slots>0)
			//{
			//	WaitForThread0();
			//	EZThread.EndThread(thread0_runner);
			//}
		#if UNITY_EDITOR
			EditorApplication.pauseStateChanged-=HandleOnPauseChanged;
		#endif
		}
		#endregion

		#region PRE & POST EVENTS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		internal void processUpdatePre()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.PRE UPDATE");
		#endif
		#if DRAKKAR
			if (WaitForColdstart && !started && !ColdStart.Ready)
			{
			#if UNITY_EDITOR
				UnityEngine.Profiling.Profiler.EndSample();
			#endif
				return;
			}
		#endif   
			DrakkarTime.deltaTime=Time.deltaTime;
			DrakkarTime.time=Time.time;
			DrakkarTime.timeSinceLevelLoad=Time.timeSinceLevelLoad;
			DrakkarTime.timeSinceLevelLoadDouble=Time.timeSinceLevelLoadAsDouble;
			DrakkarTime.realTime=Time.realtimeSinceStartup;
			DrakkarTime.realTimeDouble=Time.realtimeSinceStartupAsDouble;

			DrakkarTime.Update_GUI_DeltaTime();
			if (PreUpdates && PreUpdateSlots>0)
				executePreList(preUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		internal void processUpdatePost()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.POST UPDATE");
		#endif
			if (PostUpdates && PostUpdateSlots>0)
				executePostList(postUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		internal void processLateUpdatePre()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.PRE LATE UPDATE");
		#endif
		#if DRAKKAR
			if (WaitForColdstart && !started && !ColdStart.Ready)
			{
			#if UNITY_EDITOR
				UnityEngine.Profiling.Profiler.EndSample();
			#endif
				return;
			}
		#endif   
			if (PreLateUpdates && LateUpdateSlots>0)
				executePreLateList(preLateUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		internal void processLateUpdatePost()
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarUpdater.POST LATE UPDATE");
		#endif
			if (PostLateUpdates && PostLateUpdateSlots>0)
				executePostLateList(postLateUpdate);
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
		}
		#endregion
		#region LISTS EXECUTION
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executeList(IndexedStaticList<IUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnUpdate();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executePreList(IndexedStaticList<IPreUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnPreUpdate();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executePostList(IndexedStaticList<IPostUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnPostUpdate();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executePreLateList(IndexedStaticList<IPreLateUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnPreLateUpdate();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executeLateList(IndexedStaticList<ILateUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnLateUpdate();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executePostLateList(IndexedStaticList<IPostLateUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnPostLateUpdate();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		private void executeFixedList(IndexedStaticList<IFixedUpdatable> list)
		{
			list.RemoveBooked();
			length=list.Length;
			if (length>0)
			{
				for (int i=0;i<length;i++)
					list.Array[i].OnFixedUpdate();
			}
		}
		#endregion
	}
}
