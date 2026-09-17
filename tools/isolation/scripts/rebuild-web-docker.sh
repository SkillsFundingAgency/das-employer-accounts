#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$ROOT"
OUT="$ROOT/src/SFA.DAS.EmployerAccounts.Web/bin/net10.0"

docker run --rm \
  -v "$ROOT:/src" \
  -v "$HOME/.nuget/packages:/root/.nuget/packages" \
  -w /src \
  -e DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet build src/SFA.DAS.EmployerAccounts.Web/SFA.DAS.EmployerAccounts.Web.csproj -c Debug --nologo

python3 - <<PY
import json
from pathlib import Path
root = Path("$ROOT")
out = Path("$OUT")
www = str(root / "src/SFA.DAS.EmployerAccounts.Web/wwwroot") + "/"
comp = str(root / "src/SFA.DAS.EmployerAccounts.Web/obj/Debug/net10.0/compressed") + "/"
f = out / "SFA.DAS.EmployerAccounts.Web.staticwebassets.runtime.json"
if f.is_file():
    d = json.loads(f.read_text())
    d["ContentRoots"] = [www, comp]
    f.write_text(json.dumps(d, separators=(",", ":")))
    print("ContentRoots ->", d["ContentRoots"])
for p in (root / "src/SFA.DAS.EmployerAccounts.Web/obj/Debug/net10.0").glob("*staticwebassets*"):
    if not p.is_file():
        continue
    t = p.read_text()
    if '"/src/' in t:
        p.write_text(t.replace('"/src/', '"' + str(root) + '/'))
        print("rewrote", p.name)
PY
echo "Built $OUT"
