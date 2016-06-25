//LambertRimLit.shader
//Created by Aaron C Gaudette on 20.08.14
//Like the Lambert shader, but with rim lighting

Shader "Custom/Fragment/Lambert Rim Lit"{
	Properties{
		_Color("Color",Color) = (1.0,1.0,1.0,1.0)
		_MinLight("Minimum Light",Range(0.0,0.5)) = 0.0
		_RimSpread("Rim Spread",Range(5.0,0.1)) = 5.0
		_RimStrength("Rim Strength",Range(1.0,40.0)) = 1.0
	}
	SubShader{
		Tags{"RenderType" = "Opaque"}
		Pass{
			Tags{"LightMode"="ForwardBase"}
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			//For Unity shadows
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_fwdbase
			
			//Properties
			fixed4 _Color;
			fixed _MinLight;
			half _RimSpread;
			half _RimStrength;
			
			//Unity property
			half4 _LightColor0;
			
			struct VertexInput{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
			};
			struct VertexOutput{
				half4 pos : SV_POSITION;
				fixed4 col : COLOR;
				//For Unity shadows
				fixed shadowMask : TEXCOORD0;
				LIGHTING_COORDS(1,2)
			};
			
			VertexOutput vert(VertexInput input){
				VertexOutput output;
				
				//Vertex position
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				float4 worldPosition = mul(_Object2World,input.vertex);
				
				//Direction vectors
				fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPosition.xyz);
				fixed3 normalDirection = normalize(mul(float4(input.normal,0.0),_World2Object).xyz);
				
				half3 vertexToLightSource = _WorldSpaceLightPos0.xyz - worldPosition.xyz;
				//Modify attenuation based on light source (directional or point)
				fixed attenuation = lerp(1.0,1.0/length(vertexToLightSource),_WorldSpaceLightPos0.w);
				//Modify light direction based on light source (directional or point)
				fixed3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz,vertexToLightSource,_WorldSpaceLightPos0.w));
				
				fixed3 light = dot(normalDirection,lightDirection);
				//Limit minimum illumination to user value
				fixed3 diffuseReflection = attenuation * _LightColor0.xyz * max(_MinLight,light);
				//Rim lighting is affected by user values Spread and Strength
				fixed rim = 1 - dot(viewDirection,normalDirection);
				fixed3 rimLighting = light * _LightColor0.xyz * _RimStrength * pow(rim,_RimSpread);
				
				//Multiply diffuse (Lambert), rim lighting, and ambient light by color
				output.col = fixed4((diffuseReflection + rimLighting + UNITY_LIGHTMODEL_AMBIENT.xyz) * _Color.rgb,1.0);
				
				//For Unity shadows--a mask that sets any shadow-affected faces to 1, and all others to 0
				output.shadowMask = max(0,normalize(dot(normalDirection,lightDirection)));
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				
				return output;
			}
			//Purely vertex shader, minus shadows
			fixed4 frag(VertexOutput output) : COLOR{
				//For Unity shadows--apply mask then apply the inverse to create a perfect shadow layer
				fixed shadow = LIGHT_ATTENUATION(output)*output.shadowMask;
				shadow+=(1-output.shadowMask);
				
				return output.col*shadow;
			}
			
			ENDCG
		}
		//Additional lights (no shadows, no ambient light)
		Pass{
			Tags{"LightMode"="ForwardAdd"}
			Blend One One
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			//Properties
			fixed4 _Color;
			fixed _MinLight;
			half _RimSpread;
			half _RimStrength;
			
			//Unity property
			half4 _LightColor0;
			
			struct VertexInput{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
			};
			struct VertexOutput{
				half4 pos : SV_POSITION;
				fixed4 col : COLOR;
			};
			
			VertexOutput vert(VertexInput input){
				VertexOutput output;
				
				//Vertex position
				output.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				float4 worldPosition = mul(_Object2World,input.vertex);
				
				//Direction vectors
				fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPosition.xyz);
				fixed3 normalDirection = normalize(mul(float4(input.normal,0.0),_World2Object).xyz);
				
				half3 vertexToLightSource = _WorldSpaceLightPos0.xyz - worldPosition.xyz;
				//Modify attenuation based on light source (directional or point)
				fixed attenuation = lerp(1.0,1.0/length(vertexToLightSource),_WorldSpaceLightPos0.w);
				//Modify light direction based on light source (directional or point)
				fixed3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz,vertexToLightSource,_WorldSpaceLightPos0.w));
				
				fixed3 light = dot(normalDirection,lightDirection);
				//Limit minimum illumination to user value
				fixed3 diffuseReflection = attenuation * _LightColor0.xyz * max(_MinLight,light);
				//Rim lighting is affected by user values Spread and Strength
				fixed rim = 1 - dot(viewDirection,normalDirection);
				fixed3 rimLighting = light * _LightColor0.xyz * _RimStrength * pow(rim,_RimSpread);
				
				//Multiply diffuse (Lambert) and rim lighting by color
				output.col = fixed4((diffuseReflection + rimLighting) * _Color.rgb,1.0);
				
				return output;
			}
			//Purely vertex shader
			fixed4 frag(VertexOutput output) : COLOR{return output.col;}
			
			ENDCG
		}
	}
	Fallback "Diffuse"
}