Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		[PerRendererData]_MainTex ("Texture", 2D) = "white" {}
		_EmojiTex("Texture",2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				fixed4 color : COLOR;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _EmojiTex;
			float4 _MainTex_ST;
			fixed4 _TextureSampleAdd;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = v.uv1;
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
				if(i.uv.x>0 && i.uv1.y>0){
					col = tex2D(_EmojiTex,i.uv1);
				}else{
					col = (tex2D(_MainTex, i.uv)+_TextureSampleAdd)*i.color;
				}
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
