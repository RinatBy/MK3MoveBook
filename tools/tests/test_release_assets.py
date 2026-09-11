from pathlib import Path
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from release_assets import required_web_assets, validate_web_assets

WEB = Path(__file__).resolve().parents[2] / 'web'


class ReleaseResourcesTests(unittest.TestCase):
    def test_every_non_video_reference_exists_in_source(self):
        required = required_web_assets(WEB)
        missing = sorted(path for path in required if not path.startswith('assets/moves/') and not (WEB / path).is_file())
        self.assertEqual([], missing)

    def test_missing_font_blocks_publication(self):
        members = required_web_assets(WEB)
        members.remove('assets/fonts/RussoOne-Regular.ttf')
        with self.assertRaisesRegex(ValueError, 'RussoOne-Regular.ttf'):
            validate_web_assets(WEB, members)

    def test_generated_portrait_paths_are_checked(self):
        members = required_web_assets(WEB)
        for portrait in ('assets/portraits/kabal.png', 'assets/portraits_sega/kabal.jpg'):
            with self.subTest(portrait=portrait):
                with self.assertRaisesRegex(ValueError, 'kabal'):
                    validate_web_assets(WEB, members - {portrait})

    def test_history_and_codes_are_checked(self):
        members = required_web_assets(WEB)
        for path in ('assets/history/unmasked-sub-zero.webp', 'assets/codes/symbol-9.png'):
            with self.subTest(path=path):
                with self.assertRaises(ValueError):
                    validate_web_assets(WEB, members - {path})


if __name__ == '__main__':
    unittest.main()