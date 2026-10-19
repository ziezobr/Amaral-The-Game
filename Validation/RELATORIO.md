# Validação — demo de combate e campanha (18/10/2026)

Unity 6000.6.6f1; Input System 1.20.0; Cinemachine 6.6.0.

- EditMode: 7 aprovados, 0 falhas — Campaign-EditMode.xml.
- PlayMode: 17 aprovados, 0 falhas — Campaign-PlayMode.xml.
- Movimento/parada, normalização das diagonais, sensibilidade analógica, fôlego, recarga e distância da esquiva.
- Rigidbody2D contra móveis e paredes; mira independente com gamepad virtual.
- Fôlego inicialmente oculto, visível durante o uso e oculto novamente após recuperar.
- Dano entre equipes filtrado, morte emitida uma única vez, kits consumidos somente quando necessários e bloqueio ao morrer.
- Golpe do jogador danifica/atordoa; projéteis respeitam paredes.
- Alcance dos objetivos e spawn sem colisões nas fases da campanha. A verificação ignora a cena temporária inserida pelo Unity Test Runner.
- Fluxo automatizado do sequestro em 03_Casa até o epílogo 23_Amanhecer, completando regras de luta, investigação, loja, sequências e transições. O teste resolve os encontros por código; não mede equilíbrio ou tempo de uma partida humana.
- Build Windows concluída: Logs/campaign-release.log contém PONTO_CEGO_WINDOWS_BUILD_OK.
- Treze capturas reais do player Windows em Validation/Campaign. As posições e etapas são preparadas pelo modo de QA; não são mockups nem comprovação de uma partida inteira.
- Corrigidos recortes do atlas para impedir pequenos fragmentos de poses vizinhas. Os arquivos de arte originais são preservados.
- Os quadrinhos não são exibidos pela gameplay. As cenas de sequestro, Martins e Adryan usam atores no cenário.
- Arte/proveniência em Assets/PontoCego/UrbanArt/PROMPTS.md e Campaign/PROMPTS.md, ADDITIONAL-PROMPTS.md.

## Limites desta entrega

Demo/protótipo com 23 fases lineares e ambientes reutilizados, não campanha final com cinco horas comprovadas. A duração mínima solicitada permanece uma meta pendente de expansão de conteúdo e testes humanos. Confrontos finais e encenações ainda são simplificados, com alguns acontecimentos concluídos em diálogo. Equilíbrio, animações de chefes, gamepad físico e Android precisam de validação adicional.

O executável abre um menu com Nova Campanha, Continuar e Demo de Combate. O salvamento da demo é separado da campanha. O modo de captura e os testes não gravam progresso do jogador.
