# Amaral The Game — Ponto Cego

Demo 2D para Windows, ambientada em Patos de Minas e desenvolvida na Unity 6.

## Baixar e jogar

- [Instalador Windows 64 bits — versão 0.20.1](https://github.com/ziezobr/Amaral-The-Game/releases/download/v0.20.1/Amaral-The-Game-0.20.1-Windows-Setup.exe)
- [Todas as opções de download e notas da versão](https://github.com/ziezobr/Amaral-The-Game/releases/tag/v0.20.1)

Execute o instalador e abra **Amaral The Game** no menu Iniciar. A edição portátil também está disponível: extraia o ZIP inteiro e execute `PontoCego.exe`. Não é necessário instalar a Unity para jogar. O instalador não possui certificado de assinatura de código.

## Controles

WASD/setas: mover · mouse: mirar · Shift: correr · Espaço: esquivar · clique: câmera/flash · F: golpe · E: interagir · T: celular · Esc: pausa. Consulte os atalhos do menu e as configurações para remapeamento e gamepad.

## Demo 0.20.1

Cidade conectada em 2D, missões e combate, loja de Henzo, transporte com Higor, ciclo de dia/noite e clima. Fachadas da Riberal e da oficina, parque Mocambo, ferro-velho e esconderijo de Mateus foram revisados. A campanha está em desenvolvimento; esta publicação é uma demo, sem promessa de cinco horas ou ausência de bugs.

## Abrir o projeto

Use **Unity 6000.6.6f1**, com suporte a Windows Build (Mono). Os pacotes são fixados em `Packages/manifest.json` e `Packages/packages-lock.json`. Abra `Assets/PontoCego/Urban/Scenes/00_Menu.unity`. O projeto completo também está no arquivo `Amaral-The-Game-0.20.1-Unity-Source.zip` da release.

Para compilar e gerar o instalador, consulte [Packaging/README.md](Packaging/README.md). A pasta `Library` é recriada pela Unity e não faz parte da distribuição.

## Validação e créditos

Compilação Windows em modo Release e instalação local verificadas. Os relatórios da demo estão em `Validation/RELATORIO-DEMO-0.20.1.md`; o conjunto consolidado registra 100 verificações distintas aprovadas, das quais 25 foram repetidas após a revisão visual. Isso não substitui testes extensivos em outros computadores.

Sprites, referências e músicas foram fornecidos para o projeto; a procedência de áudio e arte consta em `Audio-PROVENANCE.md` e nos documentos `PROVENANCE.md` dos assets. Esta publicação não concede licença de reutilização de materiais de terceiros.
