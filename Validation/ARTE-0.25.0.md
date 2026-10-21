# Arte e animação — 0.25.0

Geração: ferramenta nativa image_gen, com referências originais do usuário; sem conversão para 3D. Arquivos originais gerados preservados. Importação Unity separa componentes alfa completos com margem transparente e pivô nos pés. Carros usam pivô central.

## Escopo das animações

Cada direção tem um ciclo de 24 poses de deformação 2D em tempo de execução, combinado com três desenhos-chave. São 192 estados por ciclo nas oito direções. Não são 192 desenhos novos independentes por personagem. A malha mantém UVs fixas, sem recortar a textura. Ataques existentes mantêm prioridade; essa atualização não redesenha 192 imagens de cada ataque.

## Conjunto de prompts

Atlas de personagem: 8 colunas × 3 linhas; direções frente/baixo, baixo-esquerda, esquerda, cima-esquerda, costas/cima, cima-direita, direita, baixo-direita. Linhas: pé esquerdo à frente, passagem neutra, pé direito à frente. Corpo completo, proporções chibi coerentes, traço escuro limpo, pintura cartoon 2D, pés alinhados, fundo alfa transparente, sem texto/cenário/sombra. Preservar identidade e roupa da referência individual e padronizar acabamento pela referência de Higor. Variantes de Amaral: câmera preta; mãos vazias; câmera rosa pequena segura com ambas as mãos. Adryan é criança e usa escala menor.

Correções de espaçamento: preservar exatamente identidade, roupas, direções e poses; reduzir figuras para 70–75% da célula, deixar pelo menos 40 pixels de intervalo entre linhas e colunas e margem externa transparente. Aplicadas a citizen1, citizen2, citizen4 e enemy2 após detecção de contornos unidos.

Pedestres:
- citizen0: navy Brazilian police uniform, navy cap, sunglasses
- citizen1: adult man in orange cap, white T-shirt, brown shorts
- citizen2: blonde adult woman in pink top and blue jeans
- citizen3: male courier in red uniform and red cap, small parcel backpack
- citizen4: adult man in dark cap, green hoodie and blue jeans
- citizen5: elderly man with gray hair, glasses, beige shirt and brown trousers
- citizen6: young adult male student, dark hair and glasses, white T-shirt, black trousers and backpack
- citizen7: adult woman with long dark hair, rust-red top and blue jeans

Inimigos:
- enemy0: street fighter, tan skin, black hoodie, red sneakers, raised fists
- enemy1: athletic sprinter, dark skin, red sports jacket, black track pants, white sneakers
- enemy2: flanker, tan skin, gray hoodie, black beanie, blue jeans
- enemy3: armored guard, black helmet and charcoal protective vest, dark green trousers
- enemy4: marksman, dark blue cap, blue tactical jacket, black trousers, carrying small pistol lowered safely
- enemy5: large muscular enforcer, shaved head, dark beard, sleeveless brown leather vest, black trousers

Veículos: quatro colunas (frente, esquerda, traseira, direita), oito linhas: Uno branco, Strada branca, Onix azul, Celta preto, Fox bege, Gol G5 prata, Palio vermelho, Peugeot 206 cinza. Corpos completos, rodas e para-choques sem cortes, escala consistente, pintura 2D com contorno escuro e fundo alfa transparente.

## Arquivos

- amaral: Assets/PontoCego/UrbanArt/References/amaral-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-c0792a5f-9092-4f50-b46c-5f88b4a6fdb4.png
- ribeiro: Assets/PontoCego/UrbanArt/References/ribeiro-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-f5380e59-a374-4bf6-9e89-405500db6f5e.png
- cristyan: Assets/PontoCego/UrbanArt/References/cristyan-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-952abac9-8065-4e30-9ec7-696758044b54.png
- henzo: Assets/PontoCego/UrbanArt/References/henzo-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-4729b19f-0ae2-4dae-ae46-8213f6d43873.png
- martins: Assets/PontoCego/UrbanArt/References/martins-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-627c8484-cde1-4bfd-a440-93a32d0b15b1.png
- kevyn: Assets/PontoCego/UrbanArt/References/kevyn-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-404fab90-854a-4420-b4bf-fe9faccb0ed5.png
- paulo: Assets/PontoCego/UrbanArt/References/paulo-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-9d870179-a316-449c-80c1-ae9308079008.png
- mateus: Assets/PontoCego/UrbanArt/References/mateus-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-21ef70ee-ff96-48ee-93be-2d282a43edd9.png
- adryan: Assets/PontoCego/UrbanArt/References/adryan-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-36bb8dce-771a-4342-89b2-aff43bfe3307.png
- citizen0: Assets/PontoCego/UrbanArt/References/citizen0-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-9e41787b-27bb-40c8-870b-2bf2bb3bd811.png
- citizen3: Assets/PontoCego/UrbanArt/References/citizen3-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-78718f5b-850c-4da5-b426-dc84f1edc8bd.png
- citizen5: Assets/PontoCego/UrbanArt/References/citizen5-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-6281029d-5053-4a1d-b424-7c6ff24b3080.png
- citizen6: Assets/PontoCego/UrbanArt/References/citizen6-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-539f9ba5-08d9-4523-be64-65454a1e72f4.png
- citizen7: Assets/PontoCego/UrbanArt/References/citizen7-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-3776d666-b0e2-4314-8849-4c4b9eb95ced.png
- enemy0: Assets/PontoCego/UrbanArt/References/enemy0-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-61df0adb-7eab-4a22-87a2-5975215c52a8.png
- enemy1: Assets/PontoCego/UrbanArt/References/enemy1-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-b0075c02-78c5-4f7d-8554-cb526e145139.png
- enemy3: Assets/PontoCego/UrbanArt/References/enemy3-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-63ec800b-ad60-47bc-a6e7-546ec0f80206.png
- enemy4: Assets/PontoCego/UrbanArt/References/enemy4-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-2c35c1fb-3c9a-41b1-af87-15329c44071d.png
- enemy5: Assets/PontoCego/UrbanArt/References/enemy5-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-aaf7036e-b852-45de-b1a0-253c2dfdd5c4.png
- amaral-fists: Assets/PontoCego/UrbanArt/References/amaral-fists-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-ff829711-b86f-4262-bd99-29583301530e.png
- amaral-pink: Assets/PontoCego/UrbanArt/References/amaral-pink-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-38ebd694-e925-4f88-86ad-237f796d1bdc.png
- vehicles: Assets/PontoCego/UrbanArt/References/vehicles-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-d5532f48-b412-43be-afa8-be7975e02c8f.png
- citizen1: Assets/PontoCego/UrbanArt/References/citizen1-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-323dea03-c6ed-490c-8f20-c89d27c5e2e5.png
- citizen2: Assets/PontoCego/UrbanArt/References/citizen2-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-fb9702ab-0300-4a4c-b4da-bbdcb35332c7.png
- citizen4: Assets/PontoCego/UrbanArt/References/citizen4-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-51c7cf62-c502-4ffc-8526-5c2456872b66.png
- enemy2: Assets/PontoCego/UrbanArt/References/enemy2-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-1493f445-2730-43b0-9724-69cd1b879935.png
- higor: Assets/PontoCego/UrbanArt/References/higor-coherent-v3.png
  - Original: C:\Users\User\.codex\generated_images\01a14cb5-7d27-7d92-9cce-9dba986965ea\exec-0e315d70-ac1b-47a5-8604-d77190f50a37.png


Pivô: chefe de basquete do Mocambo, uniforme laranja, bola na cintura e tênis pretos. Atlas adicional de oito direções e três poses-chave, com margens amplas. Arquivo: Assets/PontoCego/UrbanArt/References/pivo-coherent-v3.png. Gerado com image_gen nativo. Os bancos finais ficam separados em Assets/PontoCego/Urban/Animation026/.
