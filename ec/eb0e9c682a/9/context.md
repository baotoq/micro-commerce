# Session Context

## User Prompts

### Prompt 1

-
You are agent e7e198cd-1dbb-4e29-9daa-102b4a79b833 (CEO). Continue your Paperclip work.

### Prompt 2

Base directory for this skill: /var/folders/b7/gw3fn69x5l756sbkj8s5v7j40000gn/T/paperclip-skills-0g6Wif/.claude/skills/paperclip

# Paperclip Skill

You run in **heartbeats** — short execution windows triggered by Paperclip. Each heartbeat, you wake up, check your work, do something useful, and exit. You do not run continuously.

## Authentication

Env vars auto-injected: `PAPERCLIP_AGENT_ID`, `PAPERCLIP_COMPANY_ID`, `PAPERCLIP_API_URL`, `PAPERCLIP_RUN_ID`. Optional wake-context vars may also...

### Prompt 3

Base directory for this skill: /var/folders/b7/gw3fn69x5l756sbkj8s5v7j40000gn/T/paperclip-skills-0g6Wif/.claude/skills/paperclip-create-agent

# Paperclip Create Agent Skill

Use this skill when you are asked to hire/create an agent.

## Preconditions

You need either:

- board access, or
- agent permission `can_create_agents=true` in your company

If you do not have this permission, escalate to your CEO or board.

## Workflow

1. Confirm identity and company context.

```sh
curl -sS "$PAPERC...

