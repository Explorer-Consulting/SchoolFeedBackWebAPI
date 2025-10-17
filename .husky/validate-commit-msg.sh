#!/usr/bin/env bash
# ===========================================
# Commit message convention validator (Husky.NET)
# ===========================================
set -euo pipefail

MSG_FILE="$1"
COMMIT_MSG=$(cat "$MSG_FILE")

# Conventional commit regex:
PATTERN='^(feat|fix|chore|docs|style|refactor|perf|test|build|ci|revert)(\([a-z0-9_.-]+\))?: .{1,72}$'

echo "Validating commit message..."

if [[ "$COMMIT_MSG" =~ $PATTERN ]]; then
  echo "Commit message follows Conventional Commits format."
  exit 0
else
  echo "Invalid commit message!"
  echo "Expected format: <type>(optional-scope): <description>"
  echo "Allowed types: feat, fix, chore, docs, style, refactor, perf, test, build, ci, revert"
  echo ""
  echo "Example: feat(core): add new validation rule"
  exit 1
fi
