// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Lda6lge/DepthFade"
{
	Properties
	{
		_Color("Color ", Color) = (1,1,1,0)
		_Brightness("Brightness", Float) = 1
		_DepthFadeStart("Depth Fade Start", Float) = 0
		_DepthFadeDistance("Depth Fade Distance", Range( 0 , 100)) = 0
		_DepthFadePower("Depth Fade Power", Range( 0 , 10)) = 1
		_XFadeWidth("X Fade Width", Range( 0 , 0.5)) = 0
		_YFadeWidth("Y Fade Width", Range( 0 , 0.5)) = 0
		_NoiseTex("Noise Tex", 2D) = "white" {}
		_Noise1TilingAndSpeed("Noise1 Tiling And Speed", Vector) = (1,0.3,0,0)
		_Noise2TilingAndSpeed("Noise2 Tiling And Speed", Vector) = (1,0.3,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 4.6
		#pragma surface surf Lambert keepalpha noshadow 
		struct Input
		{
			half2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
		};

		uniform half4 _Color;
		uniform sampler2D _NoiseTex;
		uniform half4 _Noise1TilingAndSpeed;
		uniform half4 _Noise2TilingAndSpeed;
		uniform half _Brightness;
		uniform half _XFadeWidth;
		uniform half _YFadeWidth;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform half _DepthFadeDistance;
		uniform half _DepthFadePower;
		uniform half _DepthFadeStart;

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 appendResult75 = (half2(_Noise1TilingAndSpeed.z , _Noise1TilingAndSpeed.w));
			float2 uv_TexCoord71 = i.uv_texcoord * _Noise1TilingAndSpeed.xy;
			float2 panner73 = ( _Time.x * appendResult75 + uv_TexCoord71);
			float smoothstepResult90 = smoothstep( -1.0 , 1.0 , tex2D( _NoiseTex, panner73 ).r);
			float2 appendResult78 = (half2(_Noise2TilingAndSpeed.z , _Noise2TilingAndSpeed.w));
			float2 uv_TexCoord79 = i.uv_texcoord * _Noise2TilingAndSpeed.xy;
			float2 panner81 = ( _Time.x * appendResult78 + uv_TexCoord79);
			float smoothstepResult91 = smoothstep( -1.0 , 1.0 , tex2D( _NoiseTex, panner81 ).r);
			float Emission120 = ( smoothstepResult90 * smoothstepResult91 );
			half4 temp_cast_2 = (_Brightness).xxxx;
			o.Emission = saturate( (temp_cast_2 + (( _Color * Emission120 ) - float4( 0,0,0,0 )) * (float4( 1,1,1,1 ) - temp_cast_2) / (float4( 1,1,1,1 ) - float4( 0,0,0,0 ))) ).rgb;
			float smoothstepResult33 = smoothstep( 0.0 , _XFadeWidth , i.uv_texcoord.x);
			float smoothstepResult35 = smoothstep( 0.0 , _XFadeWidth , ( 1.0 - i.uv_texcoord.x ));
			float smoothstepResult41 = smoothstep( 0.0 , _YFadeWidth , i.uv_texcoord.y);
			float smoothstepResult40 = smoothstep( 0.0 , _YFadeWidth , ( 1.0 - i.uv_texcoord.y ));
			float AroundOpacity65 = saturate( ( smoothstepResult33 * smoothstepResult35 * smoothstepResult41 * smoothstepResult40 ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth4 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth4 = saturate( abs( ( screenDepth4 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFadeDistance ) ) );
			half3 ase_worldPos = i.worldPos;
			float DepthFadeOpacity125 = saturate( ( pow( distanceDepth4 , _DepthFadePower ) * saturate( ( distance( _WorldSpaceCameraPos , ase_worldPos ) * ( 1.0 / _DepthFadeStart ) ) ) ) );
			o.Alpha = saturate( ( AroundOpacity65 * DepthFadeOpacity125 ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
316;92;846;621;-84.03461;-435.9582;1.771249;True;False
Node;AmplifyShaderEditor.CommentaryNode;119;-1794.59,1532.029;Float;False;2860.14;766.7598;;16;89;120;90;91;112;111;81;73;110;113;79;71;78;75;118;117;NoiseEmission;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;123;-1341.774,747.8774;Float;False;1370.422;660.9983;;14;5;4;7;6;8;12;11;9;10;124;125;128;129;132;Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;118;-1669.471,2028.857;Float;False;Property;_Noise2TilingAndSpeed;Noise2 Tiling And Speed;10;0;Create;True;0;0;False;0;1,0.3,0,0;0.7,0.7,-0.1,0.3;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;117;-1674.844,1626.423;Float;False;Property;_Noise1TilingAndSpeed;Noise1 Tiling And Speed;9;0;Create;True;0;0;False;1;;1,0.3,0,0;0.5,0.5,0,0.2;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;113;-1405.761,1820.879;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;78;-1318.912,2127.246;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;71;-1388.5,1557.864;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;75;-1363.666,1696.962;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;79;-1371.662,1997.602;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceCameraPos;9;-1321.505,870.0499;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;127;-1634.289,-101.5239;Float;False;1567.232;761.0975;Comment;12;65;38;41;33;35;40;43;34;44;36;32;169;Around Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1314.059,1266.338;Float;False;Property;_DepthFadeStart;Depth Fade Start;3;0;Create;True;0;0;False;0;0;4.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-1299.934,1051.759;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-1509.605,153.1229;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;110;-1099.406,1789.56;Float;True;Property;_NoiseTex;Noise Tex;8;0;Create;True;0;0;False;0;None;c48b47665fcc3dd4082b237cd7c381e7;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;73;-1018.015,1650.824;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;81;-1035.552,2040.861;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;12;-1057.2,1173.37;Float;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;8;-1038.798,972.427;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1232.937,771.6809;Float;False;Property;_DepthFadeDistance;Depth Fade Distance;4;0;Create;True;0;0;False;0;0;0.9;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;112;-730.905,1957.406;Float;True;Property;_TextureSample1;Texture Sample 1;9;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;129;-964.5134,909.0663;Float;False;Property;_DepthFadePower;Depth Fade Power;5;0;Create;True;0;0;False;0;1;1.53;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;111;-718.5482,1672.944;Float;True;Property;_TextureSample0;Texture Sample 0;9;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;36;-1121.862,172.8012;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-869.0856,1045.217;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;44;-1131.977,507.8444;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1341.819,385.0067;Float;False;Property;_YFadeWidth;Y Fade Width;7;0;Create;True;0;0;False;0;0;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1363.492,-4.911272;Float;False;Property;_XFadeWidth;X Fade Width;6;0;Create;True;0;0;False;0;0;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;4;-954.3023,806.9417;Float;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;90;-373.0686,1689.487;Float;True;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;7;-678.5997,1049.691;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;33;-847.8353,-3.282305;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;128;-602.3226,842.6523;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;41;-845.7552,317.5343;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;35;-830.4522,136.6482;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;40;-840.5652,469.6589;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;91;-366.9993,1942.192;Float;True;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-434.363,896.2573;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-21.3009,1788.083;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-568.2121,178.5462;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;132;-238.9285,921.149;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;169;-424.8956,162.9287;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;120;615.635,1801.972;Float;False;Emission;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;158.6697,325.0056;Float;False;120;Emission;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-260.7484,1072.37;Float;False;DepthFadeOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;65;-278.0049,164.5714;Float;False;AroundOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;156.1236,146.8688;Float;False;Property;_Color;Color ;1;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;171;599.2248,446.6784;Float;False;Property;_Brightness;Brightness;2;0;Create;True;0;0;False;0;1;-0.09;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;413.4625,262.3443;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;902.7287,922.2834;Float;False;125;DepthFadeOpacity;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;872.6566,766.288;Float;False;65;AroundOpacity;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;186;827.3659,331.54;Float;False;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;3;COLOR;0,0,0,0;False;4;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;1207.341,846.5305;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;191;1357.957,773.6922;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;170;1000.957,336.1901;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1589.786,439.5321;Half;False;True;6;Half;ASEMaterialInspector;0;0;Lambert;Lda6lge/DepthFade;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;78;0;118;3
WireConnection;78;1;118;4
WireConnection;71;0;117;0
WireConnection;75;0;117;3
WireConnection;75;1;117;4
WireConnection;79;0;118;0
WireConnection;73;0;71;0
WireConnection;73;2;75;0
WireConnection;73;1;113;1
WireConnection;81;0;79;0
WireConnection;81;2;78;0
WireConnection;81;1;113;1
WireConnection;12;1;11;0
WireConnection;8;0;9;0
WireConnection;8;1;10;0
WireConnection;112;0;110;0
WireConnection;112;1;81;0
WireConnection;111;0;110;0
WireConnection;111;1;73;0
WireConnection;36;0;32;1
WireConnection;6;0;8;0
WireConnection;6;1;12;0
WireConnection;44;0;32;2
WireConnection;4;0;5;0
WireConnection;90;0;111;1
WireConnection;7;0;6;0
WireConnection;33;0;32;1
WireConnection;33;2;34;0
WireConnection;128;0;4;0
WireConnection;128;1;129;0
WireConnection;41;0;32;2
WireConnection;41;2;43;0
WireConnection;35;0;36;0
WireConnection;35;2;34;0
WireConnection;40;0;44;0
WireConnection;40;2;43;0
WireConnection;91;0;112;1
WireConnection;124;0;128;0
WireConnection;124;1;7;0
WireConnection;89;0;90;0
WireConnection;89;1;91;0
WireConnection;38;0;33;0
WireConnection;38;1;35;0
WireConnection;38;2;41;0
WireConnection;38;3;40;0
WireConnection;132;0;124;0
WireConnection;169;0;38;0
WireConnection;120;0;89;0
WireConnection;125;0;132;0
WireConnection;65;0;169;0
WireConnection;122;0;1;0
WireConnection;122;1;121;0
WireConnection;186;0;122;0
WireConnection;186;3;171;0
WireConnection;2;0;66;0
WireConnection;2;1;126;0
WireConnection;191;0;2;0
WireConnection;170;0;186;0
WireConnection;0;2;170;0
WireConnection;0;9;191;0
ASEEND*/
//CHKSM=9B40E7F9C9157AE965AC453A95A972CF719C6AA5