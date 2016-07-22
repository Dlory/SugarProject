Shader "Custom/RippleWaterShader" {
	Properties {
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { }
		_Color ("Main Color", Color) = (1,1,1,1)
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_Scale ("Scale", float) = 0.1
		_WaveLength ("WaveLength", float) = 1
		_MaxAmplitude ("MaxAmplitude", float) = 1

		[HideInInspector]
		_GridNodeSize("GridNodeSize", float) = 0.5
 		[HideInInspector] 
 		_RippleData ("RippleData", 2D) = "green" {}
		[HideInInspector]
		_RippleCount ("RippleCount", int) = 0

 [HideInInspector]_WaveAmplitude1 ("WaveAmplitude1", float) = 0
 [HideInInspector]_Distance1 ("Distance1", float) = 0
 [HideInInspector]_xImpact1 ("x Impact 1", float) = 0
 [HideInInspector]_zImpact1 ("z Impact 1", float) = 0
	}

	SubShader { 
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite on Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Lambert approxview vertex:vert alpha:blend 
		//#pragma target 4.0

		sampler2D _RippleData;
		float _GridNodeSize, _Scale, _Speed, _MaxAmplitude, _WaveLength;
		int _RippleCount;
		fixed4 _Color;
		fixed4 _ReflectColor;
		samplerCUBE _Cube;

		float _WaveAmplitude1, _Distance1, _xImpact1, _zImpact1, _OffsetX1, _OffsetZ1;

		struct Input {
			float3 worldRefl;
		};

//		float3 VerteXOnGrid3 (float3 v) {
//			float y = 0;
//
//			if (_WaveAmplitude1 > 0.01 ) {
//				float waveThreshold = -7.853981625; //-2.5 * 3.14159265;
//				//float3 worldPos = mul(_Object2World, v).xyz;
//				float tDistance = sqrt(pow(v.x - _xImpact1, 2) + pow(v.z - _zImpact1, 2));
//
//				if (tDistance < _Distance1) {
//			   		float value1 = _Scale * cos(_WaveLength * (tDistance - _Distance1))  / 200;
//				    float x = tDistance / (_Distance1);
//				    if (_WaveLength * (tDistance - _Distance1)  < waveThreshold) {
//				    	float w = pow(tDistance / -waveThreshold, 2);
//				   		value1 *= w;
//				    }
////				     else
////				    if (tDistance > _Distance1 * 1 && _Distance1 > _GridNodeSize * 2) {
////				   		value1 *= 1-x;
////				    }
//					y += value1 * _WaveAmplitude1;
//				}
//			}
//			return float3(v.x, y, v.z);
//		}


		inline float UnpackFloatRGBA(float4 c)
		{
			// First, convert the color to its byte values
			int4 bytes = c * 255;

			// Extract the sign byte of the float, i.e. the most significant bit in the red channel (and overall float structure)
			int sign = (bytes.r & 128) > 0 ? -1 : 1;

			// Extract the exponent's bit parts which are spread across both the red and the green channel
			int expR = (bytes.r & 127) << 1;
			int expG = bytes.g >> 7;

			int exponent = expR + expG;

			// The remaining 23 bits constitute the float's significand. They are spread across the green, blue and alpha channels
			int signifG = (bytes.g & 127) << 16;
			int signifB = bytes.b << 8;

			float significand = (signifG + signifB + bytes.a) / pow(2, 23);

			significand += 1;

			// We now know both the sign bit, the exponent and the significand of the float and can thus reconstruct it fully like so:
			return sign * significand * pow(2, exponent - 127);
		}

		float RippleDataFloatOnPixel(float px, float py) {
			float2 texSize = float2(_RippleCount, 4);
			float4 c = tex2Dlod(_RippleData, float4((px + 0.5) / texSize.x, (py + 0.5) / texSize.y, 0, 0));
			return UnpackFloatRGBA(c);
		}

		float3 VerteXOnGrid (float3 v) {
			if (_RippleCount <= 0) {
				return v;
			}

			float y = 0;
			for (int i = 0; i < _RippleCount; i++) {
				float _Distance = RippleDataFloatOnPixel(i, 0);
				float _Amplitude =  RippleDataFloatOnPixel(i, 1);

				if (_Amplitude > 0.01 ) {
					float waveThreshold = -7.853981625; //-2.5 * 3.14159265;
				    float centerX = RippleDataFloatOnPixel(i, 2);
 				    float centerY = RippleDataFloatOnPixel(i, 3);
					float vDistance = sqrt(pow(v.x - centerX, 2) + pow(v.z -centerY, 2));

					if (vDistance < _Distance) {
				   		float value1 = _Scale * cos(_WaveLength * (vDistance - _Distance))  / 200;
					    float x = vDistance / (_Distance);
					    if (_WaveLength * (vDistance - _Distance)  < waveThreshold) {
					    	float w = pow(vDistance / -waveThreshold, 2);
					   		value1 *= w;
					    }
						y += value1 * _Amplitude;
					}
				}
			}
			return float3(v.x, min(y, _MaxAmplitude), v.z);
		}

		float3 CalculateNormal (float3 tVertex) {
			float3 upVertex = float3(tVertex.x, 0, tVertex.z + _GridNodeSize);
			float3 leftVertex = float3(tVertex.x - _GridNodeSize, 0, tVertex.z);
			float3 downVertex = float3(tVertex.x, 0, tVertex.z - _GridNodeSize);
			float3 rightVertex = float3(tVertex.x + _GridNodeSize, 0, tVertex.z);

			float x, y, z;
			x = VerteXOnGrid(leftVertex).y - VerteXOnGrid(rightVertex).y;
			z = VerteXOnGrid(downVertex).y - VerteXOnGrid(upVertex).y;
			y = _GridNodeSize * 2;
			return normalize(float3(x, y, z));
		}


		void vert( inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);

			v.vertex.y = VerteXOnGrid(v.vertex).y;
			float3 vnormal = CalculateNormal(v.vertex);
			v.normal = vnormal;
		}

		void surf (Input IN, inout SurfaceOutput o) {
		    //fixed4 basecolor = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			fixed4 reflcol = texCUBE (_Cube, worldRefl);

			o.Albedo = _Color.rgb;
			o.Emission = reflcol.rgb * (_ReflectColor.rgb * _ReflectColor.rgb);
			o.Alpha = length(reflcol.rgb) * _ReflectColor.a * _Color.a;
			o.Alpha = 0.5;
		}

		ENDCG
	}
	
	//FallBack "Reflective/Bumped Specular"
}
