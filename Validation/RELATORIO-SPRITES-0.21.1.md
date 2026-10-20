# Correção de sprites — demo 0.21.1

A correção anterior de NPOT não corrigia recortes de linhas e remoção de fundo dos personagens antigos. Esta revisão substitui os recortes por regiões identificadas em cada linha, com margens próprias para caminhada e espera. A folha de Amaral possui onze desenhos em certas linhas de caminhada, em vez da grade de oito anunciada pela imagem; o recorte agora isola desenhos inteiros e seleciona oito posições.

Preservados bonés e roupas escuras. Fragmentos de linhas, cabeças e sapatos de quadros vizinhos são excluídos. Personagens usam o apoio dos pés como origem; os carros usam o centro. NPCs e veículos foram recortados novamente a partir dos atlas existentes. O movimento secundário não altera mais a hierarquia referenciada pelo Animator. O retorno do soco para a caminhada e a apresentação da esquiva usam os quadros revisados.

A corrida usa as poses direcionais completas de caminhada com cadência acelerada; as poses de corrida inconsistentes das referências foram substituídas por essa sequência estável. Identidades e arte original mantidas. Não foi criada animação esquelética nova.

Banco em uso: Assets/PontoCego/Urban/Polish022h.asset. Pranchas de revisão: Validation/SpriteAudit022. Anteriores bancos são históricos sem referência nas cenas atuais.

36 testes EditMode e 10 PlayMode passaram. Executável: Builds/Demo0211/PontoCego.exe. Instalador: Builds/Distribution0211/Amaral-The-Game-0.21.1-Windows-Setup.exe. A revisão nativa fica em Validation/Sprite022Native e Logs/sprite022-native.log.

Saldo inicial de R$ 50, cemitério e endereço da Riberal preservados. Publicação GitHub anterior não foi alterada por esta revisão local.
Revisão nativa concluída: 25 quadros distintos nas oito direções; corrida, esquiva e ataque capturados em 1080p. Altura visual normalizada também na troca entre caminhada e golpe para evitar crescimento abrupto. Logs: SPRITE022_EIGHT_DIRECTIONS_OK e POLISH021_NATIVE_REVIEW_OK.
