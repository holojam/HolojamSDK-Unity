Shader "Custom/AvatarShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Tile("Tiling", Float) = 12

		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CrossTex1("Sketch Darkest (RGB)", 2D) = "white" {}
		_CrossTex2("Sketch (RGB)", 2D) = "white" {}
		_CrossTex3("Sketch (RGB)", 2D) = "white" {}
		_CrossTex4("Sketch (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleLambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _CrossTex1;
		sampler2D _CrossTex2;
		sampler2D _CrossTex3;
		sampler2D _CrossTex4;


		fixed4 _Color;
		fixed4 _DarkColor;
		fixed4 _MultColor;
		float _Tile;

		struct Input {
			float2 uv_MainTex;
		};

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;

			fixed3 tile1;
			fixed3 tile2;
			fixed3 tile3;
			fixed3 tile4;
		};

		void surf (Input IN, inout SurfaceOutputCustom o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a;
			o.tile1 = tex2D(_CrossTex1, IN.uv_MainTex*_Tile).rgb;
			o.tile2 = tex2D(_CrossTex2, IN.uv_MainTex*_Tile).rgb;
			o.tile3 = tex2D(_CrossTex3, IN.uv_MainTex*_Tile).rgb;
			o.tile4 = tex2D(_CrossTex4, IN.uv_MainTex*_Tile).rgb;
		}

		half4 LightingSimpleLambert(SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten) {
			float NdotL = (dot(s.Normal, lightDir)*.5) + .5;
			float camNorm = 1 - (dot(s.Normal, viewDir));
			float4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha * _LightColor0.rgb * (NdotL * atten * 2) + (pow(camNorm, 2)*s.Alpha*(1 - NdotL));
			   		
			float4 emit = lerp(float4(0, 0, 0, 0),
				lerp(float4(s.tile1.rgb, 1.0),
					lerp(float4(s.tile2.rgb, 1.0),
						lerp(float4(s.tile3.rgb, 1.0),
							lerp(float4(s.tile4.rgb, 1.0), float4(1, 1, 1, 1),
								clamp((_Color.a*2.0*c.a - 0.75)*10.0, 0.0, 1.0)),
							clamp((_Color.a*2.0*c.a - 0.6)*10.0, 0.0, 1.0)),
						clamp((_Color.a*2.0*c.a - 0.45)*10.0, 0.0, 1.0)),
					clamp((_Color.a*2.0*c.a - 0.3)*10.0, 0.0, 1.0)),
				clamp((_Color.a*2.0*c.a - 0.15)*10.0, 0.0, 1.0));

			float4 L = lerp(_Color*float4(c.r,c.g,c.b, 1.0), float4(c.r,c.g,c.b, 1.0), emit.r);
			return L;
		}


		ENDCG
	}
	FallBack "Diffuse"
}
