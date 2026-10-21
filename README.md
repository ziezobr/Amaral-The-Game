# Amaral The Game — Ponto Cego

Demo 2D para Windows, ambientada em Patos de Minas e desenvolvida na Unity 6.

## Baixar e jogar

- [Instalador Windows 64 bits — versão 0.25.0](https://github.com/ziezobr/Amaral-The-Game/releases/download/v0.25.0/Amaral-The-Game-0.25.0-Windows-Setup.exe)
- [Todas as opções de download e notas da versão](https://github.com/ziezobr/Amaral-The-Game/releases/tag/v0.25.0)

Execute o instalador e abra **Amaral The Game** no menu Iniciar. A edição portátil também está disponível: extraia o ZIP inteiro e execute `PontoCego.exe`. Não é necessário instalar a Unity para jogar. O instalador não possui certificado de assinatura de código.

## Controles

WASD/setas: mover · mouse: mirar · Shift: correr · Espaço: esquivar · scroll: câmera preta/rosa/mãos · clique esquerdo: equipamento selecionado · clique direito: tiro · R: recarga · F: golpe · E: interagir · T: celular · Esc: pausa. Consulte os atalhos do menu e as configurações para remapeamento e gamepad.

## Atualizações pelo jogo

A partir de 0.24.0, o menu consulta as releases deste repositório, baixa o pacote portátil, confere SHA-256 e oferece o reinício automático. É possível adiar ou desativar a atualização automática. Os saves ficam fora da instalação e são preservados. Atualizações exigem conexão com o GitHub e escrita na pasta do jogo.

## Demo 0.25.0

Arte 2D padronizada: 680 sprites, ciclos com 24 poses interpoladas por direção, NPCs e veículos refeitos. O trânsito segue faixas à direita em nove circuitos das ruas da cidade. As três seleções de Amaral têm corpo próprio, com a câmera alinhada à mira.


Escolha História, Normal ou Difícil antes de uma partida. O jogo ajusta vida, dano, velocidade e cadência dos inimigos. Cada missão apresenta uma introdução breve no cenário, que pode ser pulada com E.

T > Mapa permite ampliar, marcar um WAYPOINT e solicitar o Uber até a rua mais próxima do ponto. T > Mensagens inclui três trabalhos de bairro. Carros de rua podem ser dirigidos pela porta (E). Scroll alterna as câmeras e as mãos; a câmera rosa desbloqueada tem poses próprias do Amaral segurando-a.


Cidade conectada em 2D, missões e combate, loja de Henzo, transporte com Higor, ciclo de dia/noite e clima. Fachadas da Riberal e da oficina, parque Mocambo, ferro-velho e esconderijo de Mateus foram revisados. A campanha está em desenvolvimento; esta publicação é uma demo, sem promessa de cinco horas ou ausência de bugs.

## Abrir o projeto

Use **Unity 6000.6.6f1**, com suporte a Windows Build (Mono). Os pacotes são fixados em `Packages/manifest.json` e `Packages/packages-lock.json`. Abra `Assets/PontoCego/Urban/Scenes/00_Menu.unity`. O projeto completo também está no arquivo `Amaral-The-Game-0.25.0-Unity-Source.zip` da release.

Para compilar e gerar o instalador, consulte [Packaging/README.md](Packaging/README.md). A pasta `Library` é recriada pela Unity e não faz parte da distribuição.

## Validação e créditos

Compilação Windows em modo Release verificada. As mudanças e relatórios estão em [Validation/NOVIDADES-0.25.0.md](Validation/NOVIDADES-0.25.0.md): 75 testes de edição e 25 de execução distintos aprovados, além da revisão nativa em 1080p. Os 24 estados por direção usam animação 2D sobre desenhos-chave; não são 192 desenhos novos de cada ataque.

Sprites, referências e músicas foram fornecidos para o projeto; a procedência de áudio e arte consta em `Audio-PROVENANCE.md` e nos documentos `PROVENANCE.md` dos assets. Esta publicação não concede licença de reutilização de materiais de terceiros.
