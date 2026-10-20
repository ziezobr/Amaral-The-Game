using UnityEngine;
namespace PontoCego
{
    public sealed class EquipmentShop:MonoBehaviour,IInteractable
    {
        public const int ProductCount=11;
        public bool Active{get;private set;}
        public string Prompt=>"Henzo Ratão • equipamentos";
        public string Notice{get;private set;}="";
        PlayerInputReader input;
        public int storyStep=-1;public string[] storyLines;public Texture2D portrait;
        static readonly string[] names={"Primeiros socorros","Colete reforçado","Capacitor do flash","Suprimentos dos aliados","Munição mista","8 cargas de flash","Pistola / melhoria","Curativo imediato","14 cargas de flash","Pacote de campo"};
        public static readonly int[] IconIndices={4,5,6,7,7,8,9,10,11,13};
        public static string ProductName(int item)=>item==10?"Monster branco • fôlego":item>=0&&item<names.Length?names[item]:"";
        static readonly string[] descriptions={"Adiciona 1 kit médico ao inventário. Limite: 8 kits.","Melhoria permanente: +20 de vida máxima por nível. Até 5 níveis.","Reduz a recarga entre flashes e prolonga o atordoamento. Até 5 níveis.","Fortalece Ribeiro e Cristyan e repõe 18 munições dos aliados. Até 5 níveis.","Repõe 24 munições de reserva e 18 dos aliados. Estoques limitados a 120.","Repõe 8 cargas para atordoar inimigos com a câmera. Limite: 40.","Libera a pistola e aumenta seu nível de combate. Até 5 níveis.","Recupera 25 de vida agora. Não aumenta a vida máxima.","Repõe 14 cargas de flash. Limite: 40 cargas.","Inclui 2 kits médicos, 6 flashes e 12 munições de reserva.","Melhoria permanente: +20% de fôlego máximo por nível. Até 5 níveis (+100%). Não concede fôlego infinito."};
        public static string Description(int item)=>item>=0&&item<descriptions.Length?descriptions[item]:"";
        public static string LevelText(int item){var s=GameSession.Current;int level=item==1?s.armorLevel:item==2?s.flashLevel:item==3?s.allyLevel:item==6?s.weaponLevel:item==10?s.staminaLevel:-1;return level<0?"CONSUMÍVEL":level>=5?"NÍVEL MÁXIMO • 5/5":"MELHORIA • "+level+"/5";}
        public static int Price(int item)
        {
            var s=GameSession.Current;switch(item){case 0:return 40;case 1:return 160+s.armorLevel*80;case 2:return 140+s.flashLevel*100;case 3:return 120+s.allyLevel*80;case 4:return 60;case 5:return 50;case 6:return 160+s.weaponLevel*100;case 7:return 20;case 8:return 70;case 9:return 100;case 10:return 130+s.staminaLevel*90;default:return int.MaxValue;}
        }
        public void Interact()
        {
            input=FindAnyObjectByType<PlayerInputReader>();var level=FindAnyObjectByType<LevelProgress>();
            if(level.Step==storyStep){input.GetComponent<StoryDialogue>().Begin("Henzo Ratão",portrait,storyLines,()=>level.Advance());return;}
            input.SetGameplayLocked(true);Active=true;Notice="";
        }
        public void Close(){Active=false;if(input)input.SetGameplayLocked(false);GameSession.Save();}
        public void Buy(int item)
        {
            if(item<0||item>=ProductCount)return;var save=GameSession.Current;int cost=Price(item);var player=FindAnyObjectByType<CombatPlayer>();var health=player.GetComponent<Health>();
            if(save.currency<cost){Notice="Dinheiro insuficiente.";return;}
            bool full=item==0&&player.Medkits>=8||item==1&&save.armorLevel>=5||item==2&&save.flashLevel>=5||item==3&&save.allyLevel>=5||item==4&&save.ammoReserve>=120&&save.allyAmmo>=120||item==5&&save.flashCharges>=40||item==6&&save.weaponLevel>=5||item==7&&health.Current>=health.maximum||item==8&&save.flashCharges>=40||item==9&&player.Medkits>=8&&save.flashCharges>=40&&save.ammoReserve>=120;
            full|=item==10&&save.staminaLevel>=5;
            if(full){Notice="Equipamento cheio ou melhoria no máximo.";return;}save.currency-=cost;
            switch(item)
            {
                case 0:player.AddMedkit();break;
                case 1:save.armorLevel++;health.IncreaseMaximum(20);break;
                case 2:save.flashLevel++;break;
                case 3:save.allyLevel++;save.allyAmmo=Mathf.Min(120,save.allyAmmo+18);break;
                case 4:save.ammoReserve=Mathf.Min(120,save.ammoReserve+24);save.allyAmmo=Mathf.Min(120,save.allyAmmo+18);break;
                case 5:save.flashCharges=Mathf.Min(40,save.flashCharges+8);break;
                case 6:save.hasSidearm=true;save.weaponLevel++;break;
                case 7:health.Heal(25);break;
                case 8:save.flashCharges=Mathf.Min(40,save.flashCharges+14);break;
                case 9:player.AddMedkit();player.AddMedkit();save.flashCharges=Mathf.Min(40,save.flashCharges+6);save.ammoReserve=Mathf.Min(120,save.ammoReserve+12);break;
                case 10:save.staminaLevel++;player.GetComponent<PlayerMotor>().Model.SetStaminaUpgrade(save.staminaLevel);break;
            }
            Notice="Comprado: "+ProductName(item)+".";GameSession.Save();
        }
    }
}
