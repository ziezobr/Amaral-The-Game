# Ambiente 2D — ciclo e clima

O relógio global DayNightCycle é a fonte única de hora. WorldWeather2D inicializa automaticamente nas cenas 2D com PlayerMotor e câmera ortográfica e permanece entre cenas, sem reiniciar em uma troca de missão. O mundo contínuo usa um único conjunto de efeitos e consulta estradas/retângulos do WorldHub para as poças e proteção dos interiores.

A previsão usa blocos de três horas do relógio de jogo. Chuva, vento e cobertura de nuvens interpolam; um temporal não troca a tela de uma vez. As poças levam tempo para encher e secar. Chuva e fachos são ocultados nos interiores autorados. Nenhum efeito adiciona collider ou muda a navegação.

Os efeitos são estilizados em 2D: 112 gotas no máximo, 6 nuvens, 6 faixas de neblina, 5 fachos solares e 14 vagalumes, mais 32 poças no máximo. O pool é criado ao entrar no cenário e reaproveitado. A névoa é concentrada nas baixadas autoradas de Mocambo/sul. Vagalumes surgem perto de vegetação durante a noite. Nuvens atenuam o contraste e a intensidade das sombras. Não há volumetria 3D, traçado de raios, PBR ou reflexo real do cenário; fachos e reflexos são sprites pintados/paramétricos.

WorldVegetation2D deve ser anexado somente ao filho visual com SpriteRenderer, sem Collider2D. interactionRadius (padrão1.5m) controla a compressão visual. A inclinação total é limitada a6graus. Se existir collider no mesmo objeto, o componente se desabilita. ReduceMotion reduz gotas, fixa nuvens/fachos/vagalumes e retira balanço/poeira.

WorldPropShadow2D recebe source (SpriteRenderer), footLocalOffset (pés no espaço do root), height e groundSortingOrder. A sombra não tem collider. GroundedDepth2D usa a mesma direção de luz para atores e limita poeira a6sprites reutilizáveis por ator.

API para capturas/testes:

```csharp
var weather = WorldWeather2D.Instance;
weather.automaticWeather = false;
weather.SetWeather(WeatherKind.Storm, true);
float rain = weather.Rain; // 0..1
float wetness = weather.Wetness; // acumula, mesmo em SetWeather(immediate:true)
bool underRoof = weather.IsSheltered(position);
```

Testes: WeatherModelTests verifica transição, evaporação e entradas inválidas de tempo. WeatherSceneTests verifica pool limitado, ausência de obstáculos extras, interiores e desativação no menu. A execução final fica a cargo da validação central do projeto.
