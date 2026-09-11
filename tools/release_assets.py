"""Check the resources used by the book, including generated fighter image paths."""
from __future__ import annotations
import json
from pathlib import Path
import re


def required_web_assets(web: Path) -> set[str]:
    required: set[str] = set()
    for path in web.rglob('*'):
        if not path.is_file() or path.suffix not in ('.js', '.css', '.html') or 'tests' in path.relative_to(web).parts:
            continue
        text = path.read_text(encoding='utf-8-sig')
        for reference in re.findall(r'''["'`](?:\.\./)*(assets/[^"'`\r\n]+)["'`]''', text):
            if '${' not in reference:
                required.add(reference.split('?', 1)[0].split('#', 1)[0])
    source = (web / 'data/moves.js').read_text(encoding='utf-8-sig')
    data = json.loads(source[source.index('{'):].rstrip('; \r\n'))
    fighters = {fighter['id'] for version in data['versions'].values() for fighter in version['fighters']}
    if not fighters:
        raise ValueError('No fighters found when checking portrait resources')
    required.update('assets/portraits/' + fighter + '.png' for fighter in fighters)
    app = (web / 'app.js').read_text(encoding='utf-8-sig')
    sega = re.search(r'const segaPortraits\s*=\s*\{(.*?)\};', app, re.S)
    history = re.search(r'const historyImageFighters\s*=\s*new Set\(\[(.*?)\]\);', app, re.S)
    if not sega or not history:
        raise ValueError('Fighter image mappings changed; update the resource validation')
    required.update('assets/portraits_sega/' + name for name in re.findall(r':\s*"([^"]+)"', sega.group(1)))
    required.update('assets/history/' + fighter + '.webp' for fighter in re.findall(r'"([^"]+)"', history.group(1)))
    required.update('assets/codes/symbol-' + str(number) + '.png' for number in range(10))
    required.update(('assets/fonts/RussoOne-OFL.txt', 'assets/fonts/Literata-OFL.txt'))
    return required


def validate_web_assets(web: Path, members: set[str]) -> set[str]:
    """Members are paths relative to web, as stored in the target package."""
    required = required_web_assets(web)
    missing = sorted(required - members)
    if missing:
        raise ValueError('Referenced resources missing from package: ' + ', '.join(missing))
    return required