# Demo 0.24.0 — dificuldade, equipamentos, missões e transporte

Escolha História, Normal ou Difícil no menu antes de Novo Jogo ou Escolher Fase. A escolha permanece no save; Normal preserva o balanceamento anterior. Inimigos mudam vida, dano recebido pelo grupo, velocidade e intervalo de ataque.

Todas as ativações de missões do mundo têm uma introdução de 4,5 segundos no cenário, com câmera mais aberta, título, objetivo e opção E para pular. A introdução trava os controles sem mudar a cena ou posição do personagem. Continuar uma etapa já avançada não repete a introdução.

Celular > Mensagens oferece Retratos do Mocambo (três fotografias com enquadramento), Encomenda de bairro (retirada e entrega) e Lacre recuperado (dois cobradores e recuperação). Os trabalhos têm etapas e pagamento único salvos. As três sequências foram exercitadas até a recompensa.

T > Mapa > Ampliar permite clicar para marcar um waypoint. O marcador rosa aparece no mapa e minimapa. Pode marcar a rota a pé ou selecionar Uber até o waypoint. O carro deixa o jogador na rua acessível próxima do local; interiores sem rua não viram destinos por teleporte.

O app Uber agrupa destinos por local, usa o destino da história quando está nesse distrito e mostra preço e aviso atual. O trânsito tem cessão de passagem com deslocamento físico; a viagem continua, sem pular o caminho.

Carros do trânsito têm porta interativa (E), podem ser dirigidos com WASD/analógico e Shift. E sai por uma posição livre. O motorista automático é desativado ao assumir esse carro, que permanece estacionado ao sair.

Scroll alterna câmera preta, câmera rosa e mãos. A câmera rosa só aparece após desbloquear com Mateus. Clique esquerdo fotografa com a câmera selecionada ou inicia combo com as mãos. F continua como atalho de golpe. A rosa tem 24 sprites do próprio Amaral segurando-a, com oito direções e três poses, recortados por corpos inteiros. O objeto avulso é ocultado para não duplicar a câmera. Apenas a câmera rosa equipada aplica o efeito de 10% da vida máxima por flash energizado.

O menu possui atualizador GitHub para releases futuras, com download do ZIP portátil, SHA-256, extração validada contra caminhos externos e auxiliar nativo de cópia/reinício. Não altera política de execução do Windows. Guarda arquivos anteriores para recuperação. Os saves Unity ficam fora da pasta do jogo. Pode adiar ou desativar o automático. Exige escrita na instalação e conexão com o GitHub.

## Evidências
Features025-EditMode.xml: 70 aprovados.
Features025-PlayMode.xml: 22 aprovados.
Features025-Contracts.xml: entrega e recuperação aprovadas.
Features025-UberTrip.xml: teste adicional da corrida física com carro contrário.
Features025Native: imagens 00, 17, 19, 20, 21 mostram dificuldade, câmera rosa, introdução, mapa e Uber.
Logs/features025-native.log: POLISH021_NATIVE_REVIEW_OK.
Validation/UpdaterHelperFixture contém a cópia nova e o backup antigo do teste do auxiliar (exit 0).

## Origem dos sprites
Ferramenta image_gen integrada, modo de referência, sem CLI. Fonte copiada para Assets/PontoCego/UrbanArt/References/amaral-held-pink-v1.png; derivação em Urban/Equipment025.asset.
Referências: amaral-fluid-walk-v1.png (identidade) e xuxa.png (câmera).
Prompt: sheet transparente 8 colunas por 3 linhas, mesmo Amaral chibi, camisa cinza, calça bege, mochila preta, segurando pequena câmera rosa com as duas mãos; direções frente, baixo-esquerda, esquerda, cima-esquerda, costas, cima-direita, direita, baixo-direita; parado e duas passadas; sem textos, sombras de chão ou fragmentos.
A extração usa componentes completos e não cortes em células rígidas; pivô dos pés e altura são normalizados.
