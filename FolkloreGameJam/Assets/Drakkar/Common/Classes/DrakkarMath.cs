using System;
using UnityEngine;
#if DRAKKAR
using Drakkar.Path;
#endif
using Unity.IL2CPP.CompilerServices;
using System.Runtime.CompilerServices;
using System.Globalization;
using UnityEngine.UIElements;

namespace Drakkar
{
	public static class DrakkarMath
	{
		public readonly static Vector3 Vector3Zero=Vector3.zero;
		public readonly static Vector3 Vector3One=Vector3.one;
		public readonly static Vector2 Vector2Zero=Vector2.zero;
		public readonly static Quaternion QIdentity=Quaternion.identity;
		public readonly static Vector3 Vector3Up=new(0,1,0);

		private static Vector3 v3,vv3;
		private static float length;
		#region CATMUL-ROM TABLES
		private static Vector4   CR_Half    =  new(-62.5f*0.001f,562.5f*0.001f,562.5f*0.001f,-62.5f*0.001f);
		private static Vector4[] CR_Thirds  ={ new(-74.1f*0.001f,777.8f*0.001f,333.3f*0.001f,-37.0f*0.001f)
											  ,new(-37.0f*0.001f,333.3f*0.001f,777.8f*0.001f,-74.1f*0.001f) };
		private static Vector4[] CR_Fourths ={ new(-70.3f*0.001f,867.2f*0.001f,226.6f*0.001f,-23.4f*0.001f)
											  ,new(-62.5f*0.001f,562.5f*0.001f,562.5f*0.001f,-62.5f*0.001f)
											  ,new(-23.4f*0.001f,226.6f*0.001f,867.2f*0.001f,-70.3f*0.001f) };
		private static Vector4[] CR_Fifths  ={ new(-64.0f*0.001f,912.0f*0.001f,168.0f*0.001f,-16.0f*0.001f)
											  ,new(-72.0f*0.001f,696.0f*0.001f,424.0f*0.001f,-48.0f*0.001f)
											  ,new(-48.0f*0.001f,424.0f*0.001f,696.0f*0.001f,-72.0f*0.001f)
											  ,new(-16.0f*0.001f,168.0f*0.001f,912.0f*0.001f,-64.0f*0.001f) };
		private static Vector4[] CR_Sevenths={ new(-0.05248f,0.95335f,0.10787f,-0.00875f),
											   new(-0.07289f,0.83090f,0.27114f,-0.02915f),
											   new(-0.06997f,0.65889f,0.46356f,-0.05248f),
											   new(-0.05248f,0.46356f,0.65889f,-0.06997f),
											   new(-0.02915f,0.27114f,0.83090f,-0.07289f),
											   new(-0.00875f,0.10787f,0.95335f,-0.05248f) };
		private static Vector4[] CR_Sixths  ={ new(-0.05787f,0.93750f,0.13194f,-0.01157f),
											   new(-0.07407f,0.77778f,0.33333f,-0.03704f),
											   new(-0.06250f,0.56250f,0.56250f,-0.06250f),
											   new(-0.03704f,0.33333f,0.77778f,-0.07407f),
											   new(-0.01157f,0.13194f,0.93750f,-0.05787f) };
		private static Vector4[] CR_Eighths ={ new(-0.04785f,0.96387f,0.09082f,-0.00684f),
											   new(-0.07031f,0.86719f,0.22656f,-0.02344f),
											   new(-0.07324f,0.72754f,0.38965f,-0.04395f),
											   new(-0.06250f,0.56250f,0.56250f,-0.06250f),
											   new(-0.04395f,0.38965f,0.72754f,-0.07324f),
											   new(-0.02344f,0.22656f,0.86719f,-0.07031f),
											   new(-0.00684f,0.09082f,0.96387f,-0.04785f) };
		private static Vector4[] CR_Nineths ={ new(-0.04390f,0.97119f,0.07819f,-0.00549f),
											   new(-0.06722f,0.89300f,0.19342f,-0.01920f),
											   new(-0.07407f,0.77778f,0.33333f,-0.03704f),
											   new(-0.06859f,0.63786f,0.48560f,-0.05487f),
											   new(-0.05487f,0.48560f,0.63786f,-0.06859f),
											   new(-0.03704f,0.33333f,0.77778f,-0.07407f),
											   new(-0.01920f,0.19342f,0.89300f,-0.06722f),
											   new(-0.00549f,0.07819f,0.97119f,-0.04390f) };
		private static Vector4[] CR_Tenths  ={ new(-0.04050f,0.97650f,0.06850f,-0.00450f)
											  ,new(-0.06400f,0.91200f,0.16800f,-0.01600f)
											  ,new(-0.07350f,0.81550f,0.28950f,-0.03150f)
											  ,new(-0.07200f,0.69600f,0.42400f,-0.04800f)
											  ,new(-0.06250f,0.56250f,0.56250f,-0.06250f)
											  ,new(-0.04800f,0.42400f,0.69600f,-0.07200f)
											  ,new(-0.03150f,0.28950f,0.81550f,-0.07350f)
											  ,new(-0.01600f,0.16800f,0.91200f,-0.06400f)
											  ,new(-0.00450f,0.06850f,0.97650f,-0.04050f) };
		private static Vector4 CR_Precalc;
		#endregion
	#if UNITY_EDITOR
		public static string GetCatmullCoefficients(int steps)
		{
			CultureInfo ci=CultureInfo.CreateSpecificCulture("en-GB");
			string s=string.Empty;
			string format="0.00000";
			float scarto=1f/(float)steps;
			for (int i=1;i<steps;i++)
			{
				Vector4 val=DrakkarMath.GetCatmullRomValues(scarto*i);
				s+=val.x.ToString(format,ci)+"f,"+val.y.ToString(format,ci)+"f,"+val.z.ToString(format,ci)+"f,"+val.w.ToString(format,ci)+"f\n";
			}
			return s;
		}
	#endif

		#region CLAMPS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp0(float f) => (f<0) ? 0 : f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp0(int f) => (f<0) ? 0 : f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ClampMin(int f,int min) => (f<min) ? min : f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ClampMin(float f,float min) => (f<min) ? min : f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ClampMax(float f,float max) => (f>max) ? max : f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ClampMax(int f,int max) => (f>max) ? max : f;
		#endregion
		#region INTERPOLATION
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float LerpUnclamped(float a,float b,float t) => a+(b-a)*t;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float InverseLerpUnclamped(float a,float b,float value) => (value-a)/(b-a);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SymmetricRamp(float t)
		{
			t=Mathf.Clamp01(t);
			return t<=0.5f? 2*t : 2*(1f-t);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SymmetricRampUnclamped(float t) => t<=0.5f? 2*t : 2*(1f-t);
		#region CURVES INTERPOLATION
		/// <summary>
		/// Catmullrom over position and rotation of a transform based on X axis
		/// </summary>
		/// <param name='Point1'>
		/// Point1.
		/// </param>
		/// <param name='Point2'>
		/// Point2.
		/// </param>
		/// <param name='t'>
		/// intermediate position (0-1)
		/// </param>
		/// <param name='target'>
		/// Target.
		/// </param>
		public static void CatmullRom_Xaxis(Transform Point1, Transform Point2, float t, Transform target)
		{
			Vector3 p1=Point1.position;
			Vector3 p2=Point2.position;
			float d=2*Vector3.Distance(p1,p2);

			target.SetPositionAndRotation(PointOnCurve(p1-(Point1.right*d),p1,p2,p2+(Point2.right*d),t),Quaternion.LerpUnclamped(Point1.rotation,Point2.rotation,t));
		}

		static float t0,t1,t2,t3;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector4 GetCatmullRomValues(float t) => new(((-t+2f)*t-1f)*t*0.5f,(((3f*t-5f)*t)*t+2f)*0.5f,((-3f*t+4f)*t+1f)*t*0.5f,((t-1f)*t*t)*0.5f);

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 PointOnCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			Vector3 result;

			t0 = ((-t+2f)*t-1f)*t*0.5f;
			t1 = (((3f*t-5f)*t)*t+2f)*0.5f;
			t2 = ((-3f*t+4f)*t+1f)*t*0.5f;
			t3 = ((t-1f)*t*t)*0.5f;

			result.x=p0.x*t0+p1.x*t1+p2.x*t2+p3.x*t3;
			result.y=p0.y*t0+p1.y*t1+p2.y*t2+p3.y*t3;
			result.z=p0.z*t0+p1.z*t1+p2.z*t2+p3.z*t3;

			return result;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,float t,out Vector3 result)
		{
			t0=((-t+2f)*t-1f)*t*0.5f;
			t1=(((3f*t-5f)*t)*t+2f)*0.5f;
			t2=((-3f*t+4f)*t+1f)*t*0.5f;
			t3=((t-1f)*t*t)*0.5f;

			result.x=p0.x*t0+p1.x*t1+p2.x*t2+p3.x*t3;
			result.y=p0.y*t0+p1.y*t1+p2.y*t2+p3.y*t3;
			result.z=p0.z*t0+p1.z*t1+p2.z*t2+p3.z*t3;
		}
		#region X2
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Half(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,out Vector3 result)
		{
			result.x=p0.x*CR_Half.x+p1.x*CR_Half.y+p2.x*CR_Half.z+p3.x*CR_Half.w;
			result.y=p0.y*CR_Half.x+p1.y*CR_Half.y+p2.y*CR_Half.z+p3.y*CR_Half.w;
			result.z=p0.z*CR_Half.x+p1.z*CR_Half.y+p2.z*CR_Half.z+p3.z*CR_Half.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Half(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result)
		{
			result.x=p0.x*CR_Half.x+p1.x*CR_Half.y+p2.x*CR_Half.z+p3.x*CR_Half.w;
			result.y=p0.y*CR_Half.x+p1.y*CR_Half.y+p2.y*CR_Half.z+p3.y*CR_Half.w;
			result.z=p0.z*CR_Half.x+p1.z*CR_Half.y+p2.z*CR_Half.z+p3.z*CR_Half.w;
		}
		#endregion
		#region X3
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Third(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int third,out Vector3 result)
		{
			CR_Precalc=CR_Thirds[third];

			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Third(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int third,out Vector3 result)
		{
			CR_Precalc=CR_Thirds[third];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Thirds(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2)
		{
			CR_Precalc=CR_Thirds[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Thirds[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X4
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Fourth(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int fourth,out Vector3 result)
		{
			CR_Precalc=CR_Fourths[fourth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Fourths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int fourth,out Vector3 result)
		{
			CR_Precalc=CR_Fourths[fourth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Fourths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3)
		{
			CR_Precalc=CR_Fourths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Fourths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Fourths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Fifth(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Fifths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Fifth(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Fifths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Fifths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3,out Vector3 result4)
		{
			CR_Precalc=CR_Fifths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Fifths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Fifths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Fifths[3];
			result4.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result4.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result4.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X6
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Sixths(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Sixths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Sixths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Sixths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Sixths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3,out Vector3 result4,out Vector3 result5)
		{
			CR_Precalc=CR_Sixths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sixths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sixths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sixths[3];
			result4.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result4.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result4.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sixths[4];
			result5.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result5.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result5.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X7
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Sevenths(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Sevenths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Sevenths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Sevenths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Sevenths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3,out Vector3 result4,out Vector3 result5,out Vector3 result6)
		{
			CR_Precalc=CR_Sevenths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sevenths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sevenths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sevenths[3];
			result4.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result4.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result4.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sevenths[4];
			result5.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result5.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result5.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Sevenths[5];
			result6.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result6.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result6.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X8
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Eighths(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Eighths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Eighths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Eighths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Eighths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3,out Vector3 result4,out Vector3 result5,out Vector3 result6,out Vector3 result7)
		{
			CR_Precalc=CR_Eighths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Eighths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Eighths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Eighths[3];
			result4.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result4.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result4.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Eighths[4];
			result5.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result5.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result5.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Eighths[5];
			result6.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result6.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result6.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Eighths[6];
			result7.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result7.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result7.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X9
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Nineths(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Nineths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Nineths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int fifth,out Vector3 result)
		{
			CR_Precalc=CR_Nineths[fifth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Nineths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3,out Vector3 result4,out Vector3 result5,out Vector3 result6,out Vector3 result7,out Vector3 result8)
		{
			CR_Precalc=CR_Nineths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[3];
			result4.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result4.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result4.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[4];
			result5.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result5.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result5.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[5];
			result6.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result6.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result6.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[6];
			result7.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result7.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result7.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Nineths[7];
			result8.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result8.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result8.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#region X10
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Tenths(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int tenth,out Vector3 result)
		{
			CR_Precalc=CR_Tenths[tenth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Tenths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,int tenth,out Vector3 result)
		{
			CR_Precalc=CR_Tenths[tenth];
			result.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void PointOnCurve_Tenths(ref Vector3 p0,ref Vector3 p1,ref Vector3 p2,ref Vector3 p3,out Vector3 result1,out Vector3 result2,out Vector3 result3,out Vector3 result4,out Vector3 result5,out Vector3 result6,out Vector3 result7,out Vector3 result8,out Vector3 result9)
		{
			CR_Precalc=CR_Tenths[0];
			result1.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result1.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result1.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[1];
			result2.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result2.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result2.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[2];
			result3.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result3.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result3.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[3];
			result4.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result4.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result4.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[4];
			result5.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result5.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result5.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[5];
			result6.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result6.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result6.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[6];
			result7.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result7.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result7.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[7];
			result8.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result8.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result8.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
			CR_Precalc=CR_Tenths[8];
			result9.x=p0.x*CR_Precalc.x+p1.x*CR_Precalc.y+p2.x*CR_Precalc.z+p3.x*CR_Precalc.w;
			result9.y=p0.y*CR_Precalc.x+p1.y*CR_Precalc.y+p2.y*CR_Precalc.z+p3.y*CR_Precalc.w;
			result9.z=p0.z*CR_Precalc.x+p1.z*CR_Precalc.y+p2.z*CR_Precalc.z+p3.z*CR_Precalc.w;
		}
		#endregion
		#endregion
		#endregion
		#region SELECT
		/// <summary>
		/// Returns the index of the range whose limits are defined by "ranges" (zero-based)
		/// </summary>
		/// <param name="val">the value to test</param>
		/// <param name="ranges">float array of thresholds (ascending order)</param>
		/// <param name="length">how many threshold there are</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static int SelectFromRange(float val,float[] ranges,int length)
		{
			for (int i=0;i<length;i++)
			{
				if (val<ranges[i])
					return i;
			}
			return length;
		}
		#endregion
		#region CLAMP
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float Clamp(float value,float min,float max)
		{
			if (value<min)
				return min;
			else if (value>max)
				return max;
			return value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static int Clamp(int value,int min,int max)
		{
			if (value<min)
				return min;
			else if (value>max)
				return max;
			return value;
		}
		#endregion
		#region VIEWPORT
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInsideViewport(Vector3 v) => (v.z>0)&&(v.x<=1)&&(v.x>=0)&&(v.y<=1)&&(v.y>=0);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 PointToViewport(Matrix4x4 VP,Vector3 world)
		{
			Vector3 point;
			point.x=VP.m00*world.x+VP.m01*world.y+VP.m02*world.z+VP.m03;
			point.y=VP.m10*world.x+VP.m11*world.y+VP.m12*world.z+VP.m13;
			point.z=VP.m30*world.x+VP.m31*world.y+VP.m32*world.z+VP.m33; //distance to the camera
			float num = 1f / point.z;
			point.x*=num;
			point.y*=num;
			return point;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInsideViewportSigned(float x,float y,float z) => (z>=0)&&(x<=1)&&(x>=-1)&&(y<=1)&&(y>=-1);
		#endregion
		#region ANGLES
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float SignedAngleX(Vector3 from_Normalized,Vector3 to_Normalized)
		{
			float num=Vector3Angle(from_Normalized,to_Normalized);
			float num2=from_Normalized.y*to_Normalized.z-from_Normalized.z*to_Normalized.y;
			return num*Mathf.Sign(num2);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float SignedAngleXFromForward(Vector3 to_Normalized)
		{
			float num=(float)Math.Acos(to_Normalized.z)*57.29578f;
			float num2=-1*to_Normalized.y;
			return num*Mathf.Sign(num2);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float SignedAngleYFromForwardToRight(Vector3 to_Normalized) => (float)Math.Acos(to_Normalized.z)*57.29578f*Mathf.Sign(to_Normalized.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float SignedAngleYFromRightToForward(Vector3 to_Normalized) => (float)Math.Acos(to_Normalized.y)*57.29578f*Mathf.Sign(to_Normalized.z);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float SignedAngleZFromUpToRight(Vector3 to_Normalized) => (float)Math.Acos(to_Normalized.y)*57.29578f*Mathf.Sign((float)(-1*to_Normalized.x));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float SignedAngleZFromRightToUp(Vector3 to_Normalized) => (float)Math.Acos(to_Normalized.x)*57.29578f*Mathf.Sign((float)(-1*to_Normalized.y));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Vector3Angle(Vector3 fromNormalized,Vector3 toNormalized)
		{
			float num2=Mathf.Clamp(Vector3.Dot(fromNormalized,toNormalized),-1f,1f);
			return (float)Math.Acos(num2)*57.29578f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float UniformAngle_old(float angle)
		{
			if (angle>180)
				angle=360-angle;
			if (angle<-180)
				angle+=360;
			return angle;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float UniformAngle(float angle)
		{
			if (angle>180)
				angle-=360;
			if (angle<-180)
				angle+=360;
			return angle;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 UniformAngle(Vector3 angles)
		{
			angles.x=UniformAngle(angles.x);
			angles.y=UniformAngle(angles.y);
			angles.z=UniformAngle(angles.z);
			return angles;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float ClampAngle_old(float angle,float min,float max) => Clamp(UniformAngle_old(angle),min,max);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float ClampAngle(float angle,float min,float max) => Clamp(UniformAngle(angle),min,max);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float AngleFromVector2(float threshold,ref Vector2 v) => v.sqrMagnitude<threshold ? Mathf.Infinity : Mathf.Atan2(v.x,-v.y)*Mathf.Rad2Deg;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleFromVector2(float x,float y) => Mathf.Atan2(x,-y)*Mathf.Rad2Deg;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Angle2DFromVector3(Vector3 dir) => Mathf.Atan2(dir.x,dir.z)*Mathf.Rad2Deg;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleSnap(float snapAngle,float angle) => (float)Mathf.Round(angle/snapAngle)*snapAngle;
		#endregion
		#region SIN & COS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PositiveSin(float angle) => Mathf.Sin(angle)*0.5f+0.5f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PositiveCos(float angle) => Mathf.Cos(angle)*0.5f+0.5f;
		#endregion
		#region VECTOR3
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 FaceNormal(ref Vector3 a,ref Vector3 b,ref Vector3 c) => Vector3.Cross(b-a,c-a).normalized;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 TransformDirection(ref Vector3 source,ref Matrix4x4 mat)
		{
			Matrix4x4MultByPoint3x3(ref mat,ref source,out Vector3 dir);
			return dir;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 TransformDirection(ref Vector3 source,Transform transform)
		{
			Matrix4x4 m=transform.localToWorldMatrix;
			Matrix4x4MultByPoint3x3(ref m,ref source,out Vector3 dir);
			return dir;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Manhattan(ref Vector3 a,ref Vector3 b) => Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y)+Mathf.Abs(a.z-b.z);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static void Vector3Lerp(ref Vector3 from,ref Vector3 to,out Vector3 dest,float t)
		{
			dest.x=from.x+(to.x-from.x)*t;
			dest.y=from.y+(to.y-from.y)*t;
			dest.z=from.z+(to.z-from.z)*t;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float Vector3Magnitude(ref Vector3 vector) => Mathf.Sqrt(vector.x*vector.x+vector.y*vector.y+vector.z*vector.z);

		/// <summary>
		/// Returns the normalized direction between the 2 vectors on the plane XZ
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 Vector3Direction2DNormalized(ref Vector3 dest,ref Vector3 source) => new Vector3(dest.x-source.x,0,dest.z-source.z).normalized;

		/// <summary>
		/// Returns the direction between the 2 vectors on the plane XZ
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 Vector3Direction2D(ref Vector3 dest,ref Vector3 source) => new Vector3(dest.x-source.x,0,dest.z-source.z);

		/// <summary>
		/// Returns the squared distance between 2 points on the plane XZ
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="dest"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SquaredDistance2D(Vector3 pos,Vector3 dest) => new Vector2(pos.x-dest.x,pos.z-dest.z).sqrMagnitude;

		#endregion
		#region VECTOR2
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Manhattan(ref Vector2 a,ref Vector2 b) => Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Vector2Scale(ref Vector2 dest,float t)
		{
			dest.x*=t;
			dest.y*=t;
		}
		/// <summary>
		/// Creates a Vector2 of magnitude 1 rotated of radians
		/// </summary>
		/// <param name="radians"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RotatedVector2(float radians) => new(-(float)Mathf.Sin(radians),(float)Mathf.Cos(radians));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Vector2Rotate(ref Vector2 dest,float degrees)
		{
			float radians=degrees*Mathf.Deg2Rad;
			float sin=Mathf.Sin(radians);
			float cos=Mathf.Cos(radians);
			float tx=dest.x;
			float ty=dest.y;

			dest.x=cos*tx-sin*ty;
			dest.y=sin*tx+cos*ty;
		}
		#endregion
		#region QUATERNIONS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 RotatedForward(Quaternion q)
		{
			float num   =q.x*2f;
			float num2  =q.y*2f;
			float num3  =q.z*2f;
			float num8  =q.x*num3;
			float num9  =q.y*num3;
			float num10 =q.w*num;
			float num11 =q.w*num2;
			return new(num8+num11,num9-num10,1f-(q.x*num+q.y*num2));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 RotatedUp(Quaternion q)
		{
			float num   =q.x*2f;
			float num2  =q.y*2f;
			float num3  =q.z*2f;
			float num4  =q.x*num;
			float num6  =q.z*num3;
			float num7  =q.x*num2;
			float num9  =q.y*num3;
			float num10 =q.w*num;
			float num12 =q.w*num3;
			return new(num7-num12,1f-(num4+num6),num9+num10);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion QuaternionXAxisRotation(float degrees)
		{
			float radians=degrees*Mathf.Deg2Rad*0.5f;
			return new()
			{
				x=(float)Math.Sin(radians),
				y=0,
				z=0,
				w=(float)Math.Cos(radians)
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion QuaternionYAxisRotation(float degrees)
		{
			float radians=degrees*Mathf.Deg2Rad*0.5f;
			return new()
			{
				x=0,
				y=(float)Math.Sin(radians),
				z=0,
				w=(float)Math.Cos(radians)
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion QuaternionZAxisRotation(float degrees)
		{
			float radians=degrees*Mathf.Deg2Rad*0.5f;
			return new()
			{
				x=0,
				y=0,
				z=(float)Math.Sin(radians),
				w=(float)Math.Cos(radians)
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion QuaternionMultiplyByXAxisRotation(Quaternion lhs,Quaternion rhs) => new(lhs.w*rhs.x+lhs.x*rhs.w,lhs.y*rhs.w+lhs.z*rhs.x,lhs.z*rhs.w-lhs.y*rhs.x,lhs.w*rhs.w-lhs.x*rhs.x);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion QuaternionMultiplyByYAxisRotation(Quaternion lhs,Quaternion rhs) => new(lhs.x*rhs.w-lhs.z*rhs.y,lhs.w*rhs.y+lhs.y*rhs.w,lhs.z*rhs.w+lhs.x*rhs.y,lhs.w*rhs.w-lhs.y*rhs.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion QuaternionMultiplyByZAxisRotation(Quaternion lhs,Quaternion rhs) => new(lhs.x*rhs.w+lhs.y*rhs.z,lhs.y*rhs.w-lhs.x*rhs.z,lhs.w*rhs.z+lhs.z*rhs.w,lhs.w*rhs.w-lhs.z*rhs.z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float QuaternionEulerY(Quaternion rotation)
		{
			float sqw=rotation.w*rotation.w;
			float sqx=rotation.x*rotation.x;
			float sqy=rotation.y*rotation.y;
			float sqz=rotation.z*rotation.z;
			float unit=sqx+sqy+sqz+sqw;				// if normalised is one, otherwise is correction factor
			float test=rotation.x*rotation.w-rotation.y*rotation.z;

			if (test>0.4995f*unit)					// singularity at north pole
				return normalizeAngle(2f*Mathf.Atan2(rotation.y,rotation.x)*Mathf.Rad2Deg);

			if (test<-0.4995f*unit)					// singularity at south pole
				return normalizeAngle(-2f*Mathf.Atan2(rotation.y,rotation.x)*Mathf.Rad2Deg);

			Quaternion q=new(rotation.w,rotation.z,rotation.x,rotation.y);     // Yaw
			return normalizeAngle((float)Math.Atan2(2f*q.x*q.w+2f*q.y*q.z,1-2f*(q.z*q.z+q.w*q.w))*Mathf.Rad2Deg);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float normalizeAngle(float angle)
		{
			float modAngle=angle%360.0f;
			return modAngle<0 ? modAngle+360.0f : modAngle;
		}
		#endregion
		#region MATRICES
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4MultByForward3x4(ref Matrix4x4 mat,out Vector3 result)
		{
			result.x=mat.m02+mat.m03;
			result.y=mat.m12+mat.m13;
			result.z=mat.m22+mat.m23;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4MultByPoint3x4(ref Matrix4x4 mat,ref Vector3 point,out Vector3 result)
		{
			result.x=mat.m00*point.x+mat.m01*point.y+mat.m02*point.z+mat.m03;
			result.y=mat.m10*point.x+mat.m11*point.y+mat.m12*point.z+mat.m13;
			result.z=mat.m20*point.x+mat.m21*point.y+mat.m22*point.z+mat.m23;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4MultByPoint3x3(ref Matrix4x4 mat,ref Vector3 point,out Vector3 result)
		{
			result.x=mat.m00*point.x+mat.m01*point.y+mat.m02*point.z;
			result.y=mat.m10*point.x+mat.m11*point.y+mat.m12*point.z;
			result.z=mat.m20*point.x+mat.m21*point.y+mat.m22*point.z;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Matrix4x4GetPosition(ref Matrix4x4 m) => new(m.m03,m.m13,m.m23);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Matrix4x4UnscaledGetPositionAndForward(ref Matrix4x4 m,out Vector3 fwd)
		{
			fwd=new(m.m02,m.m12,m.m22);
			return new(m.m03,m.m13,m.m23);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Matrix4x4GetScale(ref Matrix4x4 m) => new(m.GetColumn(0).magnitude,m.GetColumn(1).magnitude,m.GetColumn(2).magnitude);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Quaternion Matrix4x4GetRotation(ref Matrix4x4 mat)
		{
			float num;
			Vector3 x=new Vector3(mat.m00,mat.m10,mat.m20).normalized;
			Vector3 y=new Vector3(mat.m01,mat.m11,mat.m21).normalized;
			Vector3 z=new Vector3(mat.m02,mat.m12,mat.m22).normalized;
			float tr=x.x+y.y+z.z;
			Quaternion quaternion;
			if (tr>0f)
			{
				num=Mathf.Sqrt(tr+1f);
				quaternion.w=num*0.5f;
				num=0.5f/num;
				quaternion.x=(y.z-z.y)*num;
				quaternion.y=(z.x-x.z)*num;
				quaternion.z=(x.y-y.x)*num;
				return quaternion;
			}
			if (x.x>=y.y && x.x>z.z)
			{
				num=Mathf.Sqrt(1f+x.x-y.y-z.z);
				quaternion.x=0.5f*num;
				num=0.5f/num;
				quaternion.y=(x.y+y.x)*num;
				quaternion.z=(x.z+z.x)*num;
				quaternion.w=(y.z-z.y)*num;
				return quaternion;
			}
			if (y.y>z.z)
			{
				num=Mathf.Sqrt(1f+y.y-x.x-z.z);
				quaternion.y=0.5f*num;
				num=0.5f/num;
				quaternion.x=(y.x+x.y)*num;
				quaternion.z=(z.y+y.z)*num;
				quaternion.w=(z.x-x.z)*num;
				return quaternion;
			}
			num=Mathf.Sqrt(1f+z.z-x.x-y.y);
			quaternion.z=0.5f*num;
			num=0.5f/num;
			quaternion.x=(z.x+x.z)*num;
			quaternion.y=(z.y+y.z)*num;
			quaternion.w=(x.y-y.x)*num;
			return quaternion;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Quaternion Matrix4x4UniformGetRotation(ref Matrix4x4 mat)
		{
			float num,l;
			Vector3 x=new(mat.m00,mat.m10,mat.m20);
			l=1/x.magnitude;
			x*=l;
			Vector3 y=new(mat.m01*l,mat.m11*l,mat.m21*l);
			Vector3 z=new(mat.m02*l,mat.m12*l,mat.m22*l);
			float tr=x.x+y.y+z.z;
			Quaternion quaternion;
			if (tr>0f)
			{
				num=Mathf.Sqrt(tr+1f);
				quaternion.w=num*0.5f;
				num=0.5f/num;
				quaternion.x=(y.z-z.y)*num;
				quaternion.y=(z.x-x.z)*num;
				quaternion.z=(x.y-y.x)*num;
				return quaternion;
			}
			if (x.x>=y.y && x.x>z.z)
			{
				num=Mathf.Sqrt(1f+x.x-y.y-z.z);
				quaternion.x=0.5f*num;
				num=0.5f/num;
				quaternion.y=(x.y+y.x)*num;
				quaternion.z=(x.z+z.x)*num;
				quaternion.w=(y.z-z.y)*num;
				return quaternion;
			}
			if (y.y>z.z)
			{
				num=Mathf.Sqrt(1f+y.y-x.x-z.z);
				quaternion.y=0.5f*num;
				num=0.5f/num;
				quaternion.x=(y.x+x.y)*num;
				quaternion.z=(z.y+y.z)*num;
				quaternion.w=(z.x-x.z)*num;
				return quaternion;
			}
			num=Mathf.Sqrt(1f+z.z-x.x-y.y);
			quaternion.z=0.5f*num;
			num=0.5f/num;
			quaternion.x=(z.x+x.z)*num;
			quaternion.y=(z.y+y.z)*num;
			quaternion.w=(x.y-y.x)*num;
			return quaternion;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Quaternion Matrix4x4UnscaledGetRotation(ref Matrix4x4 mat)
		{
			float num;
			float tr=mat.m00+mat.m11+mat.m22;
			Quaternion quaternion;
			if (tr>0f)
			{
				num=Mathf.Sqrt(tr+1f);
				quaternion.w=num*0.5f;
				num=0.5f/num;
				quaternion.x=(mat.m21-mat.m12)*num;
				quaternion.y=(mat.m02-mat.m20)*num;
				quaternion.z=(mat.m10-mat.m01)*num;
				return quaternion;
			}
			if (mat.m00>=mat.m11 && mat.m00>=mat.m22)
			{
				num=Mathf.Sqrt(1f+mat.m00-mat.m11-mat.m22);
				quaternion.x=0.5f*num;
				num=0.5f/num;
				quaternion.y=(mat.m10+mat.m01)*num;
				quaternion.z=(mat.m20+mat.m02)*num;
				quaternion.w=(mat.m21-mat.m12)*num;
				return quaternion;
			}
			if (mat.m11>mat.m22)
			{
				num=Mathf.Sqrt(1f+mat.m11-mat.m00-mat.m22);
				quaternion.y=0.5f*num;
				num=0.5f/num;
				quaternion.x=(mat.m01+mat.m10)*num;
				quaternion.z=(mat.m12+mat.m21)*num;
				quaternion.w=(mat.m02-mat.m20)*num;
				return quaternion;
			}
			num=Mathf.Sqrt(1f+mat.m22-mat.m00-mat.m11);
			quaternion.z=0.5f*num;
			num=0.5f/num;
			quaternion.x=(mat.m02+mat.m20)*num;
			quaternion.y=(mat.m12+mat.m21)*num;
			quaternion.w=(mat.m10-mat.m01)*num;
			return quaternion;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Quaternion Matrix4x4UnscaledGetPositionAndRotation(ref Matrix4x4 mat,out Vector3 pos)
		{
			float num;
			float tr=mat.m00+mat.m11+mat.m22;
			pos=new(mat.m03,mat.m13,mat.m23);
			Quaternion quaternion;
			if (tr>0f)
			{
				num=Mathf.Sqrt(tr+1f);
				quaternion.w=num*0.5f;
				num=0.5f/num;
				quaternion.x=(mat.m21-mat.m12)*num;
				quaternion.y=(mat.m02-mat.m20)*num;
				quaternion.z=(mat.m10-mat.m01)*num;
				return quaternion;
			}
			if (mat.m00>=mat.m11 && mat.m00>=mat.m22)
			{
				num=Mathf.Sqrt(1f+mat.m00-mat.m11-mat.m22);
				quaternion.x=0.5f*num;
				num=0.5f/num;
				quaternion.y=(mat.m10+mat.m01)*num;
				quaternion.z=(mat.m20+mat.m02)*num;
				quaternion.w=(mat.m21-mat.m12)*num;
				return quaternion;
			}
			if (mat.m11>mat.m22)
			{
				num=Mathf.Sqrt(1f+mat.m11-mat.m00-mat.m22);
				quaternion.y=0.5f*num;
				num=0.5f/num;
				quaternion.x=(mat.m01+mat.m10)*num;
				quaternion.z=(mat.m12+mat.m21)*num;
				quaternion.w=(mat.m02-mat.m20)*num;
				return quaternion;
			}
			num=Mathf.Sqrt(1f+mat.m22-mat.m00-mat.m11);
			quaternion.z=0.5f*num;
			num=0.5f/num;
			quaternion.x=(mat.m02+mat.m20)*num;
			quaternion.y=(mat.m12+mat.m21)*num;
			quaternion.w=(mat.m10-mat.m01)*num;
			return quaternion;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Quaternion Matrix4x4UnscaledGetPositionRotationAndForward(ref Matrix4x4 mat,out Vector3 pos,out Vector3 forward)
		{
			float num;
			float tr=mat.m00+mat.m11+mat.m22;
			pos=new(mat.m03,mat.m13,mat.m23);
			forward =new(mat.m02,mat.m12,mat.m22);
			Quaternion quaternion;
			if (tr>0f)
			{
				num=Mathf.Sqrt(tr+1f);
				quaternion.w=num*0.5f;
				num=0.5f/num;
				quaternion.x=(mat.m21-mat.m12)*num;
				quaternion.y=(mat.m02-mat.m20)*num;
				quaternion.z=(mat.m10-mat.m01)*num;
				return quaternion;
			}
			if (mat.m00>=mat.m11 && mat.m00>=mat.m22)
			{
				num=Mathf.Sqrt(1f+mat.m00-mat.m11-mat.m22);
				quaternion.x=0.5f*num;
				num=0.5f/num;
				quaternion.y=(mat.m10+mat.m01)*num;
				quaternion.z=(mat.m20+mat.m02)*num;
				quaternion.w=(mat.m21-mat.m12)*num;
				return quaternion;
			}
			if (mat.m11>mat.m22)
			{
				num=Mathf.Sqrt(1f+mat.m11-mat.m00-mat.m22);
				quaternion.y=0.5f*num;
				num=0.5f/num;
				quaternion.x=(mat.m01+mat.m10)*num;
				quaternion.z=(mat.m12+mat.m21)*num;
				quaternion.w=(mat.m02-mat.m20)*num;
				return quaternion;
			}
			num=Mathf.Sqrt(1f+mat.m22-mat.m00-mat.m11);
			quaternion.z=0.5f*num;
			num=0.5f/num;
			quaternion.x=(mat.m02+mat.m20)*num;
			quaternion.y=(mat.m12+mat.m21)*num;
			quaternion.w=(mat.m10-mat.m01)*num;
			return quaternion;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4GetAxes(ref Matrix4x4 mat,out Vector3 forward,out Vector3 up,out Vector3 right)
		{
			forward =new(mat.m02,mat.m12,mat.m22);
			up      =new(mat.m01,mat.m11,mat.m21);
			right   =new(mat.m00,mat.m10,mat.m20);
			forward.Normalize();
			up     .Normalize();
			right  .Normalize();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UnscaledGetAxes(ref Matrix4x4 mat,out Vector3 forward,out Vector3 up,out Vector3 right)
		{
			forward =new(mat.m02,mat.m12,mat.m22);
			up      =new(mat.m01,mat.m11,mat.m21);
			right   =new(mat.m00,mat.m10,mat.m20);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UnscaledGetUp(ref Matrix4x4 mat,out Vector3 up) => up=new(mat.m01,mat.m11,mat.m21);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UnscaledGetForward(ref Matrix4x4 mat,out Vector3 forward) => forward=new(mat.m02,mat.m12,mat.m22);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UnscaledGetForwardAndRight(ref Matrix4x4 mat,out Vector3 forward,out Vector3 right)
		{
			forward =new(mat.m02,mat.m12,mat.m22);
			right   =new(mat.m00,mat.m10,mat.m20);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UnscaledGetForwardAndUp(ref Matrix4x4 mat,out Vector3 forward,out Vector3 up)
		{
			forward =new(mat.m02,mat.m12,mat.m22);
			up		=new(mat.m01,mat.m11,mat.m21);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UnscaledGetUpAndRight(ref Matrix4x4 mat,out Vector3 up,out Vector3 right)
		{
			up    =new(mat.m01,mat.m11,mat.m21);
			right =new(mat.m00,mat.m10,mat.m20);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void Matrix4x4UniformScaledGetAxes(ref Matrix4x4 mat,out Vector3 forward,out Vector3 up,out Vector3 right)
		{
			forward =new(mat.m02,mat.m12,mat.m22);
			float n=forward.magnitude;
			forward/=n;
			up.x=mat.m01/n;
			up.y=mat.m11/n;
			up.z=mat.m21/n;
			right.x=mat.m00/n;
			right.y=mat.m10/n;
			right.z=mat.m20/n;
		}
		#endregion
		#region ROUNDING
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static int RoundToIntBy(float f,int i)
		{
			f=Mathf.Round(f/(float)i);
			return Mathf.RoundToInt(f*i);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float RoundBy(float f,int i)
		{
			f=Mathf.Round(f/(float)i);
			return Mathf.Round(f*i);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static int RoundBy(int f,int i)
		{
			f=f/i;
			return f*i;
		}
		#endregion
		#region NON-LINEAR
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SemiExponential(int val,float weight) => (int)Math.Floor(val*val*weight);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SemiExponentialClampedMin(int val,float weight,int clampMin) => ClampMin(Mathf.RoundToInt(val*val*weight),clampMin);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SemiExponentialClampedMin(int val,float weight,float clampMin) => ClampMin(val*val*weight,clampMin);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SemiExponentialClamped(int val,float weight,float clampMin,float clampMax) => Clamp(val*val*weight,clampMin,clampMax);
		#endregion
		#region BULLET PREDICTION
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetProjectileSpeedByHeight(float height,float gravity)
		{
		#if UNITY_EDITOR
			if (gravity<0)
				DrakkarEditor.NameAndReasonError("GetProjectileSpeedByHeight","Gravity must be positive");
		#endif
			return Mathf.Sqrt(2*gravity*height);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetProjectileTimeBySpeed(float speed,float gravity)
		{
		#if UNITY_EDITOR
			if (gravity<0)
				DrakkarEditor.NameAndReasonError("GetProjectileSpeedByHeight","Gravity must be positive");
		#endif
			return speed/gravity;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetProjectileTimeByHeight(float height,float gravity)
		{
		#if UNITY_EDITOR
			if (gravity<0)
				DrakkarEditor.NameAndReasonError("GetProjectileSpeedByHeight","Gravity must be positive");
		#endif
			return Mathf.Sqrt(2*height/gravity);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 FirstOrderIntercept(Vector3 shooterPosition,Vector3 shooterVelocity,float shotSpeed,Vector3 targetPosition,Vector3 targetVelocity)
		{
			Vector3 targetRelativePosition=targetPosition-shooterPosition;
			Vector3 targetRelativeVelocity=targetVelocity-shooterVelocity;
			float t=FirstOrderInterceptTime(shotSpeed,targetRelativePosition,targetRelativeVelocity);
			return targetPosition+t*targetRelativeVelocity;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float FirstOrderInterceptTime(float shotSpeed,Vector3 targetRelativePosition,Vector3 targetRelativeVelocity)
		{
			float velocitySquared=targetRelativeVelocity.sqrMagnitude;
			if (velocitySquared<0.001f)
				return 0f;

			float a=velocitySquared-shotSpeed*shotSpeed;
			float c=targetRelativePosition.sqrMagnitude;

			if (Mathf.Abs(a)<0.001f)
			{
				float t=-c/(2*Vector3.Dot(targetRelativeVelocity,targetRelativePosition));
				return Mathf.Max(t,0); //don't shoot back in time
			}

			float b=2*Vector3.Dot(targetRelativeVelocity,targetRelativePosition);
			float determinant=b*b-4*a*c;
			float a2=2*a;
			if (determinant>0) //determinant > 0; two intercept paths (most common)
			{
				float sqdet=Mathf.Sqrt(determinant);
				float t1=(-b+sqdet)/a2;
				float t2=(-b-sqdet)/a2;
				return (t1>0)?((t2>0) ? Mathf.Min(t1,t2) : t1):Mathf.Max(t2,0);
			}
			else if (determinant<0) //determinant < 0; no intercept path
				return 0;
			else //determinant=0; one intercept path, pretty much never happens
				return Mathf.Max(-b/a2,0); //don't shoot back in time
		}
		#endregion
		#region BEZIER
		public static float SmoothStep(float t) => t*t*(3-2*t);
		#region CUBIC
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateBezier(Vector3 p0,Vector3 p1,Vector3 cp0,Vector3 cp1,float t)
		{
			float oneMinusT=1f-t;
			float oneMinusT2=oneMinusT*oneMinusT;
			float t2=t*t;
			return (oneMinusT*oneMinusT2)*p0+3f*oneMinusT2*t*cp0+3f*oneMinusT*t2*cp1+t2*t*p1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateCubicBezier(Vector3[] p,float t)
		{
			float oneMinusT=1f-t;
			float oneMinusT2=oneMinusT*oneMinusT;
			float t2=t*t;
			return (oneMinusT*oneMinusT2)*p[0]+3f*oneMinusT2*t*p[1]+3f*oneMinusT*t2*p[2]+t2*t*p[3];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public unsafe static Vector3 CalculateCubicBezier(Vector3* p,float t)
		{
			float oneMinusT=1f-t;
			float oneMinusT2=oneMinusT*oneMinusT;
			float t2=t*t;
			return (oneMinusT*oneMinusT2)*p[0]+3f*oneMinusT2*t*p[1]+3f*oneMinusT*t2*p[2]+t2*t*p[3];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateBezierDerivative(Vector3 p0,Vector3 p1,Vector3 cp0,Vector3 cp1,float t)
		{
			float oneMinusT=1f-t;
			return (3f*oneMinusT*oneMinusT)*(cp0-p0)+6f*oneMinusT*t*(cp1-cp0)+3f*t*t*(p1-cp1);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateCubicBezierDerivative(Vector3[] p,float t)
		{
			float oneMinusT=1f-t;
			Vector3 p1=p[1],p2=p[2];
			return (3f*oneMinusT*oneMinusT)*(p1-p[0])+6f*oneMinusT*t*(p2-p1)+3f*t*t*(p[3]-p2);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public unsafe static Vector3 CalculateCubicBezierDerivative(Vector3* p,float t)
		{
			float oneMinusT=1f-t;
			Vector3 p1=p[1],p2=p[2];
			return (3f*oneMinusT*oneMinusT)*(p1-p[0])+6f*oneMinusT*t*(p2-p1)+3f*t*t*(p[3]-p2);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateCubicBezierAndDerivative(Vector3[] p,float t,out Vector3 derivative)
		{
			float oneMinusT=1f-t;
			float oneMinusT2=oneMinusT*oneMinusT;
			Vector3 p0=p[0],p1=p[1],p2=p[2],p3=p[3];
			float t2=t*t,oneMinusTX3=oneMinusT*3f;
			derivative=oneMinusTX3*oneMinusT*(p1-p0)+6f*oneMinusT*t*(p2-p1)+3f*t2*(p3-p2);
			return oneMinusT*oneMinusT2*p0+3f*oneMinusT2*t*p1+(oneMinusTX3*t2)*p2+t2*t*p3;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public unsafe static Vector3 CalculateCubicBezierAndDerivative(Vector3* p,float t,out Vector3 derivative)
		{
			float oneMinusT=1f-t;
			float oneMinusT2=oneMinusT*oneMinusT;
			Vector3 p0=p[0],p1=p[1],p2=p[2],p3=p[3];
			float t2=t*t,oneMinusTX3=oneMinusT*3f;
			derivative=oneMinusTX3*oneMinusT*(p1-p0)+6f*oneMinusT*t*(p2-p1)+3f*t2*(p3-p2);
			return oneMinusT*oneMinusT2*p0+3f*oneMinusT2*t*p1+oneMinusTX3*t2*p2+t2*t*p3;
		}
		#endregion
		#region QUADRATIC
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateBezier(Vector3 p0,Vector3 p1,Vector3 cp0,float t)
		{
			float oneMinusT=1f-t;
			return (oneMinusT*oneMinusT)*p0+2f*oneMinusT*t*cp0+t*t*p1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateQuadraticBezier(Vector3[] p,float t)
		{
			float oneMinusT=1f-t;
			return (oneMinusT*oneMinusT)*p[0]+2f*oneMinusT*t*p[1]+t*t*p[2];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public unsafe static Vector3 CalculateQuadraticBezier(Vector3* p,float t)
		{
			float oneMinusT=1f-t;
			return oneMinusT*oneMinusT*p[0]+2f*oneMinusT*t*p[1]+t*t*p[2];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateBezierDerivative(Vector3 p0,Vector3 p1,Vector3 cp0,float t)
		{
			float oneMinusT=1f-t;
			return -2f*oneMinusT*p0+(2f-4f*t)*cp0+2f*t*p1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static Vector3 CalculateQuadraticBezierDerivative(Vector3[] p,float t)
		{
			float oneMinusT=1f-t;
			return -2f*oneMinusT*p[0]+(2f-4f*t)*p[1]+(2f*t)*p[2];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public unsafe static Vector3 CalculateQuadraticBezierDerivative(Vector3* p,float t)
		{
			float oneMinusT=1f-t;
			return -2f*oneMinusT*p[0]+(2f-4f*t)*p[1]+2f*t*p[2];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public unsafe static Vector3 CalculateQuadraticBezierAndDerivative(Vector3* p,float t,out Vector3 derivative)
		{
			float oneMinusT=1f-t,oneMinusTX2=2f*oneMinusT;
			Vector3 p0=p[0],p1=p[1],p2=p[2];
			derivative=-oneMinusTX2*p0+(2f-4f*t)*p1+2f*t*p2;
			return oneMinusT*oneMinusT*p0+oneMinusTX2*t*p1+t*t*p2;
		}
		#endregion
		#endregion
		#region SLOPES
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion LookAtOnPlane(Vector3 dir,Vector3 normal) => Quaternion.LookRotation(Vector3.Cross(normal,Vector3.Cross(dir,normal)),normal);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		/// <summary>
		/// Returns the sliding angle of a slope
		/// </summary>
		public static float SlopeSlideAngle(Vector3 normal)
		{
			float n1=1-normal.y;
			float x=n1*normal.x;
			float z=n1*normal.z;
			return AngleFromVector2(x,z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		/// <summary>
		/// Returns the sliding direction vector of a slope (on the plane)
		/// </summary>
		public static Vector3 SlopeSlideDir(Vector3 normal)
		{
			Vector3 discesa;
			float n1=1-normal.y;
			discesa.x=n1*normal.x;
			discesa.y=0;
			discesa.z=n1*normal.z;
			return discesa.normalized;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Returns the sliding direction vector of a slope (on the slope's plane)
		/// </summary>
		public static Vector3 SlopeSlideVector(Vector3 normal) => Vector3.Cross(normal,Vector3.Cross(normal,Vector3Up));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		/// <summary>
		/// Returns the normalized sliding direction vector of a slope (on the slope's plane)
		/// </summary>
		public static Vector3 SlopeSlideVectorNormalized(Vector3 normal) => Vector3.Cross(normal,Vector3.Cross(normal,Vector3Up)).normalized;
		#endregion
		#region POINTS, LINES & PLANES
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 MidPoint(Vector3 a,Vector3 b) => a+(b-a)*0.5f;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector2 MidPoint(Vector2 a,Vector2 b) => a+(b-a)*0.5f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 Vector3FromVector2(Vector2 v,float y=0) => new(v.x,y,v.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector2 Vector2FromVector3(Vector3 v) => new(v.x,v.z);
		/// <summary>
		/// Returns the projection length of "point" on the A-B segment
		/// </summary>
		/// <param name="point"></param>
		/// <param name="segA"></param>
		/// <param name="segB"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float PointProjectionOnSegment(Vector3 point,Vector3 segA,Vector3 segB)
		{
			v3=(segB-segA).normalized;
			vv3=point-segA;
			return Vector3.Dot(vv3,v3);
		}
	#if DRAKKAR
		/// <summary>
		/// Returns the distance of "point" on Path from currentNode
		/// </summary>
		/// <param name="point"></param>
		/// <param name="nodes"></param>
		/// <param name="currentNode"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static float PointProjectionOnPath(Vector3 point,DrakkarPathNode[] nodes,int currentNode)
		{
			float f=nodes[currentNode].Length;
			v3=nodes[currentNode].forward;
			vv3=point-nodes[currentNode].point;
			if (f!=0)
				f=1/f;
			vv3*=f;
			return Vector3.Dot(vv3,v3);
		}
	#endif
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Return the position of the projection of a point on a line.
		/// </summary>
		/// <param name="linePointA">Point on the Line</param>
		/// <param name="linePointB">Point on the Line</param>
		/// <param name="point">Point to project on the line</param>
		public static Vector3 PointLineProjection(Vector3 linePointA,Vector3 linePointB,Vector3 point)
		{
			Vector3 ab=linePointB-linePointA;
			Vector3 ac=point-linePointA;
			float abM=ab.magnitude;
			Vector3 abN=ab/abM;
			float acm=ac.magnitude;
			float d=Vector3.Dot(abN,ac/acm);
			return linePointA+ab*(d*(acm/abM));

		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Returns the shortest distance bewteen a point and a line.
		/// </summary>
		/// <param name="linePointA">Point on the Line</param>
		/// <param name="linePointB">Point on the Line</param>
		/// <param name="point">Point to get the distance from</param>
		public static float PointLineDistance(Vector3 linePointA,Vector3 linePointB,Vector3 point)
		{
			Vector3 ab=linePointB-linePointA;
			Vector3 ac=point-linePointA;
			return Vector3.Cross(ab,ac).magnitude/ab.magnitude;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Returns if a point is on a line.
		/// </summary>
		/// <param name="linePointA">Point on the Line</param>
		/// <param name="linePointB">Point on the Line</param>
		/// <param name="point">Point to test if it's on the line</param>
		public static bool IsPointOnLine(Vector3 linePointA,Vector3 linePointB,Vector3 point)
		{
			Vector3 ab=linePointB-linePointA;
			Vector3 ac=point-linePointA;
			return 1-Mathf.Abs(Vector3.Dot(ab.normalized,ac.normalized))<=float.Epsilon;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// The intersection point between two line.
		/// </summary>
		/// <param name="lineAPointA">Point on first line</param>
		/// <param name="lineAPointB">Point on first line</param>
		/// <param name="lineBPointA">Point on second line</param>
		/// <param name="lineBPointB">Point on second line</param>
		public static Vector3 LineLineIntersection(Vector3 lineAPointA,Vector3 lineAPointB,Vector3 lineBPointA,Vector3 lineBPointB)
		{
			Vector3 a=lineAPointB-lineAPointA;
			Vector3 b=lineBPointB-lineBPointA;
			Vector3 c=lineBPointA-lineAPointA;
			Vector3 crossA=Vector3.Cross(a,b);
			Vector3 crossB=Vector3.Cross(b,c);
			return lineAPointA+a*(Vector3.Dot(crossB,crossA)/Mathf.Pow(crossA.magnitude,2));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Distance between two skewed lines.
		/// </summary>
		/// <param name="lineAPointA">Point on first line</param>
		/// <param name="lineAPointB">Point on first line</param>
		/// <param name="lineBPointA">Point on second line</param>
		/// <param name="lineBPointB">Point on second line</param>
		public static float LineLineDistance(Vector3 lineAPointA,Vector3 lineAPointB,Vector3 lineBPointA,Vector3 lineBPointB)
		{
			Vector3 a=lineAPointB-lineAPointA;
			Vector3 b=lineBPointB-lineBPointA;
			Vector3 c=lineBPointA-lineAPointA;
			return Mathf.Abs(Vector3.Dot(c,Vector3.Cross(a,b))/Vector3.Cross(a,b).magnitude);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Return the angle between two lines.
		/// </summary>
		/// <param name="lineAPointA">Point on first line</param>
		/// <param name="lineAPointB">Point on first line</param>
		/// <param name="lineBPointA">Point on second line</param>
		/// <param name="lineBPointB">Point on second line</param>
		public static float LineLineAngle(Vector3 lineAPointA,Vector3 lineAPointB,Vector3 lineBPointA,Vector3 lineBPointB)
		{
			Vector3 a=lineAPointB-lineAPointA;
			Vector3 b=lineBPointB-lineBPointA;
			float angle=Mathf.Acos(Vector3.Dot(a.normalized,b.normalized));
			return angle<90 ? angle : 180-angle;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Return the position of the projection of a point on a plane.
		/// </summary>
		/// <param name="planePointA">Point on the Plane</param>
		/// <param name="planePointB">Point on the Plane</param>
		/// <param name="planePointC">Point on the Plane</param>
		/// <param name="point">Point to project on the plane</param>
		public static Vector3 PointPlaneProjection(Vector3 planePointA,Vector3 planePointB,Vector3 planePointC,Vector3 point)
		{
			Vector3 abc=Vector3.Cross(planePointB-planePointA,planePointC-planePointA).normalized;
			return point+Vector3.Dot(planePointA-point,abc)*abc;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Returns the distance between a point and a plane.
		/// </summary>
		/// <param name="planePointA">Point on the Plane</param>
		/// <param name="planePointB">Point on the Plane</param>
		/// <param name="planePointC">Point on the Plane</param>
		/// <param name="point">Point to get the distance to the plane</param>
		public static float PointPlaneDistance(Vector3 planePointA,Vector3 planePointB,Vector3 planePointC,Vector3 point)
		{
			Vector3 abc=Vector3.Cross(planePointB-planePointA,planePointC-planePointA).normalized;
			return (Vector3.Dot(planePointA-point,abc)*abc).magnitude;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PointPlaneDistanceFast(Vector3 planePoint,Vector3 planeNormal,Vector3 point) => Vector3.Dot(planePoint-point,planeNormal);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		public static void PlaneNormal(ref Vector3 v1,ref Vector3 v2,ref Vector3 v3,out Vector3 norm)
		{
			Vector3 a,b;
			a.x=v1.x-v2.x; a.y=v1.y-v2.y; a.z=v1.z-v2.z;
			b.x=v2.x-v3.x; b.y=v2.y-v3.y; b.z=v2.z-v3.z;
			norm=Vector3.Cross(a,b);
			norm.Normalize();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		/// <summary>
		/// Returns the point of an intersection between a line and a plane.
		/// Returns a Vector3 NaN in case where the line and plane never touch
		/// Returns a Vector3 PositiveInfinity where there's more than one point of contact. 
		/// </summary>
		/// <param name="planePoint">Point on the plane</param>
		/// <param name="planeNormal">Normal of the plane</param>
		/// <param name="linePoint">Point on the line</param>
		/// <param name="lineDirection">Direction of the line</param>
		public static Vector3 LinePlaneIntersection(Vector3 planePoint,Vector3 planeNormal,Vector3 linePoint,Vector3 lineDirection)
		{
			lineDirection=lineDirection.normalized;
			float d1=Vector3.Dot(planePoint-linePoint,planeNormal);
			float d2=Vector3.Dot(lineDirection,planeNormal);
			// Parallel
			if (!Mathf.Approximately(d2,0))
				return linePoint+(d1/d2*lineDirection);
			else
			{
				// 0 == infinite, otherwise never touch
				return Mathf.Approximately(d1,0)
					? new(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity)
					: new(float.NaN,float.NaN,float.NaN);
			}
		}
		#endregion
		#region BOUNDS
		public static void CalculateBoundingPoints(Bounds b,Vector3[] boundPoints)
		{
			Vector3 p1=b.min,p2=b.max;
			boundPoints[0]=p1;
			boundPoints[1]=p2;
			boundPoints[2]=new(p1.x,p1.y,p2.z);
			boundPoints[3]=new(p1.x,p2.y,p1.z);
			boundPoints[4]=new(p2.x,p1.y,p1.z);
			boundPoints[5]=new(p1.x,p2.y,p2.z);
			boundPoints[6]=new(p2.x,p1.y,p2.z);
			boundPoints[7]=new(p2.x,p2.y,p1.z);
		}
		public static void CalculateBoundingPoints(Vector3 min,Vector3 max,Vector3[] boundPoints)
		{
			Vector3 p1=min,p2=max;
			boundPoints[0]=p1;
			boundPoints[1]=p2;
			boundPoints[2]=new(p1.x,p1.y,p2.z);
			boundPoints[3]=new(p1.x,p2.y,p1.z);
			boundPoints[4]=new(p2.x,p1.y,p1.z);
			boundPoints[5]=new(p1.x,p2.y,p2.z);
			boundPoints[6]=new(p2.x,p1.y,p2.z);
			boundPoints[7]=new(p2.x,p2.y,p1.z);
		}
		#endregion
		#region COLORS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void ColorLerp(ref Color a,ref Color b,out Color dest,float t)
		{
			t=Mathf.Clamp01(t);
			dest.r=a.r+(b.r-a.r)*t;
			dest.g=a.g+(b.g-a.g)*t;
			dest.b=a.b+(b.b-a.b)*t;
			dest.a=a.a+(b.a-a.a)*t;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void ColorRGBLerp(ref Color a,ref Color b,ref Color dest,float t)
		{
			t=Mathf.Clamp01(t);
			dest.r=a.r+(b.r-a.r)*t;
			dest.g=a.g+(b.g-a.g)*t;
			dest.b=a.b+(b.b-a.b)*t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void ColorLerpUnclamped(ref Color a,ref Color b,out Color dest,float t)
		{
			dest.r=a.r+(b.r-a.r)*t;
			dest.g=a.g+(b.g-a.g)*t;
			dest.b=a.b+(b.b-a.b)*t;
			dest.a=a.a+(b.a-a.a)*t;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void Color32RGBLerpUnclamped(ref Color32 a,ref Color32 b,ref Color32 dest,float t)
		{
			dest.r=(byte)((double)a.r+(double)(b.r-a.r)*(double)t);
			dest.g=(byte)((double)a.g+(double)(b.g-a.g)*(double)t);
			dest.b=(byte)((double)a.b+(double)(b.b-a.b)*(double)t);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void Color32LerpUnclamped(ref Color32 a,ref Color32 b,ref Color32 dest,float t)
		{
			dest.r=(byte)((double)a.r+(double)(b.r-a.r)*(double)t);
			dest.g=(byte)((double)a.g+(double)(b.g-a.g)*(double)t);
			dest.b=(byte)((double)a.b+(double)(b.b-a.b)*(double)t);
			dest.a=(byte)((double)a.a+(double)(b.a-a.a)*(double)t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void ColorRGBLerpUnclamped(ref Color a,ref Color b,ref Color dest,float t)
		{
			dest.r=a.r+(b.r-a.r)*t;
			dest.g=a.g+(b.g-a.g)*t;
			dest.b=a.b+(b.b-a.b)*t;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static void ColorRGBScale(ref Color c,float scale)
		{
			c.r*=scale;
			c.g*=scale;
			c.b*=scale;
		}
		#endregion
		#region INTERSECTIONS
		#region CIRCLE-RECTANGLE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CircleRectangleIntersection(ref Vector2 circlePos,float radiussquared,ref Vector2 rectLeftTop,float rectWidth,float rectHeight)
		{
			float deltaX=circlePos.x-Mathf.Max(rectLeftTop.x,Mathf.Min(circlePos.x,rectLeftTop.x+rectWidth));
			float deltaY=circlePos.y-Mathf.Max(rectLeftTop.y,Mathf.Min(circlePos.y,rectLeftTop.y+rectHeight));
			return deltaX*deltaX+deltaY*deltaY<radiussquared;
		}
		#endregion
		#region SEGMENTS
		public static bool SegmentPlaneIntersect(Vector3 p0,Vector3 p1,Vector3 planeCenter,Vector3 planeNormal)
		{
			Vector3 u=p1-p0;
			Vector3 w=p0-planeCenter;

			float D=Vector3.Dot(planeNormal,u);
			float N=-Vector3.Dot(planeNormal,w);

			if (Mathf.Abs(D)<Mathf.Epsilon)
				return N==0;

			// they are not parallel
			// compute intersect param
			float sI=N/D;
			if (sI<0 || sI>1)
				return false;	// no intersection
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SegmentIntersectsViewport(Vector2 p1,Vector2 p2) // faster version (Liang-Barsky)
		{
			float dx=p2.x-p1.x;
			float dy=p2.y-p1.y;

			if (dx==0) dx=float.Epsilon;
			if (dy==0) dy=float.Epsilon;

			float tEnter=0.0f;
			float tExit=1.0f;
			float p=-dx;
			float r=p1.x/p;

			tEnter=Mathf.Max(tEnter,(p<0) ? r : 0f);
			tExit=Mathf.Min(tExit,(p>0) ? r : 1f);
			r=(1-p1.x)/dx;
			tEnter=Mathf.Max(tEnter,(dx<0) ? r : 0f);
			tExit=Mathf.Min(tExit,(dx>0) ? r : 1f);
			r=p1.y/p;
			tEnter=Mathf.Max(tEnter,(p<0) ? r : 0f);
			tExit=Mathf.Min(tExit,(p>0) ? r : 1f);
			r=(1-p1.y)/dy;
			tEnter=Mathf.Max(tEnter,(dy<0) ? r : 0f);
			tExit=Mathf.Min(tExit,(dy>0) ? r : 1f);
			return tEnter<=tExit;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SegmentIntersectsViewport(Vector2 p1,Vector2 p2,Rect viewportRect)	// (Liang-Barsky)
		{
			float dx=p2.x-p1.x;
			float dy=p2.y-p1.y;

			if (dx==0) dx=float.Epsilon;
			if (dy==0) dy=float.Epsilon;

			float tEnter=0.0f;
			float tExit=1.0f;
			float xMin=viewportRect.x;
			float yMin=viewportRect.y;
			float xMax=viewportRect.width;
			float yMax=viewportRect.height;
			float p=-dx;
			float q=p1.x-xMin;
			tEnter=Mathf.Max(tEnter,(p<0) ? (q/p) : 0f);
			tExit=Mathf.Min(tExit,(p>0) ? (q/p) : 1f);

			p=dx;
			q=xMax-p1.x;
			tEnter=Mathf.Max(tEnter,(p<0) ? (q/p) : 0f);
			tExit=Mathf.Min(tExit,(p>0) ? (q/p) : 1f);

			p=-dy;
			q=p1.y-yMin;
			tEnter=Mathf.Max(tEnter,(p<0) ? (q/p) : 0f);
			tExit=Mathf.Min(tExit,(p>0) ? (q/p) : 1f);

			p=dy;
			q=yMax-p1.y;
			tEnter=Mathf.Max(tEnter,(p<0) ? (q/p) : 0f);
			tExit=Mathf.Min(tExit,(p>0) ? (q/p) : 1f);

			return tEnter<=tExit;
		}

		//private static readonly Rect viewportRect=new(0,0,1,1);
		//private static Vector2 viewportXMinYMax=new(0,1);
		//private static Vector2 viewportXMaxYMin=new(1,0);
		//private static Vector2 viewportXMaxYMax=new(1,1);
		//private static Vector2 viewportXMinYMin=new(0,0);
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//[Il2CppSetOption(Option.NullChecks,false)]
		//[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		//[Il2CppSetOption(Option.DivideByZeroChecks,false)]
		//public static bool SegmentIntersectsViewport(Vector2 start,Vector2 end)
		//{
		//	float minX=Mathf.Min(start.x,end.x);
		//	float maxX=Mathf.Max(start.x,end.x);
		//	float minY=Mathf.Min(start.y,end.y);
		//	float maxY=Mathf.Max(start.y,end.y);

		//	if (maxX<viewportRect.xMin||minX>viewportRect.xMax||maxY<viewportRect.yMin||minY>viewportRect.yMax)
		//		return false;

		//	return SegmentsIntersect(start,end,viewportXMinYMax,viewportXMaxYMax)||
		//		   SegmentsIntersect(start,end,viewportXMaxYMax,viewportXMaxYMin)||
		//		   SegmentsIntersect(start,end,viewportXMaxYMin,viewportXMinYMin)||
		//		   SegmentsIntersect(start,end,viewportXMinYMin,viewportXMinYMax);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static bool SegmentsIntersect(Vector2 p1,Vector2 p2,Vector2 p3,Vector2 p4)
		//{
		//	float orientation1=GetOrientation(p1,p2,p3);
		//	float orientation2=GetOrientation(p1,p2,p4);
		//	float orientation3=GetOrientation(p3,p4,p1);
		//	float orientation4=GetOrientation(p3,p4,p2);

		//	return orientation1!=orientation2&&orientation3!=orientation4;
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static float GetOrientation(Vector2 p1,Vector2 p2,Vector2 p3) => Mathf.Sign((float)((p2.y-p1.y)*(p3.x-p2.x)-(p2.x-p1.x)*(p3.y-p2.y)));

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public static Rect GetSegmentBoundingBox(Vector2 start,Vector2 end)
		//{
		//	float minX=Mathf.Min(start.x,end.x);
		//	float minY=Mathf.Min(start.y,end.y);
		//	float maxX=Mathf.Max(start.x,end.x);
		//	float maxY=Mathf.Max(start.y,end.y);

		//	return Rect.MinMaxRect(minX,minY,maxX,maxY);
		//}
		#endregion
		#endregion
		#region CAMERA
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Matrix4x4 CameraPVMatrix(Camera cam) => cam.projectionMatrix*cam.worldToCameraMatrix;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static Vector3 WorldToViewportPoint(Matrix4x4 PVMatrix,Vector3 worldPosition)
		{
			Vector4 clipPosition=PVMatrix*new Vector4(worldPosition.x,worldPosition.y,worldPosition.z,1);
			Vector3 normalizedClipPosition=new(clipPosition.x/clipPosition.w,clipPosition.y/clipPosition.w,clipPosition.z/clipPosition.w);
			return new(normalizedClipPosition.x*0.5f+0.5f,normalizedClipPosition.y*0.5f+0.5f,normalizedClipPosition.z);
		}
		#endregion
	}

	#region CLASSES EXTENSIONS
	public static class Vector2Extension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.NullChecks,false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		public static Vector2 Rotate(this Vector2 v,float degrees)
		{
			float radians=degrees*Mathf.Deg2Rad;
			float sin=Mathf.Sin(radians);
			float cos=Mathf.Cos(radians);

			return new(cos*v.x-sin*v.y,sin*v.x+cos*v.y);
		}
	}
	public static class FloatExtensions
	{
		public static float Remap(this float value,float from1,float to1,float from2,float to2) => (value-from1)/(to1-from1)*(to2-from2)+from2;
	}
	public static class FloatArrayExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public static float Sample(this float[] fArr,float t)
		{
			int count=fArr.Length;
		#if UNITY_EDITOR
			if (count==0)
			{
				Debug.LogError("<color=red>Unable to sample Float Array - it has no elements.</color>");
				return 0;
			}
		#endif
			if (count!=1)
			{
				float f=t*(count-1);
				int idLower=(int)System.Math.Floor(f);
				int idUpper=idLower+1;

				return idUpper>=count ? fArr[count-1] : idLower>=0 ? DrakkarMath.LerpUnclamped(fArr[idLower],fArr[idUpper],f-idLower) : fArr[0];
			}
			else
				return fArr[0];
		}
	}
	#endregion
}