#!/usr/bin/env python3
"""Build and deploy dashboard to qa-sosgroup (rg-aitbp-app).

Zip entries must be marked Unix (create_system=3), not Windows/FAT — Azure App
Service Linux (WEBSITE_RUN_FROM_PACKAGE=1) mounts the zip directly and fails to
resolve nested wwwroot/ folders for Windows-marked entries, serving them as
0-byte files. This has broken qa-sosgroup's static assets twice (2026-09-15,
2026-09-21) when deployed with PowerShell's Compress-Archive instead.
"""
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

RESOURCE_GROUP = "rg-aitbp-app"
APP_NAME = "qa-sosgroup"

ROOT = Path(__file__).parent
PUBLISH_DIR = ROOT / "publish"
ZIP_PATH = ROOT / "deploy-unix.zip"


def run(cmd):
    # az on Windows is az.cmd — shutil.which() resolves it so subprocess.run
    # can find it without shell=True (PATHEXT lookup doesn't apply otherwise).
    resolved = shutil.which(cmd[0]) or cmd[0]
    print(f"$ {' '.join(cmd)}")
    subprocess.run([resolved, *cmd[1:]], check=True, cwd=ROOT)


def build():
    if PUBLISH_DIR.exists():
        import shutil
        shutil.rmtree(PUBLISH_DIR)
    run(["dotnet", "publish", "-c", "Release", "-o", str(PUBLISH_DIR)])


def package():
    if ZIP_PATH.exists():
        ZIP_PATH.unlink()
    with zipfile.ZipFile(ZIP_PATH, "w", zipfile.ZIP_DEFLATED) as zf:
        for file in PUBLISH_DIR.rglob("*"):
            if file.is_file():
                rel = file.relative_to(PUBLISH_DIR).as_posix()
                zi = zipfile.ZipInfo.from_file(file, rel)
                zi.compress_type = zipfile.ZIP_DEFLATED
                zi.create_system = 3  # Unix — see module docstring
                zf.writestr(zi, file.read_bytes())
    print(f"Zip listo: {ZIP_PATH} ({ZIP_PATH.stat().st_size:,} bytes)")


def deploy():
    run([
        "az", "webapp", "deploy",
        "--resource-group", RESOURCE_GROUP,
        "--name", APP_NAME,
        "--src-path", str(ZIP_PATH),
        "--type", "zip",
    ])


if __name__ == "__main__":
    try:
        build()
        package()
        deploy()
    except subprocess.CalledProcessError as e:
        print(f"Fallo en: {e.cmd}", file=sys.stderr)
        sys.exit(1)
