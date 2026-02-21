Shader "Trail Shader"
{
	Properties
	{
		_Color1("Color 1",Color)=(1,1,1,0)
		_Color2("Color 2",Color)=(0,0.1931255,0.7264151,0)
		_Texture("Texture",2D)="white" {}
		_Power("Power",Float)=3
	}
	
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha One
		AlphaToMask Off
		Cull Off
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Pass
		{
			Name "Unlit"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float2 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 ase_texcoord1 : TEXCOORD0;
			};

			uniform float4 _Color1;
			uniform float4 _Color2;
			uniform sampler2D _Texture;
			uniform float4 _Texture_ST;
			uniform float _Power;

			
			v2f vert ( appdata v )
			{
				v2f o;
				o.ase_texcoord1=v.ase_texcoord.xy;
				o.vertex=UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv_Texture=i.ase_texcoord1.xy;
				float tex=tex2D(_Texture,uv_Texture).r;
				float4 lerpResult6=lerp(_Color1,_Color2,pow(tex,_Power));
				lerpResult6.a*=tex.r;
				return lerpResult6;
			}
			ENDCG
		}
	}
	Fallback Off
}