using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Drakkar
{
	public interface IStaticLister
	{
		public int StaticIndex { get; set; }
	}

	public sealed class SubLister<T> : IStaticLister
	{
		public T Owner;
		public int ListIndex=-1;

		public SubLister(T parent) => Owner=parent;
		public int StaticIndex { get =>ListIndex; set => ListIndex=value; }
	}

	public class StaticList<T> where T : IStaticLister
	{
		public T[] Array;

		protected SimpleStaticList<IStaticLister> removableItems;
		protected HashSet<IStaticLister> removableHash;
		protected int maxElements;
		protected int nextPosition;

		public T this[int i] => Array[i];

		public int Length => nextPosition;

		public int Capacity => maxElements;

		public int Room => maxElements-nextPosition;

		public StaticList(int max)
		{
			Array=new T[max];
			maxElements=max;
			nextPosition=0;
		}
		public StaticList(int max,int removableNum)
		{
			Array=new T[max];
			maxElements=max;
			nextPosition=0;
			if (removableNum>0)
			{
				removableItems=new(removableNum);
				removableHash=new(removableNum);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void Add(IStaticLister lister)
		{
			if (nextPosition>=maxElements)
			{
			#if UNITY_EDITOR
				Debug.LogError("Maximum number of Static List elements reached!");
			#endif
				return;
			}
			Array[nextPosition]=(T)lister;
			lister.StaticIndex=nextPosition++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public int AddAndGetIndex(IStaticLister lister)
		{
			if (nextPosition>=maxElements)
			{
			#if UNITY_EDITOR
				Debug.LogError("Maximum number of Static List elements reached!");
			#endif
				return -1;
			}
			Array[nextPosition]=(T)lister;
			lister.StaticIndex=nextPosition;
			return nextPosition++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public int Remove(int pos)
		{
			if (nextPosition<=0)
			{
			#if UNITY_EDITOR
				Debug.LogWarning("<color=yellow>StaticList</color>: Trying to remove element with index < 0");
			#endif
				return -1;
			}
			pos=Mathf.Clamp(pos,0,nextPosition-1);
			if (nextPosition==1)
			{
				nextPosition=0;
				Array[0].StaticIndex=-1;
				return -1;
			}
			if (Array[pos]!=null)
				Array[pos].StaticIndex=-1;
			if (pos==nextPosition-1)
				RemoveLast();
			else
			{
				int last=nextPosition-1;
				Array[pos]=Array[last];
				Array[last].StaticIndex=pos;
				nextPosition--;
			}
			return pos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void Remove(IStaticLister sl)
		{
			if (nextPosition<=0)
			{
			#if UNITY_EDITOR
				Debug.LogWarning("Trying to remove element with index < 0");
			#endif
				return;
			}
			int pos=Mathf.Clamp(sl.StaticIndex,0,nextPosition-1);
			if (nextPosition==1)
			{
				nextPosition=0;
				Array[0].StaticIndex=-1;
				return;
			}
			if (Array[pos]!=null)
				Array[pos].StaticIndex=-1;
			if (pos==nextPosition-1)
				RemoveLast();
			else
			{
				int last=nextPosition-1;
				Array[pos]=Array[last];
				Array[last].StaticIndex=pos;
				nextPosition--;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveLast() => nextPosition--;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ExtractLast()
		{
			int i=(nextPosition--)-1;
			if (Array[i]!=null)
				Array[i].StaticIndex=-1;
			return Array[i];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void BookRemoveSafe(int pos)
		{
			if (removableHash.Add(Array[pos]))
				removableItems.Add(Array[pos]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void BookRemoveSafe(IStaticLister ls)
		{
			if (removableHash.Add(ls))
				removableItems.Add(ls);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void BookRemove(int pos) => removableItems.Add(Array[pos]);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void BookRemove(IStaticLister ls) => removableItems.Add(ls);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void RemoveBooked()
		{
		#if UNITY_EDITOR
			if (removableItems==null || removableItems.Capacity<1)
				DrakkarEditor.NameAndReasonError("StaticList","Has no room for booked removable items");
		#endif
			int l=removableItems.Length;
			if (l<=0)
				return;
			for (int i=0;i<l;i++)
				Remove(removableItems.Array[i]);

			removableItems.Clear();
			removableHash.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() => nextPosition=0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void CopyTo(StaticList<T> list)
		{
		#if UNITY_EDITOR
			if (list.Capacity<nextPosition)
				DrakkarEditor.NameAndReasonError("StaticList","Destination StaticList has not enough room");
		#endif
			System.Array.Copy(Array,list.Array,nextPosition);
			list.nextPosition=nextPosition;
		}
	}

	public class SimpleStaticList<T>
	{
		public T[] Array;
		protected int maxElements;
		protected int nextPosition,head,count;

		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public T this[int i] => Array[i];

		public int Length => nextPosition;
		public int Capacity => maxElements;
		public int Room => maxElements-nextPosition;

		public SimpleStaticList(int max)
		{
			Array=new T[max];
			maxElements=max;
			nextPosition=head=count=0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public int Add(T lister)
		{
			if (nextPosition>=maxElements)
			{
			#if UNITY_EDITOR
				Debug.LogError("Maximum number of Static List elements reached!");
			#endif
				return -1;
			}
			Array[nextPosition]=lister;
			return nextPosition++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void Remove(int pos)
		{
			if (nextPosition<=0)
			{
			#if UNITY_EDITOR
				Debug.LogWarning("<color=yellow>StaticList</color>: Trying to remove element with index < 0");
			#endif
				return;
			}
			pos=Mathf.Clamp(pos,0,nextPosition-1);
			if (nextPosition==1)
			{
				nextPosition=0;
				return;
			}
			if (pos==nextPosition-1)
				RemoveLast();
			else
			{
				int last = nextPosition-1;
				Array[pos]=Array[last];
				nextPosition--;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveLast() => nextPosition--;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ExtractLast() => Array[(nextPosition--)-1];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetLast() => Array[nextPosition-1];
		public bool Contains(T item)
		{
			for (int i=0;i<nextPosition;i++)
			{
				if (Array[i].Equals(item))
					return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() => nextPosition=0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetLenght(int len) => nextPosition=len;
		#region STACK FUNCTIONS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public int Push(T lister) => Add(lister);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public T Pop()
		{
			if (nextPosition>0)
				return Array[--nextPosition];
			else
			{
			#if UNITY_EDITOR
				Debug.LogError("Trying to pop an element from an empty SimpleStaticList");
			#endif
				return default(T);
			}
		}
		#endregion
		#region QUEUE FUNCTIONS
		public int QueueHead => head;
		public int QueueLength => count;
		public int QueueCapacity => maxElements-count;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public bool Enqueue(T lister)
		{
			if (count>=maxElements)
			{
			#if UNITY_EDITOR
				Debug.LogError("Maximum number of Static Queue elements reached!");
			#endif
				return false;
			}
			Array[nextPosition++]=lister;
			count++;
			if (nextPosition>=maxElements)
				nextPosition=0;
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public T Dequeue()
		{
			if (count>0)
			{
				T elem=Array[head++];
				count--;
				if (head>=maxElements)
					head=0;
				return elem;
			}
			else
			{
			#if UNITY_EDITOR
				Debug.LogError("Trying to Dequeue an element from an empty Queue!");
			#endif
				return default;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void ClearQueue()
		{
			nextPosition=0;
			head=0;
			count=0;
		}
		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void CopyTo(SimpleStaticList<T> list)
		{
		#if UNITY_EDITOR
			if (list.Capacity<nextPosition)
				DrakkarEditor.NameAndReasonError("StaticList","Destination StaticList has not enough room");
		#endif
			System.Array.Copy(Array,list.Array,nextPosition);
			list.nextPosition=nextPosition;
		}
	}

	#region INDEXED LIST
	public sealed class IndexedStaticList<T> : SimpleStaticList<T>
	{
		private Dictionary<T,int> index,indexRemoved;
		private SimpleStaticList<T> removableItems;
		private int remItems;

		public IndexedStaticList(int max) : base(max)
		{
			Array=new T[max];
			maxElements=max;
			nextPosition=0;
			index=new(max);
			indexRemoved=new(max);
		}

		public IndexedStaticList(int max,int removableNum) : base(max)
		{
			Array=new T[max];
			maxElements=max;
			nextPosition=0;
			remItems=removableNum;
			if (removableNum>0)
			{
				removableItems=new(removableNum);
				indexRemoved=new(max);
			}
			index=new(max);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public new void Clear()
		{
			nextPosition=0;
			index.Clear();
			removableItems?.Clear();
			indexRemoved?.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public new bool Contains(T lister) => index.ContainsKey(lister);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int GetIndex(T lister)
		{
			if (index.TryGetValue(lister,out int ind))
				return ind;
			else
				return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public new void Add(T lister)
		{
			if (index.ContainsKey(lister))
				return;
			if (nextPosition>=maxElements)
			{
			#if UNITY_EDITOR
				DrakkarEditor.NameAndReasonError("IndexedList","Maximum number of elements reached");
			#endif
				return;
			}
			if (remItems>0 && indexRemoved.TryGetValue(lister,out int remIndex))  //check if it has just been removed
			{
				removableItems.Remove(remItems);
				indexRemoved.Remove(lister);
			}
			index.Add(lister,nextPosition);
			Array[nextPosition++]=lister;
			return;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public int AddAndGetIndex(T lister)
		{
			if (index.ContainsKey(lister))
				return -1;
			if (nextPosition>=maxElements)
			{
			#if UNITY_EDITOR
				DrakkarEditor.NameAndReasonError("IndexedList","Maximum number of elements reached");
			#endif
				return -1;
			}
			index.Add(lister,nextPosition);
			Array[nextPosition]=(T)lister;
			return nextPosition++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void Remove(T lister)
		{
			if (nextPosition<=0)
			{
			#if UNITY_EDITOR
				Debug.LogError("Trying to remove element with index < 0");
			#endif
				return;
			}
			int pos;
			if (index.TryGetValue(lister,out pos))
			{
				if (nextPosition==1)
				{
					index.Clear();
					nextPosition=0;
					return;
				}
				if (pos==nextPosition-1)
				{
					index.Remove(lister);
					nextPosition--;
				}
				else
				{
					int last=nextPosition-1;
					index.Remove(lister);
					index[Array[last]]=pos;
					Array[pos]=Array[last];
					nextPosition--;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void BookRemove(T lister) => removableItems.Add(lister);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public void RemoveBooked()
		{
			if (removableItems.Length<=0)
				return;

			for (int i = 0;i<removableItems.Length;i++)
				Remove(removableItems.Array[i]);
			removableItems.Clear();
			indexRemoved?.Clear();
		}
	}
	#endregion
}
