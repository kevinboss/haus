#!/usr/bin/env bash
# Generates the README demo GIF. Covers scripting + mutating ops across haus APIs.
#
# Re-render:
#   asciinema rec --command "bash docs/demo.sh" --window-size 110x30 --idle-time-limit 2 --overwrite docs/demo.cast
#   agg --theme github-dark --font-size 14 docs/demo.cast docs/haus-demo.gif
#
# Requires: haus (authenticated), jq, asciinema, agg.
# The demo toggles light.living_room off then on, and creates+deletes an
# input_boolean helper. Initial state is captured and restored on exit so
# repeated recordings leave Home Assistant clean.

set -euo pipefail

LIGHT_ENTITY="light.living_room"
HELPER_ID="input_boolean.haus_demo"
LIGHT_INITIAL=$(haus state get "$LIGHT_ENTITY" --json | jq -r '.state')

cleanup() {
  if [[ "$LIGHT_INITIAL" == "on" ]]; then
    haus service call light.turn_on --entity "$LIGHT_ENTITY" >/dev/null 2>&1 || true
  else
    haus service call light.turn_off --entity "$LIGHT_ENTITY" >/dev/null 2>&1 || true
  fi
  haus helper delete "$HELPER_ID" >/dev/null 2>&1 || true
}
trap cleanup EXIT

PROMPT=$'\e[32m$\e[0m '
TYPE_DELAY=0.035
PAUSE_BEFORE_RUN=0.35
PAUSE_AFTER_RUN=1.3

type_and_run() {
  local cmd=$1
  printf "%s" "$PROMPT"
  for (( i=0; i<${#cmd}; i++ )); do
    printf "%s" "${cmd:i:1}"
    sleep "$TYPE_DELAY"
  done
  printf "\n"
  sleep "$PAUSE_BEFORE_RUN"
  eval "$cmd"
  sleep "$PAUSE_AFTER_RUN"
}

sleep 0.5

# state + porcelain + Unix pipeline — top 3 entity domains
type_and_run "haus state list --porcelain | tail -n +2 | cut -d. -f1 | sort | uniq -c | sort -rn | head -3"

# template — server-side Jinja eval
type_and_run "haus template '{{ states.light.living_room.state }}'"

# service — mutate the world
type_and_run "haus service call light.turn_off --entity light.living_room"

# template — verify the state changed
type_and_run "haus template '{{ states.light.living_room.state }}'"

# service — restore
type_and_run "haus service call light.turn_on --entity light.living_room"

# helper — create a new input_boolean
type_and_run "haus helper create-boolean --object-id haus_demo --name 'Haus Demo'"

# state get — default human output (Spectre table)
type_and_run "haus state get input_boolean.haus_demo"

# helper — clean up after ourselves
type_and_run "haus helper delete input_boolean.haus_demo"

printf "%s\n" "$PROMPT"
sleep 0.6
