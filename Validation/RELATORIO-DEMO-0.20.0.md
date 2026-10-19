# Demo 0.20.0 — reconstrução do mundo 2D

Entrega local de 19/10/2026. Executável: `D:/The Amaral Game/V 1.0/Builds/Demo0200/PontoCego.exe`.

## Mundo e integração

- Mundo contínuo de 330 × 210 unidades, cerca de 51% mais área que o anterior.
- Rede canônica de 55 trechos, com interseções divididas, segmentos colineares unificados, calçadas fora das pistas e faixas de pedestres.
- 147 lotes ao longo dos caminhos e no centro; casas, comércio, mobiliário e vegetação. 16 veículos e 44 moradores nas ruas.
- Mocambo, ferro-velho e rodovia reconstruídos com solo, objetos e colisões separados. Quadra, lago, quiosque, sucata, guarita e defensas têm posições próprias.
- Cinco fachadas revelam os interiores por aproximação. Seis pontos de exploração: ruínas, córrego, abrigo, horta, manutenção e mirante.
- Entrada de missão preserva a mesma cena, jogador, câmera e posição. A carga inicial do mundo ainda existe.
- Pedestres usam trajetos verificados; trânsito freia diante de personagens. Uber mantém chegada, embarque, percurso e desembarque físicos.

## Ambiente, interface e áudio

Relógio global de oito minutos por dia, clima progressivo, luar, postes, sombras direcionais, vento, névoa nas baixadas, vagalumes e poças que enchem e secam. Pool limitado: 112 gotas, seis nuvens, seis faixas de névoa, cinco fachos, 14 vagalumes e até 32 poças.

HUD discreto, minimapa local, relógio/clima, menus que adaptam a paleta ao horário, seis pastas de pausa, texto 90–130%, paletas acessíveis e redução de movimento. A moldura do Uber não encobre mais Início/relógio. Personagens e telas fornecidos pelo usuário continuam usados.

Ambiência mistura dia, noite e chuva; fontes próximas usam áudio espacial adequado ao plano 2D. Cliques e notificações têm sons próprios. Músicas fornecidas pelo usuário foram preservadas.

Culling usa cache de sprites e distância/frustum, sem desativar física ou missões. Trânsito reutiliza buffer de detecção e referência de entrada, evitando buscas e arrays a cada tick.

## Validação automatizada

94 casos distintos possuem resultado final aprovado: 24 EditMode e 70 PlayMode. Resultados consolidados em `World0200-Consolidated.json`.

| Execução | Resultado |
| --- | --- |
| World0200-Final-EditMode.xml | 24/24 |
| World0200-Final-PlayMode.xml | 68/70 |
| World0200-Final-Retry.xml | 2/2, repetição dos dois casos abaixo |

A rodada completa encontrou duas falhas na infraestrutura dos testes, corrigidas e verificadas novamente sem alteração posterior do código de runtime: a auditoria de 25 cenas de origem excedeu o limite durante carregamentos lentos; o teste de transição mantinha o comando de andar e comparava a posição após um novo frame. O primeiro recebeu orçamento explícito para carga de cenas; o segundo verifica a posição no instante da transição. As asserções de acesso e identidade foram mantidas.

O teste de estados da história percorreu os 23 capítulos até o epílogo em 17,2 segundos de execução acelerada. A aceleração de esperas agora mantém a frequência física real, sem multiplicar o trabalho por seis. Isso verifica estados e objetivos, não a duração ou dificuldade humana da campanha.

Também aprovados: coleta pela tecla de interação, seis tipos de peças acessíveis, progressão de Mateus, Uber com percurso completo, frenagem antes de pedestre e retomada, tempestade visível, entradas conectadas, identidade da câmera/jogador, colisões e configurações de acessibilidade.

## Evidências da compilação

Unity 6000.6.6f1, Input System 1.20.0 e Cinemachine 6.6.0, conforme projeto instalado. Renderizador Built-in. Build Windows de release, sem Development Build. Geração completa da geometria: `Logs/world0200-release-candidate.log`; compilação final: `Logs/world0200-final-release.log` e marcador `PONTO_CEGO_WINDOWS_BUILD_OK`.

Arte adicional e prompts estão em [PROVENANCE.md](<D:/The Amaral Game/V 1.0/Assets/PontoCego/UrbanArt/Landscape/PROVENANCE.md>). Originais preservados, integração por sprites e materiais Unity.

## Limites e continuidade

Os efeitos são estilizados em 2D. Não há PBR físico, volumetria 3D, reflexo real do cenário nem occlusion bake tridimensional. Não foi medida uma campanha de cinco horas. Testes automatizados e capturas não garantem ausência de todo bug ou equilíbrio final.

A auditoria dos spawns das áreas reconstruídas não encontrou personagens dentro dos novos sólidos. Uma pilha do ferro-velho tem folga pequena junto a um atirador; futuras mudanças de layout devem incluir proteção explícita dos spawns, além dos objetivos e coletáveis já protegidos.

