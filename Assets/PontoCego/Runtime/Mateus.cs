using UnityEngine;
namespace PontoCego
{
    /// <summary>Neutral, persistent quest giver; deliberate player attacks provoke one telegraphed retaliation.</summary>
    public sealed class Mateus:MonoBehaviour,IInteractable
    {
        // One location contract for authoring, quest GPS, rewards and validation captures.
        public static readonly Vector2 HomePosition=new Vector2(-99,77);
        public static readonly Vector2 ApproachPosition=HomePosition+Vector2.down*1.5f;
        public static readonly Vector2 TrailEntrance=new Vector2(-90,72.5f);
        public static readonly Vector2 GlockStashPosition=HomePosition+Vector2.left*3.4f;
        public static readonly Vector2 RazorStashPosition=new Vector2(178,-74);
        public static readonly Vector2 CapturePosition=HomePosition+new Vector2(2,-2.4f);
        public static Rect HideoutBounds=>new Rect(-105,69,16,15);
        public Sprite cameraIcon; public SpriteRenderer visual; public Sprite idle,side,back,fighting; public int aggressions;
        float strikeAt=-1,calmAt; PlayerMotor player;
        public static float LethalChance(int attacks)=>Mathf.Clamp(.01f+Mathf.Max(0,attacks-1)*.025f,.01f,.5f);
        public string Prompt=>Time.time<calmAt?"Mateus • Pare de atacar e afaste-se":"E • Conversar com Mateus";
        public bool ReceiveAttack(CombatTeam attacker)
        {
            if(attacker!=CombatTeam.Player)return false;
            aggressions++;calmAt=Time.time+12;if(strikeAt<0)strikeAt=Time.time+.75f;
            FindAnyObjectByType<Interaction>()?.Say("Mateus: Pare! Continuar atacando aumenta o risco.");return true;
        }
        void Update()
        {
            if(!player)player=FindAnyObjectByType<PlayerMotor>();if(!player)return;
            var input=player.GetComponent<PlayerInputReader>();if(input.Paused||input.GameplayLocked){if(strikeAt>=0)strikeAt+=Time.deltaTime;calmAt+=Time.deltaTime;return;}
            if(strikeAt>=0&&Time.time>=strikeAt)
            {
                if(Vector2.Distance(transform.position,player.transform.position)<=2.3f)
                {var hp=player.GetComponent<Health>();hp.Damage(Random.value<LethalChance(aggressions)?hp.maximum*10:18,CombatTeam.Enemy);}
                strikeAt=-1;
            }
            if(visual)
            {
                Vector2 direction=player.transform.position-transform.position;bool attacking=strikeAt>=0&&fighting;
                visual.flipX=false;
                if(attacking){visual.sprite=fighting;visual.flipX=direction.x<0;}
                else if(direction.sqrMagnitude<36&&Mathf.Abs(direction.x)>Mathf.Abs(direction.y)&&side){visual.sprite=side;visual.flipX=direction.x<0;}
                else visual.sprite=direction.sqrMagnitude<36&&direction.y>.6f&&back?back:idle;
                visual.sortingOrder=WorldSort.At(transform.position.y)+1;
            }
        }
        void OnGUI(){if(!player)return;var input=player.GetComponent<PlayerInputReader>();if(input.Paused||input.GameplayLocked)return;if(Vector2.Distance(player.transform.position,transform.position)<2)CollectibleCover.Hint(transform.position,Prompt);}
        public void Interact(){if(Time.time<calmAt)return;GetComponent<GoldenWeaponQuest>()?.Talk();
            if(!GameSession.Current.collected.Contains("weapon:xuxa")){GameSession.Current.collected.Add("weapon:xuxa");GameSession.Save();var equipment=FindAnyObjectByType<PlayerEquipment2D>();if(equipment){equipment.pinkIcon=cameraIcon;equipment.Select(PlayerEquipment.PinkCamera);}FindAnyObjectByType<Interaction>()?.Say("Mateus: Câmera da Xuxa equipada! Cada flash tira 10% da vida máxima de inimigos no enquadramento.");return;}}
    }
}
