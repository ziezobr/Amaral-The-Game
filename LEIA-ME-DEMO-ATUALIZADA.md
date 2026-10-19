# Ponto Cego — demo 2D 0.20.1

A revisão 0.20.1 acrescenta 35 lotes, refaz fachadas e elementos da Riberal, oficina, Mocambo e ferro-velho, apoia móveis/placas no chão e melhora o Mateus, agora numa clareira com acesso por trilha.

Esta demo mantém os personagens e a direção de arte 2D do projeto. A cidade foi ampliada e reconstruída para ligar as missões por ruas, calçadas e trilhas percorridas pelo jogador.

## Como jogar

O executável desta versão é `D:\The Amaral Game\V 1.0\Builds\Demo0201\PontoCego.exe`.

Os atalhos da pasta `D:\The Amaral Game` abrem esta versão:

- `JOGAR DEMO ATUALIZADA.cmd`
- `Jogar Ponto Cego 2D.cmd`

Confira **0.20.1** no rodapé do menu. A pasta `Builds\DemoAtualizada` preserva a demo 0.18.4 como cópia de segurança.

## Menu principal

- **Continuar:** retoma o checkpoint da campanha.
- **Novo jogo:** começa a campanha novamente.
- **Escolher fase:** inicia uma partida de teste no capítulo escolhido, usando um slot separado da campanha.
- **Acessibilidade:** permite ajustar texto, cores e movimento da interface antes de começar.

## Controles

| Ação | Teclado e mouse |
| --- | --- |
| Andar | WASD ou setas |
| Correr | Shift |
| Esquivar | Espaço |
| Mirar | Mouse |
| Fotografar / flash | Botão esquerdo do mouse |
| Combate corporal / corrente de Martins na perseguição | F |
| Disparar arma secundária | Botão direito do mouse |
| Recarregar | R |
| Interagir, recolher, avançar diálogo, entrar ou sair de veículo | E |
| Abrir ou fechar o celular | T |
| Usar kit de vida | H |
| Usar artefato da placa-mãe, após montar o PC | G |
| Pausa | Esc |

No controle, o analógico esquerdo movimenta e o direito mira. Os comandos podem ser consultados e reconfigurados em **Pausa → Controles**. A seta do sistema fica oculta durante a gameplay e reaparece nas interfaces.

Correr consome somente fôlego. A vida diminui ao receber dano. O fôlego aparece durante o uso e a recuperação; o HUD fica mais discreto em exploração segura e recupera a visibilidade em situações de perigo.

## Cidade e caminhos entre missões

A área do mundo passou para **330 × 210 unidades**, aproximadamente **51% maior** que a anterior. A composição distribui 182 lotes, incluindo nove da área central, 16 carros e 44 moradores. Cinco fachadas identificam acessos a interiores e seis pontos de interesse acrescentam locais de exploração e narrativa ambiental.

Mocambo, ferro-velho e rodovia foram reconstruídos. Casas, comércio, vegetação, ruínas e trilhas ajudam a ligar as áreas. As missões compartilham a cidade, o personagem e a câmera: não há troca de cena entre as entradas e os objetivos desse mundo contínuo. A abertura inicial do jogo ainda precisa carregar seus recursos.

As caixas de texto junto aos pontos relevantes indicam o objetivo atual. O minimapa acompanha a região e mostra a rota; o aplicativo de mapa do celular permite consultar o caminho e alternar entre a história e os serviços de Mateus. A missão começa ao alcançar sua entrada indicada.

## História, peças de PC e equipamentos

Antes do confronto com Paulo, a progressão orienta a coleta dos seis tipos de peças de PC e a reunião na bancada de Henzo. As peças ficam escondidas em moitas que se tornam transparentes quando Amaral se aproxima. Use **E** perto do item para recolher.

A montagem libera artefatos de apoio: placa-mãe como bomba, memória RAM para Cristyan e melhoria de disparo para Ribeiro. Adryan acompanha o grupo após o resgate.

Mateus está no extremo noroeste. Ele oferece duas sequências de três serviços pela cidade; é necessário retornar a ele para liberar cada arma escondida. A Glock dourada dá munição infinita a Ribeiro e a navalha dourada dobra o dano de Cristyan.

A câmera da Xuxa é obtida com Mateus. O flash causa 10% da vida máxima aos inimigos enquadrados, respeitando alcance e paredes. As barras dos inimigos mostram a vida restante.

Banana restaura toda a vida. Suco de laranja oferece fôlego infinito por dez segundos. Picanha assada protege de dano por cinco segundos. São itens raros de chefe: após cinco impactos na última fase, o confronto libera um dos três, uma vez.

## Celular e Uber

Abra o celular com **T**, entre no aplicativo Uber e escolha o destino. Higor sempre usa um **Peugeot 206 cinza**. A chegada leva dez segundos; aproxime-se da porta para embarcar. A viagem inclui entrada no veículo, percurso pelas ruas e desembarque, sem pular a rota.

O celular também reúne mapa, objetivos, informações de Mateus e contadores de peças, flash, munição, kits e dinheiro. Textos maiores podem ser rolados dentro dos aplicativos. Ao pausar, o aparelho fica oculto e retorna ao mesmo aplicativo quando a partida é retomada.

## Dia, noite e clima

O ciclo completo dura **oito minutos** e mantém a hora sincronizada entre as áreas. O relógio e o estado do clima aparecem junto ao minimapa. Postes e outras fontes de luz ajudam a orientar a exploração noturna.

O clima alterna entre céu limpo, nublado, chuva e tempestade. Nuvens, névoa, raios de luz, vagalumes, vento e poças usam efeitos 2D; a umidade aumenta com a chuva e diminui gradualmente depois. Os efeitos utilizam conjuntos de objetos reutilizados para limitar a criação de novos objetos durante a partida.

A interface adapta suas cores ao horário. A loja mantém os verdes de referência, com uma apresentação mais escura à noite.

## Pausa e acessibilidade

**Esc** abre seis pastas: **Jogo, Controles, Áudio, Vídeo, Interface e Acessibilidade**.

- Áudio separa volume geral, música e efeitos/ambiente.
- Vídeo reúne tela cheia, resolução, sincronização vertical e limite de quadros.
- Interface permite configurar minimapa, objetivos, vida dos inimigos, relógio, HUD discreto e tamanho da mira.
- Acessibilidade oferece texto de 90% a 130%, paletas original/protanopia/deuteranopia/tritanopia e redução das animações da interface.

Use **1–6**, **Tab** ou **LB/RB** para trocar de pasta. Os atalhos de abas ficam suspensos durante a configuração de um comando. As preferências são independentes do progresso da campanha.

## Trilha e ambiente sonoro

A perseguição da Ninja contra o Gol usa **Asphalt Grip**, fornecida pelo usuário. O menu principal usa **Riders on the Storm**, também fornecida pelo usuário. As faixas respeitam o volume de música; a perseguição acompanha a pausa e as transições da sequência.

Os ambientes sonoros acompanham o horário e o clima. Cliques, abertura de menus e notificações usam efeitos próprios de interface.

## Limites desta versão de teste

O projeto continua inteiramente **2D**. Luar, sombras, névoa, reflexos e raios de luz são efeitos estilizados em sprites; não representam iluminação PBR nem volumetria física real.

Esta é uma demo para avaliar mundo, navegação, combate, progressão e interface. Não há comprovação de cinco horas de campanha. As 25 checagens específicas desta revisão passaram, incluindo acesso, coleta, Uber, história e novos sprites. Evidências estão em Validation/World0201-Consolidated.json e Validation/World0201FinalVisual. Nenhuma taxa fixa de FPS ou ausência total de bugs é garantida por este guia.
