# Demo 0.20.1 — arte, apoios e cidade mais densa

Revisão de 19/10/2026. Executável: `D:/The Amaral Game/V 1.0/Builds/Demo0201/PontoCego.exe`.

- 35 lotes adicionais: 182 ao todo, incluindo nove do centro. Ruas e calçadas mantêm suas faixas de passagem. 44 moradores e 16 veículos.
- Fachadas novas da Agência Riberal e Oficina do Henzo; quiosque, cestas completas, portal, sucatas e guarita redesenhados e recortados por alfa. Os interiores e os objetivos da história foram preservados.
- Bancos com pés/encosto, postes visíveis, documentos sobre mesas, caixas sobre pallets e sombras presas aos pés. Placa antiga duplicada do parque retirada e orientações recebem suporte.
- Mateus com quatro poses HD, identidade do desenho original, pivô nos sapatos e alvo físico fixo. Clareira reservada fora das avenidas, com moitas e trilha acessível. GPS e recompensas usam coordenadas centralizadas; serviços e reação a ataques foram preservados.

## Validação

25 casos específicos foram verificados nesta revisão. A rodada inicial aprovou 23; dois testes usavam Transform.Find com nomes que continham barras, interpretadas como separadores de caminho. A busca semântica correta confirmou ambos na reexecução. Nenhuma asserção de acesso, tamanho ou apoio foi removida. A história chegou novamente ao epílogo.

World0201-Consolidated.json reúne 100 casos distintos com resultado aprovado mais recente: 25 foram reexecutados após esta revisão; os casos não alterados conservam evidência da 0.20.0. Capturas nativas em World0201FinalVisual verificam a aparência final. A última limpeza de placas é visual, sem novos obstáculos.

## Arte

Ferramenta integrada ImageGen; prompts completos e originais em [ART-V2-PROMPTS.json](<D:/The Amaral Game/V 1.0/Assets/PontoCego/UrbanArt/Landscape/ART-V2-PROMPTS.json>).

Arquivos usados:

- `Assets/PontoCego/UrbanArt/Landscape/environment-v2.png`
- `Assets/PontoCego/UrbanArt/References/mateus-clean-v2.png`
- `Assets/PontoCego/Resources/workshop-henzo-v2.png`

Os originais foram preservados. O projeto usa os novos arquivos copiados localmente, com recortes nativos Unity.

## Limites

Continua inteiramente 2D. Efeitos de luz e clima são estilizados, sem PBR/volumetria física. Não foi comprovada uma campanha de cinco horas. O benchmark World0200-Benchmark.json pertence à versão anterior; não representa uma garantia de desempenho desta revisão mais densa. A 0.20.0 validada fica preservada em Builds/Demo0200.

## Conferência final do executável

Build de release concluído: Logs/world0201-clean-release.log, marcador PONTO_CEGO_WINDOWS_BUILD_OK. Captura executada com saída 0, marcadores PONTO_GROUNDED_SIGNS_V2 e PONTO_ART_REVIEW_OK em Logs/world0201-final-visual.log. Sete imagens em World0201FinalVisual. A duplicação da placa e os rótulos sem suporte foram conferidos após a correção. Atalhos da pasta principal apontam para Demo0201; a Demo0200 anterior permanece disponível.
