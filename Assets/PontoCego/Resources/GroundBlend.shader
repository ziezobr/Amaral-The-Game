Shader "PontoCego/GroundBlend"
{
 Properties { _MainTex("Atlas",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _UVRect("Atlas region",Vector)=(0,0,1,1) _Repeat("Repeat",Vector)=(1,1,0,0) _Feather("Soft edge",Range(0.001,0.5))=0.06 _WorldSpace("Continuous world UV",Float)=0 }
 SubShader
 {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
  Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
   struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float2 world:TEXCOORD1; fixed4 color:COLOR; };
   sampler2D _MainTex;float4 _MainTex_TexelSize,_UVRect,_Repeat;fixed4 _Color;float _Feather,_WorldSpace;
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xy;o.color=v.color*_Color;return o;}
   fixed4 frag(v2f i):SV_Target
   {
    float2 local=(i.uv-_UVRect.xy)/max(_UVRect.zw,float2(.00001,.00001));
    float2 edge=min(local,1-local);float fade=smoothstep(0,_Feather,min(edge.x,edge.y));
    float2 coord=_WorldSpace>.5?i.world/6:local*max(_Repeat.xy,float2(1,1));
    float2 tiled=1-abs(frac(coord*.5)*2-1);
    float2 margin=_MainTex_TexelSize.xy*1.5;
    float2 at=_UVRect.xy+margin+tiled*max(_UVRect.zw-margin*2,float2(.00001,.00001));
    fixed4 c=tex2D(_MainTex,at);
    if(_WorldSpace>.5||_Repeat.x>1.01||_Repeat.y>1.01){float2 uv2=1-abs(frac(coord*.367+float2(.327,.619))*2-1);fixed4 b=tex2D(_MainTex,_UVRect.xy+margin+uv2*max(_UVRect.zw-margin*2,float2(.00001,.00001)));c=lerp(c,b,.45);}
    c*=i.color;c.a*=fade;return c;
   }
   ENDCG
  }
 }
}
