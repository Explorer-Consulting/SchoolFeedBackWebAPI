#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
cd "$ROOT"

# csak a stage-elt .cs fájlok
mapfile -t FILES < <(git diff --cached --name-only --diff-filter=ACM | grep -E '\.cs$' || true)
[ ${#FILES[@]} -eq 0 ] && { echo "No staged .cs files."; exit 0; }

echo "Running dotnet format (folder mode) on staged .cs files..."
dotnet format --folder --no-restore --verify-no-changes --include "${FILES[@]}"
