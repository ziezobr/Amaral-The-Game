# Áudio ambiente e visibilidade 2D

`WorldAmbience2D` cria sons originais por síntese em memória: pássaros, grilos, coruja distante, vento, chuva, água e pequenos ruídos de postes/fogueira. São efeitos estilizados; não são gravações de campo. Não baixa nem modifica as músicas fornecidas pelo usuário.

- O relógio global controla a mistura dia/noite por crossfade de potência constante.
- Chuva/vento usam `WorldWeather2D`; interiores atenuam o exterior.
- Até oito fontes locais reutilizadas usam espacialização e distância no plano XY da câmera. Nenhum AudioListener é criado.
- A simulação pausada suspende o ambiente. O volume de efeitos e o volume geral permanecem independentes da música.
- `UiSound.PlayClick()`, `PlayOpen()` e `PlayNotify()` são chamadas estáticas. Os sons de interface funcionam durante a pausa, possuem limite contra repetição e respeitam o volume de efeitos.

`WorldVisibility2D` registra renderizadores de cenário uma vez e reavalia sua proximidade a cada 0,2 s. Usa a caixa do sprite inteira mais margem de segurança, sem desativar objetos, colisões, NPCs, gatilhos ou scripts. Objetivos/interações e atores de combate são excluídos. O marcador opcional `WorldStaticVisual2D` permite cadastrar novos detalhes. Esse filtro complementa o culling normal do Unity; não implementa oclusão volumétrica nem garante FPS em qualquer dispositivo.

Benchmark opcional no player compilado:

`PontoCego.exe --ponto-world-benchmark "C:\caminho\resultado.json"`

O relatório mede três pontos de câmera durante tempestade noturna, oito segundos por ponto após aquecimento. Inclui mediana, percentil 95, pior quadro, contagem de sprites, cenário filtrado e identificação do hardware. Não há screenshots durante a amostragem. FPS/vSync configurados são registrados e não alterados. O argumento é reservado ao QA e não deve ser usado para jogar a campanha.
