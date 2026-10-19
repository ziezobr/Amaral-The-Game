Shader "PontoCego/Pintura com contorno"
{
 Properties {
  _Color("Cor",Color)=(1,1,1,1)
  _MainTex("Textura original",2D)="white"{}
  _EmissionColor("Emissão noturna",Color)=(0,0,0,1)
  _Grain("Variação da pintura",Range(0,.2))=.06
  _Outline("Espessura do traço",Range(0,.06))=.012
 }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 200
  Pass {
   Name "CONTORNO" Cull Front ZWrite On
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _Outline;
   struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;};
   float4 vert(appdata v):SV_POSITION {float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;float3 n=UnityObjectToWorldNormal(v.normal);return mul(UNITY_MATRIX_VP,float4(p+n*_Outline,1));}
   fixed4 frag():SV_Target{return fixed4(.055,.043,.045,1);}
   ENDCG
  }
  CGPROGRAM
  #pragma surface surf Painted fullforwardshadows addshadow
  #pragma target 3.0
  sampler2D _MainTex;fixed4 _Color;fixed4 _EmissionColor;half _Grain;
  struct Input {float2 uv_MainTex;float3 worldPos;};
  half4 LightingPainted(SurfaceOutput s,half3 lightDir,half atten)
  {
   half d=dot(s.Normal,lightDir);half band=.32+.35*smoothstep(.12,.2,d)+.33*smoothstep(.62,.69,d);
   return half4(s.Albedo*_LightColor0.rgb*band*atten,s.Alpha);
  }
  void surf(Input IN,inout SurfaceOutput o)
  {
   fixed3 c=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;
   float3 p=IN.worldPos;half strokes=sin(p.x*47+p.z*19+sin(p.y*23))*sin(p.y*57+p.z*31);
   o.Albedo=c*(1+strokes*_Grain);o.Alpha=1;o.Emission=_EmissionColor.rgb;
  }
  ENDCG
 }
 Fallback "Diffuse"
}
