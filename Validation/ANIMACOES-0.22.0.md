# Animações — demo 0.22.0

Amaral recebeu cinco poses por direção na caminhada, com cadência diferente na corrida. As direções diagonais usam espelhamento quando necessário para manter a orientação. O novo atlas foi gerado por ImageGen usando as pranchas revisadas do personagem como referência; mantém a identidade, roupa, mochila e câmera. Origem: C:/Users/User/.codex/generated_images/01a14cb5-7d27-7d92-9cce-9dba986965ea/exec-d5adf57f-7051-4973-96a0-111a3be78981.png. Cópia no projeto: Assets/PontoCego/UrbanArt/References/amaral-fluid-walk-v1.png.

A geração contém cinco linhas completas de oito personagens. O editor identifica os componentes transparentes por coluna, valida os corpos inteiros e acrescenta margens transparentes. Não usa uma divisão arbitrária de seis linhas. Banco: Assets/PontoCego/Urban/Fluid023.asset.

Nos personagens existentes, as animações de apoio incluem respiração, transferência de peso, inclinação nas curvas, aceleração/frenagem, impulso de ataque, reação a dano e compressão visual na esquiva. As transições mantêm a hierarquia do Animator e a escala do corpo. O modo de movimento reduzido neutraliza os efeitos adicionais. Colisores e lógica de combate não mudaram.

Validação: 40 testes EditMode e 12 testes PlayMode passaram. Incluem quadros completos com bordas transparentes, cadência independente do número de poses, estabilidade da direção, parada, hierarquia e ausência de acúmulo de escala. Execução local em Builds/Demo0220/PontoCego.exe; instalador em Builds/Distribution0220. Nova versão local, sem nova publicação GitHub nesta tarefa.