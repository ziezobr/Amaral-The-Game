# Demo local 0.23.0 — missões, integração e combate

## Correções entregues
- Mateus: as seis tarefas agora usam as entradas reais dos distritos, com posição livre de colisões e fora das plantas fechadas dos edifícios. GPS e objeto usam a mesma posição.
- Rotas dos seis serviços verificadas desde a clareira. A navegação dos acompanhantes e cobradores em exploração usa a cidade inteira, em vez da antiga grade central de 64 x 42 unidades.
- O encontro dos cobradores não depende de estar fora de outra missão; a etapa não aceita a coleta sem preparar os dois guardas e derrotá-los. Objetos não aceitam interação remota.
- Enquanto o GPS acompanha Mateus, as entradas não iniciam a história automaticamente. No celular, Mapa > Rota da História retoma a campanha. Selecionar um destino da campanha também troca o GPS para a história.
- Investigação e entrega levam 1,6 s; reparo leva 3 s, cancelando se Amaral se afasta. A foto exige flash, enquadramento e linha de visão. Recompensas e pagamento preservam IDs anteriores do save.
- Acessos pavimentados junto às entradas dos locais, mantendo as construções e colisões existentes. Não foram aproximados bairros por teleporte nem alteradas coordenadas dos interiores.
- Combate: buffer de comandos de 180 ms, combo de três golpes com dano progressivo, recuperação maior no finalizador e cancelamento de golpe pendente por esquiva. Reações só ocorrem quando o dano é aceito.
- Tiro: dispersão progressiva ao disparar e correr, recuperação da precisão, clarão, impacto, pequeno recuo visual e projétil varrido com espessura. O tiro nasce no corpo para não atravessar paredes pela posição do cano.
- Recarga exibe porcentagem e respeita munição disponível. Tiro não é permitido durante esquiva ou animação de golpe.
- Preservadas as cinco poses de caminhada por direção do Amaral, transições e movimentos secundários de 0.22.0. O projeto continua totalmente 2D.

## Verificação
- Fix024-EditMode.xml: 66 testes aprovados.
- Fix024-PlayMode.xml: primeira verificação das duas sequências completas de Mateus, seis rotas, interação remota, combate corpo a corpo e bloqueio de tiros por paredes: 5 testes aprovados.
- Fix024-FinalPlayMode.xml: 19 testes aprovados (85 no conjunto final, contando os 66 de EditMode). Inclui integridade dos sprites, animação reduzida, vida, loja, colisões/portas e ambas as sequências de Mateus.
- Logs/fix024-build-final.log registra o resultado da compilação.
- O teste de fotografia antigo foi atualizado para usar o acesso exterior real; agora verifica também a rejeição de uma foto através de parede.
- Primeira revisão nativa 1080p: oito direções (35 sprites distintos), hierarquia preservada e revisão de HUD concluída. Capturas em Validation/Fix024Native.
- Após a revisão, o piso dos acessos passou a usar a textura canônica das calçadas e as caixas foram afastadas da faixa. Fix024-PlacementFinal.xml valida novamente as duas sequências completas, seis rotas e fotografia.

## Jogar
Builds/Demo0230/PontoCego.exe
Instalador: Builds/Distribution0230/Amaral-The-Game-0.23.0-Windows-Setup.exe
F: golpe/combo; botão direito: tiro; R: recarga; botão esquerdo: flash; espaço: esquiva; E: interação; T: celular.
A publicação do GitHub permanece anterior; esta entrega é local.

Compilação final e instalador concluídos. Revisão nativa final: POLISH021_NATIVE_REVIEW_OK; 35 sprites distintos nas oito direções, sem alteração da hierarquia. Capturas finais em Validation/Fix024NativeFinal. Atalhos da pasta principal atualizados para Demo0230.
