---
status: pending
parallelizable: true
blocked_by: ["1.0","2.0"]
---

<task_context>
<domain>engine/infra/ci</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>"5.0"</unblocks>
</task_context>

# Tarefa 4.0: Implementar job `admin-tests` (Node/Vitest)

## Visão Geral
Criar o job do frontend que instala dependências com `npm ci`, executa `npm run build` e `npm run test -- --runInBand`, aproveitando cache npm e evitando downloads desnecessários do Playwright durante testes unitários.

## Requisitos
- Uso de `actions/setup-node@v4` com `node-version: 20.x` e cache npm (`cache: npm`, `cache-dependency-path: barbapp-admin/package-lock.json`).
- Execução de `npm ci` com `PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1` para impedir download dos browsers E2E.
- Execução de `npm run build` e `npm run test -- --runInBand`, com `CI=true` para modo não interativo.
- Falha no comando de teste deve marcar o job como `failure`.

## Subtarefas
- [ ] 4.1 Adicionar passos de checkout e `actions/setup-node@v4` com cache configurado.
- [ ] 4.2 Executar `npm ci` no diretório `barbapp-admin` com as variáveis de ambiente definidas.
- [ ] 4.3 Acrescentar passos `npm run build` e `npm run test -- --runInBand` com `CI=true`.
- [ ] 4.4 Validar o job via execução do workflow, garantindo que falhas de teste quebram a pipeline.

## Sequenciamento
- Bloqueado por: 1.0, 2.0
- Desbloqueia: 5.0
- Paralelizável: Sim (pode evoluir em paralelo ao job backend após configuração do serviço)

## Detalhes de Implementação
- Tech Spec: “Visão Geral dos Componentes” (job `admin-tests`), “Interfaces Principais” (bloco YAML do job frontend), “Modelos de Dados” (variáveis e cache npm).
- PRD: “Funcionalidades Principais – Job de frontend (`barbapp-admin`)” requisitos R6-R8.

## Critérios de Sucesso
- Job `admin-tests` conclui com sucesso quando `npm run test` passa e falha quando algum teste quebra.
- Logs mostram uso do cache npm em execuções subsequentes.
- Nenhum download de browsers Playwright ocorre durante a instalação.
