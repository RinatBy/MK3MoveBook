"""Build full releases from current sources and an explicitly supplied video library.

Unlike build-release-from-base.ps1, this always packages the newly built desktop
host. It never updates an installation or overwrites an existing release asset.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile

from release_assets import validate_web_assets

RUNTIME_FILES = (
    "MK3 MoveBook.exe", "Web MK3 MBook.exe", "Microsoft.Web.WebView2.Core.dll",
    "Microsoft.Web.WebView2.WinForms.dll", "WebView2Loader.dll",
    "MicrosoftEdgeWebview2Setup.exe",
)


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def build(args: argparse.Namespace) -> None:
    source = Path(__file__).resolve().parents[1]
    web = source / "web"
    media = args.media_root.resolve(strict=True)
    desktop = args.desktop_build.resolve(strict=True)
    output = args.output_directory.resolve()
    if not re.fullmatch(r"\d+\.\d+\.\d+", args.version):
        raise ValueError("Version must be X.Y.Z")
    if json.loads((web / "version.json").read_text(encoding="utf-8-sig"))["version"] != args.version:
        raise ValueError("Source web version does not match the release")
    web_files = {
        "web/" + path.relative_to(web).as_posix(): path
        for path in web.rglob("*")
        if path.is_file() and "tests" not in path.relative_to(web).parts
        and "moves" not in path.relative_to(web).parts
    }
    videos = {"web/assets/moves/" + path.relative_to(media).as_posix(): path
              for path in media.rglob("*.webm") if path.is_file()}
    if not videos:
        raise ValueError("The video library is empty")
    animation_source = (web / "data/animations.js").read_text(encoding="utf-8-sig")
    references = set(re.findall(r'"(assets/moves/[^"\r\n]+\.webm)"', animation_source))
    missing = sorted("web/" + path for path in references if "web/" + path not in videos)
    if missing or not references:
        raise ValueError("Video references missing from library: " + repr(missing))
    web_files.update(videos)
    required_assets = validate_web_assets(web, {name.removeprefix("web/") for name in web_files})
    # Current catalog images must be present in the current source, never inherited
    # from a previous release containing an older catalog.
    catalog = (web / "data/achievements.js").read_text(encoding="utf-8-sig")
    icons = set(re.findall(r'icon:\s*"([^"]+)"', catalog))
    if any("web/" + icon not in web_files for icon in icons):
        raise ValueError("Achievement icon missing from current source")
    files = dict(web_files)
    for name in RUNTIME_FILES:
        path = desktop / name
        if not path.is_file():
            raise ValueError("New desktop build file missing: " + name)
        files[name] = path
    files["README.txt"] = source / "README.txt"
    for path in (source / "desktop/licenses").iterdir():
        if path.is_file():
            files["licenses/" + path.name] = path
    for name, path in files.items():
        if ".." in Path(name).parts or path.is_symlink():
            raise ValueError("Unsafe package path: " + name)
    output.mkdir(parents=True, exist_ok=True)
    names = [f"MK3MoveBook-{args.version}-win-x64.zip", f"MK3MoveBook-web-{args.version}.zip",
             "update-manifest.json", "SHA256SUMS.txt", "build-manifest.json"]
    if any((output / name).exists() for name in names):
        raise FileExistsError("Release assets already exist; use a fresh output directory")
    hashes = {name: digest(path) for name, path in files.items()}
    for name, members in [(names[0], files), (names[1], web_files)]:
        target = output / name
        with zipfile.ZipFile(target, "x", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
            for relative, path in sorted(members.items()):
                archive.write(path, relative)
        # Verify every uncompressed file, including all original video bytes.
        with zipfile.ZipFile(target) as archive:
            if set(archive.namelist()) != set(members):
                raise ValueError("Archive member list does not match the package")
            for relative in members:
                with archive.open(relative) as stream:
                    actual = hashlib.file_digest(stream, "sha256").hexdigest()
                if actual != hashes[relative]:
                    raise ValueError("Archive content mismatch: " + relative)
        print(f"Verified {name}: {len(members)} files, {target.stat().st_size} bytes", flush=True)
    manifest = {
        "version": args.version,
        "packageUrl": f"https://github.com/RinatBy/MK3MoveBook/releases/download/v{args.version}/{names[1]}",
        "sha256": digest(output / names[1]).upper(),
    }
    (output / names[2]).write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    (output / names[3]).write_text("".join(digest(output / name) + "  " + name + "\n" for name in names[:3]), encoding="ascii")
    report = {
        "version": args.version, "video_count": len(videos), "referenced_video_count": len(references),
        "referenced_asset_count": len(required_assets),
        "achievement_icon_count": len(icons),
        "files": [{"path": name, "bytes": files[name].stat().st_size, "sha256": hashes[name]} for name in sorted(files)],
    }
    (output / names[4]).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Verified {len(videos)} videos and {len(icons)} achievement icons. Release assets: {output}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version", required=True)
    parser.add_argument("--media-root", type=Path, required=True)
    parser.add_argument("--desktop-build", type=Path, required=True)
    parser.add_argument("--output-directory", type=Path, required=True)
    build(parser.parse_args())
