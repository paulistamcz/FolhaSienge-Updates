# FolhaSienge-Updates

Repositório público de atualizações do app **FolhaSienge** (Importação de Folha de Pagamento para o Sienge - ENGEMAT).

O app consulta automaticamente o último Release aqui na inicialização e se atualiza.

## Como publicar uma atualização

1. No projeto privado, aumente a versão em FolhaSienge.csproj.
2. Publique o build (dotnet publish -c Release -r win-x64 --self-contained false).
3. Compacte o conteúdo da pasta publicado em FolhaSienge_<versao>.zip.
4. Crie um Release <versao> neste repositório anexando o zip.

> O nome do arquivo do zip é o que o app usa para baixar o pacote.

