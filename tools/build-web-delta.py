"""Build a content delta accepted by the existing in-app updater (no EXE install)."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import zipfile


def web_files(archive: zipfile.ZipFile) -> dict[str, str]:
    result = {}
    for member in archive.infolist():
        if member.is_dir():
            continue
        path = PurePosixPath(member.filename)
        if path.is_absolute() or '..' in path.parts or not member.filename.startswith('web/'):
            raise ValueError('Unexpected web archive path: ' + member.filename)
        relative = member.filename[4:]
        if relative in result:
            raise ValueError('Duplicate archive path: ' + relative)
        with archive.open(member) as stream:
            result[relative] = hashlib.file_digest(stream, 'sha256').hexdigest().upper()
    return result


def build(base_package: Path, target_package: Path, output: Path, from_version: str, version: str) -> dict:
    with zipfile.ZipFile(base_package) as base, zipfile.ZipFile(target_package) as target:
        if json.loads(base.read('web/version.json'))['version'] != from_version:
            raise ValueError('Base package version mismatch')
        if json.loads(target.read('web/version.json'))['version'] != version:
            raise ValueError('Target package version mismatch')
        before, after = web_files(base), web_files(target)
        changed = sorted(name for name in after if before.get(name) != after[name])
        deleted = sorted(before.keys() - after.keys())
        name = f'MK3MoveBook-delta-{from_version}-to-{version}.zip'
        output.mkdir(parents=True, exist_ok=True)
        destination = output / name
        with zipfile.ZipFile(destination, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as patch:
            for relative in changed:
                patch.writestr('web/' + relative, target.read('web/' + relative))
            patch.writestr('patch-info.json', json.dumps({'TargetVersion': version, 'SupportedBaseVersions': [from_version]}))
            patch.writestr('deleted-files.txt', ''.join(relative + '\n' for relative in deleted))
            patch.writestr('target-files.sha256', ''.join(after[relative] + ' ' + relative + '\n' for relative in sorted(after)))
        # Simulate the existing updater: copy base, overlay patch, delete listed
        # obsolete files, then verify every target file. No media re-encoding.
        with zipfile.ZipFile(destination) as patch:
            installed = (set(before) | set(changed)) - set(deleted)
            if installed != set(after):
                raise ValueError('Delta target file set mismatch')
            for relative, expected in after.items():
                source = patch if relative in changed else base
                with source.open('web/' + relative) as stream:
                    if hashlib.file_digest(stream, 'sha256').hexdigest().upper() != expected:
                        raise ValueError('Delta reconstruction failed: ' + relative)
    with destination.open('rb') as stream:
        sha = hashlib.file_digest(stream, 'sha256').hexdigest().upper()
    result = {'fromVersion': from_version, 'version': version, 'file': name, 'sha256': sha,
              'bytes': destination.stat().st_size, 'changed_files': len(changed),
              'deleted_files': len(deleted), 'verified_files': len(after)}
    (output / (name + '.report.json')).write_text(json.dumps(result, indent=2), encoding='utf-8')
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-web-package', type=Path, required=True)
    parser.add_argument('--target-web-package', type=Path, required=True)
    parser.add_argument('--output-directory', type=Path, required=True)
    parser.add_argument('--from-version', required=True)
    parser.add_argument('--version', required=True)
    args = parser.parse_args()
    print(json.dumps(build(args.base_web_package, args.target_web_package, args.output_directory, args.from_version, args.version), indent=2))