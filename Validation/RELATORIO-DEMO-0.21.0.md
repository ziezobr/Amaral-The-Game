# Demo 0.21.0 — revisão de personagens, combate e interface

Build Windows local: Builds/Demo0210/PontoCego.exe. Atalho atualizado em D:/The Amaral Game/JOGAR DEMO ATUALIZADA.cmd.

- Seis inimigos comuns: lutador, velocista com investida, flanqueador, blindado vulnerável ao atordoamento, atirador que recua e brutamontes resistente.
- Adryan criança, animações direcionais e movimento visual secundário dos personagens.
- Corrigido redimensionamento NPOT dos atlas novos: coordenadas de recorte agora preservam a dimensão original de 1536x1024.
- Objetivo principal e tarefa de Mateus no HUD. Serviços incluem investigação, fotografia, recuperação com combate, reparo e entrega.
- Colisões de perímetro e portas físicas revisadas; fachadas urbanas se tornam transparentes ao entrar.
- Minimap rasterizado em uma única camada com rota, norte, jogador e objetivo; revisado em 1920x1080 e 1280x720.
- Loja com descrições detalhadas e Monster branco: +20% de capacidade de fôlego por nível, máximo cinco níveis. Vida não é consumida pela corrida.
- Banana (4%), suco (3%) e picanha (1%) raros nos inimigos comuns; chefes excluídos.
- Removidos pássaros e passos de Amaral. Outros efeitos e músicas preservados.

Verificação: 27 testes EditMode, 7 PlayMode de integração e 9 de regressão passaram. Revisão nativa de animação confirmou quatro sprites distintos durante caminhada. Os testes de regressão incluem passagem de porta, bloqueio de parede e interações de Mateus. Não equivale a uma campanha inteira jogada manualmente.

Esta versão é local; a publicação anterior no GitHub permanece 0.20.1.
Últimos ajustes: Novo Jogo inicia com R$ 50 (saldo de saves anteriores preservado). Cemitério com jazigos, muros e portão aberto ao lado sul da casa. Riberal identificada como Rua José de Santana, 351.
