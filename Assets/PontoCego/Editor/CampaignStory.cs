using System.Collections.Generic;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static StepDef Inspect(string text,float u,float v,string message,string speaker="Amaral")=>new StepDef("inspect",text,u,v,message,speaker);
        static StepDef Talk(string speaker,float u,float v,string text)=>new StepDef("talk","Converse com "+speaker+".",u,v,text,speaker);
        static StepDef Reach(string text,float u,float v)=>new StepDef("reach",text,u,v);
        static StepDef Picture(string text,float u,float v,string textAfter)=>new StepDef("photo",text,u,v,textAfter);
        static StepDef Exit(string text,float u,float v,string message)=>new StepDef("exit",text,u,v,message);
        static StepDef Fight(string text,float u,float v,params EnemyRole[] roles)=>new StepDef("fight",text,u,v){roles=roles};
        static StepDef Sequence(string text,float u,float v,SequenceKind sequence)=>new StepDef("sequence",text,u,v){sequence=sequence};
        static StepDef Puzzle(string text,float u,float v,string question,string[] options,int correct)=>new StepDef("puzzle",text,u,v,question){choices=options,correct=correct};
        static List<StageDef> AuthoredStages()
        {
            const EnemyRole B=EnemyRole.Baton,G=EnemyRole.Gunner,E=EnemyRole.Elite,H=EnemyRole.Heavy;
            var list=new List<StageDef>();
            StageDef Add(string scene,string title,string act,string map,bool allies,Vector2 spawn,params StepDef[] steps)
            {
                var stage=new StageDef{scene=scene,title=title,act=act,map=map,allies=allies,spawn=spawn,steps=new List<StepDef>(steps)};
                if(list.Count>0)list[list.Count-1].next=scene;list.Add(stage);return stage;
            }
            Add("03_Casa","A porta arrombada","ATO 1","home",false,new Vector2(.30f,.51f),
                Sequence("Chegue à porta de casa.",.49f,.35f,SequenceKind.Kidnapping),
                Inspect("Entre e procure pistas de Adryan.",.43f,.23f,"A fechadura foi arrancada. A mochila do Adryan ficou aqui. Ele não saiu por vontade própria."),
                Picture("Fotografe as marcas do carro na rua.",.59f,.48f,"Pneus estreitos, escapamento estourado... era o Gol branco do Cenoura."),
                Fight("Afaste os homens deixados na rua.",.60f,.53f,B,B,G),
                Inspect("Confira o telefone caído perto da porta.",.50f,.35f,"Uma mensagem para o Cenoura: 'O chefe espera na agência'. Paulo vai me dizer onde está o Adryan."),
                Exit("Volte para a Riberal.",.18f,.50f,"Não vou deixar meu irmão com esses caras."));
            Add("04_Emboscada","Colegas de verdade","ATO 1","office",true,new Vector2(.49f,.78f),
                Reach("Avance até a produção para procurar Paulo.",.47f,.59f),
                Fight("Sobreviva à emboscada na produção.",.46f,.49f,B,B,G,B),
                Inspect("Reagrupe com Ribeiro e Cristyan.",.52f,.61f,"Ribeiro: Abaixa! Meu 38 não é bonito, mas resolve. Cristyan: Então deixa a navalha comigo. Vamos tirar você daqui.","Ribeiro"),
                Fight("Abra passagem pelo corredor da diretoria.",.61f,.31f,G,B,G,H,B),
                Inspect("Procure a ordem deixada por Paulo.",.82f,.32f,"O Paulo mandou limpar o escritório. A maleta e as rotas eram reais. O sequestro foi para me calar."),
                Fight("Rompa o bloqueio da recepção.",.50f,.70f,B,G,B,G,H),
                Exit("Saia da agência com seus colegas.",.50f,.85f,"Cenoura está lá fora. Não parem!"));
            Add("05_ResgateMartins","Uma Ninja na noite","ATO 1","home",true,new Vector2(.20f,.52f),
                Fight("Resista ao cerco do Gol branco.",.48f,.52f,G,B,G,B),
                Reach("Procure uma abertura pelo lado da rua.",.72f,.53f),
                Sequence("Alcance a fumaça da Ninja verde.",.67f,.50f,SequenceKind.MartinsRescue),
                Fight("Cubra a retirada do grupo.",.75f,.56f,B,G,B),
                Exit("Siga Martins até a oficina.",.87f,.55f,"Martins: Amaral vai na garupa. Ribeiro, segue no skate; Cristyan, pega a BMX. Nos reunimos na oficina. Depois explico o resto."));
            Add("06_Oficina","O que existe por trás","ATO 2","workshop",false,new Vector2(.50f,.76f),
                Talk("Martins",.52f,.28f,"Paulo não vive de marketing. A Riberal lava o dinheiro de um sindicato criminal. O Cenoura administra as fachadas e faz as transferências."),
                Inspect("Examine o quadro de rotas da oficina.",.50f,.22f,"Distribuidoras, galpões e pontos na rodovia. Se cruzarmos as rotas com as fotos da gala, chegamos ao Adryan."),
                Talk("Henzo",.78f,.46f,"Me chamam de Ratão. Tenho flash de atordoamento, munição para o 38 e pedra para essa navalha. Dinheiro de capanga também é dinheiro."),
                Inspect("Prepare o equipamento com Ribeiro.",.43f,.45f,"Ribeiro: O flash abre a brecha; eu cubro o corredor. Cristyan pega quem chegar perto. Não se separa da gente.","Ribeiro"),
                Exit("Parta para a primeira distribuidora.",.50f,.78f,"Primeiro derrubamos as fachadas. Depois cobramos a dívida do Cenoura."));
            Add("07_Distribuidora","Caixas e notas frias","ATO 2","warehouse",true,new Vector2(.40f,.75f),
                Reach("Entre pelo corredor de carga.",.39f,.64f),
                Fight("Tire os vigias do caminho.",.40f,.52f,B,G,B,G),
                Picture("Registre as caixas sem identificação.",.28f,.40f,"O mesmo código da rota entregue ao Paulo. A distribuidora só existe no papel."),
                Inspect("Leia a nota fiscal no escritório.",.75f,.24f,"A carga saiu sem bebida e voltou com peso dobrado. O destinatário usa uma empresa de fachada da Riberal."),
                Fight("Impeça a destruição dos registros.",.54f,.39f,G,H,G,B,E),
                Picture("Fotografe a planilha do carregamento.",.76f,.23f,"A próxima parada é o pátio da rodovia. A transferência foi antecipada."),
                Puzzle("Cruze a nota com o pagamento da agência.",.75f,.24f,"Quem recebe pelas campanhas que nunca existiram?",new[]{"A padaria da esquina","A Agência Riberal","A oficina abandonada"},1),
                Fight("Rompa a saída vigiada.",.39f,.68f,B,G,H,B),
                Exit("Vá ao pátio de cargas.",.40f,.77f,"Temos a rota. Ainda dá tempo."));
            Add("08_Patio","Carregamento antecipado","ATO 2","warehouse",true,new Vector2(.79f,.73f),
                Fight("Domine o pátio de descarga.",.76f,.55f,G,G,B,H),
                Picture("Fotografe os lacres do lote.",.83f,.65f,"Os lacres foram trocados. Alguém está preparando uma transferência de emergência."),
                Inspect("Encontre a chave entre os paletes.",.39f,.39f,"A chave abre o arquivo de despacho. O registro do Gol precisa estar lá."),
                Fight("Atravesse o depósito interno.",.39f,.55f,B,H,G,E,G),
                Inspect("Confira o despacho no escritório.",.74f,.23f,"Cenoura assinou como motorista. O número da viagem bate com o horário do sequestro."),
                Puzzle("Confirme quem assinou o despacho.",.75f,.24f,"Qual nome liga o Gol branco ao carregamento?",new[]{"Henzo Ratão","Martins","Kevyn, o Cenoura"},2),
                Fight("Afaste os reforços da rodovia.",.76f,.64f,E,G,B,G,H),
                Exit("Leve os registros para o ponto de apoio.",.78f,.78f,"Se o Gol passou por aqui, deixou outras pistas."));
            Add("09_RastroCenoura","O rastro do Gol","ATO 2","home",true,new Vector2(.86f,.54f),
                Reach("Chegue à esquina indicada pelo despacho.",.67f,.61f),
                Inspect("Pergunte pelo carro no ponto de apoio.",.26f,.59f,"Um Gol branco passou cantando pneu. O motorista discutia no telefone e seguiu para os galpões."),
                Fight("Afaste os homens que seguem o grupo.",.52f,.51f,G,B,H,G),
                Picture("Registre a marca do escapamento no meio-fio.",.63f,.41f,"O mesmo Gol. O Cenoura acelerou daqui direto para o corredor de serviço."),
                Fight("Atravesse a rua bloqueada.",.76f,.52f,G,E,B,G),
                Inspect("Recolha a etiqueta caída na calçada.",.72f,.38f,"A etiqueta é de um lote guardado num segundo armazém."),
                Exit("Investigue o segundo armazém.",.90f,.54f,"Cada prova fecha mais uma saída para eles."));
            Add("10_ArquivoCargas","A contabilidade do crime","ATO 2","warehouse",true,new Vector2(.39f,.74f),
                Fight("Limpe o acesso aos arquivos.",.38f,.63f,B,G,E,G),
                Inspect("Encontre o livro-caixa.",.73f,.24f,"A agência recebe por campanhas que nunca existiram. O dinheiro atravessa as distribuidoras e volta limpo."),
                Picture("Fotografe a relação de pagamentos.",.77f,.20f,"Paulo controla tudo. O Cenoura recebe pelas rotas e pela cobrança."),
                Fight("Proteja as provas dos capangas.",.54f,.39f,E,G,H,G,B),
                Inspect("Cruze a placa com os horários.",.39f,.38f,"O Gol deve sair do próximo depósito antes do amanhecer. Há uma observação: 'passageiro especial'."),
                Puzzle("Interprete a observação do carregamento.",.39f,.39f,"O que o horário e a expressão 'passageiro especial' indicam?",new[]{"A carga pode encobrir a transferência de Adryan","É um evento corporativo da Riberal","É uma compra de equipamentos de Henzo"},0),
                Fight("Abra a passagem até o portão.",.40f,.65f,G,H,E,G,B),
                Exit("Volte à oficina com as provas.",.40f,.77f,"Os horários sugerem que Adryan será levado com a carga. Ainda precisamos confirmar."));
            Add("11_OficinaRotas","A transferência","ATO 2","workshop",false,new Vector2(.50f,.76f),
                Talk("Martins",.50f,.28f,"A placa foi vista na estrada de serviço. O comboio segue para o ferro-velho, mas antes passa pelo último depósito."),
                Inspect("Atualize o quadro com as provas.",.50f,.22f,"Rotas, horários e pagamentos se encaixam. Uma transferência falsa está escondendo o caminho do Adryan."),
                Talk("Henzo",.78f,.46f,"Agora eles sabem que vocês estão perto. Revisa esse flash e leva curativo. O próximo pessoal não vai estar de brincadeira."),
                Exit("Siga para o depósito de transferência.",.50f,.78f,"Martins prepara a Ninja. Nós abrimos a passagem."));
            Add("12_Transferencia","A última fachada","ATO 2","warehouse",true,new Vector2(.79f,.75f),
                Fight("Rompa o bloqueio do pátio.",.76f,.58f,E,G,H,G,B),
                Reach("Entre no corredor entre os paletes.",.39f,.40f),
                Fight("Neutralize a escolta da transferência.",.52f,.41f,E,H,G,E,B),
                Picture("Registre o mapa deixado na carga.",.39f,.25f,"Saída pela estrada de serviço. Ponto final: ferro-velho."),
                Inspect("Confira a hora da partida.",.76f,.23f,"O comboio acabou de sair. Cenoura está no Gol branco à frente."),
                Puzzle("Escolha a rota de interceptação.",.76f,.24f,"Qual destino aparece no mapa recém-fotografado?",new[]{"O ferro-velho pela estrada de serviço","A padaria pelo centro","A casa de Amaral pela rua de trás"},0),
                Fight("Garanta a saída para Martins.",.40f,.66f,E,G,H,G,E),
                Exit("Encontre a Ninja na estrada.",.40f,.77f,"Amaral vai na garupa. Cristyan acompanha na BMX cinza e Ribeiro vem de skate. Vamos atrás do Gol."));
            Add("13_Estrada","Entre faróis e sucata","ATO 3","road",false,new Vector2(.06f,.49f),
                Reach("Alcance o primeiro ponto da rota.",.25f,.49f),
                Reach("Desvie do bloqueio na pista.",.46f,.55f),
                Reach("Mantenha a rota pela faixa livre.",.68f,.40f),
                Reach("Chegue à entrada do ferro-velho.",.89f,.48f),
                Exit("Entre no ferro-velho com o grupo.",.94f,.49f,"Martins: O Gol entrou ali. Agora ele não tem mais para onde correr."));
            Add("14_FerroVelho","O cerco","ATO 3","scrap",true,new Vector2(.50f,.79f),
                Fight("Abra o corredor de sucata.",.50f,.61f,G,H,E,G),
                Inspect("Examine as marcas do Gol.",.48f,.42f,"Óleo fresco. O motor não aguenta muito mais. Avisei Higor pelo celular: precisamos de ajuda no ferro-velho. Ele está vindo."),
                Fight("Cerque a equipe do Cenoura.",.80f,.42f,E,G,H,G,E),
                Picture("Registre as placas escondidas na guarita.",.83f,.17f,"As placas ligam o ferro-velho aos galpões. Mais provas para derrubar o Paulo."),
                Fight("Avance até o motor fundido.",.50f,.30f,H,E,G,E,B),
                Exit("Confronte o Cenoura no pátio superior.",.50f,.20f,"O motor fundiu. Ele vai ter que sair do carro."));
            Add("15_Cenoura","A porta como escudo","ATO 3","scrap",true,new Vector2(.50f,.74f),
                Fight("Rompa a última escolta do Cenoura.",.50f,.41f,E,G,H,E),
                Reach("Alcance a arena entre as pilhas.",.50f,.23f),
                Fight("Derrube o Cenoura. Use o flash para abrir a guarda.",.50f,.24f,EnemyRole.Cenoura,G,E),
                Inspect("Exija a localização de Adryan.",.50f,.22f,"Cenoura: A rota era uma isca. Paulo mudou o plano depois da gala e levou Adryan para a cobertura da Riberal. Vocês perseguiram o carro errado...", "Kevyn"),
                Exit("Retorne à agência para resgatar Adryan.",.50f,.77f,"A Riberal virou uma fortaleza. Desta vez vamos até o último andar."));
            Add("16_Retorno","De volta ao começo","ATO 4","home",true,new Vector2(.20f,.53f),
                Fight("Atravesse o perímetro da agência.",.40f,.52f,E,G,E,H),
                Picture("Registre os homens protegendo Paulo.",.63f,.45f,"A fachada acabou. A segurança da agência está toda armada."),
                Fight("Abra a entrada lateral.",.75f,.52f,E,G,H,E,G),
                Exit("Invada a Riberal.",.88f,.54f,"Adryan, aguenta mais um pouco."));
            Add("17_RiberalTerreo","A fachada cai","ATO 4","office",true,new Vector2(.49f,.78f),
                Fight("Retome a recepção.",.47f,.65f,E,G,H,E),
                Picture("Ofusque o sensor no corredor.",.64f,.46f,"Sensor cego. Temos uma abertura para avançar."),
                Fight("Atravesse a produção sob fogo.",.48f,.48f,E,E,G,H,G),
                Inspect("Recolha a ordem da diretoria.",.82f,.32f,"Paulo trancou Adryan na sala-cofre. O acesso é pela diretoria."),
                Fight("Garanta o corredor para subir.",.60f,.29f,E,G,E,H,E),
                Exit("Avance para o setor de produção.",.88f,.68f,"Cristyan segue pelas sombras. Ribeiro cobre nossas costas."));
            Add("18_RiberalProducao","O ponto cego","ATO 4","office",true,new Vector2(.86f,.66f),
                Picture("Cegue o primeiro sensor com o flash.",.65f,.44f,"Primeiro sensor fora."),
                Fight("Neutralize a guarda do corredor.",.57f,.50f,E,E,H,G),
                Inspect("Encontre o disjuntor do setor.",.42f,.16f,"Esse circuito alimenta parte do estúdio. Cortar a luz vai impedir o Paulo de enxergar tudo."),
                Fight("Proteja Cristyan durante o avanço.",.37f,.42f,E,H,E,G,E),
                Picture("Cegue o sensor da passagem de serviço.",.22f,.38f,"Corredor liberado. O elevador de serviço ainda funciona."),
                Exit("Procure Henzo no acesso de serviço.",.50f,.80f,"Precisamos revisar o equipamento antes do último andar."));
            Add("19_Servico","A última preparação","ATO 4","workshop",false,new Vector2(.50f,.76f),
                Talk("Henzo",.78f,.46f,"Antes de enfrentar Paulo, precisamos montar o PC na minha bancada no bairro. Recolham os seis tipos de peças espalhados pela cidade; o mapa vai guiar vocês. Depois libero os artefatos para o confronto."),
                Inspect("Confira a planta do estúdio.",.50f,.22f,"Holofotes de alta potência e uma estrutura suspensa. Paulo preparou o próprio estúdio para matar."),
                Talk("Martins",.51f,.29f,"Eu fico pronto para tirar vocês de lá. Resgata o Adryan e segue para a saída de emergência."),
                Exit("Enfrente Paulo no estúdio.",.50f,.78f,"Primeiro, reunir as peças e montar o PC com Henzo. Depois voltamos para o estúdio de Paulo."));
            Add("20_Paulo","Quem controla a luz","ATO 4","office",true,new Vector2(.49f,.78f),
                Fight("Abra o acesso ao estúdio.",.49f,.53f,E,E,H,E),
                Picture("Sobrecarregue o holofote esquerdo.",.12f,.21f,"A luz oscilou. Paulo perdeu um dos ângulos."),
                Picture("Sobrecarregue o holofote direito.",.33f,.21f,"Outra área de sombra. Cristyan está alcançando a energia principal."),
                Fight("Desarme Paulo e derrube a estrutura de luz.",.24f,.25f,EnemyRole.Paulo,E,E),
                Inspect("Confirme a queda do maquinário.",.24f,.24f,"Cristyan cortou a energia. Ribeiro desarmou Paulo. A estrutura de iluminação desabou sobre o chefe."),
                Exit("Abra caminho até a sala-cofre.",.79f,.33f,"Adryan está no cofre da agência. Vamos buscá-lo agora!"));
            Add("21_Adryan","O resgate","ATO 4","office",true,new Vector2(.50f,.72f),
                Fight("Afaste os últimos guardas da diretoria.",.61f,.31f,E,E,H),
                Inspect("Destrave o cofre da diretoria.",.85f,.32f,"A trava cedeu. Amaral: Adryan! Fica atrás de mim. A gente vai sair daqui."),
                Sequence("Resgate Adryan da sala-cofre.",.83f,.32f,SequenceKind.RescueAdryan),
                Exit("Siga para a saída de emergência.",.50f,.80f,"O curto está se espalhando. Não parem!"));
            Add("22_Fuga","A Riberal em chamas","ATO 4","office",true,new Vector2(.83f,.31f),
                Reach("Saia da diretoria com Adryan.",.62f,.30f),
                Fight("Afaste os capangas que bloqueiam a fuga.",.49f,.50f,E,H,E),
                Reach("Atravesse o corredor longe das chamas.",.63f,.64f),
                Exit("Deixe a Riberal pela emergência.",.50f,.85f,"Martins está esperando. Estamos saindo daqui juntos."));
            Add("23_Amanhecer","Pão de queijo e café","EPÍLOGO","home",false,new Vector2(.50f,.53f),
                Inspect("Confira as provas salvas na câmera.",.40f,.57f,"As fotos registram os pagamentos, as rotas e o flagrante. A imprensa local já está recebendo tudo."),
                Inspect("Encontre o grupo perto da padaria.",.30f,.86f,"Adryan está seguro. Ribeiro e Cristyan fazem curativos; Martins enfim desliga a Ninja."),
                Exit("Entre na padaria e encerre esta noite.",.20f,.86f,"Um café quente, pão de queijo e a primeira manhã em paz depois de tudo isso."));
            foreach(var stage in list)
            {
                if(stage.map=="office")stage.tint=new Color(.76f,.80f,.96f,1);
                if(stage.scene=="22_Fuga")stage.tint=new Color(1,.69f,.47f,1);
            }
            var park=new StageDef{scene="24_Mocambo",title="Mocambo • Jogo sujo",act="MISSÃO PARALELA / ATO 2",map="mocambo",next="07_Distribuidora",spawn=new Vector2(.66f,.89f),allies=true,steps=new List<StepDef>{
                Inspect("Investigue a entrega no quiosque.",.30f,.34f,"Henzo: O Cenoura usou um torneio clandestino para esconder os registros. O Pivô guarda a chave da carga. Não envolvam os frequentadores.","Henzo"),
                Picture("Fotografe as anotações junto ao lago.",.36f,.51f,"As apostas pagavam as entregas da Riberal. O recibo cita a mesma distribuidora."),
                Fight("Afaste os cobradores na trilha.",.41f,.38f,B,B,E),
                Reach("Entre na quadra de basquete.",.65f,.65f),
                Fight("Derrote o Pivô, dono da quadra.",.67f,.44f,EnemyRole.Pivo),
                Inspect("Recupere o comprovante do torneio.",.69f,.30f,"Pivô: Eu só guardava os recibos... O Cenoura levou tudo para a distribuidora. Amaral: O dinheiro recuperado vai para equipamento com o Henzo."),
                Exit("Saia do Mocambo e siga a investigação.",.66f,.89f,"Temos uma nova prova contra o Paulo. Próxima parada: distribuidora.")}};
            list.Insert(4,park);
            // The authored exit also stores its destination through StageDef.next.
            return list;
        }
    }
}
