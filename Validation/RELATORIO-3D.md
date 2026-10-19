# Teste curto 3D — 18/10/2026

Build: Builds/Windows3D/PontoCego3D.exe (separada da demo 2D).

PlayMode: 6 aprovados, 0 falhas em District3D-Tests.xml.

Cobertura: 36 pedestres posicionados sobre o NavMesh e realmente caminhando; malhas com skinning e articulação dos ossos ao longo de um ciclo de locomoção; diagonais sem ganho de velocidade e movimento relativo à câmera; golpes com alcance limitado e bloqueados por paredes; proteção temporária; civis fugindo de confrontos; câmera evitando paredes atrás do personagem.

Build Windows: Logs/district-3d-release.log, marcador PONTO_CEGO_3D_WINDOWS_OK.

O modo de captura prepara posições no player renderizado e salva imagens em Validation/District3D. Não é uma partida humana completa.

Para experimentar o combate sem seguir os objetivos: Esc > TESTAR COMBATE • IR AO PÁTIO.

Esta é uma versão curta de teste. O personagem usa um modelo humano provisório adaptado com roupas, cabelo, barba, óculos e câmera. A campanha 2D não foi convertida integralmente e não há duração de cinco horas nesta versão. Veículos estão estacionados e não são dirigíveis. Gamepad foi exercitado virtualmente nos testes; Android não foi compilado.

Fontes e licenças dos modelos/animações estão em Assets/PontoCego3D/README.md e nos arquivos LICENSE em Art. Unity 6000.6.6f1, Input System 1.20.0 e Cinemachine 6.6.0.
