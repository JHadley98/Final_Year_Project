Shader "Custom/Terrain"
{
	Properties
	{
		testTexture("Texture", 2D) = "white"{}
		testScale("Scale", Float) = 1

	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			const static int maxLayerCount = 8;
			const static float epsilon = 1E-4;

			int layerCount;
			float3 baseColours[maxLayerCount];
			float baseStartHeights[maxLayerCount];
			float baseBlends[maxLayerCount];
			float baseColourStrength[maxLayerCount];
			float baseTextureScales[maxLayerCount];

			float minHeight;
			float maxHeight;

			sampler2D testTexture;
			float testScale;

			UNITY_DECLARE_TEX2DARRAY(baseTextures);

			struct Input
			{
				float3 worldPos;
				float3 worldNormal;
			};


			// InverseLerp function, where a is the minimum value, b is the maximum value and value is the current value
			float inverseLerp(float a, float b, float value)
			{
				// Return difference between value and minimum value divided by the difference between maximum value and minimum value
				// Saturate result which means to clamp the values between 0 and 1
				return saturate((value - a) / (b - a));
			}

			float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
			{
				float3 scaledWorldPos = worldPos / scale;
				float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
				float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
				float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
				return xProjection + yProjection + zProjection;
			}

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Set heightPercent to be 0 and 1 for the lowest and highest points of the terrain
				float heightPercent = inverseLerp(minHeight,maxHeight, IN.worldPos.y);
				float3 blendAxes = abs(IN.worldNormal);
				blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

				// Loop through colours
				for (int i = 0; i < layerCount; i++)
				{
					float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

					float3 baseColour = baseColours[i] * baseColourStrength[i];
					float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);

					o.Albedo = o.Albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
				}
			}

			ENDCG
		}
			FallBack "Diffuse"
}