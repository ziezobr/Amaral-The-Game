# Bairro, Mocambo e motorista de Uber — 18/10/2026

## Jogar

Abra `Builds/Windows/PontoCego.exe`. No menu:

- **Bairro aberto / Explorar**: demo com ruas, pedestres, três carros dirigíveis, Henzo e trabalhos.
- **Mocambo / Jogo sujo**: inicia a nova fase do parque.
- **Nova campanha**: começa pelo bairro e permite entrar na missão principal.

O motorista com a identificação **UBER / DESTINOS** abre a seleção com E. Também é possível usar **Esc → Chamar Uber / Escolher fase**. O seletor oferece as 24 fases e o bairro. Viajar mantém dinheiro, munição e melhorias; cada destino começa no primeiro objetivo. O acesso livre é intencional nesta versão de testes.

## Mocambo

Cenário original ilustrado, interpretação ficcional do Parque do Mocambo: lago bloqueado, quiosque, trilhas, bancos e quadra com duas cestas. A placa **PARQUE DO MOCAMBO** é um objeto com texto legível junto à entrada. Sete objetivos conectam o torneio clandestino aos recibos da Riberal.

Pivô é um chefe original com quatro poses de combate, barra de vida, arremesso triplo de bolas com um rebote, ataque de área anunciado e segunda fase abaixo de metade da vida. Há preparação e recuperação para permitir esquiva e contra-ataque. Não foi usado o nome ou a imagem de um atleta real.

## Bairro e economia

- Novo cenário de quarteirões com 20 pedestres e quatro modelos de moradores, com ciclos de caminhada.
- Três carros dirigíveis: E na porta, WASD/analógico para dirigir, Shift para acelerar, E para sair em um ponto livre de colisão.
- Entrega oficina/padaria (R$120), foto da praça (R$100), câmera perdida (R$150), Mocambo (R$250 ao concluir pelo bairro). Recompensas únicas por partida.
- Loja física de Henzo: kits, colete, capacitor, suprimentos dos aliados, munição, cargas de atordoamento e pistola/melhorias.
- Munição finita de pistola e calibre .38; carregador, reserva e recarga. Cargas de atordoamento finitas. Fotografias de provas continuam possíveis sem cargas.
- Botão direito/RB: pistola após comprá-la. R/LB: recarga. Botão esquerdo/RT: fotografia/flash. F: golpe. Botões de tiro e recarga também no painel de toque.
- Concluir uma missão principal volta ao bairro e libera o próximo objetivo. Mocambo é um desvio opcional e preserva o próximo destino principal.

## Verificação

`Bairro-Final-Tests.xml`: 32 testes PlayMode aprovados, abrangendo campanha, colisões, navegação, chefes, economia, carros e Uber. O teste de viagem confere todos os destinos instalados e a manutenção de equipamentos.

`Bairro-EditMode.xml`: sete testes EditMode aprovados. `BairroMocamboFinal/` contém capturas do executável real: placa, chefe, bairro, carro, loja e seletor do Uber. A placa foi ajustada pelo tamanho real da malha de texto; o HUD fica oculto durante a escolha de destinos.

Esta versão continua sendo uma demo em desenvolvimento. A duração de cinco horas não foi medida ou certificada. Veículos são dirigíveis pelo jogador; tráfego autônomo não foi implementado. Os cenários antigos da campanha permanecem, além dos dois mapas novos desta revisão.
