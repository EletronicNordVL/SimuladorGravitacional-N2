# Órbita — Simulador gravitacional 2D

Interface web para o projeto em C#/.NET 10. A física é executada pelas classes `Universo` e `Corpo` originais; o navegador desenha os estados retornados pelo servidor, sem duplicar o cálculo gravitacional.

A [documentação completa](DOCUMENTACAO.md) descreve as classes, o encapsulamento, as equações físicas, a integração da interface e o roteiro de apresentação.

## Executar a interface

Na pasta do projeto:

Para a apresentação, dê dois cliques em **`Iniciar.cmd`**. Na primeira execução ele prepara uma versão compilada em `.local-runtime/app`, inicia o servidor em segundo plano e abre o navegador. Nas próximas execuções, reutiliza o servidor existente, evitando instâncias duplicadas. O servidor não depende do terminal, do Visual Studio ou desta conversa.

Um supervisor local reinicia o servidor se o processo encerrar inesperadamente. Para encerrar intencionalmente, use **`Parar.cmd`**. Para abrir sem iniciar o navegador, execute `powershell -NoProfile -ExecutionPolicy Bypass -File Iniciar.ps1 -NoBrowser`. A inicialização pelo Visual Studio usa a mesma porta **5080**. Os registros ficam em `.local-runtime/server.log`, `server-error.log` e `supervisor.log`.

Ou execute pelo terminal:

```powershell
dotnet run --project Web/TrabalhoN1.Web.csproj
```

Abra **http://localhost:5080** no navegador. É necessário o SDK .NET 10. Não há dependências npm, fontes externas ou serviços de terceiros. O servidor aceita conexões locais.

## Executar o programa de console

```powershell
dotnet run --project TrabalhoN1.csproj
```

## Recursos

- Universo de corpos aleatórios gerados diretamente pelo método original `Universo.GerarCorposAleatorios`: massas de 10²² a 10²⁵ kg, densidades de 3.000 a 6.000 kg/m³, posições até ±5 × 10⁹ m e velocidades até ±200 m/s em cada eixo.
- Iniciar, pausar, avançar um passo, restaurar condições iniciais e definir limite de iterações.
- Passo físico entre 0,001 e 86.400 segundos; reprodução de 1 a 10 passos por atualização.
- Visualização com zoom, deslocamento, enquadramento, nomes, grade e histórico de trajetórias.
- Consulta e edição de corpos; até 100 corpos por universo.
- Salvamento, carregamento e exclusão de `universo_N.txt` na pasta raiz, compatíveis com o console.
- Importação/exportação de arquivos `.txt`; importação aceita ponto ou vírgula decimal.
- Interface responsiva, controles por teclado e confirmação antes de substituir um universo ou excluir um arquivo.

## Comportamento e precisão

O passo de tempo é o intervalo físico integrado, não um atraso em tempo real. O cálculo mantém o método original: gravitação de Newton, atualização por MRUV e colisões elásticas. Esse integrador é uma aproximação e pode acumular erro, especialmente com passos grandes ou encontros próximos. Reduza o passo nesses casos.

Não há estrela central nem órbitas pré-definidas: todos os corpos interagem pela gravitação. Os raios visuais são ampliados: as colisões usam os raios físicos calculados por massa e densidade. Os rastros guardam as últimas 800 posições retornadas; na reprodução acelerada, mostram os estados ao final de cada grupo de passos.

Editar ou adicionar/remover corpos redefine o estado inicial e os contadores. Restaurar volta a esse estado. Salvar/exportar guarda posições e velocidades atuais no formato original, que não contém tempo acumulado nem histórico de trajetórias. Ao carregar/importar, os contadores recomeçam.

## Verificação

```powershell
dotnet build Web/TrabalhoN1.Web.csproj
node --check Web/wwwroot/app.js
```

Com o servidor rodando, `node scripts/verify-api.mjs` verifica a integração HTTP, a evolução dos corpos, ação e reação, importação/exportação e rejeição de entradas inválidas, sem criar ou excluir universos salvos.

`scripts/verify-browser.mjs` verifica a interface, os arquivos, o mouse, os atalhos e o layout em desktop e celular usando um Chrome de teste com a porta de depuração 9222. Ele cria e exclui somente o universo salvo pelo próprio teste.

`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-runtime.ps1` verifica os atalhos, a recuperação automática e a permanência do servidor. Esse teste interrompe brevemente o processo deste projeto para provar a recuperação e deixa o localhost em funcionamento ao terminar.
