#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;

namespace Drakkar
{
	public static class DrakkarEditor
	{
		public static Color OpenActive     =Color.red;
		public static Color OpenInactive   =Color.blue;
		public static Color ClosedActive   =new(0.5f,0,0);
		public static Color ClosedInactive =Color.black;

		#region STYLES
		private static GUIStyle rightAlignLabel,rightAlignementBoldLabel,centered_Label,richHelpBox,textAreaWrap,colorPanel,boolFilled;

		public static GUIStyle BoolFilled => boolFilled??=new GUIStyle(GUI.skin.toggle)
		{
			fixedHeight=20,
			fixedWidth=20
		};
		public static GUIStyle borderPanel => colorPanel??=new GUIStyle(GUI.skin.button)
		{
			padding=new(10,10,0,10),
			richText=true,
			wordWrap=true
		};
		public static GUIStyle TextAreaWrap => textAreaWrap??=new GUIStyle(EditorStyles.textArea)
		{
			wordWrap=true
		};
		public static GUIStyle rightAlignementStyleLabel => rightAlignLabel??=new GUIStyle(EditorStyles.label)
		{
			alignment=TextAnchor.MiddleRight,
			richText=true
		};
		public static GUIStyle centeredLabel => centered_Label??=new GUIStyle(EditorStyles.label)
		{
			alignment=TextAnchor.MiddleCenter,
			richText=true
		};
		public static GUIStyle rightAlignementBoldStyleLabel => rightAlignementBoldLabel??=new GUIStyle(EditorStyles.miniBoldLabel)
		{
			alignment=TextAnchor.UpperRight,
			richText=true
		};
		public static GUIStyle RichHelpBox => richHelpBox??=new GUIStyle(EditorStyles.helpBox)
		{
			richText=true
		};
		private static GUIStyle DMBM;
		public static GUIStyle DrakkarMiniButtonMid => DMBM??=new(EditorStyles.miniButtonMid)
		{
			clipping=TextClipping.Overflow,
			name="DrakkarMiniButtonMid"
		};
		private static GUIStyle DMBL;
		public static GUIStyle DrakkarMiniButtonLeft => DMBL??=new(EditorStyles.miniButtonLeft)
		{
			clipping=TextClipping.Overflow,
			padding=new RectOffset(0,0,0,0),
			name="DrakkarMiniButtonLeft"
		};
		private static GUIStyle DMBR;
		public static GUIStyle DrakkarMiniButtonRight => DMBR??=new(EditorStyles.miniButtonRight)
		{
			clipping=TextClipping.Overflow,
			padding=new RectOffset(0,0,0,0),
			name="DrakkarMiniButtonRight"
		};
		private static GUIStyle DP;
		public static GUIStyle DrakkarPane => DP??=new(GUI.skin.box)
		{
			wordWrap=true,
			name="DrakkarPane"
		};
		private static GUIStyle DBR;
		public static GUIStyle ButtonRich => DBR??=new GUIStyle("Button")
		{
			alignment=TextAnchor.MiddleCenter,
			contentOffset=new Vector2(0,-1),
			name="DrakkarButtonRich",
			richText=true
		};
		private static GUIStyle DB;
		public static GUIStyle DrakkarButton => DB??=new GUIStyle("AppToolbarButtonMid")
		{
			alignment=TextAnchor.MiddleLeft,
			contentOffset=new Vector2(0,0),
			name="DrakkarButton",
			fixedHeight=17,
			richText=true
		};
		private static GUIStyle DBc;
		public static GUIStyle DrakkarButtonCentered => DBc??=new GUIStyle("AppToolbarButtonMid")
		{
			alignment=TextAnchor.MiddleCenter,
			contentOffset=new Vector2(0,0),
			name="DrakkarButtonCentered",
			fixedHeight=17,
			richText=true
		};
		private static GUIStyle DLBB;
		public static GUIStyle DrakkarLabelButtonBig => DLBB??=new GUIStyle(DrakkarWhiteLargeLabel)
		{
			alignment=TextAnchor.MiddleLeft,
			contentOffset=new Vector2(0,-1),
			name="DrakkarLabelButtonBig",
			fixedHeight=28,
			fontSize=20
		};
		private static GUIStyle DMBRich;
		public static GUIStyle DrakkarMiniButtonRich => DMBRich??=new("AppToolbarButtonMid")
		{
			alignment=TextAnchor.MiddleLeft,
			contentOffset=new Vector2(0,0),
			name="DrakkarButtonRich",
			fixedHeight=18,
			richText=true
		};
		private static GUIStyle DPanel;
		public static GUIStyle DrakkarPanel => DPanel??=new("helpBox")
		{
			alignment=TextAnchor.MiddleLeft,
			contentOffset=new Vector2(0,-1),
			name="DrakkarPanel",
			fixedHeight=0
		};
		private static GUIStyle DrT;
		public static GUIStyle DrakkarToggle
		{
			get
			{
				DrT??=new GUIStyle("miniButton")
				{
					name="DrakkarToggle",
					alignment=TextAnchor.MiddleCenter,
					fontStyle=FontStyle.Bold
				};
				DrT.normal=DrT.active;
				return DrT;
			}
		}
		private static GUIStyle DTT;
		public static GUIStyle DrakkarToggleTitle => DTT??=new("Button")
		{
			name="DrakkarToggleTitle",
			alignment=TextAnchor.MiddleLeft,
			fontStyle=FontStyle.Bold,
			normal=DrT.active,
			fontSize=20,
			richText=true,
			stretchWidth=true
		};
		public static GUIStyle DrakkarWhiteLargeLabel => drakkarLogoLabel??=new("whiteLargeLabel")
		{
			fontSize=22,
			contentOffset=new Vector2(0,-4),
			name="DrakkarWhiteLargeLabel",
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarLogoLabel;
		public static GUIStyle DrakkarLogoLabel => new("whiteLargeLabel")
		{
			fontSize=22,
			contentOffset=new Vector2(0,-4),
			name="DrakkarLogoLabel",
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarTitleBox;
		public static GUIStyle DrakkarTitleBox => drakkarTitleBox??=new("helpBox")
		{
			fixedHeight=30
		};
		private static GUIStyle drakkarFoldOut;
		public static GUIStyle DrakkarFoldOut => drakkarFoldOut??=new("FoldoutPreDrop")
		{
			name="DrakkarFoldout",
			contentOffset=new Vector2(0,-2),
			alignment=TextAnchor.MiddleCenter,
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarBoldWhiteLargeLabel;
		public static GUIStyle DrakkarBoldWhiteLargeLabel => drakkarBoldWhiteLargeLabel??new("whiteLargeLabel")
		{
			fontSize=24,
			contentOffset=new Vector2(0,-4),
			name="DrakkarBoldWhiteLargeLabel",
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarBoldMediumLabel;
		public static GUIStyle DrakkarBoldMediumLabel => drakkarBoldMediumLabel??new("whiteLargeLabel")
		{
			fontSize=18,
			contentOffset=new Vector2(0,-4),
			name="DrakkarBoldMediumLabel",
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarBoldLabel;
		public static GUIStyle DrakkarBoldLabel => drakkarBoldLabel??new("whiteLargeLabel")
		{
			fontSize=14,
			alignment=TextAnchor.MiddleLeft,
			contentOffset=new Vector2(0,-2),
			name="DrakkarBoldLabel",
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarBoldCenteredLabel;
		public static GUIStyle DrakkarBoldCenteredLabel => drakkarBoldCenteredLabel??=new("whiteLargeLabel")
		{
			fontSize=14,
			alignment=TextAnchor.MiddleCenter,
			contentOffset=new Vector2(0,-2),
			name="DrakkarBoldCentgeredLabel",
			fontStyle=FontStyle.Bold,
			richText=true
		};
		private static GUIStyle drakkarSmallLabel;
		public static GUIStyle DrakkarSmallLabel => drakkarSmallLabel??=new("whiteLargeLabel")
		{
			fontSize=12,
			contentOffset=new Vector2(0,-4),
			name="DrakkarSmallLabel",
			fontStyle=FontStyle.Normal,
			richText=true
		};
		private static GUIStyle drakkarTinyLabel;
		public static GUIStyle DrakkarTinyLabel => drakkarTinyLabel??=new("whiteLargeLabel")
		{
			fontSize=9,
			contentOffset=new Vector2(0,-4),
			name="DrakkarSmallLabel",
			fontStyle=FontStyle.Normal,
			richText=true
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Color OpenColor(bool b) => b?OpenInactive:ClosedInactive;
		#endregion
		#region LAYER MASKS
		public static string[] LayerNames()
		{
			string[] layers=new string[32];
			for (int i=0;i<31;i++)
				layers[i]=LayerMask.LayerToName(i);
			return layers;
		}
		public static void LayerMaskField(ref LayerMask selected, int width)
		{
			List<string> layers=null;
			List<int> layerNumbers=null;
			string[] layerNames=null;

			if (layers==null)
			{
				layers=new List<string>();
				layerNumbers=new List<int>();
				layerNames=new string[4];
			}
			else
			{
				layers.Clear();
				layerNumbers.Clear();
			}

			int emptyLayers = 0;
			for (int i=0;i<32;i++)
			{
				string layerName = LayerMask.LayerToName(i);

				if (layerName!="")
				{
					for (;emptyLayers>0;emptyLayers--) layers.Add("Layer "+(i-emptyLayers));
					layerNumbers.Add(i);
					layers.Add(layerName);
				}
				else
					emptyLayers++;
			}

			if (layerNames.Length != layers.Count)
				layerNames = new string[layers.Count];
			for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
			selected.value =  EditorGUILayout.MaskField(selected.value,layerNames,GUILayout.MaxWidth(width));
		}
		public static void LayerMaskField(ref LayerMask selected)
		{
			List<string> layers=null;
			List<int> layerNumbers=null;
			string[] layerNames=null;

			if (layers==null)
			{
				layers=new List<string>();
				layerNumbers=new List<int>();
				layerNames=new string[4];
			}
			else
			{
				layers.Clear();
				layerNumbers.Clear();
			}

			int emptyLayers=0;
			for (int i=0;i<32;i++)
			{
				string layerName=LayerMask.LayerToName(i);

				if (layerName != "")
				{
					for (;emptyLayers>0;emptyLayers--)
						layers.Add("Layer "+(i-emptyLayers));
					layerNumbers.Add(i);
					layers.Add(layerName);
				}
				else
					emptyLayers++;
			}

			if (layerNames.Length!=layers.Count)
				layerNames=new string[layers.Count];
			for (int i=0;i<layerNames.Length;i++)
				layerNames[i]=layers[i];

			selected.value=EditorGUILayout.MaskField(selected.value,layerNames);
		}
		#endregion
		#region DRAW LINE
		public static Texture2D lineTex;
		public static void DrawLine(Rect rect) => DrawLine(rect,GUI.contentColor,1.0f);
		public static void DrawLine(Rect rect,Color color) => DrawLine(rect,color,1.0f);
		public static void DrawLine(Rect rect,float width) => DrawLine(rect,GUI.contentColor,width);
		public static void DrawLine(Rect rect,Color color,float width) => DrawLine(new Vector2(rect.x,rect.y),new Vector2(rect.x+rect.width,rect.y+rect.height),color,width);
		public static void DrawLine(Vector2 pointA,Vector2 pointB) => DrawLine(pointA,pointB,GUI.contentColor,1.0f);
		public static void DrawLine(Vector2 pointA,Vector2 pointB,Color color) => DrawLine(pointA,pointB,color,1.0f);
		public static void DrawLine(Vector2 pointA,Vector2 pointB,float width) => DrawLine(pointA,pointB,GUI.contentColor,width);
		public static void DrawLine(Vector2 pointA,Vector2 pointB,Color color, float width)
		{
			Matrix4x4 matrix=GUI.matrix;
			if (!lineTex)
				lineTex=new Texture2D(1,1);
			Color savedColor=GUI.color;
			GUI.color=color;
			float angle=Vector3.Angle(pointB - pointA, Vector2.right);
			if (pointA.y>pointB.y)
				angle=-angle;
			GUIUtility.RotateAroundPivot(angle, pointA);
			GUI.DrawTexture(new Rect(pointA.x,pointA.y,(pointB-pointA).magnitude,width),lineTex);
			GUI.matrix=matrix;
			GUI.color=savedColor;
		}
		#endregion
		#region GUI
		public static GUIContent emptyContent=new("");

		private static object propertyObject;
		private static Type objectType;
		private static readonly Regex matchArrayElement=new(@"^data\[(\d+)\]$");
		public static object GetObjectFromProperty(SerializedProperty prop)
		{
			SerializedObject serializedObject=prop.serializedObject;
			string path=prop.propertyPath;

			propertyObject=serializedObject==null || serializedObject.targetObject==null ? null : serializedObject.targetObject;
			objectType=propertyObject==null ? null : propertyObject.GetType();
			if (!string.IsNullOrEmpty(path) && propertyObject!=null)
			{
				string[] splitPath=path.Split('.');
				Type fieldType=null;
				for (int i=0;i<splitPath.Length;i++)
				{
					string pathNode=splitPath[i];

					if (fieldType!=null && typeof(IList).IsAssignableFrom(fieldType))
					{
						//Debug.AssertFormat(pathNode.Equals("Array",StringComparison.Ordinal),serializedObject.targetObject,"Expected path node 'Array', but found '{0}'",pathNode);
						pathNode=splitPath[++i];
						Match elementMatch=matchArrayElement.Match(pathNode);
						if (elementMatch.Success&&int.TryParse(elementMatch.Groups[1].Value,out int index))
						{
							IList objectArray=(IList)propertyObject;
							bool validArrayEntry=objectArray!=null && index<objectArray.Count;
							propertyObject=validArrayEntry ? objectArray[index] : null;
							objectType=fieldType.IsArray
								? fieldType.GetElementType()
								: fieldType.GenericTypeArguments[0];
						}
						else
							Debug.LogErrorFormat(serializedObject.targetObject,"Unexpected path format for array item: '{0}'",pathNode);
						fieldType=null;
					}
					else
					{
						FieldInfo field;
						Type instanceType=objectType;
						BindingFlags fieldBindingFlags=BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
						do
						{
							field=instanceType.GetField(pathNode,fieldBindingFlags);
							fieldBindingFlags=BindingFlags.Instance | BindingFlags.NonPublic;
							instanceType=instanceType.BaseType;
						}
						while (field==null && instanceType!=typeof(object));
						propertyObject=field==null || propertyObject==null ? null : field.GetValue(propertyObject);
						//fieldType=field==null ? null : field.FieldType;
						//objectType=fieldType;
					}
				}
			}
			return propertyObject;
		}

		public static bool IsArrayElement(this SerializedProperty property) => property.propertyPath.Contains("Array");

		public static int ArrayIndex(this SerializedProperty property)
		{
			string path=property.propertyPath;
			int startIndex=path.LastIndexOf('[')+1;
			int endIndex=path.LastIndexOf(']');
        
			if (startIndex>0 && endIndex>startIndex)
			{
				string indexStr=path.Substring(startIndex, endIndex - startIndex);
				if (int.TryParse(indexStr, out int index))
					return index;
			}
			return -1;
		}
		public static void PrefixRect(ref Rect r)
		{
			float x=EditorGUI.PrefixLabel(r,new GUIContent(" ")).x;
			r.x=x;
			r.width-=x;
		}

		public static Rect CenterRect(Rect r,Rect Container)
		{
			r.x=Container.x+(Container.width*0.5f)-(r.width*0.5f);
			r.y=Container.y+(Container.height*0.5f)-(r.height*0.5f);
			return r;
		}

		public static Rect ExtractRightRect(ref Rect container,float width)
		{
			Rect n=container;
			container.width-=width;
			n.x=container.xMax;
			n.width=width;
			return n;
		}
		public static Rect ExtractRightRectPercentage(ref Rect container,float perc) => ExtractRightRect(ref container,(float)(container.width*perc));
		public static Rect ExtractLeftRect(ref Rect container,float width)
		{
			Rect n=container;
			container.width-=width;
			container.x+=width;
			n.width=width;
			return n;
		}

		public static Rect RectHorizontalPadding(Rect original,float padding)
		{
			Rect n=original;
			n.x+=padding;
			n.width-=padding*2;
			return n;
		}

		public static bool GUIToggle(bool b,string title,Rect pos,bool customColor=false)
		{
			int ind=EditorGUI.indentLevel;
			EditorGUI.indentLevel=0;
			GUI.backgroundColor=(customColor) ? b ? GUI.backgroundColor : Color.black : b ? Color.cyan : Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUI.Button(pos,title))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
			EditorGUI.indentLevel=ind;
			return b;
		}
		public static bool GUIToggle(bool b,string title,Rect pos,Color onCol,Color offCol)
		{
			int ind=EditorGUI.indentLevel;
			EditorGUI.indentLevel=0;
			GUI.backgroundColor=b ? onCol : offCol;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			if (GUI.Button(pos,title))
				b=!b;
			GUI.backgroundColor=Color.white;
			EditorGUI.indentLevel=ind;
			return b;
		}
		public static bool GUIFolder(bool b,string title,Rect pos)
		{
			int ind=EditorGUI.indentLevel;
			EditorGUI.indentLevel=0;
			GUI.backgroundColor=b ? Color.white : Color.gray;
			//GUI.contentColor=b ? Color.white : Color.gray;
			GUI.color=b ? Color.white : Color.white;
			if (GUI.Button(pos,title,DrakkarEditor.DrakkarFoldOut))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
			EditorGUI.indentLevel=ind;
			return b;
		}
		public static bool GUIFolder(bool b,GUIContent title,Rect pos)
		{
			int ind=EditorGUI.indentLevel;
			EditorGUI.indentLevel=0;
			GUI.backgroundColor=b ? Color.white : Color.gray;
			GUI.color=b ? Color.white : Color.white;
			if (GUI.Button(pos,title,DrakkarEditor.DrakkarFoldOut))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
			EditorGUI.indentLevel=ind;
			return b;
		}
		public static void GUIRoundedRectangle(Rect fillRect,float radius,Color col)
		{
			Color oldCol=Handles.color;
			Handles.color=col;
			Handles.BeginGUI();
			Handles.DrawSolidArc(new Vector3(fillRect.x+radius,   fillRect.y+radius)   ,Vector3.forward,Vector3.left,90,radius);
			Handles.DrawSolidArc(new Vector3(fillRect.x+radius,   fillRect.yMax-radius),Vector3.forward,Vector3.left,-90,radius);
			Handles.DrawSolidArc(new Vector3(fillRect.xMax-radius,fillRect.y+radius)   ,Vector3.forward,Vector3.right,-90,radius);
			Handles.DrawSolidArc(new Vector3(fillRect.xMax-radius,fillRect.yMax-radius),Vector3.forward,Vector3.right,90,radius);
			Handles.EndGUI();
			Handles.color=oldCol;
			oldCol=GUI.color;
			GUI.color=new Color32(255,255,255,255);
			float radius2=radius+radius;
			fillRect.x+=radius;
			fillRect.width-=radius2-1;	// 1 is pixel safe-margin
			EditorGUI.DrawRect(fillRect,col);
			fillRect.x-=radius;
			fillRect.width+=radius2;
			fillRect.yMin+=radius;
			fillRect.yMax-=radius;
			EditorGUI.DrawRect(fillRect,col);
			GUI.color=oldCol;
		}
		public static void GUIRoundedRectangleOutline(Rect rect,float radius,Color col,float thickness=1)
		{
			Color oldCol=Handles.color;
			Handles.color=col;
			Handles.BeginGUI();
			Handles.DrawWireArc(new Vector3(rect.x+radius,   rect.y+radius)   ,Vector3.forward,Vector3.left,90  ,radius,thickness);
			Handles.DrawWireArc(new Vector3(rect.x+radius,   rect.yMax-radius),Vector3.forward,Vector3.left,-90 ,radius,thickness);
			Handles.DrawWireArc(new Vector3(rect.xMax-radius,rect.y+radius)   ,Vector3.forward,Vector3.right,-90,radius,thickness);
			Handles.DrawWireArc(new Vector3(rect.xMax-radius,rect.yMax-radius),Vector3.forward,Vector3.right,90 ,radius,thickness);

			Handles.DrawLines(new Vector3[]
			{
				new(rect.x   +radius,   rect.y),
				new(rect.xMax-radius,   rect.y),
				new(rect.x   +radius,   rect.yMax),
				new(rect.xMax-radius,   rect.yMax),
				new(rect.x,   rect.y   +radius),
				new(rect.x,   rect.yMax-radius),
				new(rect.xMax,rect.y   +radius),
				new(rect.xMax,rect.yMax-radius)
			});

			Handles.EndGUI();
			Handles.color=oldCol;
		}

		public static float GUIArray(Rect pos,SerializedProperty prop,Color color,float indent=10,bool showLabel=true,string name="",Type dragnDropType=null,Action<UnityEngine.Object,SerializedProperty> dragnDropAction=null)
		{
			float lines=1;
			float totalHeight=0;
			int count=prop.arraySize;
			Rect initPos=pos;
			GUI.contentColor=Color.white;

			#region FOLDOUT
			Color offColor=color*0.5f;
			offColor.a=1;
			GUI.backgroundColor=prop.isExpanded ? color : offColor;
			pos.width-=14+60;
			pos.height=18;
			totalHeight+=18;
			if (string.IsNullOrEmpty(name))
				name=prop.name;
			if (GUI.Button(pos,name+" [<color=yellow><b>"+count+"</b></color>]",DrakkarMiniButtonRich))
				prop.isExpanded=!prop.isExpanded;

			dragAndDropProperty(pos,prop,dragnDropType,dragnDropAction);
			#endregion
			#region ARRAY BUTTON
			Rect minibutt=pos;
			minibutt.x+=pos.width;
			minibutt.width=20;
			GUI.backgroundColor=OpenColor(showArrayButtons);
			if (GUI.Button(minibutt,"\u21b4",EditorStyles.miniButtonRight))
				showArrayButtons=!showArrayButtons;
			#endregion
			GUI.backgroundColor=Color.white;
			pos.x+=pos.width+20;
			if (arrayItemButtonsGUI(pos,prop))
				return totalHeight;

			if (prop.isExpanded)
			{
				initPos.x+=indent;
				initPos.width-=indent+(showArrayButtons? 54 : 0);
				Rect elemPos=initPos;
				elemPos.width=15;
				elemPos.y+=18;
				initPos.y+=18;
				for (int i=0;i<count;i++)
				{
					elemPos.x=initPos.x+initPos.width;
					SerializedProperty p=prop.GetArrayElementAtIndex(i);
					float propertyHeight=EditorGUI.GetPropertyHeight(p);
					initPos.height=propertyHeight;
					if (showLabel)
						EditorGUI.PropertyField(initPos,p);
					else
						EditorGUI.PropertyField(initPos,p,emptyContent);

					#region POSITION BUTTONS
					Rect buttonRect=elemPos;
					buttonRect.height=18;
					if (GUI.Button(buttonRect,DrakkarAssetsContainer.instance.ArrowUp,DrakkarMiniButtonLeft))
						prop.MoveArrayElement(i,Mathf.Clamp(i-1,0,10000));
					buttonRect.x+=15;
					if (GUI.Button(buttonRect,"+",EditorStyles.miniButtonMid))
						prop.InsertArrayElementAtIndex(i);
					buttonRect.x+=15;
					if (GUI.Button(buttonRect,"-",EditorStyles.miniButtonMid))
					{
						prop.DeleteArrayElementAtIndex(i);
						break;
					}
					buttonRect.x+=15;
					if (GUI.Button(buttonRect,DrakkarAssetsContainer.instance.ArrowDown,DrakkarMiniButtonRight))
						prop.MoveArrayElement(i,Mathf.Clamp(i+1,0,prop.arraySize-1));
					#endregion
					lines+=Mathf.FloorToInt(propertyHeight/17);
					elemPos.y+=propertyHeight;
					initPos.y+=propertyHeight;
					totalHeight+=propertyHeight;
				}
			}
			return totalHeight;
		}

		private static void dragAndDropArray<T>(Rect pos,ref T[] arr,Type type) where T: UnityEngine.Object
		{
			if (type==null)
				return;
			Event evt=Event.current;
			if (evt.type==EventType.DragUpdated || evt.type==EventType.DragPerform)
			{
				if (pos.Contains(evt.mousePosition))
				{
					UnityEngine.Object[] objects=DragAndDrop.objectReferences;
					Type objType=objects[0].GetType();
					bool isGameObject=objType==typeof(GameObject);
					bool accepted=objType==type || isGameObject;
					if (accepted)
					{
						DragAndDrop.AcceptDrag();
						DragAndDrop.visualMode=DragAndDropVisualMode.Copy;
						if (evt.type==EventType.DragPerform)
						{
							if (isGameObject)
							{
								foreach (UnityEngine.Object draggedObject in objects)
								{
									UnityEngine.Object t=(UnityEngine.Object)((draggedObject as GameObject).GetComponent(type));
									if (t is T)
										arr=arr.Add((T)t);
								}
							}
							else
								foreach (UnityEngine.Object draggedObject in objects)
									arr=arr.Add((T)draggedObject);
						}
					}
					else
						DragAndDrop.visualMode=DragAndDropVisualMode.Rejected;
				}
			}
		}
		private static void dragAndDropProperty(Rect pos,SerializedProperty prop,Type type,Action<UnityEngine.Object,SerializedProperty> dragnDropAction=null)
		{
			if (type==null)
				return;
			Event evt=Event.current;
			if (evt.type==EventType.DragUpdated || evt.type==EventType.DragPerform)
			{
				if (pos.Contains(evt.mousePosition))
				{
					UnityEngine.Object[] objects=DragAndDrop.objectReferences;
					Type objType=objects[0].GetType();
					bool isGameObject=objType==typeof(GameObject);
					bool accepted=type.IsAssignableFrom(objType) || isGameObject;
					if (accepted)
					{
						DragAndDrop.AcceptDrag();
						DragAndDrop.visualMode=DragAndDropVisualMode.Copy;
						if (evt.type==EventType.DragPerform)
						{
							if (isGameObject)
							{
								foreach (UnityEngine.Object draggedObject in objects)
								{
									int ind=prop.arraySize;
									prop.InsertArrayElementAtIndex(ind);
									SerializedProperty newElement=prop.GetArrayElementAtIndex(ind);
									UnityEngine.Object o=(type==typeof(GameObject)) ? draggedObject : (draggedObject as GameObject).GetComponent(type);
									if (dragnDropAction!=null)
										dragnDropAction(o,newElement);
									else
										newElement.objectReferenceValue=o;
								}
								prop.serializedObject.ApplyModifiedProperties();
							}
							else
							{
								foreach (UnityEngine.Object draggedObject in objects)
								{
									int ind=prop.arraySize;
									prop.InsertArrayElementAtIndex(ind);
									SerializedProperty newElement=prop.GetArrayElementAtIndex(ind);
									if (dragnDropAction!=null)
										dragnDropAction(draggedObject,newElement);
									else
										newElement.objectReferenceValue=draggedObject;
								}
								prop.serializedObject.ApplyModifiedProperties();
							}
						}
					}
					else
						DragAndDrop.visualMode=DragAndDropVisualMode.Rejected;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool arrayItemButtonsGUI(Rect pos,SerializedProperty prop)
		{
			GUI.backgroundColor=Color.white;
			pos.height=18;
			pos.width=18;
			if (GUI.Button(pos,"+",EditorStyles.miniButtonLeft))
			{
				prop.InsertArrayElementAtIndex(prop.arraySize);
				return true;
			}
			pos.x+=18;
			if (GUI.Button(pos,"-",EditorStyles.miniButtonLeft) && prop.arraySize>0)
			{
				prop.DeleteArrayElementAtIndex(prop.arraySize-1);
				return true;
			}
			pos.x+=18;
			if (GUI.Button(pos,"x",EditorStyles.miniButtonLeft) && EditorUtility.DisplayDialog("Delete All","Remove all elements?","Yes","No"))
			{

				int l=prop.arraySize;
				for (int i=0;i<l;i++)
					prop.DeleteArrayElementAtIndex(0);
				return true;
			}
			return false;
		}
		#region GUI COLORS
		public static void GUIColors(Color fg,Color bg)
		{
			GUI.contentColor=fg;
			GUI.backgroundColor=bg;
		}
		public static void GUIColorWhite()
		{
			GUI.contentColor=Color.white;
			GUI.backgroundColor=Color.white;
			GUI.color=Color.white;
		}
		#endregion
		#endregion
		#region ARRAYGUI
		public static bool showArrayButtons=true;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,string name,SerializedProperty prop,Action<T,int> inspector,Comparison<T> sort) where T : new()
		{
			ArrayGUI(ref arr,name,prop,inspector);
			EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.contentColor=Color.yellow;
				if (GUILayout.Button("Sort",EditorStyles.miniButton,GUILayout.MaxWidth(66)))
					Array.Sort(arr,sort);
				GUI.contentColor=Color.white;
			EditorGUILayout.EndHorizontal();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,string name,SerializedProperty prop,Action<T,int> inspector,bool noEdit=false) where T : new()
		{
			bool isExpanded=prop.isExpanded;
			ArrayGUI<T>(ref arr,name,ref isExpanded,inspector,null,null,noEdit);
			prop.isExpanded=isExpanded;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,int start,int length,string name,SerializedProperty prop,Action<T,int> inspector) where T : new()
		{
			bool isExpanded=prop.isExpanded;
			ArrayGUI<T>(ref arr,start,length,name,ref isExpanded,inspector);
			prop.isExpanded=isExpanded;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,string name,ref bool isExpanded,Action<T,int> inspector,Comparison<T> sort,bool noEdit=false) where T : new()
		{
			ArrayGUI<T>(ref arr,name,ref isExpanded,inspector,null,null,noEdit);
			EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.contentColor=Color.yellow;
				if (sort!=null && GUILayout.Button("Sort",EditorStyles.miniButton,GUILayout.MaxWidth(66)))
					Array.Sort(arr,sort);
				GUI.contentColor=Color.white;
			EditorGUILayout.EndHorizontal();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,string name,ref bool isExpanded,Action<T,int> inspector,Action<T[]> arrayAction=null,Action<T> elementAction=null,bool noEdit=false) where T : new()
		{
			if (arr==null)
				arr=new T[0];
			EditorGUILayout.BeginVertical();
			int no=arr.Length;
			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor=OpenColor(isExpanded);
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button(name+" [<color=yellow><b>"+no+"</b></color>]",DrakkarMiniButtonRich))
				isExpanded=!isExpanded;
			GUI.backgroundColor=OpenColor(showArrayButtons);
			if (GUILayout.Button("\u21b4",EditorStyles.miniButtonRight,GUILayout.MaxWidth(21),GUILayout.MinHeight(16)))
				showArrayButtons=!showArrayButtons;
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor=Color.white;
			arrayItemButtons<T>(ref arr,ref no);
			EditorGUILayout.EndHorizontal();

			if (isExpanded)
			{
				arrayAction?.Invoke(arr);
				BeginIndent(10);
				for (int i=0;i<no;i++)
				{
					if (arr==null)
						return;
					if (showArrayButtons)
						EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
					elementAction?.Invoke(arr[i]);
					inspector(arr[i],i);
					EditorGUILayout.EndVertical();
					if (showArrayButtons)
					{
						if (!noEdit)
						{
							if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowUp,DrakkarMiniButtonLeft,GUILayout.MaxWidth(17),GUILayout.MinHeight(15)))
							{
								int dest=(i>0)?i-1:0;
								(arr[i],arr[dest])=(arr[dest],arr[i]);
							}
							if (GUILayout.Button("+",EditorStyles.miniButtonMid,GUILayout.MaxWidth(17)))
								AddToArray<T>(ref arr,i+1);
							if (GUILayout.Button("-",EditorStyles.miniButtonMid,GUILayout.MaxWidth(17)) && EditorUtility.DisplayDialog("Delete Element","Remove selected element?","Yes","No"))
							{
								RemoveFromArray<T>(ref arr,i);
								no--;
							}
							if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowDown,DrakkarMiniButtonRight,GUILayout.MaxWidth(17),GUILayout.MinHeight(15)))
							{
								int dest=(i<no-1)?i+1:no-1;
								(arr[i],arr[dest])=(arr[dest],arr[i]);
							}
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				EndIndent();
			}
			EditorGUILayout.EndVertical();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void arrayItemButtons<T>(ref T[] arr,ref int no) where T : new()
		{
			GUI.backgroundColor=Color.white;
			if (GUILayout.Button("+",EditorStyles.miniButtonLeft,GUILayout.MaxWidth(18)))
			{
				Array.Resize<T>(ref arr, arr.Length+1);
				arr[^1]=new T();
				no++;
			}
			if (GUILayout.Button("-",EditorStyles.miniButtonMid,GUILayout.MaxWidth(18)) && no>0 &&EditorUtility.DisplayDialog("Delete Element","Remove the last element?","Yes","No"))
			{
				RemoveFromArray<T>(ref arr,no-1);
				no--;
			}
			if (GUILayout.Button("X",EditorStyles.miniButtonRight,GUILayout.MaxWidth(18)) && EditorUtility.DisplayDialog("Delete All","Remove all elements?","Yes","No"))
				arr=null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,int start,int length,string name,ref bool isExpanded,Action<T,int> d) where T: new()
		{
			if (arr==null)
				arr=new T[0];
			EditorGUILayout.BeginVertical();
			int no = length;

			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor=OpenColor(isExpanded);
			if(GUILayout.Button(name+" [<color=white>"+no+"</color>]",DrakkarMiniButtonRich,GUILayout.MaxHeight(19)))
				isExpanded=!isExpanded;
			GUI.backgroundColor=Color.white;
			arrayItemButtons<T>(ref arr,ref no);
			EditorGUILayout.EndHorizontal();
			if (isExpanded)
			{
				BeginIndent(10);
				for (int i=start;i<start+no;i++)
				{
					if (arr==null)
						return;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
					d(arr[i],i);
					EditorGUILayout.EndVertical();

					if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowUp,DrakkarMiniButtonLeft,GUILayout.MaxWidth(17),GUILayout.MinHeight(15)))
					{
						int dest=(i>0)?i-1:0;
						(arr[i],arr[dest])=(arr[dest],arr[i]);
					}
					if (GUILayout.Button("+",DrakkarMiniButtonMid,GUILayout.MaxWidth(18)))
						AddToArray<T>(ref arr,i+1);
					if (GUILayout.Button("-",DrakkarMiniButtonMid,GUILayout.MaxWidth(18)) && EditorUtility.DisplayDialog("Delete Element","Remove selected element?","Yes","No"))
					{
						RemoveFromArray<T>(ref arr,i);
						no--;
					}
					if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowDown,DrakkarMiniButtonRight,GUILayout.MaxWidth(17),GUILayout.MinHeight(15)))
					{
						int dest=(i<no-1)?i+1:no-1;
						(arr[i],arr[dest])=(arr[dest],arr[i]);
					}
					EditorGUILayout.EndHorizontal();
				}
				EndIndent();
			}
			EditorGUILayout.EndVertical();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,string name,SerializedProperty prop,bool indexed,Type type=null) where T:UnityEngine.Object, new()
		{
			bool isExpanded=prop.isExpanded;
			ArrayGUI<T>(ref arr,name,ref isExpanded,indexed,type);
			prop.isExpanded=isExpanded;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI<T>(ref T[] arr,string name,ref bool isExpanded,bool indexed,Type type=null) where T:UnityEngine.Object, new()
		{
			if (arr==null)
				arr=new T[0];
			EditorGUILayout.BeginVertical();
			int no=arr.Length;

			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor=OpenColor(isExpanded);
			if(GUILayout.Button(name+" [<color=white>"+no+"</color>]",DrakkarMiniButtonRich,GUILayout.MaxHeight(19)))
				isExpanded=!isExpanded;
			GUI.backgroundColor=Color.white;
			dragAndDropArray(GUILayoutUtility.GetLastRect(),ref arr,type);

			if (GUILayout.Button("+",EditorStyles.miniButtonLeft,GUILayout.MaxWidth(18)))
			{
				Array.Resize<T>(ref arr, arr.Length+1);
				no++;
			}
			if (GUILayout.Button("-",EditorStyles.miniButtonMid,GUILayout.MaxWidth(18)) && (no>0))
			{
				RemoveFromArray<T>(ref arr,no-1);
				no--;
			}
			if (GUILayout.Button("X",EditorStyles.miniButtonRight,GUILayout.MaxWidth(18)) && EditorUtility.DisplayDialog("Delete All","Remove all elements?","Yes","No"))
				arr=null;
			EditorGUILayout.EndHorizontal();
			if (isExpanded)
			{
				for (int i=0;i<no;i++)
				{
					if (arr==null)
						return;
					EditorGUILayout.BeginHorizontal();
					if (indexed)
						EditorGUILayout.LabelField(i.ToString()+":",GUILayout.MaxWidth(50));
					arr[i]=(T)EditorGUILayout.ObjectField(arr[i],typeof(T),true);

					if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowUp,DrakkarMiniButtonLeft,GUILayout.MaxWidth(18),GUILayout.MinHeight(15)))
					{
						int dest=(i>0)?i-1:0;
						(arr[i],arr[dest])=(arr[dest],arr[i]);
					}
					if (GUILayout.Button("+",DrakkarMiniButtonMid,GUILayout.MaxWidth(18)))
						AddToArray<T>(ref arr,i+1);
					if (GUILayout.Button("-",DrakkarMiniButtonMid,GUILayout.MaxWidth(18)))
					{
						RemoveFromArray<T>(ref arr,i);
						no--;
					}
					if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowDown,DrakkarMiniButtonRight,GUILayout.MaxWidth(18),GUILayout.MinHeight(15)))
					{
						int dest=(i<no-1)?i+1:no-1;
						(arr[i],arr[dest])=(arr[dest],arr[i]);
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUI(SerializedObject obj,string name,string displayName="",float indent=0,bool indexed=true,bool overrideColor=false,Type dragnDropType=null,Action<SerializedProperty> addAction=null,Action<UnityEngine.Object,SerializedProperty> dragnDropAction=null,bool noEdit=false)
		{
			EditorGUI.indentLevel=0;
			EditorGUILayout.BeginVertical();
			SerializedProperty pr=obj.FindProperty(name);
			SerializedProperty iV=obj.FindProperty(name+".Array.size");
			if (iV==null)
			{
				//NameAndReasonError("ArrayGUI","Object or Property is null");
				EditorGUILayout.EndVertical();
				return;
			}
			int no=iV.intValue;
			EditorGUILayout.BeginHorizontal();
			Color ovCol=GUI.backgroundColor;
			Color ovColOff=ovCol*0.5f;
			ovColOff.a=1;
			GUI.backgroundColor=overrideColor ? pr.isExpanded? ovCol : ovColOff : OpenColor(pr.isExpanded);
			if (GUILayout.Button((string.IsNullOrEmpty(displayName)?name:displayName)+" [<color=white>"+no+"</color>]",DrakkarMiniButtonRich,GUILayout.MaxHeight(19)))
				pr.isExpanded=!pr.isExpanded;
			dragAndDropProperty(GUILayoutUtility.GetLastRect(),pr,dragnDropType,dragnDropAction);

			GUI.backgroundColor=OpenColor(showArrayButtons);
			if (GUILayout.Button("\u21b4",EditorStyles.miniButtonRight,GUILayout.MaxWidth(21),GUILayout.MinHeight(16)))
				showArrayButtons=!showArrayButtons;
			GUI.backgroundColor=Color.white;

			obj.Update();
			if (GUILayout.Button("+",EditorStyles.miniButtonLeft,GUILayout.MaxWidth(18)))
			{
				iV.intValue++;
				obj.ApplyModifiedProperties();
				addAction?.Invoke(pr.GetArrayElementAtIndex(iV.intValue-1));
			}
			if (GUILayout.Button("-",EditorStyles.miniButtonMid,GUILayout.MaxWidth(18)))
			{
				iV.intValue--;
				obj.ApplyModifiedProperties();
			}
			if (GUILayout.Button("X",EditorStyles.miniButtonRight,GUILayout.MaxWidth(18)))
			{
				if (EditorUtility.DisplayDialog("Delete All","Remove all elements?","Yes","No"))
					pr.ClearArray();
				obj.ApplyModifiedProperties();
			}
			EditorGUILayout.EndHorizontal();
			if (pr.isExpanded)
			{
				for (int i=0;i<no;i++)
				{
					SerializedProperty prop=obj.FindProperty(string.Format("{0}.Array.data[{1}]",name,i));
					if (prop==null)
						return;
					if (indent>0)
						BeginIndent(indent);

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(prop,indexed? new GUIContent(i+":") : emptyContent);
					if (showArrayButtons && !noEdit)
					{
						if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowUp,DrakkarMiniButtonLeft,GUILayout.MaxWidth(15),GUILayout.MinHeight(15)))
							pr.MoveArrayElement(i,(i>0) ? i-1 : 0);
						if (GUILayout.Button("+",DrakkarMiniButtonMid,GUILayout.MaxWidth(15)))
						{
							pr.InsertArrayElementAtIndex(i);
							obj.ApplyModifiedProperties();
							addAction?.Invoke(pr.GetArrayElementAtIndex(i+1));
						}
						if (GUILayout.Button("-",DrakkarMiniButtonMid,GUILayout.MaxWidth(15)))
						{
							pr.DeleteArrayElementAtIndex(i);
							obj.ApplyModifiedProperties();
						}
						if (GUILayout.Button(DrakkarAssetsContainer.instance.ArrowDown,DrakkarMiniButtonRight,GUILayout.MaxWidth(15),GUILayout.MinHeight(15)))
							pr.MoveArrayElement(i,(i<(no-1)) ? i+1 : no-1);
					}
					EditorGUILayout.EndHorizontal();
					if (indent>0)
						EndIndent();
				}
			}
			EditorGUILayout.EndVertical();
			obj.ApplyModifiedProperties();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArrayGUIReadOnly<T>(ref T[] arr,int count,string name,ref bool isExpanded,bool indexed) where T:UnityEngine.Object, new()
		{
			if (arr==null)
				return;

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor=OpenColor(isExpanded);
			if(GUILayout.Button(name+" [<color=white>"+count+"</color>] Read-only",DrakkarMiniButtonRich,GUILayout.MaxHeight(19)))
				isExpanded=!isExpanded;
			GUI.backgroundColor=Color.white;

			EditorGUILayout.EndHorizontal();
			if (isExpanded)
			{
				for (int i=0;i<count;i++)
				{
					EditorGUILayout.BeginHorizontal();
					if (indexed)
						EditorGUILayout.LabelField(i.ToString()+":",GUILayout.MaxWidth(50));
					EditorGUILayout.ObjectField(arr[i],typeof(T),true);
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
		}
		#endregion
		#region TOGGLE
		public static void Toggle(SerializedProperty sp,string text)
		{
			bool b=sp.boolValue;
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
			sp.boolValue=b;
			sp.serializedObject.ApplyModifiedProperties();
		}
		public static void Toggle(SerializedProperty sp,string text,float width)
		{
			bool b=sp.boolValue;
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17),GUILayout.MinWidth(width),GUILayout.MaxWidth(width)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
			sp.boolValue=b;
		}
		public static void Toggle(ref bool b,string text,float width)
		{
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17),GUILayout.MinWidth(width),GUILayout.MaxWidth(width)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void Toggle(ref bool b,string text,float width,Color col)
		{
			GUI.backgroundColor=b?col:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17),GUILayout.MaxWidth(width)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void Toggle(ref ShortBitArray ba,int pos,string text,int width,Color col,GUIStyle style)
		{
			bool t=ba.Get(pos);
			Toggle(ref t,text,width,col,style);
			ba.Set(pos,t);
		}
		public static void Toggle(ref bool b,string text,float width,Color col,GUIStyle style)
		{
			GUI.backgroundColor=b?col:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=Color.white;
			if (GUILayout.Button(text,style,GUILayout.MaxHeight(17),GUILayout.MaxWidth(width),GUILayout.MinWidth(2)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void Toggle(ref bool b,string text,Color col)
		{
			GUI.backgroundColor=b?col:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17),GUILayout.MinWidth(2)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void Toggle(ref bool b,string text)
		{
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void Toggle(ref bool b,string text,string tooltip,string textoff="")
		{
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (string.IsNullOrEmpty(textoff))
				textoff=text;
			if (GUILayout.Button(new GUIContent(b?text:textoff,tooltip),GUILayout.MaxHeight(17)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void Toggle(ref bool b,string text,string tooltip,float width)
		{
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(new GUIContent(text,tooltip),GUILayout.MaxHeight(17),GUILayout.MaxWidth(width)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void ToggleByteBitArray(ref byte arr,int bitNum,string[] names)
		{
			bitNum=Mathf.Min(bitNum,8);
			EditorGUILayout.BeginHorizontal();
			bool b;
			for (int i=0;i<bitNum;i++)
			{
				b=ByteBitArray.Get(ref arr,i);
				Toggle(ref b,names[i],Color.magenta);
				ByteBitArray.Set(ref arr,(byte)i,b);
			}
			EditorGUILayout.EndHorizontal();
		}

		public static void ToggleShortArrayBit(ref ShortBitArray arr,int bit,string text)
		{
			bool b=arr.Get(bit);
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17)))
				arr.Set(bit,!b);
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void ToggleShortArrayBit(ref ShortBitArray arr,int bit,string text,float width)
		{
			bool b=arr.Get(bit);
			GUI.backgroundColor=b?Color.cyan:Color.black;
			GUI.contentColor=b?Color.white:Color.gray;
			GUI.color=b?Color.cyan:Color.white;
			if (GUILayout.Button(text,GUILayout.MaxHeight(17),GUILayout.MaxWidth(width)))
				arr.Set(bit,!b);
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		public static void ToggleTitle(ref bool b,string text)
		{
			GUI.backgroundColor=b ? Color.cyan : Color.black;
			GUI.contentColor=b ? Color.white : Color.gray;
			GUI.color=b ? Color.cyan : Color.white;
			if (GUILayout.Button(text,GUILayout.MinHeight(30),GUILayout.MaxHeight(30)))
				b=!b;
			GUI.contentColor=Color.white;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
		}
		#endregion
		#region PREVIEW
		public static void TexturePreview(Texture2D tex,float sizeX,float sizeY)
		{
			EditorGUILayout.LabelField("",GUILayout.MinWidth(sizeX),GUILayout.MinHeight(sizeY),GUILayout.MaxWidth(sizeX));
			EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetLastRect(),tex);
		}
		public static void DrawUVQuad(Vector2[] uvs,Rect viewport)
		{
			Rect r=new()
			{
				x=viewport.x+uvs[1].x*viewport.width,
				width=(uvs[0].x-uvs[1].x)*viewport.width,
				y=viewport.y+(1-uvs[1].y)*viewport.height,
				height=(uvs[1].y-uvs[3].y)*viewport.height
			};
			GUI.color=Color.cyan;
			GUI.DrawTexture(new Rect(r.x,r.y,r.width,1),EditorGUIUtility.whiteTexture);
			GUI.DrawTexture(new Rect(r.x+r.width,r.y,1,r.height),EditorGUIUtility.whiteTexture);
			GUI.DrawTexture(new Rect(r.x,r.y+r.height,r.width,1),EditorGUIUtility.whiteTexture);
			GUI.DrawTexture(new Rect(r.x,r.y,1,r.height),EditorGUIUtility.whiteTexture);
		}
		#endregion
		#region INSPECTORS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool PropertyInspector(SerializedObject ob,string pr)
		{
			SerializedProperty p=ob.FindProperty(pr);
			if (p==null) return false;
			EditorGUILayout.PropertyField(ob.FindProperty(pr));
			ob.ApplyModifiedProperties();
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool PropertyInspectorNoLabel(SerializedObject ob,string pr)
		{
			SerializedProperty p=ob.FindProperty(pr);
			if (p==null) return false;
			EditorGUILayout.PropertyField(ob.FindProperty(pr),new GUIContent());
			ob.ApplyModifiedProperties();
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void PropertyArrayInspector(SerializedObject ob,string property,int index)
		{
			SerializedProperty sp=ob.FindProperty(property);
			if (sp==null)
			{
				GUI.contentColor=Color.yellow;
				EditorGUILayout.LabelField("Property: "+property+" is missing!");
				return;
			}
			EditorGUILayout.PropertyField(sp.GetArrayElementAtIndex(index));
			ob.ApplyModifiedProperties();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void PropertySubArrayInspector(SerializedObject ob,string property,int index,string subProperty)
		{
			SerializedProperty sp=ob.FindProperty(property);
			if (sp==null)
			{
				GUI.contentColor=Color.yellow;
				EditorGUILayout.LabelField("Property: "+property+" is missing!");
				return;
			}
			sp=sp.GetArrayElementAtIndex(index);
			EditorGUILayout.PropertyField(sp.FindPropertyRelative(subProperty));
			ob.ApplyModifiedProperties();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void PropertySubArrayInspector(SerializedObject ob,SerializedProperty property,int index,string subProperty)
		{
			SerializedProperty sp=property;
			sp=sp.GetArrayElementAtIndex(index);
			EditorGUILayout.PropertyField(sp.FindPropertyRelative(subProperty));
			ob.ApplyModifiedProperties();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TimeSliceInspector(ref TimeSlices ts)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Time Slice",GUILayout.MaxWidth(80));
			ts=(TimeSlices)EditorGUILayout.EnumPopup(ts,GUILayout.MaxWidth(120));
			EditorGUILayout.EndHorizontal();
		}
		#endregion
		#region FORMATTING GROUPS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MainLogo(string name)
		{
			GUI.backgroundColor=OpenInactive;
			EditorGUILayout.BeginHorizontal(DrakkarTitleBox);
			GUI.color=Color.white;
			GUILayout.Label(DrakkarAssetsContainer.instance.DrakkarLogo,GUILayout.MaxWidth(28),GUILayout.MaxHeight(23));
			GUILayout.Label(name,DrakkarLogoLabel);
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TextureLogo(string name,string icon)
		{
			GUI.backgroundColor=OpenInactive;
			EditorGUILayout.BeginHorizontal(DrakkarTitleBox);
			GUI.color=Color.white;
			if (!string.IsNullOrEmpty(icon))
				GUILayout.Label(DrakkarAssetsContainer.GetTexture(icon),DrakkarWhiteLargeLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));

			GUILayout.Label(name,DrakkarLogoLabel);
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor=Color.white;
		}
		public static void TextureLogo(UnityEngine.Object obj,string icon,ref bool opt,string optName,float optWidth)
		{
			TextureLogo(obj,icon);
			Rect r=GUILayoutUtility.GetLastRect();
			r.xMin=r.xMax-optWidth;
			r.yMin=r.center.y-9;
			r.yMax=r.center.y+7;
			GUI.backgroundColor=Color.blue;
			opt=GUIToggle(opt,optName,r,true);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TextureLogo(UnityEngine.Object obj,string icon)
		{
			GUI.backgroundColor=OpenInactive;
			EditorGUILayout.BeginHorizontal(DrakkarTitleBox);
			GUI.color=Color.white;
			if (!string.IsNullOrEmpty(icon))
			{
				Texture txt=DrakkarAssetsContainer.GetTexture(icon);
				if (txt!=null)
					GUILayout.Label(txt,DrakkarLogoLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));
				else
				{
					txt=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Gizmos/"+icon+".png");
					GUILayout.Label(txt,DrakkarLogoLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));
				}
			}
			else
				iconFromObject(obj);
			GUILayout.Label(ObjectNames.NicifyVariableName(obj.GetType().Name),DrakkarWhiteLargeLabel);
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TextureLogo(UnityEngine.Object obj,bool rename=false,string name="")
		{
			GUI.backgroundColor=OpenInactive;
			EditorGUILayout.BeginHorizontal(DrakkarTitleBox);
			GUI.color=Color.white;
			Texture txt=ObjectIcon(obj);
			if (txt!=null)
				GUILayout.Label(txt,DrakkarLogoLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));

			if (!rename)
				name=ObjectNames.NicifyVariableName(obj.GetType().Name);
			GUILayout.Label(name,DrakkarWhiteLargeLabel);
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TextureLogo(UnityEngine.Object obj,bool rename,string name,ref bool opt,string optName,float optWidth=70)
		{
			TextureLogo(obj,rename,name);
			Rect r=GUILayoutUtility.GetLastRect();
			r.xMin=r.xMax-optWidth;
			r.yMin=r.center.y-9;
			r.yMax=r.center.y+7;
			GUI.backgroundColor=Color.blue;
			opt=GUIToggle(opt,optName,r,true);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TextureLogoFolder(UnityEngine.Object obj,string icon,bool open)
		{
			GUI.backgroundColor=OpenColor(open);
			EditorGUILayout.BeginHorizontal(DrakkarTitleBox);
				GUI.color=Color.white;
				if (!string.IsNullOrEmpty(icon))
				{
					Texture txt=DrakkarAssetsContainer.GetTexture(icon);
					if (txt!=null)
						GUILayout.Label(txt,DrakkarLogoLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));
					else
					{
						txt=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Gizmos/"+icon+".png");
						GUILayout.Label(txt,DrakkarLogoLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));
					}
				}
				else
					iconFromObject(obj);
				bool res=GUILayout.Button(ObjectNames.NicifyVariableName(obj.GetType().Name),DrakkarLabelButtonBig);
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor=Color.white;
			return res;
		}

		public static Texture2D ObjectIcon(UnityEngine.Object obj) => AssetPreview.GetMiniThumbnail(obj);
		private static void iconFromObject(UnityEngine.Object obj)
		{
			Texture2D t=ObjectIcon(obj);
			if (t.name!="d_cs Script Icon")
				GUILayout.Label(t,DrakkarLogoLabel,GUILayout.MaxWidth(32),GUILayout.MaxHeight(30));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SettingTitle(string name)
		{
			Color cont,back;
			cont=GUI.contentColor;
			back=GUI.backgroundColor;
			GUI.contentColor=Color.white;
			GUI.backgroundColor=Color.black;
			EditorGUILayout.LabelField(name,RichHelpBox);
			GUI.backgroundColor=Color.gray;
			GUI.contentColor=cont;
			GUI.backgroundColor=back;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SettingTitle(string name,float width)
		{
			Color cont,back;
			cont=GUI.contentColor;
			back=GUI.backgroundColor;
			GUI.contentColor=Color.white;
			GUI.backgroundColor=Color.black;
			EditorGUILayout.LabelField(name,RichHelpBox,GUILayout.MaxWidth(width));
			GUI.backgroundColor=Color.gray;
			GUI.contentColor=cont;
			GUI.backgroundColor=back;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SettingTitle(string name,Color foreg,Color backg)
		{
			Color cont,back;
			cont=GUI.contentColor;
			back=GUI.backgroundColor;
			GUI.contentColor=foreg;
			GUI.backgroundColor=backg;
			EditorGUILayout.HelpBox(name,MessageType.None,true);
			GUI.backgroundColor=Color.gray;
			GUI.contentColor=cont;
			GUI.backgroundColor=back;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FoldOutRich(ref bool b,string title)
		{
			GUI.backgroundColor=OpenColor(b);
			if (GUILayout.Button(title,DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
				b=!b;
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FoldOutWithBool(SerializedObject so,string propName)
		{
			SerializedProperty sp=so.FindProperty(propName);

			GUI.backgroundColor=Color.white;
			EditorGUILayout.BeginHorizontal();
			sp.boolValue=EditorGUILayout.Toggle(sp.boolValue,GUILayout.MaxWidth(15));
			GUI.backgroundColor=OpenColor(sp.isExpanded);
			if (GUILayout.Button(sp.displayName,DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
				sp.isExpanded=!sp.isExpanded;
			GUI.backgroundColor=Color.white;
			EditorGUILayout.EndHorizontal();
			so.ApplyModifiedProperties();
			return sp.isExpanded;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FoldOutWithBool(SerializedObject so,string propName,Color colorOFF,Color colorON)
		{
			SerializedProperty sp=so.FindProperty(propName);

			GUI.backgroundColor=Color.white;
			EditorGUILayout.BeginHorizontal();
			sp.boolValue=EditorGUILayout.Toggle(sp.boolValue,BoolFilled,GUILayout.MaxWidth(15));
			GUI.backgroundColor=sp.isExpanded? colorON : colorOFF;
			if (GUILayout.Button(sp.displayName,DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
				sp.isExpanded=!sp.isExpanded;
			GUI.backgroundColor=Color.white;
			EditorGUILayout.EndHorizontal();
			so.ApplyModifiedProperties();
			return sp.isExpanded;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FoldOut(ref bool b,string title)
		{
			GUI.backgroundColor=OpenColor(b);
			if (GUILayout.Button(title,DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
				b=!b;
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FoldOut(ref bool b,string title,float width)
		{
			GUI.backgroundColor=OpenColor(b);
			if (GUILayout.Button(title,DrakkarMiniButtonRich,GUILayout.MaxHeight(18),GUILayout.MaxWidth(width)))
				b=!b;
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FoldOut(ref bool b,string title,Color colorOFF,Color colorON)
		{
			GUI.backgroundColor=b? colorON : colorOFF;
			if (GUILayout.Button(title,DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
				b=!b;
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FoldOut(ref bool b,string title,float width,Color colorOFF,Color colorON)
		{
			GUI.backgroundColor=b?colorON:colorOFF;
			if (GUILayout.Button(title,DrakkarMiniButtonRich,GUILayout.MaxHeight(18),GUILayout.MaxWidth(width)))
				b=!b;
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FoldOut(ref bool b,string title,bool active,Texture icon=null)
		{
			Color x;

			if (b&&active)
				x=OpenActive;
			else if (b&&!active)
				x=OpenInactive;
			else if (!b&&active)
				x=ClosedActive;
			else
				x=ClosedInactive;

			GUI.backgroundColor=x;
			if (icon!=null)
			{
				if (GUILayout.Button(new GUIContent(title,icon),DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
					b=!b;
			}
			else
			{
				if (GUILayout.Button(title,DrakkarMiniButtonRich,GUILayout.MaxHeight(18)))
					b=!b;
			}
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void BeginIndent(float i,string Style)
		{
			EditorGUILayout.BeginHorizontal();
			if (i>0)
				EditorGUILayout.Space(i);
			EditorGUILayout.BeginVertical(Style);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void BeginIndent(float i)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space(i);
			EditorGUILayout.BeginVertical();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void EndIndent()
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void BeginColorPanel(DRAKKAR_MARKER_COLOR col)
		{
			GUI.backgroundColor=DrakkarAssetsContainer.GetSchemaColor(col);
			EditorGUILayout.BeginVertical(DrakkarEditor.borderPanel);
			EditorGUILayout.Space();
			GUI.backgroundColor=Color.white;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void EndColorPanel() => EditorGUILayout.EndVertical();
		#endregion
		#region ANIMATION RELATED
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetAnimationsList(Animation ani)
		{
			if (ani==null)
				return null;
			ArrayList anims=new();

			foreach (AnimationState A in ani)
				anims.Add(A.name);

			return (string[])anims.ToArray(typeof(string));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetAnimatorStatesList(Animator animator)
		{
			string[] names=null;
			UnityEditor.Animations.AnimatorController ac;
			UnityEditor.Animations.AnimatorStateMachine stateMachine;
			ac=animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			if (ac?.layers!=null)
			{
				stateMachine=ac.layers[0].stateMachine;
				UnityEditor.Animations.ChildAnimatorState[] ch_animStates=stateMachine.states;
				names=new string[ch_animStates.Length];
				for (int i=0;i<names.Length;i++)
					names[i]=ch_animStates[i].state.name;
			}
			return names;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetAnimatorTriggersList(Animator animator) => GetAnimatorList(animator,AnimatorControllerParameterType.Trigger);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetAnimatorFloatsList(Animator animator) => GetAnimatorList(animator,AnimatorControllerParameterType.Float);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetAnimatorBoolsList(Animator animator) => GetAnimatorList(animator,AnimatorControllerParameterType.Bool);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetAnimatorList(Animator animator,AnimatorControllerParameterType type)
		{
			string[] names=null;
			UnityEditor.Animations.AnimatorController ac;
			ac=animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			if (ac?.layers!=null)
			{
				List<string> ls=new();
				foreach (AnimatorControllerParameter acp in ac.parameters)
				{
					if (acp.type==type)
						ls.Add(acp.name);
				}
				names=ls.ToArray();
			}
			return names;
		}
		#endregion
		#region FORMATTED DATAS
		#region STRING POP-UP
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string PopupString(string[] table,string str,float width,bool allowNotInList=false)
		{
			int ind;
			if (string.IsNullOrEmpty(str))
				ind=0;
			ind=Array.IndexOf<string>(table,str);
			if (ind<0)
			{
				if (!allowNotInList)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("<color=yellow>*ERROR* ...string not found in table</color>",DrakkarSmallLabel);
					GUI.backgroundColor=Color.red;
					if (GUILayout.Button("Reset"))
						return table[0];
					GUI.backgroundColor=Color.white;
					EditorGUILayout.EndHorizontal();
					return string.Empty;
				}
				else
					return str;
			}
			if (width>0)
				ind=EditorGUILayout.Popup(ind,table,GUILayout.MaxWidth(width));
			else
				ind=EditorGUILayout.Popup(ind,table);
			return table==null||table.Length==0 ? string.Empty : table[ind];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string PopupString(string label,Color content,Color background,string[] table,string str)
		{
			int ind;
			if (string.IsNullOrEmpty(str))
				ind=0;
			else
			{
				ind=Array.IndexOf<string>(table,str);
				if (ind<0)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("<color=yellow>*ERROR* ...string not found in table</color>",DrakkarSmallLabel);
					GUI.backgroundColor=Color.red;
					if (GUILayout.Button("Reset"))
						return table[0];
					GUI.backgroundColor=Color.white;
					EditorGUILayout.EndHorizontal();
					return string.Empty;
				}
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(label);
			GUI.contentColor=content;
			GUI.backgroundColor=background;
			ind=EditorGUILayout.Popup(ind,table);
			GUI.contentColor=Color.white;
			GUI.backgroundColor=Color.white;
			EditorGUILayout.EndHorizontal();
			return table==null || table.Length==0 ? string.Empty : table[ind];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string PopupString(string[] table,string str)
		{
			int ind;
			if (string.IsNullOrEmpty(str))
				ind=0;
			else
			{
				ind=Array.IndexOf<string>(table,str);
				if (ind<0)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("<color=yellow>*ERROR* ...string not found in table</color>",DrakkarSmallLabel);
					GUI.backgroundColor=Color.red;
					if (GUILayout.Button("Reset"))
						return table[0];
					GUI.backgroundColor=Color.white;
					EditorGUILayout.EndHorizontal();
					return string.Empty;
				}
			}
			ind=EditorGUILayout.Popup(ind,table);
			return table==null || table.Length==0 ? string.Empty : table[ind];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string PopupStringGUI(ref Rect pos,float width,string[] table,string str)
		{
			int ind;
			if (string.IsNullOrEmpty(str))
				ind=0;
			ind=Array.IndexOf<string>(table,str);
			if (ind<0)
				ind=0;
			Rect r=pos;
			r.width=width;
			ind=EditorGUI.Popup(r,ind,table);
			pos.x+=width;
			return table==null || table.Length==0 ? string.Empty : table[ind];
		}
		#endregion
		#region INT
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizedIntField(string name,int val,int fieldwidth)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name);
			val=EditorGUILayout.IntField(val,EditorStyles.helpBox,GUILayout.MaxWidth(fieldwidth));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizedIntField(string name,int val,int namewidth,int fieldwidth)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name,GUILayout.MaxWidth(namewidth));
			val=EditorGUILayout.IntField(val,EditorStyles.helpBox,GUILayout.MaxWidth(fieldwidth));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		#endregion
		#region FLOAT
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SizedFloatField(string name,float val,int fieldwidth)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name);
			val=EditorGUILayout.FloatField(val,EditorStyles.helpBox,GUILayout.MaxWidth(fieldwidth));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SizedFloatField(string name,float val,int namewidth,int fieldwidth)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name,GUILayout.MaxWidth(namewidth));
			val=EditorGUILayout.FloatField(val,EditorStyles.helpBox,GUILayout.MaxWidth(fieldwidth));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PercentageSlider(string name,float val)
		{
			EditorGUILayout.BeginHorizontal();
			val=EditorGUILayout.Slider(name,val*100,0,100)*0.01f;
			EditorGUILayout.LabelField("%",EditorStyles.boldLabel,GUILayout.MaxWidth(20));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DoublePercentageSlider(string name,float val)
		{
			EditorGUILayout.BeginHorizontal();
			val=EditorGUILayout.Slider(name,val*100,0,200)*0.01f;
			EditorGUILayout.LabelField("%",EditorStyles.boldLabel,GUILayout.MaxWidth(20));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float QuadPercentageSlider(string name,float val)
		{
			EditorGUILayout.BeginHorizontal();
			val=EditorGUILayout.Slider(name,val*100,0,400)*0.01f;
			EditorGUILayout.LabelField("%",EditorStyles.boldLabel,GUILayout.MaxWidth(20));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float TenthPercentageSlider(string name,float val)
		{
			EditorGUILayout.BeginHorizontal();
			val=EditorGUILayout.Slider(name,val*100,0,1000)*0.01f;
			EditorGUILayout.LabelField("%",EditorStyles.boldLabel,GUILayout.MaxWidth(20));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PercentageSlider(string name,float val,int width)
		{
			if (width<20)
				width=20;
			EditorGUILayout.BeginHorizontal();
			val=EditorGUILayout.Slider(name,val*100,0,100,GUILayout.MaxWidth(width-20))*0.01f;
			EditorGUILayout.LabelField("%",EditorStyles.boldLabel,GUILayout.MaxWidth(20));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		#endregion
		#region CURVES
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AnimationCurve SizedCurveField(string name,AnimationCurve val,int fieldwidth)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name);
			GUI.backgroundColor=Color.gray;
			val=EditorGUILayout.CurveField(val,GUILayout.MaxWidth(fieldwidth));
			EditorGUILayout.EndHorizontal();
			return val;
		}
		#endregion
		#endregion
		#region OVERLAPPING LABELS
		public static void OverlappingLabel(Rect rect,string label,Color col,float offset=0)
		{
			GUI.contentColor=col;
			Rect r=RectHorizontalPadding(rect,5);
			r.width-=offset;
			EditorGUI.LabelField(r,label,rightAlignementStyleLabel);
			GUI.contentColor=Color.white;
		}
		public static void OverlappingLabel(string label,Color col,float offset=0)
		{
			GUI.contentColor=col;
			Rect r=RectHorizontalPadding(GUILayoutUtility.GetLastRect(),5);
			r.width-=offset;
			EditorGUI.LabelField(r,label,rightAlignementStyleLabel);
			GUI.contentColor=Color.white;
		}
		#endregion
		#region DRAKKAR FIELDS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DrakkarPropertyInspector(SerializedObject ob,string pr,float width=-1,bool recolor=false,float lableIndent=0,bool applyProperties=true)
		{
			SerializedProperty p=ob.FindProperty(pr);
			if (width<0)
				EditorGUILayout.PropertyField(p,emptyContent);
			else
				EditorGUILayout.PropertyField(p,emptyContent,GUILayout.MaxWidth(width));
			if (applyProperties)
				ob.ApplyModifiedProperties();
			OverlappingLabel(p.displayName,recolor? GUI.contentColor : Color.green,lableIndent);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DrakkarPropertyInspector(SerializedObject ob,string pr,string displayName,float width=-1,bool recolor=false,float lableTndent=0,bool applyProperties=true)
		{
			SerializedProperty p=ob.FindProperty(pr);
			if (width<0)
				EditorGUILayout.PropertyField(p,emptyContent);
			else
				EditorGUILayout.PropertyField(p,emptyContent,GUILayout.MaxWidth(width));
			if (applyProperties)
				ob.ApplyModifiedProperties();
			OverlappingLabel(displayName,recolor? GUI.contentColor : Color.green,lableTndent);
		}
		public static T DrakkarObjectField<T>(string label,T field,bool allowSceneObjects=true,bool showLabelwhenNull=true,float width=-1) where T : UnityEngine.Object
		{
			UnityEngine.Object o=field;
			if (width>0)
				o=EditorGUILayout.ObjectField(o,typeof(T),allowSceneObjects,GUILayout.MaxWidth(width)) as T;
			else
				o=EditorGUILayout.ObjectField(o,typeof(T),allowSceneObjects) as T;
			if (showLabelwhenNull && (o==null || o.Equals(null)))
				OverlappingLabel(label,Color.cyan,20);
			return o as T;
		}
		public static int DrakkarIntField(string label,int field,float width=-1,bool recolor=false)
		{
			if (width>0)
				field=EditorGUILayout.IntField(field,GUILayout.MaxWidth(width));
			else
				field=EditorGUILayout.IntField(field);
			OverlappingLabel(label,recolor? GUI.contentColor : Color.green);
			return field;
		}
		public static float DrakkarFloatField(string label,float field,float width=-1,bool recolor=false)
		{
			if (width>0)
				field=EditorGUILayout.FloatField(field,GUILayout.MaxWidth(width));
			else
				field=EditorGUILayout.FloatField(field);
			OverlappingLabel(label,recolor? GUI.contentColor : Color.green);
			return field;
		}
		public static string DrakkarStringField(string label,string field,float width=-1,bool recolor=false,bool hideLabel=false)
		{
			if (width>0)
				field=EditorGUILayout.TextField(field,GUILayout.MaxWidth(width));
			else
				field=EditorGUILayout.TextField(field);
			if (!hideLabel || string.IsNullOrEmpty(field))
				OverlappingLabel(label,recolor? GUI.contentColor : Color.green);
			return field;
		}
		public static string DrakkarPopupStringField(string[] names,string label,string field,float width=-1,bool recolor=false,bool hideLabel=false)
		{
			field=PopupString(names,field,width);
			if (!hideLabel || string.IsNullOrEmpty(field) || string.Compare(field,"*NONE*")==0)
				OverlappingLabel(label,recolor? GUI.contentColor : Color.cyan,15);
			return field;
		}

		public static int DrakkarIntFieldGUI(Rect r,string label,int field,bool recolor=false)
		{
			field=EditorGUI.IntField(r,field);
			OverlappingLabel(r,label,recolor? GUI.contentColor : Color.green);
			return field;
		}
		public static float DrakkarFloatFieldGUI(Rect r,string label,float field,bool recolor=false)
		{
			field=EditorGUI.FloatField(r,field);
			OverlappingLabel(r,label,recolor? GUI.contentColor : Color.green);
			return field;
		}
		#endregion
		#region UTILITIES
		#region LOAD SCRIPTABLE OBJECTS
		public static T LoadFirst<T>() where T : UnityEngine.Object
		{
			T[] obj=Resources.LoadAll<T>("");
			if (obj!=null && obj.Length>0)
				return obj[0];
			return null;
		}
		#endregion
		#region MESSAGES
		public static void Notification(string text) => SceneView.lastActiveSceneView.ShowNotification(new GUIContent(text));
		public static void ObjectIsNullError(string type,string name,string variable) => Debug.LogError("<color=red>"+type+" </color><color=yellow>"+name+"</color><color=#ff40ff> has </color>NULL <color=red>"+variable+"!</color>");
		public static void NameAndReasonError(string name,string reason) => Debug.LogError("<color=yellow>"+name+" </color><color=#ff40ff>"+reason+"!</color>");
		public static void NameAndReasonWarning(string name,string reason) => Debug.LogWarning("<color=yellow>"+name+" </color><color=#ff40ff>"+reason+"!</color>");
		#endregion
		#region GET LISTS (or Arrays)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string[] GetComponentsList(GameObject go)
		{
			ArrayList comps=new();
			comps.Add("*NONE*");

			foreach (Component C in go.GetComponents(typeof(Component)))
				comps.Add(C.GetType().Name);

			return (string[])comps.ToArray(typeof (string));
		}
		#endregion
		#region ARRAY RELATED
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveFromArray<T>(ref T[] array,int i)
		{
			if (array==null)
				return;
			ArrayList list=new(array.Length);
			for (int j=0;j<array.Length;j++)
				list.Add(array[j]);
			list.RemoveAt(i);
			array=list.ToArray(typeof(T)) as T[];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddToArray<T>(ref T[] array,int i) where T:new()
		{
			if (array==null)
				return;
			ArrayList list=new(array.Length);
			for (int j=0;j<i;j++)
				list.Add(array[j]);
			list.Add(new T());
			for (int j=i;j<array.Length;j++)
				list.Add(array[j]);
			array=list.ToArray(typeof(T)) as T[];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddToArrayTop<T>(ref T[] array,T item)
		{
			if (array==null)
				return;
			ArrayList list=new(array.Length+1);
			list.Add(item);
			for (int j=0;j<array.Length;j++)
				list.Add(array[j]);
			array=list.ToArray(typeof(T)) as T[];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddToArray<T>(ref T[] array) where T : new()
		{
			if (array==null)
				return;
			Array.Resize<T>(ref array,array.Length+1);
	 		array[^1] = new T();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddToArray(ref string[] array)
		{
			if (array==null)
				return;
			string[] tmp=new string[array.Length+1];
			for (int i=0;i<array.Length;i++)
				tmp[i]=array[i];
			tmp[^1]=string.Empty;
			array=tmp;
		}
		#endregion
		#region SERIALIZED PROPERTIES
		private static bool tempBool;
		private static SerializedProperty tempProp;
		public static ref bool PropertyBool => ref tempBool;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBoolFromProperty(SerializedObject so,string prop)
		{
			tempProp=so.FindProperty(prop);
			if (tempProp!=null)
				PropertyBool=tempProp.isExpanded;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetBoolToProperty()
		{
			if (tempProp!=null)
				tempProp.isExpanded=tempBool;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SerializedProperty GetSiblingProperty(SerializedProperty property,string siblingName)
		{
			string propertyPath=property.propertyPath;
			string conditionPath=propertyPath.Replace(property.name,siblingName);
			return property.serializedObject.FindProperty(conditionPath);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool PropertyPartOfArray(SerializedProperty prop) => prop.propertyPath.EndsWith("]");
		public static int PropertyArrayIndex(SerializedProperty prop)
		{
			int startIndex=prop.propertyPath.LastIndexOf("[")+1;
			int endIndex=prop.propertyPath.LastIndexOf("]");
			string indexString=prop.propertyPath.Substring(startIndex,endIndex-startIndex);

			return int.TryParse(indexString,out int index) ? index : -1;
		}
		#endregion
		#region REQUIRED COMPONENTS
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RequestComponent<T>(GameObject go,ref T comp) where T: Component
		{
			if (comp==null)
				comp=go.GetComponent<T>();
			if (comp==null)
				comp=go.AddComponent<T>();
		}
		#endregion
		#region RECURSIVE COMPONENTS
		public static void GetAllChildrenGameobjects(GameObject parent,List<GameObject>list)
		{
			foreach (Transform child in parent.transform)
			{
				list.Add(child.gameObject);
				GetAllChildrenGameobjects(child.gameObject,list);
			}
		}
		#endregion
		#region WINDOWS
		public static void DrawWindowBackGround(EditorWindow win,Color32 color) => EditorGUI.DrawRect(new Rect(0,0,win.position.width,win.position.height),color);
		public static void DrawWindowGrid(Rect rect,float gridSize,Color color,ref Vector2 scrollPos,float scale=1)
		{
			Handles.color=color;
			float startX=rect.x+scrollPos.x-scrollPos.x % gridSize;
			float startY=rect.y+scrollPos.y-scrollPos.y % gridSize;
			float endX=scrollPos.x+rect.width/scale;
			float endY=scrollPos.y+rect.height/scale;

			for (float x=startX;x<endX;x+=gridSize)
				Handles.DrawLine(new(x,0,0),new(x,endY,0));

			for (float y=startY;y<endY;y+=gridSize)
				Handles.DrawLine(new(0,y,0),new(endX,y,0));

			Handles.color=Color.white;
		}
		#endregion
		#region COLORS
		public static Color GetColorChannel(ColorChannel c) => c switch { ColorChannel.R => Color.red , ColorChannel.G => Color.green, ColorChannel.B => Color.blue, _ => Color.white};
		#endregion
		#region SCRIPTABLE OBJECTS
		public static void CreateScriptableObject<T>(ref T dest,string defaultName) where T : ScriptableObject
		{
			T schema=ScriptableObject.CreateInstance<T>();
			string path=EditorUtility.SaveFilePanelInProject("New "+typeof(T).Name,defaultName,"asset","");
			if (!string.IsNullOrEmpty(path))
			{
				AssetDatabase.CreateAsset(schema,path);
				dest=schema;
			}
		}
		public static T FindScriptableObject<T>() where T : ScriptableObject
		{
			string[] guids=AssetDatabase.FindAssets("t:"+typeof(T).Name);
			for(int i=0;i<guids.Length;++i)
			{
				string path=AssetDatabase.GUIDToAssetPath(guids[i]);
				T asset=AssetDatabase.LoadAssetAtPath<T>(path);
				if(asset!=null)return asset;
			}
			return null;
		}
		#endregion
		#region CHECK DRAKKARUPDATER
		public static void CheckDrakkarUpdater()
		{
			if (GameObject.FindFirstObjectByType(typeof(DrakkarUpdater))==null)
			{
				GUI.contentColor=Color.yellow;
				EditorGUILayout.HelpBox("DrakkarUpdater is not present in the scene!",MessageType.Error);
				GUI.contentColor=Color.white;
			}
		}
		#endregion
		#region GUI RECTS
		public static Rect GetXSizedRect(ref Rect pos,float sizex)
		{
			Rect n=pos;
			pos.x+=sizex;
			return new(n.x,n.y,sizex,18);
		}
		public static void NewLineGUI(ref Rect pos) => pos.y+=18;
		public static void NewLineGUI(ref Rect pos,Rect orig)
		{
			pos.x=orig.x;
			pos.width=orig.width;
			pos.y+=18;
		}
		public static void IndentGUI(ref Rect pos,int indent)
		{
			pos.x+=indent;
			pos.width-=indent;
		}
		#endregion
		#region SHADERS
		public static string ShaderFloatProperty(Shader shader,string prop) => DrakkarPopupStringField(GetShaderFloats(shader),"Fade Property",prop);
		public static string[] GetShaderFloats(Shader shader)
		{
			if (shader==null) return null;

			List<string> properties=new();
			int count=shader.GetPropertyCount();

			for (int i=0;i<count;i++)
			{
				ShaderPropertyType type=shader.GetPropertyType(i);
				if (type==ShaderPropertyType.Float || type==ShaderPropertyType.Range)
					properties.Add(shader.GetPropertyName(i));
			}
			return properties.ToArray();
		}
		#endregion
		#endregion
	}
}
#endif