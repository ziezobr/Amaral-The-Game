# Validação — Ponto Cego 2D / profundidade e chefes (18/10/2026)

## Resultado

- PlayMode: 26 aprovados, 0 falhas em Bosses2D-Tests.xml.
- EditMode: 7 aprovados, 0 falhas em Bosses2D-EditMode.xml.
- Os testes de gameplay incluem o fluxo da campanha até o epílogo, colisões, mira independente, fôlego, interações e fotografias, projéteis, cura e morte.
- Novas verificações: avisos de ataque aguardam a preparação e respeitam diálogo/pausa; ataques em área respeitam paredes; encontros de demonstração restauram a etapa correta sem gravar a campanha; chefes mudam de fase; flash interrompe preparação e remove os avisos; flashes repetidos não mantêm o chefe atordoado; terceiro golpe causa mais dano; chefe derrotado durante recuperação termina sua animação de morte; pose de ataque acompanha a mira e retorna à escala normal.

## Implementado

A base 2D foi mantida com os cenários pintados e personagens Chibi. Sombras projetadas acompanham as poses, com sombra de contato e poeira de movimento. Amaral tem 32 poses de combate, direção fixada no início do ataque e janela de impacto de 0,12 s. O terceiro golpe encadeado é mais forte. Os chefes têm poses de preparação, execução e recuperação próprias, baseadas nas imagens originais.

Atiradores mantêm distância e se deslocam lateralmente; elite dispara em leque. Golpes curtos repetidos não paralisam elite, pesados e chefes indefinidamente. Cenoura usa escudo, investida e impacto em área, com 900 de vida. Paulo usa áreas de holofote e sobrecarga de projéteis, com 1200. Ambos mudam de fase abaixo de metade da vida. Depois de um flash eficaz, há resistência temporária de 2,4 s; atordoamento de chefes é limitado a 0,6 s. Os poderes especiais podem ser interrompidos por um flash disponível, e têm recuperação que permite contra-atacar.

O menu oferece testes diretos de Cenoura e Paulo usando o slot separado de demo. A build Windows seleciona somente cenas da pasta Urban/Scenes, preservando a experiência 2D. A experiência experimental 3D continua separada.

## Arte e limites

Arte nova: Assets/PontoCego/UrbanArt/amaral-combat-v3.png e Campaign/boss-combat-v3.png. Criadas com image_gen integrada, com referências e prompts nos arquivos COMBAT-V3-PROMPT.md e Campaign/BOSS-COMBAT-V3-PROMPT.md. Originais preservados.

Os testes automatizados verificam regras e progressão, não substituem uma sessão humana de equilíbrio. A versão continua uma demo/protótipo; não há comprovação de campanha de cinco horas. Gamepad físico e Android ainda precisam de validação em aparelho.
