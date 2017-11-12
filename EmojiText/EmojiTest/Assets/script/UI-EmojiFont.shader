Shader "UI/EmojiFont" {
	Properties {
		[PerRendererData] _MainTex ("Font Texture", 2D) = "white" {}
		_EmojiTex ("Emoji Texture", 2D) = "white" {}
	}
	
	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};

			fixed4 _TextureSampleAdd;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(float4(IN.vertex.x, IN.vertex.y, IN.vertex.z, 1.0));

				OUT.texcoord = IN.texcoord;
				OUT.texcoord1 = IN.texcoord1;
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * OUT.vertex.w;
				#endif
				
				OUT.color = IN.color ;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _EmojiTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color;
				if (IN.texcoord1.x >0 && IN.texcoord1.y > 0)
				{
					color = tex2D(_EmojiTex, IN.texcoord1);
				}else
				{
					// it's a text, and render it as normal ugui text
					color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				}
				return color;
			}
		ENDCG
		}
	}
}
