Shader "Sprites/DynamicMaskShader"
{
	Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _MaskTex ("MaskTexture", 2D) = "white" {}
      _MaskRotation ("MaskRotation", float) = 0
      _MaskEnable ("MaskEnable", int) = 0
    }
    SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
		}
		Blend SrcAlpha OneMinusSrcAlpha

     	CGPROGRAM
     	#pragma surface surf Lambert alpha 

		struct Input {
			float2 uv_MainTex;
		};
		sampler2D _MainTex, _MaskTex;
		float _MaskRotation;
		int _MaskEnable;

		float2 RotateVertex (float2 v) {
			float x = cos(_MaskRotation) * v.x - sin(_MaskRotation) * v.y;
			float y = cos(_MaskRotation) * v.y + sin(_MaskRotation) * v.x;
			return float2(x, y);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float2 uv = IN.uv_MainTex;
			float2 rotatedUV = RotateVertex(float2(uv.x -0.5, uv.y-0.5));
			rotatedUV.x += 0.5;
			rotatedUV.y += 0.5;

			float4 c = tex2D (_MainTex, uv);
			float4 maskc = tex2D(_MaskTex, rotatedUV);
			o.Albedo = c.rgb;

			if (_MaskEnable) {
				o.Alpha = c.w * maskc.w;
			} else {
				o.Alpha = c.w;
			}
		}
		ENDCG
    } 
}