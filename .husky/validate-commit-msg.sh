#!/usr/bin/env bash
# ===========================================
# Conventional Commit Validator (Husky.NET)
# ===========================================
set -euo pipefail

MSG_FILE="${1:-.git/COMMIT_EDITMSG}"
FIRST_LINE="$(head -n1 "$MSG_FILE" | tr -d '\r')"
SUBJECT_LEN=${#FIRST_LINE}

# ANSI színek
RED="\033[0;31m"
GREEN="\033[0;32m"
YELLOW="\033[1;33m"
BLUE="\033[0;34m"
BOLD="\033[1m"
RESET="\033[0m"

echo -e "${BLUE}Validating commit message...${RESET}"

# Engedélyezett automatikus commit üzenetek (merge, revert, fixup, squash)
case "$FIRST_LINE" in
  Merge\ *|Revert\ *|fixup!\ *|squash!\ *)
    echo -e "${YELLOW}Auto-generated commit message detected – skipping validation.${RESET}"
    exit 0
  ;;
esac

# Engedélyezett prefixek
ALLOWED_TYPES="build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test|security|deps|infra|config|env|meta"

# Regex: <type>(optional-scope)!?: <subject>
PATTERN="^(${ALLOWED_TYPES})(\([a-z0-9._-]+\))?(!)?: [^\s].{0,71}$"

if ! printf '%s' "$FIRST_LINE" | grep -Eq "$PATTERN"; then
  echo -e "\n${RED}Invalid commit message:${RESET}"
  echo "   $FIRST_LINE"
  echo -e "\n${YELLOW}Expected format:${RESET}"
  echo "   <type>(<optional-scope>)<optional-!>: <subject up to 72 chars>"
  echo -e "\n${YELLOW}Allowed types:${RESET}"
  echo -e "   ${BOLD}${BLUE}feat${RESET}       – new feature"
  echo -e "   ${BOLD}${BLUE}fix${RESET}        – bug fix"
  echo -e "   ${BOLD}${BLUE}chore${RESET}      – maintenance / no functional change"
  echo -e "   ${BOLD}${BLUE}docs${RESET}       – documentation only"
  echo -e "   ${BOLD}${BLUE}style${RESET}      – code style / formatting"
  echo -e "   ${BOLD}${BLUE}refactor${RESET}   – code reorganization without behavior change"
  echo -e "   ${BOLD}${BLUE}perf${RESET}       – performance improvement"
  echo -e "   ${BOLD}${BLUE}test${RESET}       – adding or updating tests"
  echo -e "   ${BOLD}${BLUE}build${RESET}      – build system, NuGet, packaging changes"
  echo -e "   ${BOLD}${BLUE}ci${RESET}         – CI/CD pipeline changes"
  echo -e "   ${BOLD}${BLUE}revert${RESET}     – revert a previous commit"
  echo -e "   ${BOLD}${BLUE}security${RESET}   – security fixes or mitigations"
  echo -e "   ${BOLD}${BLUE}deps${RESET}       – dependency updates"
  echo -e "   ${BOLD}${BLUE}infra${RESET}      – infrastructure / DevOps changes"
  echo -e "   ${BOLD}${BLUE}config${RESET}     – configuration or environment setup"
  echo -e "   ${BOLD}${BLUE}env${RESET}        – environment variable or deployment config"
  echo -e "   ${BOLD}${BLUE}meta${RESET}       – meta commits (README, license, badges)"
  echo -e "\n${YELLOW}Good examples:${RESET}"
  echo "   feat(auth): add JWT-based login"
  echo "   fix(api): handle null response safely"
  echo "   refactor(core)!: remove deprecated method"
  echo "   docs(readme): clarify setup instructions"
  echo "   test(repo): add integration tests"
  echo -e "\n${YELLOW}Bad examples:${RESET}"
  echo "   update stuff"
  echo "   fix : missing colon"
  echo "   feat(core): subject is way too long ................................................................"
  echo "   feat(core): ends with period."
  echo -e "\n${YELLOW}Tips:${RESET}"
  echo "   • Use imperative mood (add, fix, remove)"
  echo "   • Keep the first line ≤ 72 chars"
  echo "   • No trailing period"
  echo "   • Use BREAKING CHANGE: in the footer for major changes"
  echo
  exit 1
fi

# Ne végződjön pontra
if printf '%s' "$FIRST_LINE" | grep -Eq '\.\s*$'; then
  echo -e "${RED}Do not end the subject with a period:${RESET}"
  echo "   $FIRST_LINE"
  exit 1
fi

# Max hossz 72
if [ "$SUBJECT_LEN" -gt 72 ]; then
  echo -e "${RED}Subject too long (${SUBJECT_LEN}/72):${RESET}"
  echo "   $FIRST_LINE"
  echo "   → Move extra details to the body."
  exit 1
fi

echo -e "${GREEN}Commit message follows Conventional Commits.${RESET}"
exit 0
