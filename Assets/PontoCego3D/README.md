# Ponto Cego — teste curto em terceira pessoa

Execute Builds/Windows3D/PontoCego3D.exe. A demo 2D anterior permanece em Builds/Windows.

O bairro é aberto e explorável, com ruas, prédios, calçadas, carros estacionados e 36 pedestres com trajetos aleatórios. Personagens usam malhas 3D com esqueleto Humanoid, skinning e animações retargetadas de caminhada, corrida, socos, reação, rolamento e queda. A câmera orbital usa Cinemachine e detecta obstáculos.

## Testar rapidamente

Pressione Esc e escolha TESTAR COMBATE • IR AO PÁTIO. Você será levado ao encontro com quatro inimigos. Também pode explorar a pé e seguir os objetivos pela Riberal, padaria e oficina.

WASD move em relação à câmera. Mouse gira a câmera. Shift corre. Ctrl esquiva/rola. Espaço salta. Clique dá socos alternados. Botão direito mira; clique enquanto mira ou Q dispara o flash que atordoa. H usa kit. E interage. Esc libera o cursor, pausa e abre controles configuráveis.

Gamepad: analógico esquerdo move, direito gira a câmera, clique do esquerdo corre, B rola, A salta, X golpeia, Y interage, LT mira, RT usa flash, direcional para cima cura e Start pausa. Menus são clicáveis.

## Escopo

Versão curta de teste, com cinco objetivos e um encontro de combate. O personagem 3D é uma adaptação provisória: cabelo, barba, óculos, camiseta escura, calça bege e câmera remetem ao Amaral, mas a modelagem ainda precisa de acabamento para reproduzir a referência fielmente. A campanha 2D completa do protótipo não foi convertida. Não há direção de veículos nesta versão. Carros são elementos do cenário. A cidade é fictícia, inspirada no contexto de Patos de Minas.

## Fontes e licenças

Base humana e cabelos: Quaternius, Universal Base Characters (Standard), CC0. https://quaternius.com/packs/universalbasecharacters.html
Animações: Quaternius, Universal Animation Library (Standard), CC0. https://quaternius.com/packs/universalanimationlibrary.html
Prédios: Kenney, City Kit Commercial, CC0. https://kenney.nl/assets/city-kit-commercial
As licenças originais acompanham Art/Characters e Art/City. Ruas, prédios da narrativa, acessórios, veículos e lógica de jogo foram montados para este protótipo no editor Unity.

## Projeto

Cena: Assets/PontoCego3D/Generated/CentroAberto.unity. Runtime contém entrada, motor, câmera, combate, navegação e interface. Editor/DistrictBuilder.cs gera a cena e a build exclusiva 3D. A navegação usa o módulo AI já instalado; não adiciona dependências externas. Unity 6000.6.6f1, Input System 1.20.0, Cinemachine 6.6.0.

Ponto Cego > 3D • Gerar bairro aberto recria apenas o conteúdo gerado 3D. Faça backup antes de editar manualmente a cena gerada. Ponto Cego > 3D • Build Windows gera o executável separado.
