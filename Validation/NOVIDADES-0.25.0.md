# Demo 0.25.0 — sprites e trânsito 2D

- 28 atlas gerados com referências do projeto: protagonistas, Adryan criança, Mateus, Higor, Pivô, oito pedestres, seis inimigos, três equipamentos de Amaral e oito modelos de veículos.
- 680 sprites completos, separados por contorno alfa, com margens transparentes e pivôs alinhados. Bancos de arte separados por personagem.
- Ciclos de locomoção com 24 poses de deformação 2D por direção, usando desenhos-chave nas oito direções. São poses interpoladas em tempo de execução, não 192 desenhos independentes de cada ataque. As ações de combate existentes preservam prioridade.
- Corpo de Amaral acompanha a mira com câmera preta, câmera rosa ou mãos; cadência visual acompanha a passada. A câmera rosa fica nas mãos.
- Trânsito derivado das ruas reais do cenário: nove circuitos, faixas à direita, curvas amostradas, frenagem física e 16 carros com partida espaçada.
- Cruzamentos e ramificações sem saída tratados antes da geração de circuitos; desvio para dar passagem ao Uber usa o lado direito. O planejador do Uber favorece a faixa correta.
- Referências de pedestres parados também atualizadas, mantendo oito modelos distintos.

Validação: 75 testes EditMode aprovados, 24 testes PlayMode da regressão completa aprovados e mais um teste distinto de diversidade dos NPCs, espaçamento inicial e mira durante deslocamento lateral. O teste de troca de equipamento foi repetido após o último ajuste visual. Total: 100 testes distintos. Relatórios XML nesta pasta.

Revisão do executável Windows em 1080p: oito direções, HUD, inimigos, Adryan, Mateus, portas, câmera rosa, mãos, introdução de missão, waypoint e Uber. Log `POLISH021_NATIVE_REVIEW_OK`.

Arte e prompts: [ARTE-0.25.0.md](ARTE-0.25.0.md). Fontes finais em `Assets/PontoCego/UrbanArt/References/*-coherent-v3.png`, bancos em `Assets/PontoCego/Urban/Animation026/`.

Demo Windows: `Builds/Demo0250/PontoCego.exe`. Para recompilar as cenas já preparadas, use `Ponto Cego/2 • Build Windows`. Os geradores iniciais de cenário são ferramentas de reconstrução do projeto, não são necessários para recompilar esta demo.
