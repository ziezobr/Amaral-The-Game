using UnityEngine;
namespace PontoCego
{
    /// <summary>24 sampled 2D deformation poses per gait and direction. UVs stay fixed: no atlas cropping.</summary>
    [DefaultExecutionOrder(110)]
    public sealed class DirectionalPose24:MonoBehaviour
    {
        public const int FramesPerDirection=24;
        SpriteRenderer source; Mesh mesh; MeshRenderer output; Material material; Sprite last;
        ReferenceActorAnimation gait;Vector3[] rest,vertices; Vector2 previous; Rigidbody2D body; float cycle;
        public int CurrentPose{get;private set;}
        void Start()
        {
            var actor=GetComponent<ReferenceActorAnimation>();gait=actor;var walker=GetComponent<StreetWalker>();var higor=GetComponent<HigorMartialArts>();source=actor?actor.visual:walker?walker.visual:higor?higor.visual:GetComponentInChildren<SpriteRenderer>();body=GetComponent<Rigidbody2D>();previous=transform.position;
            if(!source){enabled=false;return;}
            var child=new GameObject("Animação 2D / 24 poses por direção");child.transform.SetParent(source.transform,false);
            mesh=new Mesh{name="Malha plana de animação 2D"};child.AddComponent<MeshFilter>().sharedMesh=mesh;
            output=child.AddComponent<MeshRenderer>();material=new Material(Shader.Find("Sprites/Default"));output.sharedMaterial=material;
        }
        void LateUpdate()
        {
            if(!source||!output)return;
            output.enabled=source.enabled&&source.sprite;source.forceRenderingOff=output.enabled;
            if(!output.enabled)return;
            var sprite=source.sprite;if(last!=sprite){last=sprite;Rebuild(sprite);}
            Vector2 velocity=body?body.linearVelocity:((Vector2)transform.position-previous)/Mathf.Max(.001f,Time.deltaTime);previous=transform.position;
            float speed=velocity.magnitude;cycle+=Time.deltaTime*(speed>.14f?Mathf.Clamp(speed*.34f,1.1f,2.5f):0);
            if(gait&&speed>.14f)cycle=gait.GaitCycle;CurrentPose=Mathf.FloorToInt(Mathf.Repeat(cycle,1)*FramesPerDirection);
            float phase=CurrentPose*(Mathf.PI*2/FramesPerDirection),weight=GamePreferences.ReduceMotion?0:Mathf.Clamp01(speed/2);
            for(int i=0;i<rest.Length;i++)
            {
                var p=rest[i];float y=(p.y-sprite.bounds.min.y)/Mathf.Max(.001f,sprite.bounds.size.y);
                // Grounded feet, stable face, alternating leg/torso weight. Never alter UV coordinates.
                float lower=Mathf.Clamp01((.58f-y)/.58f),side=p.x>=sprite.bounds.center.x?1:-1;
                p.x+=Mathf.Sin(phase+side*.65f)*lower*.065f*weight;
                p.y+=Mathf.Sin(phase+side*Mathf.PI*.5f)*lower*.04f*weight;
                if(source.flipX)p.x=-p.x;if(source.flipY)p.y=-p.y;vertices[i]=p;
            }
            mesh.vertices=vertices;mesh.RecalculateBounds();
            material.mainTexture=sprite.texture;material.color=source.color;
            output.sortingLayerID=source.sortingLayerID;output.sortingOrder=source.sortingOrder;
        }
        void Rebuild(Sprite sprite)
        {
            const int columns=8,rows=12;rest=new Vector3[(columns+1)*(rows+1)];vertices=new Vector3[rest.Length];var uv=new Vector2[rest.Length];var triangles=new int[columns*rows*6];var b=sprite.bounds;var rect=sprite.textureRect;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {int i=y*(columns+1)+x;float u=x/(float)columns,v=y/(float)rows;rest[i]=new Vector3(Mathf.Lerp(b.min.x,b.max.x,u),Mathf.Lerp(b.min.y,b.max.y,v),0);uv[i]=new Vector2((rect.x+u*rect.width)/sprite.texture.width,(rect.y+v*rect.height)/sprite.texture.height);}
            int at=0;for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){int i=y*(columns+1)+x;triangles[at++]=i;triangles[at++]=i+columns+1;triangles[at++]=i+1;triangles[at++]=i+1;triangles[at++]=i+columns+1;triangles[at++]=i+columns+2;}
            mesh.Clear();mesh.vertices=rest;mesh.uv=uv;mesh.triangles=triangles;
        }
        void OnDisable(){if(source)source.forceRenderingOff=false;if(output)output.enabled=false;}
        void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
    }
}
