#!/usr/bin/env bash
# ===========================================================
# Author: Antal Teofil — FeedBackApp DevOps Quality Rules
# ===========================================================

set -euo pipefail

MSG_FILE="${1:-.git/COMMIT_EDITMSG}"
FIRST_LINE="$(head -n1 "$MSG_FILE" | tr -d '\r')"
SUBJECT_LEN=${#FIRST_LINE}

# ANSI Colors
RED="\033[0;31m"
GREEN="\033[0;32m"
YELLOW="\033[1;33m"
BLUE="\033[0;34m"
CYAN="\033[0;36m"
BOLD="\033[1m"
RESET="\033[0m"

echo -e "${CYAN}───────────────────────────────────────────────${RESET}"
echo -e "${BOLD}${BLUE}Conventional Commit Message Validator${RESET}"
echo -e "${CYAN}───────────────────────────────────────────────${RESET}"
echo

# Skip auto-generated commits
case "$FIRST_LINE" in
  Merge\ *|Revert\ *|fixup!\ *|squash!\ *)
    echo -e "${YELLOW}Auto-generated commit message detected — skipping validation.${RESET}"
    exit 0
  ;;
esac

# Allowed types (official 1.0.0 + extended)
ALLOWED_TYPES="build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test"

# Strict regex per spec
PATTERN="^(${ALLOWED_TYPES})(\([a-z0-9._-]+\))?(!)?: [^\s].{0,71}$"

if ! printf '%s' "$FIRST_LINE" | grep -Eq "$PATTERN"; then
  echo -e "${RED}Invalid commit message format.${RESET}"
  echo
  echo -e "${YELLOW}Required format:${RESET}"
  echo -e "   ${BOLD}<type>${RESET}(optional-${BOLD}scope${RESET})${BOLD}!${RESET}: <subject>"
  echo
  echo -e "${CYAN}Example:${RESET}"
  echo "   feat(auth): add password reset endpoint"
  echo "   fix(ui): correctly handle null references"
  echo "   refactor(core)!: remove deprecated API"
  echo
  echo -e "${CYAN}Allowed types:${RESET}"
  echo "   feat       – a new feature"
  echo "   fix        – a bug fix"
  echo "   chore      – maintenance, no production code change"
  echo "   docs       – documentation only"
  echo "   style      – formatting, white-space, missing semicolons, etc."
  echo "   refactor   – code change that neither fixes a bug nor adds a feature"
  echo "   perf       – performance improvements"
  echo "   test       – adding or correcting tests"
  echo "   build      – build system or dependency changes"
  echo "   ci         – continuous integration related"
  echo "   revert     – revert a previous commit"
  echo
  echo -e "${YELLOW}Hints:${RESET}"
  echo "   • Must start with a valid lowercase type"
  echo "   • Scope (if any) must be lowercase, no spaces"
  echo "   • Subject must start with lowercase and be ≤72 chars"
  echo "   • Subject cannot end with a period"
  echo "   • Use BREAKING CHANGE: footer for major changes"
  echo
  exit 1
fi

# Extract parts
TYPE=$(echo "$FIRST_LINE" | sed -E 's/^([a-z]+).*/\1/')
SCOPE=$(echo "$FIRST_LINE" | grep -oE '\([a-z0-9._-]+\)' || true)
SUBJECT="${FIRST_LINE#*: }"

# === Additional Strict Validations ===

# 1. Lowercase type
if [[ "$TYPE" =~ [A-Z] ]]; then
  echo -e "${RED}Type must be lowercase: ${RESET}$TYPE"
  exit 1
fi

# 2. Valid scope format
if [[ -n "$SCOPE" ]] && [[ "$SCOPE" =~ [A-Z\ ] ]]; then
  echo -e "${RED}Scope must be lowercase, no spaces: ${RESET}$SCOPE"
  exit 1
fi

# 3. Subject starts lowercase
FIRST_CHAR=$(printf '%s' "$SUBJECT" | cut -c1)
if [[ "$FIRST_CHAR" =~ [A-Z] ]]; then
  echo -e "${YELLOW}Subject should start with lowercase (recommended by spec): ${RESET}$SUBJECT"
fi

# 4. Subject cannot end with period
if [[ "$SUBJECT" =~ \.$ ]]; then
  echo -e "${RED}Subject must not end with a period.${RESET}"
  echo "   $FIRST_LINE"
  exit 1
fi

# 5. No ellipsis
if [[ "$SUBJECT" == *"..."* ]]; then
  echo -e "${RED}Subject must not contain ellipsis (...).${RESET}"
  exit 1
fi

# 6. No meaningless subjects
if echo "$SUBJECT" | grep -Eiq '(^|[^a-z])fix([^a-z]|$)'; then
  echo -e "${RED}Subject must not be just 'fix' — describe what was fixed.${RESET}"
  exit 1
fi
if echo "$SUBJECT" | grep -Eiq '(^|[^a-z])bugfix([^a-z]|$)'; then
  echo -e "${RED}Avoid using 'bugfix' in subject — use 'fix:' type and describe what was fixed.${RESET}"
  exit 1
fi

# 7. Warn on ticket refs
if echo "$SUBJECT" | grep -Eiq 'issue|ticket|task[0-9]*'; then
  echo -e "${YELLOW}Avoid referencing issue IDs in subject; use footer for that.${RESET}"
fi

# 8. ASCII only
if echo "$FIRST_LINE" | grep -Eq '[^[:print:]]'; then
  echo -e "${RED}Non-ASCII characters detected. Use plain ASCII text.${RESET}"
  exit 1
fi

# 9. Max length
if [ "$SUBJECT_LEN" -gt 72 ]; then
  echo -e "${RED}Subject too long (${SUBJECT_LEN}/72).${RESET}"
  echo "   $FIRST_LINE"
  echo "   → Move details into the body."
  exit 1
fi

# 10. Disallow WIP/draft/temp
if echo "$FIRST_LINE" | grep -Eiq '(wip|draft|temp|temporary)'; then
  echo -e "${RED}Work-in-progress or temporary commits are not allowed.${RESET}"
  exit 1
fi

# 11. Breaking change consistency
if echo "$FIRST_LINE" | grep -q '!:' && ! grep -iq 'BREAKING CHANGE:' "$MSG_FILE"; then
  echo -e "${YELLOW}'!' used but no 'BREAKING CHANGE:' footer found.${RESET}"
  echo "   Add a footer line: BREAKING CHANGE: <description>"
fi

# Passed
echo
echo -e "${GREEN}Commit message validated successfully according to Conventional Commits 1.0.0${RESET}"
echo -e "${BLUE}───────────────────────────────────────────────${RESET}"
echo -e "${BOLD}Type:${RESET}   $TYPE"
[[ -n "$SCOPE" ]] && echo -e "${BOLD}Scope:${RESET}  $SCOPE"
echo -e "${BOLD}Subject:${RESET} $SUBJECT"
echo -e "${BLUE}───────────────────────────────────────────────${RESET}"
exit 0
