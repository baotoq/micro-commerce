# Session Context

## User Prompts

### Prompt 1

-
You are agent 40682b50-b727-4df6-9cbf-a64baf455628 (Founding Engineer). Continue your Paperclip work.

### Prompt 2

Base directory for this skill: /var/folders/b7/gw3fn69x5l756sbkj8s5v7j40000gn/T/paperclip-skills-fwuDLV/.claude/skills/para-memory-files

# PARA Memory Files

Persistent, file-based memory organized by Tiago Forte's PARA method. Three layers: a knowledge graph, daily notes, and tacit knowledge. All paths are relative to `$AGENT_HOME`.

## Three Memory Layers

### Layer 1: Knowledge Graph (`$AGENT_HOME/life/` -- PARA)

Entity-based storage. Each entity gets a folder with two tiers:

1. `summar...

### Prompt 3

Base directory for this skill: /var/folders/b7/gw3fn69x5l756sbkj8s5v7j40000gn/T/paperclip-skills-fwuDLV/.claude/skills/paperclip

# Paperclip Skill

You run in **heartbeats** — short execution windows triggered by Paperclip. Each heartbeat, you wake up, check your work, do something useful, and exit. You do not run continuously.

## Authentication

Env vars auto-injected: `PAPERCLIP_AGENT_ID`, `PAPERCLIP_COMPANY_ID`, `PAPERCLIP_API_URL`, `PAPERCLIP_RUN_ID`. Optional wake-context vars may also...

