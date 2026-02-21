using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
#if DRAKKAR_EVENTS
using Drakkar.Events;
#endif
#if UNITY_EDITOR
using UnityEngine.Profiling;
using UnityEditor;
#endif
using static Drakkar.GameUtils.DrakkarTrail;

namespace Drakkar.GameUtils
{
	public class DrakkarTrail : MonoBehaviour, ILateUpdatable
	{
		public enum TRAIL_INTERPOLATION
		{
			[InspectorName("No Interpolation")]NONE   =0,
			[InspectorName("Interpolate X 2")]HALFS   =1,
			[InspectorName("Interpolate X 3")]THIRDS  =2,
			[InspectorName("Interpolate X 4")]FOURTHS =3,
			[InspectorName("Interpolate X 5")]FIFTHS  =4,
			[InspectorName("Interpolate X 6")]SIXTHS  =5,
			[InspectorName("Interpolate X 7")]SEVENTHS=6,
			[InspectorName("Interpolate X 8")]EIGHTHS =7,
			[InspectorName("Interpolate X 9")]NINETHS =8,
			[InspectorName("Interpolate X 10")]TENTHS =9
		}
		public enum EDGES
		{
			[InspectorName("NO SPLITS")]NONE	=0,
			[InspectorName("1 SPLIT")]ONE		=1,
			[InspectorName("2 SPLITS")]TWO		=2
		}

		public enum STEPTIME
		{
			[InspectorName("Sync at 60 FPS")]SIXTY_FPS,
			[InspectorName("Sync at 30 FPS")]THIRTY_FPS,
			[InspectorName("Sync at 20 FPS")]TWENTY_FPS
		}

		public static int InterpolationBias=0;
		#region PUBLICS
	#if DRAKKAR
		public bool UseColdStart;
	#endif
		public float Length=1;
		[SerializeField]
		internal int Steps=4;
		[SerializeField]
		internal STEPTIME StepTime;
		public int EndingSpeed=4;
		[SerializeField]
		internal float Bounds=1;
		[SerializeField]
		internal TRAIL_INTERPOLATION Interpolation;
		[SerializeField]
		internal EDGES Edges;
		[SerializeField]
		internal float Edge1=0.5f,Edge2=0.666666f;
		public Material TrailMaterial;
		public int Layer;
	#if DRAKKAR_VFX
		public DrakkarVfx Vfx;
	#endif
	#if DRAKKAR_EVENTS
		public DrakkarEvent[] OnBegin=null,OnEnd=null,OnClear=null;
	#endif
	#endregion
		#region INTERNALS
		internal bool active,dying,added;
		internal Matrix4x4 matrix,identity;
		#endregion
		#region PRIVATES
		private DrakkarTrailData Trail;
		private Transform _transform;
		private bool initialized;
		#endregion

		private void Awake()
		{
			_transform=transform;
		#if UNITY_EDITOR
			if (_transform.lossyScale!=Vector3.one)
				Debug.LogError("<color=yellow>TrailFeed: "+gameObject.name+" has SCALED TRANSFORM!!!</color>");
		#endif
		#if DRAKKAR
			if (UseColdStart)
				ColdStart.Add(Init);
			else
		#endif
				Init();
		}

		private void OnDisable() => Clear();

		private void OnDestroy()
		{
			//if (added)
			{
				DrakkarUpdater.TryRemoveLate(this);
				added=false;
			}
		}

		public void Init()
		{
			if (initialized)
				return;
			identity=Matrix4x4.identity;
			Interpolation=(TRAIL_INTERPOLATION)Mathf.Clamp((int)Interpolation+InterpolationBias,0,10);
			Trail.Init(this,(int)Edges);
			initialized=true;
		#if DRAKKAR_VFX
			if (Vfx.hasRoot)
				Vfx.Init();
		#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Begin()
		{
			Trail.Reset();
			active=true;
			dying=false;
			//if (!added)
				DrakkarUpdater.TryAddLate(this);
			added=true;
		#if DRAKKAR_VFX
			if (Vfx.hasRoot)
				VFXMaster.PlayVFX(Vfx,true);
		#endif
		#if DRAKKAR_EVENTS
			DrakkarEvent.Execute(OnBegin);
		#endif
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void End()
		{
			if (!dying)
			{
				dying=true;
			#if DRAKKAR_VFX
				if (Vfx.hasRoot)
					VFXMaster.StopVFX(Vfx);
			#endif
			#if DRAKKAR_EVENTS
				DrakkarEvent.Execute(OnEnd);
			#endif
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Clear()
		{
			//if (added)
			{
				DrakkarUpdater.TryRemoveLate(this);
				added=false;
			}
			dying=false;
			if (active)
			{
				active=false;
			#if DRAKKAR_VFX
				if (Vfx.hasRoot)
				{
					VFXMaster.StopVFX(Vfx);
					Vfx.Clear();
				}
			#endif
			#if DRAKKAR_EVENTS
				DrakkarEvent.Execute(OnClear);
			#endif
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void OnLateUpdate()
		{
			matrix=_transform.localToWorldMatrix;
			if (Trail.NextTrailNode(ref matrix,Length))
				Trail.RebuildAndDraw();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void GetCurrentVertices(out Vector3 point,out Vector3 limb)
		{
			Matrix4x4 matrix=_transform.localToWorldMatrix;
			point.x=matrix.m03;
			point.y=matrix.m13;
			point.z=matrix.m23;
			limb.x=matrix.m02*Length+point.x;
			limb.y=matrix.m12*Length+point.y;
			limb.z=matrix.m22*Length+point.z;
		}

	#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color=Color.cyan;
			Gizmos.DrawLine(transform.position,transform.position+transform.forward*Length);
			switch (Edges)
			{
			case EDGES.ONE:
				Gizmos.color=Color.red;
				Gizmos.DrawLine(transform.position+Edge1*Length*transform.forward,transform.position+Edge1*Length*transform.forward+transform.up*Length);
				break;
			case EDGES.TWO:
				Gizmos.color=Color.red;
				Gizmos.DrawLine(transform.position+Edge1*Length*transform.forward,transform.position+Edge1*Length*transform.forward+transform.up*Length);
				Gizmos.color=Color.yellow;
				Gizmos.DrawLine(transform.position+Edge2*Length*transform.forward,transform.position+Edge2*Length*transform.forward+transform.up*Length);
				break;
			}
			if (Trail.vertices!=null)
			{
				Gizmos.color=Color.green;
				Gizmos.DrawLine(Trail.vertices[0],Trail.vertices[0]+Trail.forward*Length);
				Gizmos.color=Color.magenta;
				Gizmos.DrawLine(Trail.vertices[1],Trail.vertices[1]+Trail.forward*Length);
				if (Application.isPlaying && Trail.feed.active)
				{
					Gizmos.color=Color.cyan;
					Gizmos.DrawLine(Trail.vertices[0],Trail.vertices[1]);
					for(int i=2;i<Steps*2*((int)Interpolation+1-(int)Interpolation);i+=2)
					{
						Gizmos.color=Interpolation switch
						{
							TRAIL_INTERPOLATION.HALFS   => ((i&2)==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.THIRDS  => (i%3==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.FOURTHS => ((i&3)==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.FIFTHS  => (i%5==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.SIXTHS  => (i%12==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.SEVENTHS=> (i%14==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.EIGHTHS => (i%16==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.NINETHS => (i%18==0)?Color.cyan:Color.blue,
							TRAIL_INTERPOLATION.TENTHS  => (i%20==0)?Color.cyan:Color.blue,
							_							=> Color.yellow,
						};
						Gizmos.DrawLine(Trail.vertices[i],Trail.vertices[i+1]);
						Gizmos.color=Color.yellow;
						Gizmos.DrawLine(Trail.vertices[i],Trail.vertices[i-2]);
						Gizmos.DrawLine(Trail.vertices[i+1],Trail.vertices[i-1]);
					}
				}
			}
			Gizmos.color=Color.magenta;
			if (Bounds>0)
				Gizmos.DrawWireCube(transform.position,new Vector3(Bounds,Bounds,Bounds));
		}
	#endif
	}

	public struct DrakkarTrailData
	{
		private static readonly float[] timeSteps={0.016666666f,0.033333333f,0.05f};

		public DrakkarTrail feed;

		internal Vector3 forward;
		internal Vector3[] vertices;
		internal Mesh _mesh;

		private Vector2[] uvs;
		private Vector3 lastPos;
		private ushort[] indices;
		private int lastStep,steps,currentStep,edges,edgesOffset,edgesOffset2,edgesOffset3,interpolation;
		private float life,stepLife,stepTime;
		private Bounds bounds;

		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Init(DrakkarTrail feed,int interEdges)
		{
			edges=interEdges;

			this.feed=feed;
			steps=feed.Steps;
			stepTime=timeSteps[(int)feed.StepTime];
			interpolation=(int)(feed.Interpolation+1);
			steps=feed.Steps*interpolation-(interpolation-1);
			lastStep=interpolation<<1;

			indices=new ushort[(steps-1)*6*(1+edges)];
			vertices=new Vector3[steps*(2+edges)];
			uvs=new Vector2[steps*(2+edges)];
			edgesOffset=steps<<1;				// starting index of edges
			edgesOffset2=edgesOffset+steps;		// starting index of edges 2
			edgesOffset3=edgesOffset2+steps;	// starting index of edges 2

			_mesh=new();
			_mesh.MarkDynamic();
			float uvStep=1.0f/steps;

			int s=edgesOffset;
			Vector2 uvsP=new(0,0);
			switch (edges)
			{
			case 0:
				for (int i=0;i<s;i+=2)
				{
					uvs[i]=uvsP;
					uvs[i+1]=new(uvsP.x,1);
					uvsP.x+=uvStep;
				}
				break;
			case 1:
				int e=s;
				for (int i=0;i<s;i+=2)
				{
					uvs[i]=uvsP;
					uvs[i+1]=new(uvsP.x,1);
					uvs[e++]=new(uvsP.x,.5f);
					uvsP.x+=uvStep;
				}
				break;
			case 2:
				e=s;
				int e2=edgesOffset2;
				for (int i=0;i<s;i+=2)
				{
					uvs[i]=uvsP;
					uvs[i+1]=new(uvsP.x,1);
					uvs[e++]=new(uvsP.x,feed.Edge2);
					uvs[e2++]=new(uvsP.x,feed.Edge1);
					uvsP.x+=uvStep;
				}
				break;
			}
			ushort iOff=0;
			s=(steps-1)*6*(1+edges);
			ushort edOff=(ushort)edgesOffset;
			switch (edges)
			{
			#region SINGLE QUAD
			case 0:
				for (int i = 0;i<s;i+=6)
				{
					indices[i]=(ushort)(iOff+1);
					indices[i+1]=iOff;
					indices[i+2]=(ushort)(iOff+2);
					indices[i+3]=(ushort)(iOff+2);
					indices[i+4]=(ushort)(iOff+3);
					indices[i+5]=(ushort)(iOff+1);
					iOff+=2;
				}
				break;
			#endregion
			#region ONE EDGE
			case 1:
				for (int i=0;i<s;i+=6*(1+edges))
				{
					indices[i]=edOff;				// 8
					indices[i+1]=iOff;				// 0
					indices[i+2]=(ushort)(iOff+2);	// 2
					indices[i+3]=(ushort)(iOff+2);	// 2
					indices[i+4]=(ushort)(edOff+1);	// 9
					indices[i+5]=edOff;				// 8

					indices[i+6]=(ushort)(iOff+1);	// 1
					indices[i+7]=edOff;				// 8
					indices[i+8]=(ushort)(edOff+1);	// 9
					indices[i+9]=(ushort)(edOff+1);	// 9
					indices[i+10]=(ushort)(iOff+3);	// 3
					indices[i+11]=(ushort)(iOff+1);	// 1
					iOff+=2;
					edOff++;
				}
				break;
			#endregion
			#region TWO EDGES
			case 2:
				ushort edOff2=(ushort)(edOff+steps);
				for (int i=0;i<s;i+=6*(1+edges))
				{

					indices[i]  =edOff2;			// 12
					indices[i+1]=iOff;				// 0
					indices[i+2]=(ushort)(iOff+2);  // 2
					indices[i+3]=(ushort)(iOff+2);  // 2
					indices[i+4]=(ushort)(edOff2+1);// 13
					indices[i+5]=edOff2;            // 12

					indices[i+6]=edOff;            // 8 edOff
					indices[i+7]=edOff2;            // 12
					indices[i+8]=(ushort)(edOff2+1);// 13
					indices[i+9]=(ushort)(edOff2+1);// 13
					indices[i+10]=(ushort)(edOff+1);// 9
					indices[i+11]=edOff;            // 8

					indices[i+12]=(ushort)(iOff+1); // 1
					indices[i+13]=edOff;            // 8 edOff
					indices[i+14]=(ushort)(edOff+1);// 9
					indices[i+15]=(ushort)(edOff+1);// 9
					indices[i+16]=(ushort)(iOff+3); // 3
					indices[i+17]=(ushort)(iOff+1); // 1
					iOff+=2;
					edOff++;
					edOff2++;
				}
				break;
			#endregion
			}
			Reset();
			RebuildAndDraw();
			_mesh.SetUVs(0,uvs,0,edgesOffset+steps*edges,MeshUpdateFlags.DontNotifyMeshUsers|MeshUpdateFlags.DontRecalculateBounds|MeshUpdateFlags.DontValidateIndices);
			_mesh.SetIndices(indices,MeshTopology.Triangles,0,false);
			bounds.extents=new(feed.Bounds,feed.Bounds,feed.Bounds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void Reset()
		{
			life=stepLife=0;
			currentStep=0;
			feed.GetCurrentVertices(out Vector3 p,out Vector3 l);
			int s=edgesOffset;
			int e=s;
			switch (edges)
			{
			case 0:
				for (uint i=0;i<s;i+=2)
				{
					vertices[i]=p;
					vertices[i+1]=l;
				}
				break;
			case 1:
				for (uint i=0;i<s;i+=2)
				{
					vertices[i]=p;
					vertices[i+1]=l;

					vertices[e++]=(p+l)*feed.Edge1;
				}
				break;
			case 2:
				for (uint i=0;i<s;i+=2)
				{
					vertices[i]=p;
					vertices[i+1]=l;

					Vector3 v=l-p;
					vertices[e]=p+v*feed.Edge2;
					vertices[steps+e++]=p+v*feed.Edge1;
				}
				break;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public void RebuildAndDraw()
		{
			_mesh.SetVertices(vertices,0,edgesOffset+steps*edges,MeshUpdateFlags.DontValidateIndices|MeshUpdateFlags.DontRecalculateBounds);
			bounds.center=lastPos;
			_mesh.bounds=bounds;
			Graphics.DrawMesh(_mesh,feed.identity,feed.TrailMaterial,feed.Layer,null,0,null,false,false,false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		public bool NextTrailNode(ref Matrix4x4 matrix,float Length)
		{
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.BeginSample("DrakkarTrail");
		#endif
			if (!feed.active)
				return false;
			if (feed.dying)		// dying trail
			{
				if (stepLife>=stepTime)
				{
					for (int l=0;l<feed.EndingSpeed;l++)
					{
						if (currentStep<=1)
						{
							feed.Clear();
							return false;
						}
						stepLife=0f;
						for (int i=edgesOffset-1;i>2;i-=2)
						{
							vertices[i]=vertices[i-2];
							vertices[i-1]=vertices[i-3];
						}
						currentStep--;
						switch (edges)
						{
						case 1:
							for (int u=edgesOffset+steps-1;u>edgesOffset;u--)
								vertices[u]=vertices[u-1];
							break;
						case 2:
							for (int u=edgesOffset+steps-1;u>edgesOffset;u--)
							{
								vertices[u]=vertices[u-1];
								vertices[u+steps]=vertices[u+steps-1];
							}
							break;
						}
					}
				}
				stepLife+=DrakkarTime.deltaTime;
				return true;
			}
			if (stepLife>=stepTime)		// scroll vertices
			{
				stepLife=0f;
				int fromH=2+2*(int)feed.Interpolation;
				int fromL=fromH+1;
				for (int i=(steps<<1)-1;i>fromH;i-=2)
				{
					vertices[i]=vertices[i-fromH];
					vertices[i-1]=vertices[i-fromL];
				}
				switch (edges)
				{
				case 1:
					int l=edgesOffset+3*interpolation;
					for (int u=edgesOffset+steps-1;u>l;u--)
						vertices[u]=vertices[u-interpolation];
					break;
				case 2:
					l=edgesOffset+3*interpolation;
					for (int u=edgesOffset+steps-1;u>l;u--)
					{
						vertices[u]=vertices[u-interpolation];
						vertices[u+steps]=vertices[u+steps-interpolation];
					}
					break;
				}
			}
			life+=DrakkarTime.deltaTime;
			stepLife+=DrakkarTime.deltaTime;

			Vector3 pos,limb;
			pos=new(matrix.m03,matrix.m13,matrix.m23);
			lastPos=pos;
			limb=new Vector3(matrix.m02,matrix.m12,matrix.m22)*Length+pos;
		#if UNITY_EDITOR
			forward=new(matrix.m01,matrix.m11,matrix.m21);
		#endif
			vertices[0]=pos;
			vertices[1]=limb;
			int interp=((int)feed.Interpolation+1)*2;
			switch (edges)
			{
			case 1:
				vertices[edgesOffset]=Vector3.LerpUnclamped(pos,limb,feed.Edge1);
				vertices[edgesOffset+interpolation]=Vector3.LerpUnclamped(vertices[interp],vertices[interp+1],feed.Edge1);
				vertices[edgesOffset+interpolation*2]=Vector3.LerpUnclamped(vertices[interp*2],vertices[interp*2+1],feed.Edge1);
				vertices[edgesOffset+interpolation*3]=Vector3.LerpUnclamped(vertices[interp*3],vertices[interp*3+1],feed.Edge1);
				break;
			case 2:
				vertices[edgesOffset]=Vector3.LerpUnclamped(pos,limb,feed.Edge2);
				vertices[edgesOffset+interpolation]=Vector3.LerpUnclamped(vertices[interp],vertices[interp+1],feed.Edge2);
				vertices[edgesOffset+interpolation*2]=Vector3.LerpUnclamped(vertices[interp*2],vertices[interp*2+1],feed.Edge2);
				vertices[edgesOffset+interpolation*3]=Vector3.LerpUnclamped(vertices[interp*3],vertices[interp*3+1],feed.Edge2);

				vertices[edgesOffset2]=Vector3.LerpUnclamped(pos,limb,feed.Edge1);
				vertices[edgesOffset2+interpolation]=Vector3.LerpUnclamped(vertices[interp],vertices[interp+1],feed.Edge1);
				vertices[edgesOffset2+interpolation*2]=Vector3.LerpUnclamped(vertices[interp*2],vertices[interp*2+1],feed.Edge1);
				vertices[edgesOffset2+interpolation*3]=Vector3.LerpUnclamped(vertices[interp*3],vertices[interp*3+1],feed.Edge1);
				break;
			}
			Vector3 start=pos,start2=limb,mid,mid2,endP,endP2;

			if (currentStep<steps)
				currentStep++;
			switch (feed.Interpolation)
			{
			case DrakkarTrail.TRAIL_INTERPOLATION.HALFS:
				#region HALFS
				endP=vertices[6];
				endP2=vertices[7];
				DrakkarMath.PointOnCurve_Half(ref start,ref start,ref vertices[4],ref endP,out vertices[2]);
				DrakkarMath.PointOnCurve_Half(ref start2,ref start2,ref vertices[5],ref endP2,out vertices[3]);

				DrakkarMath.PointOnCurve_Half(ref pos,ref vertices[4],ref vertices[8],ref vertices[12],out vertices[6]);
				DrakkarMath.PointOnCurve_Half(ref limb,ref vertices[5],ref vertices[9],ref vertices[13],out vertices[7]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.THIRDS:
				#region THIRDS
				endP=vertices[12];
				endP2=vertices[13];
				mid=vertices[6];
				mid2=vertices[7];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Thirds(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4]);
				DrakkarMath.PointOnCurve_Thirds(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5]);

				DrakkarMath.PointOnCurve_Thirds(ref pos,ref mid,ref endP,ref vertices[18],out vertices[8],out vertices[10]);
				DrakkarMath.PointOnCurve_Thirds(ref limb,ref mid2,ref endP2,ref vertices[19],out vertices[9],out vertices[11]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.FOURTHS:
				#region FOURTHS
				endP=vertices[16];
				endP2=vertices[17];
				mid=vertices[8];
				mid2=vertices[9];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Fourths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6]);
				DrakkarMath.PointOnCurve_Fourths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7]);

				DrakkarMath.PointOnCurve_Fourths(ref pos,ref mid,ref endP,ref vertices[24],out vertices[10],out vertices[12],out vertices[14]);
				DrakkarMath.PointOnCurve_Fourths(ref limb,ref mid2,ref endP2,ref vertices[25],out vertices[11],out vertices[13],out vertices[15]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.FIFTHS:
				#region FIFTHS
				endP=vertices[20];
				endP2=vertices[21];
				mid=vertices[10];
				mid2=vertices[11];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Fifths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6],out vertices[8]);
				DrakkarMath.PointOnCurve_Fifths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7],out vertices[9]);

				DrakkarMath.PointOnCurve_Fifths(ref pos,ref mid,ref endP,ref vertices[30],out vertices[12],out vertices[14],out vertices[16],out vertices[18]);
				DrakkarMath.PointOnCurve_Fifths(ref limb,ref mid2,ref endP2,ref vertices[31],out vertices[13],out vertices[15],out vertices[17],out vertices[19]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.SIXTHS:
				#region SIXTHS
				endP=vertices[24];
				endP2=vertices[25];
				mid=vertices[12];
				mid2=vertices[13];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Sixths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6],out vertices[8],out vertices[10]);
				DrakkarMath.PointOnCurve_Sixths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7],out vertices[9],out vertices[11]);
				DrakkarMath.PointOnCurve_Sixths(ref pos,ref mid,ref endP,ref vertices[36],out vertices[14],out vertices[16],out vertices[18],out vertices[20],out vertices[22]);
				DrakkarMath.PointOnCurve_Sixths(ref limb,ref mid2,ref endP2,ref vertices[37],out vertices[15],out vertices[17],out vertices[19],out vertices[21],out vertices[23]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.SEVENTHS:
				#region SEVENTHS
				endP=vertices[28];
				endP2=vertices[29];
				mid=vertices[14];
				mid2=vertices[15];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Sevenths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6],out vertices[8],out vertices[10],out vertices[12]);
				DrakkarMath.PointOnCurve_Sevenths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7],out vertices[9],out vertices[11],out vertices[13]);
				DrakkarMath.PointOnCurve_Sevenths(ref pos,ref mid,ref endP,ref vertices[42],out vertices[16],out vertices[18],out vertices[20],out vertices[22],out vertices[24],out vertices[26]);
				DrakkarMath.PointOnCurve_Sevenths(ref limb,ref mid2,ref endP2,ref vertices[43],out vertices[17],out vertices[19],out vertices[21],out vertices[23],out vertices[25],out vertices[27]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.EIGHTHS:
				#region EIGHTHS
				endP=vertices[32];
				endP2=vertices[33];
				mid=vertices[16];
				mid2=vertices[17];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Eighths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6],out vertices[8],out vertices[10],out vertices[12],out vertices[14]);
				DrakkarMath.PointOnCurve_Eighths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7],out vertices[9],out vertices[11],out vertices[13],out vertices[15]);

				DrakkarMath.PointOnCurve_Eighths(ref pos,ref mid,ref endP,ref vertices[48],out vertices[18],out vertices[20],out vertices[22],out vertices[24],out vertices[26],out vertices[28],out vertices[30]);
				DrakkarMath.PointOnCurve_Eighths(ref limb,ref mid2,ref endP2,ref vertices[49],out vertices[19],out vertices[21],out vertices[23],out vertices[25],out vertices[27],out vertices[29],out vertices[31]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.NINETHS:
				#region NINETHS
				endP=vertices[36];
				endP2=vertices[37];
				mid=vertices[18];
				mid2=vertices[19];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Nineths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6],out vertices[8],out vertices[10],out vertices[12],out vertices[14],out vertices[16]);
				DrakkarMath.PointOnCurve_Nineths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7],out vertices[9],out vertices[11],out vertices[13],out vertices[15],out vertices[17]);

				DrakkarMath.PointOnCurve_Nineths(ref pos,ref mid,ref endP,ref vertices[54],out vertices[20],out vertices[22],out vertices[24],out vertices[26],out vertices[28],out vertices[30],out vertices[32],out vertices[34]);
				DrakkarMath.PointOnCurve_Nineths(ref limb,ref mid2,ref endP2,ref vertices[55],out vertices[21],out vertices[23],out vertices[25],out vertices[27],out vertices[29],out vertices[31],out vertices[33],out vertices[35]);
				#endregion
				break;
			case DrakkarTrail.TRAIL_INTERPOLATION.TENTHS:
				#region TENTHS
				endP=vertices[40];
				endP2=vertices[41];
				mid=vertices[20];
				mid2=vertices[21];
				start+=pos-mid;
				start2+=limb-mid2;
				DrakkarMath.PointOnCurve_Tenths(ref start,ref pos,ref mid,ref endP,out vertices[2],out vertices[4],out vertices[6],out vertices[8],out vertices[10],out vertices[12],out vertices[14],out vertices[16],out vertices[18]);
				DrakkarMath.PointOnCurve_Tenths(ref start2,ref limb,ref mid2,ref endP2,out vertices[3],out vertices[5],out vertices[7],out vertices[9],out vertices[11],out vertices[13],out vertices[15],out vertices[17],out vertices[19]);

				DrakkarMath.PointOnCurve_Tenths(ref pos,ref mid,ref endP,ref vertices[60],out vertices[22],out vertices[24],out vertices[26],out vertices[28],out vertices[30],out vertices[32],out vertices[34],out vertices[36],out vertices[38]);
				DrakkarMath.PointOnCurve_Tenths(ref limb,ref mid2,ref endP2,ref vertices[61],out vertices[23],out vertices[25],out vertices[27],out vertices[29],out vertices[31],out vertices[33],out vertices[35],out vertices[37],out vertices[39]);
				#endregion
				break;
			}
			if (edges>0)
				calculateEdges();
		#if UNITY_EDITOR
			UnityEngine.Profiling.Profiler.EndSample();
		#endif
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Il2CppSetOption(Option.ArrayBoundsChecks,false)]
		[Il2CppSetOption(Option.NullChecks,false)]
		private void calculateEdges()
		{
			int b1=edgesOffset+1;
			int b2=b1+interpolation;
			int b3=b2+interpolation;
			int secondLane=interpolation*2+2;
			int secondLane2=secondLane+1;
			int thirdLane=interpolation*4+2;
			int thirdLane2=thirdLane+1;
			switch (edges)
			{
			case 1:
				for (int i=0;i<(int)feed.Interpolation;i++)
				{
					int u=i<<1;
					Vector3 v1=vertices[3+u]-vertices[2+u];
					Vector3 v2=vertices[secondLane2+u]-vertices[secondLane+u];
					Vector3 v3=vertices[thirdLane2+u]-vertices[thirdLane+u];
					vertices[b1+i]=vertices[2+u]+v1*feed.Edge1;
					vertices[b2+i]=vertices[secondLane+u]+v2*feed.Edge1;
					vertices[b3+i]=vertices[thirdLane+u]+v3*feed.Edge1;
				}
				break;
			case 2:
				int b12=edgesOffset2+1;
				int b22=b12+interpolation;
				int b32=b22+interpolation;
				for (int i=0;i<(int)feed.Interpolation;i++)
				{
					int u=i<<1;
					Vector3 v1=vertices[3+u]-vertices[2+u];
					Vector3 v2=vertices[secondLane2+u]-vertices[secondLane+u];
					Vector3 v3=vertices[thirdLane2+u]-vertices[thirdLane+u];
					vertices[b1+i]=vertices[2+u]+v1*feed.Edge2;
					vertices[b2+i]=vertices[secondLane+u]+v2*feed.Edge2;
					vertices[b3+i]=vertices[thirdLane+u]+v3*feed.Edge2;
					vertices[b12+i]=vertices[2+u]+v1*feed.Edge1;
					vertices[b22+i]=vertices[secondLane+u]+v2*feed.Edge1;
					vertices[b32+i]=vertices[thirdLane+u]+v3*feed.Edge1;
				}
				break;
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(DrakkarTrail),true)]
	public class DrakkarTrailEditor : Editor
	{
		private DrakkarTrail t;
		private Vector3 dest;
		private float guiLength;
		private SerializedObject so;
		private bool setupOpen,interpolationOpen,renderingOpen,eventsOpen;

		private void OnEnable()
		{
			so=serializedObject;
			t=(DrakkarTrail)target;
		}

		public override void OnInspectorGUI()
		{
			Undo.RecordObject(t,"trail");
		#if DRAKKAR
			DrakkarEditor.TextureLogo(t,false,string.Empty,ref t.UseColdStart,"ColdStart",70);
		#else
			DrakkarEditor.TextureLogo(t);
		#endif
			DrakkarEditor.CheckDrakkarUpdater();
			DrakkarEditor.FoldOutRich(ref setupOpen,"Setup");
			EditorGUI.BeginChangeCheck();
			if (setupOpen)
			{
				DrakkarEditor.BeginIndent(10);
				EditorGUILayout.BeginHorizontal();
					disabledGUIOnPlay();
					DrakkarEditor.DrakkarPropertyInspector(serializedObject,"Steps");
					GUI.contentColor=Color.yellow;
					DrakkarEditor.PropertyInspectorNoLabel(serializedObject,"StepTime");
					GUI.contentColor=Color.white;
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
					DrakkarEditor.DrakkarPropertyInspector(serializedObject,"Bounds");
					DrakkarEditor.DrakkarPropertyInspector(serializedObject,"Length");
				EditorGUILayout.EndHorizontal();
				t.EndingSpeed=EditorGUILayout.IntSlider("End Speed",t.EndingSpeed,1,50);
				EditorGUILayout.BeginHorizontal();
					if (t.Edges!=EDGES.NONE)
							DrakkarEditor.GUIColors(Color.yellow,Color.green);
					t.Edges=(EDGES)EditorGUILayout.EnumPopup(t.Edges);
					if (t.Interpolation!=TRAIL_INTERPOLATION.NONE)
						DrakkarEditor.GUIColors(Color.yellow,Color.magenta);
					else
						DrakkarEditor.GUIColorWhite();
					t.Interpolation=(TRAIL_INTERPOLATION)EditorGUILayout.EnumPopup(t.Interpolation);
				EditorGUILayout.EndHorizontal();
				DrakkarEditor.GUIColorWhite();
				switch (t.Edges)
				{
				case EDGES.ONE:
					GUI.contentColor=Color.red;
					t.Edge1=EditorGUILayout.Slider(t.Edge1,0.01f,0.99f);
					break;
				case EDGES.TWO:
					DrakkarEditor.GUIColors(Color.red,Color.green);
					EditorGUILayout.BeginHorizontal();
						GUI.contentColor=Color.red;
						t.Edge1=EditorGUILayout.Slider(t.Edge1,0.01f,0.99f);
						GUI.contentColor=Color.yellow;
						t.Edge2=EditorGUILayout.Slider(t.Edge2,0.01f,0.99f);
					EditorGUILayout.EndHorizontal();
					break;
				}
				GUI.enabled=true;
				DrakkarEditor.GUIColorWhite();
				DrakkarEditor.EndIndent();
			}
			int mult=(int)t.Interpolation+1;
			if (t.Interpolation==TRAIL_INTERPOLATION.TENTHS)
				mult=11;
			int interpolation=(int)(t.Interpolation+1);
			DrakkarEditor.FoldOutRich(ref renderingOpen,"Rendering    - <color=#00ffff>Vertices</color>: <color=yellow>"+((t.Steps*interpolation-(interpolation-1))*(2+(int)t.Edges)).ToString()+"</color>");
			if (renderingOpen)
			{
				DrakkarEditor.BeginIndent(10);
				GUILayout.Box("The DrakkarTrail object must be <color=yellow>forward-oriented along Y axis</color>",DrakkarEditor.RichHelpBox);
				EditorGUILayout.BeginHorizontal();
				t.TrailMaterial=EditorGUILayout.ObjectField(t.TrailMaterial,typeof(Material),true) as Material;
				t.Layer=EditorGUILayout.LayerField(t.Layer);
				EditorGUILayout.EndHorizontal();
				DrakkarEditor.EndIndent();
			}
			DrakkarEditor.FoldOutRich(ref eventsOpen,"Events");
			if (eventsOpen)
			{
				DrakkarEditor.BeginIndent(10);
			#if DRAKKAR_EVENTS
				GUI.backgroundColor=Color.green;
				DrakkarEditor.ArrayGUI(so,"OnBegin","On Begin",.1f,false,true,typeof(DrakkarAction),null,DrakkarAction.AddEventFromAction);
				GUI.backgroundColor=Color.green;
				DrakkarEditor.ArrayGUI(so,"OnEnd","On End",.1f,false,true,typeof(DrakkarAction),null,DrakkarAction.AddEventFromAction);
				GUI.backgroundColor=Color.green;
				DrakkarEditor.ArrayGUI(so,"OnClear","On Clear",.1f,false,true,typeof(DrakkarAction),null,DrakkarAction.AddEventFromAction);
			#else
				DrakkarEditor.GUIColors(Color.cyan,Color.black);
				if (GUILayout.Button("Get DrakkarEvents"))
					Application.OpenURL("https://assetstore.unity.com/packages/slug/302282");
				DrakkarEditor.GUIColorWhite();
			#endif
				DrakkarEditor.EndIndent();
			}
		#if DRAKKAR_VFX
			DrakkarEditor.PropertyInspector(so,"Vfx");
		#else
			DrakkarEditor.GUIColors(Color.cyan,Color.black);
			if (GUILayout.Button("Get Drakkar VFX"))
				Application.OpenURL("https://assetstore.unity.com/packages/slug/304995");
			DrakkarEditor.GUIColorWhite();
		#endif

			//if (GUI.changed)
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(t,"ChangedTrail");
				EditorUtility.SetDirty(t);
			}
			serializedObject.ApplyModifiedProperties();
		}
		private void OnSceneGUI()
		{
			if (UnityEditor.EditorApplication.isPlaying)
				return;
			Undo.RecordObject(target,"trail");
			if (guiLength!=t.Length)
				guiLength=t.Length;
			dest=t.transform.position+t.transform.forward*guiLength;
			dest=Handles.FreeMoveHandle(dest,0.5f*HandleUtility.GetHandleSize(dest),Vector3.one,Handles.ConeHandleCap);
			bool b=GUI.changed;
			dest=Vector3.Dot(dest-t.transform.position,t.transform.forward)*t.transform.forward;
			guiLength=dest.magnitude;

			if (b)
				t.Length=guiLength;
			GUI.changed|=b;
		}
		private void disabledGUIOnPlay()
		{
			if (Application.isPlaying)
				GUI.enabled=false;
		}
	}
#endif
}