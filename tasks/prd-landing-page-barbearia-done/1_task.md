---
status: deprecated
reason: "Projeto usa EF Core Code-First - migrations são geradas automaticamente a partir das Entities e EntityTypeConfiguration. Esta tarefa foi incorporada na Tarefa 2.0"
parallelizable: false
blocked_by: []
---

<task_context>
<domain>backend/database</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>database</dependencies>
<unblocks>2.0</unblocks>
</task_context>

# Tarefa 1.0: ~~Estrutura de Banco de Dados e Migrations~~ [DEPRECATED]

> ⚠️ **TAREFA DESCONTINUADA**: Este projeto usa EF Core com abordagem Code-First. As migrations são geradas automaticamente através do comando `dotnet ef migrations add` após criar as Entities e EntityTypeConfiguration. Todo o conteúdo desta tarefa foi incorporado na Tarefa 2.0.

## Visão Geral

Criar as tabelas necessárias para armazenar configurações de landing pages e suas relações com serviços. Implementar migrations que garantam a integridade referencial e índices apropriados.

<requirements>
- Tabela `landing_page_configs` com todos os campos especificados
- Tabela `landing_page_services` para relação N:N com serviços
- Constraints de unicidade (1 landing page por barbearia)
- Foreign keys com cascade apropriado
- Índices para otimizar consultas
- Migration up/down reversíveis
</requirements>

## Subtarefas

- [ ] 1.1 Criar migration para tabela `landing_page_configs`
- [ ] 1.2 Criar migration para tabela `landing_page_services`
- [ ] 1.3 Adicionar constraints e foreign keys
- [ ] 1.4 Criar índices para performance
- [ ] 1.5 Validar migrations (up e down)
- [ ] 1.6 Documentar estrutura no README

## Detalhes de Implementação

### Tabela `landing_page_configs`

```sql
CREATE TABLE landing_page_configs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    barbershop_id UUID NOT NULL,
    template_id INTEGER NOT NULL DEFAULT 1 CHECK (template_id BETWEEN 1 AND 5),
    logo_url VARCHAR(500),
    about_text TEXT,
    opening_hours TEXT,
    instagram_url VARCHAR(255),
    facebook_url VARCHAR(255),
    whatsapp_number VARCHAR(20) NOT NULL,
    is_published BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_barbershop FOREIGN KEY (barbershop_id) 
        REFERENCES barbershops(id) ON DELETE CASCADE,
    CONSTRAINT unique_barbershop UNIQUE(barbershop_id)
);

CREATE INDEX idx_landing_page_barbershop ON landing_page_configs(barbershop_id);
CREATE INDEX idx_landing_page_published ON landing_page_configs(is_published);
```

### Tabela `landing_page_services`

```sql
CREATE TABLE landing_page_services (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    landing_page_config_id UUID NOT NULL,
    service_id UUID NOT NULL,
    display_order INTEGER NOT NULL DEFAULT 0,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_landing_page FOREIGN KEY (landing_page_config_id)
        REFERENCES landing_page_configs(id) ON DELETE CASCADE,
    CONSTRAINT fk_service FOREIGN KEY (service_id)
        REFERENCES services(id) ON DELETE CASCADE,
    CONSTRAINT unique_landing_service UNIQUE(landing_page_config_id, service_id)
);

CREATE INDEX idx_landing_page_services_config ON landing_page_services(landing_page_config_id);
CREATE INDEX idx_landing_page_services_service ON landing_page_services(service_id);
CREATE INDEX idx_landing_page_services_order ON landing_page_services(landing_page_config_id, display_order);
```

## Sequenciamento

- **Bloqueado por**: Nenhuma (primeira tarefa)
- **Desbloqueia**: 2.0 (Entities e DTOs)
- **Paralelizável**: Não

## Critérios de Sucesso

- [ ] Migrations executam sem erros
- [ ] Todas as constraints funcionam corretamente
- [ ] Rollback (down migration) funciona perfeitamente
- [ ] Índices criados melhoram performance de consultas
- [ ] Documentação das tabelas está completa
- [ ] Testes de integridade referencial passam
