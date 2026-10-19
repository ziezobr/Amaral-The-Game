using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Local 2D pursuit; chain attacks and persistent road hazards, no scene teleport.</summary>
    public sealed class RoadChase:MonoBehaviour
    {
        WorldRoadAssets art;RoadDriver road;PlayerInputReader input;GameObject root;Transform gol,bike,skate;LineRenderer chain;float nextAttack,nextHazard,chainUntil,slowUntil;int attack;public int ChainHits{get;private set;}
        readonly List<GameObject> hazards=new List<GameObject>();
        Transform Sprite(string name,Sprite sprite,Vector2 at,float width)
        {var go=new GameObject(name);go.transform.SetParent(root.transform);go.transform.position=at;var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;go.transform.localScale=Vector3.one*(width/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y));return go.transform;}
        void Start()
        {
            art=WorldHub.Instance.GetComponent<WorldRoadAssets>();road=GetComponent<RoadDriver>();input=GetComponent<PlayerInputReader>();if(art.chaseMusic){var music=new GameObject("Asphalt Grip / trilha da perseguição").AddComponent<ChaseSoundtrack>();music.clip=art.chaseMusic;music.owner=road;}root=new GameObject("Perseguição / Martins e Amaral contra Kevyn");
            gol=Sprite("Kevyn / Gol bolinha branco",art.gol,(Vector2)transform.position+Vector2.right*4.5f,3.2f);
            bike=Sprite("Cristyan / BMX cinza",art.bmx,(Vector2)transform.position+new Vector2(-3,2),2.2f);var c=Sprite("Cristyan",art.cristyan,bike.position+Vector3.up*.5f,1.4f);c.SetParent(bike,true);
            skate=Sprite("Ribeiro / skate",art.skate,(Vector2)transform.position+new Vector2(-5,-2),1.2f);var r=Sprite("Ribeiro",art.ribeiro,skate.position+Vector3.up*.6f,1.4f);r.SetParent(skate,true);
            var line=new GameObject("Corrente de Martins");line.transform.SetParent(root.transform);chain=line.AddComponent<LineRenderer>();chain.sharedMaterial=new Material(Shader.Find("Sprites/Default"));chain.positionCount=9;chain.startWidth=chain.endWidth=.09f;chain.startColor=chain.endColor=new Color(.7f,.73f,.76f);chain.enabled=false;nextHazard=Time.time+3;
            FindAnyObjectByType<Interaction>()?.Say("F: Martins golpeia com a corrente. Desvie do óleo e da fumaça do Gol!");
        }
        void Update()
        {
            if(!road||!road.enabled||!input||input.GameplayLocked||input.Paused)return;
            float y=road.worldCenter.y+Mathf.Sin(Time.time*.8f)*2.8f;var wanted=new Vector3(transform.position.x+4.3f,y,0);gol.position=Vector3.MoveTowards(gol.position,wanted,8*Time.deltaTime);
            bike.position=Vector3.Lerp(bike.position,transform.position+new Vector3(-3.2f,1.9f,0),5*Time.deltaTime);skate.position=Vector3.Lerp(skate.position,transform.position+new Vector3(-5,-1.6f,0),5*Time.deltaTime);
            if(input.Actions.FindAction("Melee").WasPressedThisFrame()&&Time.time>=nextAttack){nextAttack=Time.time+.8f;chainUntil=Time.time+.24f;if(Vector2.Distance(gol.position,transform.position)<6){ChainHits++;gol.position+=Vector3.left*.6f;FindAnyObjectByType<OfficeAudio>()?.Impact();}}
            chain.enabled=Time.time<chainUntil;if(chain.enabled){for(int i=0;i<9;i++){float t=i/8f;chain.SetPosition(i,Vector3.Lerp(transform.position,gol.position,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.5f);}chain.sortingOrder=WorldSort.At(transform.position.y)+5;}
            if(Time.time>=nextHazard){nextHazard=Time.time+3.3f;bool oil=attack++%2==0;var h=Sprite(oil?"Óleo do Gol":"Fumaça do Gol",art.smoke,gol.position+Vector3.left*1.5f,oil?2.4f:4);h.GetComponent<SpriteRenderer>().color=oil?new Color(.04f,.035f,.02f,.85f):new Color(.3f,.3f,.32f,.7f);hazards.Add(h.gameObject);Destroy(h.gameObject,6);}
            foreach(var h in hazards)if(h&&Vector2.Distance(h.transform.position,transform.position)<1.3f){slowUntil=Time.time+1.2f;break;}
            road.HazardSlow=Time.time<slowUntil?.5f:1;
            foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>())sr.sortingOrder=WorldSort.At(sr.transform.position.y);
        }
        void OnDestroy(){if(road)road.HazardSlow=1;if(chain&&chain.sharedMaterial)Destroy(chain.sharedMaterial);if(root)Destroy(root);}
    }
}
