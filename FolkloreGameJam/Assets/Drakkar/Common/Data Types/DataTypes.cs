using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Drakkar
{
	public enum ColorChannel { R,G,B,A }

	#region BYTE BOOL
	[System.Serializable]
	public struct ByteBool
	{
		private byte b;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool value => b!=0;

		public ByteBool(bool boo) => b=(byte)(boo ? 255 : 0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Set(bool boo) => b=(byte)(boo ? 255 : 0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override string ToString() => ((b!=0) ? "T" : "F");
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromString(string s) => b=(byte)((s.StartsWith("T")) ? 255 : 0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static bool operator ==(bool boo,ByteBool bb) => boo==(bb.b!=0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static bool operator !=(bool boo,ByteBool bb) => boo!=(bb.b!=0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override bool Equals(object obj) => (b!=0)==(obj!=null);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override int GetHashCode() => b.GetHashCode();
	}
	#endregion
	#region BYTEARRAY
	public static class ByteArray
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static string ToString(byte[] array)
		{
			int l=array.Length;
			string s=l.ToString("X");
			for (int u = 0;u<l;u++)
				s+="#"+array[u].ToString("X");
			return s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static byte[] FromString(string s)
		{
			int len;
			byte[] i;
			string[] ss=s.Split("#".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			len=System.Convert.ToInt32(ss[0].Trim(),16);
			i=new byte[len];
			for (int u = 1;u<len+1;u++)
				i[u-1]=System.Convert.ToByte(ss[u].Trim(),16);
			return i;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void FromString(string s,ref byte[] dest)
		{
			int len;
			string[] ss=s.Split("#".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			len=System.Convert.ToInt32(ss[0].Trim(),16);
			if (len>dest.Length)
				return;
			for (int u = 1;u<len+1;u++)
				dest[u-1]=System.Convert.ToByte(ss[u].Trim(),16);
		}
	}
	#endregion
	#region INTARRAY
	public static class IntArray
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static string ToString(int[] array)
		{
			int l=array.Length;
			string s=l.ToString("X");
			for (int u=0;u<l;u++)
				s+="#"+array[u].ToString("X");
			return s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static int[] FromString(string s)
		{
			int len;
			int[] i;
			string[] ss=s.Split("#".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			len=System.Convert.ToInt32(ss[0].Trim(),16);
			i=new int[len];
			for (int u=1;u<len+1;u++)
				i[u-1]=System.Convert.ToInt32(ss[u].Trim(),16);
			return i;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void FromString(string s,ref int[] dest)
		{
			int len;
			string[] ss=s.Split("#".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			len=System.Convert.ToInt32(ss[0].Trim(),16);
			if (len>dest.Length)
				return;
			for (int u=1;u<len+1;u++)
				dest[u-1]=System.Convert.ToInt32(ss[u].Trim(),16);
		}
	}
	#endregion
	#region FLOATARRAY
	public static class FloatArray
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static string ToString(float[] array)
		{
			int l=array.Length;
			string s=l.ToString("X");
			for (int u=0;u<l;u++)
				s+="!"+System.Convert.ToString(array[u]);
			return s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float[] FromString(string s)
		{
			int len;
			float[] i;
			string[] ss=s.Split("!".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			len=System.Convert.ToInt32(ss[0].Trim(),16);
			i=new float[len];
			for (int u=1;u<len+1;u++)
				i[u-1]=System.Convert.ToSingle(ss[u].Trim());
			return i;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void FromString(string s,ref float[] dest)
		{
			int len;
			string[] ss=s.Split("!".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			len=System.Convert.ToInt32(ss[0].Trim(),16);
			if (len>dest.Length)
				return;
			for (int u=1;u<len+1;u++)
				dest[u-1]=System.Convert.ToSingle(ss[u].Trim());
		}
	}
	#endregion

	#region CUSTOM ARRAYS
	#region BYTEBITARRAY
	[System.Serializable]
	public struct ByteBitArray
	{
		[SerializeField]
		private byte val;

		public ByteBitArray(byte v) => val=v;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool this[byte i]
		{
			get
			{
			#if UNITY_EDITOR
				if (i>=8)
					Debug.LogError("<color=red>ByteBitArray out of bounds!</color>");
			#endif
				return (val&(byte)(1<<i))!=0;
			}
			set
			{
				if (value)
					val|=(byte)(1<<i);
				else
					val&=(byte)(~(1<<i));
			}
		}

		public static byte ToByte(ByteBitArray bb) => bb.val;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public byte ToByte() => val;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromByte(byte b) => val=b;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void Set(ref byte v,byte pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=8)
				Debug.LogError("<color=red>ByteBitArray out of bounds!</color>");
		#endif
			if (b)
				v|=(byte)(1<<pos);
			else
				v&=(byte)(~(1<<pos));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Set(byte pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=8)
				Debug.LogError("<color=red>ByteBitArray out of bounds!</color>");
		#endif
			if (b)
				val|=(byte)(1<<pos);
			else
				val&=(byte)(~(1<<pos));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Invert(short pos)
		{
		#if UNITY_EDITOR
			if (pos>=8)
				Debug.LogError("<color=red>ByteBitArray out of bounds!</color>");
		#endif
			val^=(byte)(val&(1<<pos));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void SetMultiple(byte s,bool b)
		{
			if (b)
				val|=s;
			else
				val&=(byte)~s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static bool Get(ref byte v,int pos)
		{
		#if UNITY_EDITOR
			if (pos>=8)
				Debug.LogError("<color=red>ByteBitArray out of bounds!</color>");
		#endif
			return (v&(byte)(1<<pos))!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool Get(int pos)
		{
		#if UNITY_EDITOR
			if (pos>=8)
				Debug.LogError("<color=red>ByteBitArray out of bounds!</color>");
		#endif
			return (val&(byte)(1<<pos))!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override string ToString() => val.ToString();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromString(string s) => val=System.Convert.ToByte(s.Trim(),10);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Reset(bool b) => val=b ? (byte)255 : (byte)0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int CountTrue() => CountTrue(ref val);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static int CountTrue(ref byte v)
		{
			int count=0;
			for (int u=0;u<32;u++)
			{
				if (Get(ref v,u))
					count++;
			}
			return count;
		}
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool Any => val!=0;
	}
	#endregion
	#region SHORTBITARRAY
	[System.Serializable]
	public struct ShortBitArray
	{
		[SerializeField]
		private short val;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool this[int i]
		{
			get
			{
			#if UNITY_EDITOR
				if (i>=32)
					Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
			#endif
				return (val&(1<<i))!=0;
			}
			set
			{
				if (value)
					val|=(short)(1<<i);
				else
					val&=(short)(~(1<<i));
			}
		}

		public static short ToShort(ShortBitArray bb) => bb.val;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public short ToShort() => val;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromShort(short b) => val=b;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void Set(ref short v,int pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=16)
				Debug.LogError("<color=red>ShortBitArray out of bounds!</color>");
		#endif
			if (b)
				v|=(short)(1<<pos);
			else
				v&=(short)(~(1<<pos));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Set(int pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=16)
				Debug.LogError("<color=red>ShortBitArray out of bounds!</color>");
		#endif
			if (b)
				val|=(short)(1<<pos);
			else
				val&=(short)(~(1<<pos));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Invert(short pos)
		{
		#if UNITY_EDITOR
			if (pos>=16)
				Debug.LogError("<color=red>ShortBitArray out of bounds!</color>");
		#endif
			val^=(short)(val&(1<<pos));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void SetMultiple(short s,bool b)
		{
			if (b)
				val|=s;
			else
				val&=(short)~s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static bool Get(ref short v,int pos)
		{
		#if UNITY_EDITOR
			if (pos>=16)
				Debug.LogError("<color=red>ShortBitArray out of bounds!</color>");
		#endif
			return (v&(1<<pos))!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool Get(int pos)
		{
		#if UNITY_EDITOR
			if (pos>=16)
				Debug.LogError("<color=red>ShortBitArray out of bounds!</color>");
		#endif
			return (val&(1<<pos))!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override string ToString() => val.ToString();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromString(string s) => val=System.Convert.ToInt16(s.Trim(),10);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Reset(bool b) => val=b ? (short)~0 : (short)0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int CountTrue() => CountTrue(ref val);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static int CountTrue(ref short v)
		{
			int count=0;
			for (int u=0;u<32;u++)
			{
				if (Get(ref v,u))
					count++;
			}
			return count;
		}
	}
	#endregion
	#region INTBITARRAY
	[System.Serializable]
	public struct IntBitArray
	{
		[SerializeField]
		private int val;

		public IntBitArray(int value) => val=value;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool this[int i]
		{
			get
			{
			#if UNITY_EDITOR
				if (i>=32)
					Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
			#endif
				return (val&(1<<i))!=0;
			}
			set
			{
				if (value)
					val|=1<<i;
				else
					val&=~(1<<i);
			}
		}

		public static int ToInt(IntBitArray bb) => bb.val;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int ToInt() => val;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromInt(int b) => val=b;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void Set(ref int v,int pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=32)
				Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
		#endif
			if (b)
				v|=1<<pos;
			else
				v&=~(1<<pos);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void SetMultiple(int s,bool b)
		{
			if (b)
				val|=s;
			else
				val&=~s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Set(int pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=32)
				Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
		#endif
			if (b)
				val|=1<<pos;
			else
				val&=~(1<<pos);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Invert(short pos)
		{
		#if UNITY_EDITOR
			if (pos>=32)
				Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
		#endif
			val^=val&(1<<pos);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static bool Get(ref int v,int pos)
		{
		#if UNITY_EDITOR
			if (pos>=32)
				Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
		#endif
			return (v&(1<<pos))!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool Get(int pos)
		{
		#if UNITY_EDITOR
			if (pos>=32)
				Debug.LogError("<color=red>IntBitArray out of bounds!</color>");
		#endif
			return (val&(1<<pos))!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override string ToString() => val.ToString();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromString(string s) => val=System.Convert.ToInt32(s.Trim(),10);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Reset(bool b) => val=b ? ~0 : 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int CountTrue() => CountTrue(ref val);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static int CountTrue(ref int v)
		{
			int count=0;
			for (int u=0;u<32;u++)
			{
				if (Get(ref v,u))
					count++;
			}
			return count;
		}
	}
	#endregion
	#region BITARRAY
	[System.Serializable]
	public class BitArray
	{
		public int Length;

		private uint[] array;
		private readonly int dimension;
		private static readonly uint[] mask={0x1,0x2,0x4,0x8,0x10,0x20,0x40,0x80,0x100,0x200,0x400,0x800,0x1000,0x2000,0x4000,0x8000,0x10000,0x20000,0x40000,0x80000,0x100000,0x200000,0x400000,0x800000,0x1000000,0x2000000,0x4000000,
											0x8000000,0x10000000,0x20000000,0x40000000,0x80000000};
		private static readonly uint[] invertedMask={0xfffffffe,0xfffffffd,0xfffffffb,0xfffffff7,0xffffffef,0xffffffdf,0xffffffbf,0xffffff7f,0xfffffeff,0xfffffdff,0xfffffbff,0xfffff7ff,0xffffefff,0xffffdfff,0xffffbfff,0xffff7fff,
													0xfffeffff,0xfffdffff,0xfffbffff,0xfff7ffff,0xffefffff,0xffdfffff,0xffbfffff,0xFF7FFFFF,0xFEFFFFFF,0xFDFFFFFF,0xFBFFFFFF,0xF7FFFFFF,0xEFFFFFFF,0xDFFFFFFF,0xBFFFFFFF,0x7FFFFFFF};

		public BitArray(int dim)
		{
			dimension=dim;
			int size=(dim>>5)+1;
			Length=dim;
			array=new uint[size];
		}
		public BitArray(bool[] arr)
		{
			dimension=arr.Length;
			int size=(dimension>>5)+1;
			Length=dimension;
			array=new uint[size];
			for(int i=0;i<Length;i++)
				Set(i,arr[i]);
		}

		public uint[] Array => array;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Gets or sets the <see cref="Drakkar.BitArray"/> with the specified i.
		/// </summary>
		/// <param name='i'>
		/// If set to <c>true</c> i.
		/// </param>
		public bool this[int i]
		{
			get
			{
			#if UNITY_EDITOR
				if (i>=dimension)
					Debug.LogError("<color=red>BitArray out of bounds!</color>");
			#endif
				return (array[i>>5]&mask[i&0x1F])!=0;
			}
			set
			{
			#if UNITY_EDITOR
				if (i>=dimension)
					Debug.LogError("<color=red>BitArray out of bounds!</color>");
			#endif
				if (value)
					array[i>>5]|=mask[i&0x1F];
				else
					array[i>>5]&=invertedMask[i&0x1F];
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Sets the bit "pos".
		/// </summary>
		/// <param name='pos'>
		/// Position.
		/// </param>
		/// <param name='b'>
		/// State
		/// </param>
		public void Set(int pos,bool b)
		{
		#if UNITY_EDITOR
			if (pos>=dimension)
				Debug.LogError("<color=red>BitArray out of bounds!</color>");
		#endif
			if (b)
				array[pos>>5]|=mask[pos&0x1F];
			else
				array[pos>>5]&=invertedMask[pos&0x1F];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Rurnt the "pos" bit state
		/// </summary>
		/// <param name='pos'>
		/// bit position.
		/// </param>
		public bool Get(int pos)
		{
		#if UNITY_EDITOR
			if (pos>=dimension)
				Debug.LogError("<color=red>BitArray out of bounds!</color>");
		#endif
			return (array[pos>>5]&mask[pos&0x1F])!=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override string ToString()
		{
			string s=Length.ToString("X");
			int l=array.Length;
			for (int u=0;u<l;u++)
				s+="#"+array[u].ToString("X");
			return s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromString(string s)
		{
			if (string.IsNullOrEmpty(s)) return;
			string[] ss=s.Split("#".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			Length=System.Convert.ToInt32(ss[0],16);
			int l=ss.Length;
			array=new uint[l-1];
			for (int u=1;u<l;u++)
				array[u-1]=System.Convert.ToUInt32(ss[u].Trim(),16);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Reset the array to the specified state
		/// </summary>
		/// <param name='b'>
		/// Reset state
		/// </param>
		public void Reset(bool b)
		{
			int l=array.Length;
			if (b)
				for (uint u=0;u<l;u++)
					array[u]=0xffffffff;
			else
				for (uint u=0;u<l;u++)
					array[u]=0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Counts how many "true" bits there are
		/// </summary>
		public int CountTrue()
		{
			int count=0;
			for (int u=0;u<Length;u++)
			{
				if (Get(u))
					count++;
			}
			return count;
		}
	}
	#endregion
	#region NIBBLEARRAY
	[System.Serializable]
	public class NibbleArray
	{
		public int Length;

		private uint[] array;
		private readonly int dimension;
		private int localPos;
		private int arrayPos;
		private static readonly int[] shiftMask={28,24,20,16,12,8,4,0};
		private static readonly uint[] invertedMask={0x0FFFFFFF,0xF0FFFFFF,0xFF0FFFFF,0xFFF0FFFF,0xFFFF0FFF,0xFFFFF0FF,0xFFFFFF0F,0xFFFFFFF0};

		public NibbleArray(int dim)
		{
			dimension=dim;
			int size=(dim>>3)+1;
			Length=dim;
			array=new uint[size];
		}

		public uint[] Array => array;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int this[int i]
		{
			get
			{
				localPos=i&0x7;
				arrayPos=i>>3;
				return (int)((array[arrayPos]>>shiftMask[localPos])&0xF);
			}
			set
			{
			#if UNITY_EDITOR
				if ((value&0xFFFFFFF0)!=0)
					Debug.LogError("<color=red>Nibble is out of bounds!</color>");
			#endif
				localPos=i&0x7;
				arrayPos=i>>3;
				array[arrayPos]&=invertedMask[localPos];
				array[arrayPos]|=(uint)value<<shiftMask[localPos];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Set(int pos,int value)
		{
		#if UNITY_EDITOR
			if ((value&0xFFFFFFF0)!=0)
				Debug.LogError("<color=red>Nibble is out of bounds!</color>");
		#endif
			localPos=pos&0x7;
			arrayPos=pos>>3;
			array[arrayPos]&=invertedMask[localPos];
			array[arrayPos]|=(uint)value<<shiftMask[localPos];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public int Get(int pos)
		{
			localPos=pos&0x7;
			arrayPos=pos>>3;
			return (int)((array[arrayPos]>>shiftMask[localPos])&0xF);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public override string ToString()
		{
			string s=Length.ToString("X");
			int l=array.Length;
			for (int u = 0;u<l;u++)
				s+="#"+array[u].ToString("X");
			return s;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void FromString(string s)
		{
			string[] ss=s.Split("#".ToCharArray(),System.StringSplitOptions.RemoveEmptyEntries);
			Length=System.Convert.ToInt32(ss[0],16);
			int l=ss.Length;
			array=new uint[l-1];
			for (int u = 1;u<l;u++)
				array[u-1]=System.Convert.ToUInt32(ss[u].Trim(),16);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Reset the array to the specified value
		/// </summary>
		/// <param name='value'>
		/// Reset state to value
		/// </param>
		public void Reset(int value)
		{
			int l=array.Length;
			if (value==0)
				for (uint u = 0;u<l;u++)
					array[u]=0;
			else
				for (int u = 0;u<dimension;u++)
					Set(u,value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Counts how many values != 0
		/// </summary>
		public int CountNonZero()
		{
			int count=0;
			for (int u=0;u<Length;u++)
			{
				if (Get(u)>0)
					count++;
			}
			return count;
		}
	}
	#endregion
	#endregion

	#region INTVECTORS
	[System.Serializable]
	public struct IntMinMax
	{
		public int Min,Max;
		public int Random => UnityEngine.Random.Range(Min,Max+1);
		public int Length => Max-Min;
	}
	#endregion

	#region DAMPENED VALUES
	/// <summary>
	/// A float value that changes in a dampened way
	/// </summary>
	public struct DampedFloat
	{
		public float Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DampedFloat(float b) => Value=b;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dampen(float newValue,float speed) => Value=Mathf.Lerp(Value,newValue,speed*DrakkarTime.deltaTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DampenExp(float newValue,float speed) => Value=newValue+(Value-newValue)*Mathf.Exp(-speed*DrakkarTime.deltaTime);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DampenGUI(float newValue,float speed) => Value=Mathf.Lerp(Value,newValue,speed*DrakkarTime.GUIdeltaTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DampenGUIExp(float newValue,float speed) => Value=newValue+(Value-newValue)*Mathf.Exp(-speed*DrakkarTime.GUIdeltaTime);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DampenGUI(float newValue,float speedUp,float speedDown) => Value=Mathf.LerpUnclamped(Value,newValue,DrakkarMath.ClampMax(((newValue>=Value) ? speedUp : speedDown)*DrakkarTime.GUIdeltaTime,1));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DampenGUIExp(float newValue,float speedUp,float speedDown) => Value=newValue+(Value-newValue)*Mathf.Exp(-(newValue>=Value ? speedUp : speedDown)*DrakkarTime.GUIdeltaTime);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dampen(float newValue,float speedUp,float speedDown) => Value=Mathf.LerpUnclamped(Value,newValue,DrakkarMath.ClampMax(((newValue>=Value) ? speedUp : speedDown)*DrakkarTime.deltaTime,1));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DampenExp(float newValue,float speedUp,float speedDown) => Value=newValue+(Value-newValue)*Mathf.Exp(-(newValue>=Value ? speedUp : speedDown)*DrakkarTime.deltaTime);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(float boo,DampedFloat bb) => boo==bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(float boo,DampedFloat bb) => boo!=bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator +(float boo,DampedFloat bb) => boo+bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator -(float boo,DampedFloat bb) => boo-bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator *(float boo,DampedFloat bb) => boo*bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator /(float boo,DampedFloat bb) => boo/bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) => (Value!=0)==(obj!=null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float(DampedFloat bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DampedFloat(float b) => new(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Value.ToString();
	}
	/// <summary>
	/// A quaternion that changes ina dampened way
	/// </summary>
	public struct DampedQuaternion
	{
		public Quaternion Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DampedQuaternion(Quaternion b) => Value=b;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dampen(Quaternion newValue,float speed) => Value=Quaternion.SlerpUnclamped(Value,newValue,DrakkarMath.ClampMax(speed*DrakkarTime.deltaTime,1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Quaternion(DampedQuaternion bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DampedQuaternion(Quaternion b) => new(b);
	}
	/// <summary>
	/// A Vector2 that changes ina dampened way
	/// </summary>
	public struct DampedVector2
	{
		public Vector2 Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DampedVector2(Vector2 b) => Value=b;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dampen(Vector2 newValue,float speed) => Value=Vector2.LerpUnclamped(Value,newValue,DrakkarMath.ClampMax(speed*DrakkarTime.deltaTime,1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector2(DampedVector2 bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DampedVector2(Vector2 b) => new(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator +(Vector2 boo,DampedVector2 bb) => boo+bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator -(Vector2 boo,DampedVector2 bb) => boo-bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator *(Vector2 boo,DampedVector2 bb) => boo*bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 operator /(Vector2 boo,DampedVector2 bb) => boo/bb.Value;
	}
	/// <summary>
	/// A Vector3 that changes ina dampened way
	/// </summary>
	public struct DampedVector3
	{
		public Vector3 Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DampedVector3(Vector3 b) => Value=b;

		public float x => Value.x;
		public float y => Value.y;
		public float z => Value.z;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dampen(Vector3 newValue,float speed) => Value=Vector3.LerpUnclamped(Value,newValue,DrakkarMath.ClampMax(speed*DrakkarTime.deltaTime,1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector3(DampedVector3 bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator DampedVector3(Vector3 b) => new(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator +(Vector3 boo,DampedVector3 bb) => boo+bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator -(Vector3 boo,DampedVector3 bb) => boo-bb.Value;
	}
	#endregion
	#region MUTABLE VALUES
	/// <summary>
	/// A value that returns true if it has changed during the last operation (Set, Add, Multiply, Divide)
	/// </summary>
	public struct MutableFloat
	{
		public float Value;

		private float lastValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MutableFloat(float b)
		{
			Value=b;
			lastValue=b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float(MutableFloat bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Set(float newVal)
		{
			Value=newVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Add(float addVal)
		{
			Value+=addVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Multiply(float mulVal)
		{
			Value*=mulVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Divide(float divVal)
		{
			Value/=divVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(float boo,MutableFloat bb) => boo==bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(float boo,MutableFloat bb) => boo!=bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator +(float boo,MutableFloat bb) => boo+bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator -(float boo,MutableFloat bb) => boo-bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator *(float boo,MutableFloat bb) => boo*bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float operator /(float boo,MutableFloat bb) => boo/bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) => (Value!=0)==(obj!=null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Value.GetHashCode();
	}

	/// <summary>
	/// A value that returns true if it has changed during the last operation (Set, And, Or)
	/// </summary>
	public struct MutableBool
	{
		public bool Value;

		private bool lastValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MutableBool(bool b)
		{
			Value=b;
			lastValue=b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(MutableBool bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Set(bool newVal)
		{
			Value=newVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool And(bool andVal)
		{
			Value&=andVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Or(bool orVal)
		{
			Value|=orVal;
			bool res=lastValue!=Value;
			lastValue=Value;
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(bool boo,MutableBool bb) => boo==bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(bool boo,MutableBool bb) => boo!=bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator &(bool boo,MutableBool bb) => boo && bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator |(bool boo,MutableBool bb) => boo || bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !(MutableBool bb) => !bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) => Value==(obj!=null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Value.GetHashCode();
	}
	#endregion
	#region COUNTER VALUES
	/// <summary>
	/// A value that returns true when its value becomes zero.
	/// </summary>
	public struct IntCounter
	{
		public int Value;

		public IntCounter(int v) => Value=v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(int boo,IntCounter bb) => boo==bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(int boo,IntCounter bb) => boo!=bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Inc()
		{
			bool r=Value==0;
			Value++;
			return r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Dec()
		{
			Value--;
			bool r=Value==0;
			Value=DrakkarMath.Clamp0(Value);
			return r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) => (Value!=0)==(obj!=null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Value.ToString();
	}

	/// <summary>
	/// A value that returns true when its value becomes zero (or less).
	/// </summary>
	public struct FloatCounter
	{
		public float Value;

		public FloatCounter(float v) => Value=v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(float boo,FloatCounter bb) => boo==bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(float boo,FloatCounter bb) => boo!=bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Inc(float f)
		{
			f=DrakkarMath.Clamp0(f);
			bool r=Value==0 && f>0;
			Value+=f;
			return r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Dec(float f)
		{
			Value-=f;
			bool r=Value<=0;
			Value=DrakkarMath.Clamp0(Value);
			return r;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) => Value!=0==(obj!=null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Value.ToString();
	}

	public struct FadingCounter
	{
		public float Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FadingCounter(float v) => Value=v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Fade(float v)
		{
			if (Value<0)
			{
				v=-v;
				if (Value<=v)
				{
					Value-=v;
					return v;
				}
				else if (Value<0)
				{
					float r=Value;
					Value=0;
					return r;
				}
			}
			else if (Value>=v)
			{
				Value-=v;
				return v;
			}
			else if (Value>0)
			{
				float r=Value;
				Value=0;
				return r;
			}
			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float(FadingCounter bb) => bb.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Value.ToString();
	}
	#endregion
	#region ACTION COUNTER
	/// <summary>
	/// Triggers an Action when the value gets to Max
	/// </summary>
	public struct ActionCounter
	{
		public ActionCounter(int Max,System.Action action)
		{
			max=Max;
			Value=0;
			act=action;
		}
		public int Value;
		private int max;
		private System.Action act;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset() => Value=0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Inc()
		{
			Value++;
			if (Value==max)
				act();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dec() => Value=Mathf.Max(0,Value-1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj) => Value!=0==(obj!=null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => Value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => Value.ToString();
	}
	#endregion
	#region SWING INPUT
	/// <summary>
	/// Handles the situation when 2 alternating inputs are required.
	/// </summary>
	public struct SwingInput
	{
		private bool state;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset() => state=false;
		/// <summary>
		/// Checks if the 2 inputs are executed in sequence without repetitions
		/// </summary>
		/// <param name="input1"></param>
		/// <param name="input2"></param>
		/// <returns>TRUE if the input changes every time the function is called</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Check(bool input1,bool input2)
		{
			if (input1 ^ input2)
			{
				if (input1)
				{
					if (!state)
					{
						state=true;
						return true;
					}
				}
				else
				{
					if (state)
					{
						state=false;
						return true;
					}
				}
			}
			return false;
		}
	}
	#endregion
	#region CONVERSION
	/// <summary>
	/// Class to read or write a float as it if were an int
	/// </summary>
	public static class FloatFromInt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static float Get(int value)
		{
			float* p=(float*)&value;
			return *p;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int Set(float value)
		{
			int* p=(int*)&value;
			return *p;
		}
	}
	/// <summary>
	/// Class to read or write an int as it if were a float
	/// </summary>
	public static class IntFromFloat
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int Get(float value)
		{
			int* p=(int*)&value;
			return *p;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static float Set(int value)
		{
			float* p=(float*)&value;
			return *p;
		}
	}
	#endregion

	#region TIME RELATED DATA TYPES
	#region TIME RANGE EVENT
	/// <summary>
	/// Returns TRUE if enough time has passed since the last call
	/// </summary>
	public class TimeRangeEvent
	{
		public enum TimeMode { Realtime,LevelTime }

		private TimeMode mode=TimeMode.Realtime;
		private double timeRange;
		private double lastTime=-10000000;

		public TimeRangeEvent(double range) => timeRange=range;
		public TimeRangeEvent(double range,TimeMode md)
		{
			timeRange=range;
			mode=md;
		}

		/// <summary>
		/// Sets the TimeRange
		/// </summary>
		public double TimeRange { set => timeRange=(double)value; }

		/// <summary>
		/// Checks if the TimeRangeEvent is valid and if so, sets a new "now"
		/// </summary>
		/// <param name="tre"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static implicit operator bool(TimeRangeEvent tre)
		{
			double now=tre.mode switch
			{
				TimeMode.LevelTime => DrakkarTime.timeSinceLevelLoadDouble,
				_                  => DrakkarTime.realTimeDouble,
			};
			double threshold=tre.lastTime+tre.timeRange;
			if (threshold<=now)
			{
				tre.lastTime=now;
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Returns true if the last time the TimeRangeEvent was changed is still valid
		/// </summary>
		public bool LastValid
		{
			get
			{
				double now=mode switch
				{
					TimeMode.LevelTime => DrakkarTime.timeSinceLevelLoadDouble,
					_                  => DrakkarTime.realTimeDouble,
				};
				double threshold=lastTime+timeRange;
				return threshold<=now;
			}
		}

		/// <summary>
		/// Resets all
		/// </summary>
		public void Reset() => lastTime=-1000000;

		/// <summary>
		/// Sets the "now" to the current time
		/// </summary>
		public void Now() => lastTime=mode switch
											{
												TimeMode.LevelTime => DrakkarTime.timeSinceLevelLoadDouble,
												_                  => DrakkarTime.realTimeDouble,
											};
		public void Now(float range)
		{
			timeRange=range;
			lastTime=mode switch
			{
				TimeMode.LevelTime => DrakkarTime.timeSinceLevelLoadDouble,
				_                  => DrakkarTime.realTimeDouble,
			};
		}
	}
	#endregion
	#region TIMED CONDITION
	/// <summary>
	/// Returns TRUE if the condition is TRUE for a specified time range
	/// </summary>
	public struct TimedCondition
	{
		public TimeSlices TimeSlice;
		public float floatValue;

		private float curTime;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			curTime=0;
			floatValue=0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool BoolConditionLoop(bool cond,float time)
		{
			if (curTime>=time)
			{
				curTime=-1000000;
				return true;
			}

			if (cond)
				curTime+=DrakkarTime.GetTimeSlice(TimeSlice);
			else
				curTime=0;

			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool BoolCondition(bool cond,float time)
		{
			if (cond)
				curTime+=DrakkarTime.GetTimeSlice(TimeSlice);
			else
				curTime=0;

			return curTime>=time;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float FloatConditionLoop(bool cond,float time,bool resetOnFalse=true)
		{
			if (cond && curTime>=time)
			{
				curTime=time;
				floatValue=1;
				return 1;
			}

			float dt=DrakkarTime.GetTimeSlice(TimeSlice);
			if (cond)
				curTime+=dt;
			else
			{
				if (resetOnFalse)
					curTime=0;
				else
					curTime=(curTime<dt) ? 0 : curTime;
			}

			floatValue=curTime/time;
			return floatValue;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float FloatCondition(bool cond,float time,float returnSpeed=1)
		{
			if (cond && curTime>=time)
			{
				curTime=time;
				floatValue=1;
				return 1;
			}

			if (cond)
				curTime+=DrakkarTime.GetTimeSlice(TimeSlice);
			else
			{
				curTime-=DrakkarTime.GetTimeSlice(TimeSlice)*returnSpeed;
				if (curTime<0)
					curTime=0;
			}

			floatValue=curTime/time;
			return floatValue;
		}
	}
	#endregion
	#endregion
}