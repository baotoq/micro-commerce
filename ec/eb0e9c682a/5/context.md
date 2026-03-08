# Session Context

## User Prompts

### Prompt 1

-
You are agent 4e64452d-4282-4e53-8de8-8fde4a2a394a (CTO). Continue your Paperclip work.

### Prompt 2

Base directory for this skill: /var/folders/b7/gw3fn69x5l756sbkj8s5v7j40000gn/T/paperclip-skills-Qcxe8n/.claude/skills/para-memory-files

# PARA Memory Files

Persistent, file-based memory organized by Tiago Forte's PARA method. Three layers: a knowledge graph, daily notes, and tacit knowledge. All paths are relative to `$AGENT_HOME`.

## Three Memory Layers

### Layer 1: Knowledge Graph (`$AGENT_HOME/life/` -- PARA)

Entity-based storage. Each entity gets a folder with two tiers:

1. `summar...

### Prompt 3

Base directory for this skill: /var/folders/b7/gw3fn69x5l756sbkj8s5v7j40000gn/T/paperclip-skills-Qcxe8n/.claude/skills/paperclip

# Paperclip Skill

You run in **heartbeats** — short execution windows triggered by Paperclip. Each heartbeat, you wake up, check your work, do something useful, and exit. You do not run continuously.

## Authentication

Env vars auto-injected: `PAPERCLIP_AGENT_ID`, `PAPERCLIP_COMPANY_ID`, `PAPERCLIP_API_URL`, `PAPERCLIP_RUN_ID`. Optional wake-context vars may also...

