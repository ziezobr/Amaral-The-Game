# Demo de cidade continua

Uma unica cena, `26_PatosWorld`, preserva jogador, camera e estado durante a ativacao local das missoes. Os mapas de autoria continuam no projeto para edicao, mas o fluxo principal caminha entre pontos fisicos da cidade. Objetos de missao substituem marcadores flutuantes. Navegacao usa limites locais e a camera possui um limite global.

HUD: vida vinculada a dano, folego ao movimento, contador de cargas real, saldo e minimapa. Loja com dez produtos, imagens e precos; menu com a arte fornecida. T abre celular e agenda um Uber com espera de sessenta segundos de jogo.

Novas referencias do usuario preservadas em `UrbanArt/References`. Recortes dos carros e personagens sao preparados pelo Editor Unity. Transito possui dez modelos, rotas fechadas, frenagem por obstaculos e Rigidbody2D. NPCs civis usam a prancha fornecida, cuja resolucao original e 895x420.

PC: uma peca persistente por missao, seis categorias, bancada com Henzo e montagem necessaria para iniciar Paulo. Artefatos possuem estoque finito. Higor e motorista e reforco na terceira fase de Kevyn. Paulo escala em cinco faixas de vida.

Validacao final: consultar XML e logs recentes em Validation e Logs. Resultados de capturas anteriores nao comprovam a aparencia desta versao. Projeto 3D legado nao e o executavel desta demo 2D.

## Resultado desta entrega

43/43 testes PlayMode 2D aprovados em `Validation/DemoStable-Tests.xml`. Captura executada com caminhada real pela cidade: `PONTO_WORLD_CAPTURE_OK` em `Logs/demo-stable-capture.log`. Build Windows confirmado em `Logs/demo-stable.log`; ajuste final apenas visual registrado em `Logs/demo-delivery.log`. O projeto 3D legado ficou fora desta suite e da selecao de cenas do executavel. A duracao de cinco horas ainda nao foi homologada.
