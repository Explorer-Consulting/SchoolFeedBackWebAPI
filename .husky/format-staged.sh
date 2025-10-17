#!/usr/bin/env bash
set -euo pipefail

echo "Running dotnet format on staged .cs files..."

# Stage-elt C# fájlok lekérése
mapfile -t FILES < <(git diff --cached --name-only --diff-filter=ACM | grep -E '\.cs$' || true)

# Ha nincs mit formázni, lépjünk ki
if [ ${#FILES[@]} -eq 0 ]; then
  echo "No staged .cs files to format."
  exit 0
fi

echo "Formatting ${#FILES[@]} staged file(s)..."

# Formázás (automatikusan kijavítja az eltéréseket)
dotnet format --include "${FILES[@]}"

# A kijavított fájlokat újra stage-eljük, hogy a commitba a formázott verzió kerüljön
git add "${FILES[@]}"

echo "All staged files formatted and re-staged according to .editorconfig."
