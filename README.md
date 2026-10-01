# PC BOOST PRO

Aplicativo desktop Windows real em .EXE para diagnóstico, manutenção e otimização segura de computadores Windows.

## Requisitos

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 ou VS Code com C# Dev Kit

## Estrutura do projeto

- `PCBoostPro/` — aplicação WPF
- `README.md` — instruções gerais

## Executar localmente

```powershell
dotnet restore
# abrir a solução no Visual Studio ou rodar:
dotnet build "PCBoostPro.sln" -c Release
```

## Publicar como executável Windows

```powershell
dotnet publish "PCBoostPro/PCBoostPro.csproj" -c Release -r win-x64 --self-contained false
```

O executável será gerado em:

```text
PCBoostPro\bin\Release\net8.0-windows\win-x64\publish\
```

## Funcionalidades

- Diagnóstico do sistema
- Monitor de CPU, memória, disco e rede
- Listagem de processos
- Verificação de saúde do Windows
- Limpeza segura de arquivos temporários
- Histórico local em SQLite
- Exibição explícita de dados indisponíveis

## Observação

Este projeto foi desenvolvido para rodar como app nativo no Windows. Algumas informações dependem da disponibilidade do sistema operacional e dos dados do hardware; nesses casos, o aplicativo mostra claramente `Não disponível` em vez de inventar resultados.
