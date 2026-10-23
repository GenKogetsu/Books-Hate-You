"""Normalize authored KogetsuLibrary comments without changing C# executable tokens.

Usage: python Tools/format_library_docs.py [--check]
Vendor code and generated source are excluded. Existing prose and XML links remain.
"""
import argparse
import re
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "Assets/Resources/KogetsuLibrary-main"
TOKEN = re.compile(
    r'(?:\$@|@\$|@)"(?:[^"]|"")*"|\$?"(?:\\.|[^"\\])*"|'
    r"'(?:\\.|[^'\\])*'|//[^\n]*|/\*[\s\S]*?\*/|"
    r'^[ \t]*#(?:end)?region[^\n]*|\s+|.', re.M)
DOC_BLOCK = re.compile(r'(?m)^([ \t]*)///[^\n]*(?:\n[ \t]*///[^\n]*)*')
NOTE_BLOCK = re.compile(r'(?m)^([ \t]*)//(?!/)[^\n]*(?:\n[ \t]*//(?!/)[^\n]*)*')
SUMMARY = re.compile(r'<summary>([\s\S]*?)</summary>')
RETURNS = re.compile(r'<returns>([\s\S]*?)</returns>')
HEADERS = ('Summary', 'Return', 'Parameters', 'Notes', 'Refactor', 'Flow', 'Setup')

# Reviewed translations for existing single-language documentation. Original
# detailed paragraphs stay in place; these supply the missing language.
TRANSLATIONS = {
    'Resolves library assets': ('TH', 'ค้นหา asset ของไลบรารีทั้งใน Assets และ Packages โดยไม่ผูกกับตำแหน่งติดตั้งตายตัว'),
    'Repeatable folder migration': ('TH', 'เครื่องมือจัดโฟลเดอร์และตรวจ reference ที่เรียกซ้ำได้ โดยไม่เปลี่ยนข้อมูล gameplay'),
    'Runtime helper for hierarchy menu': ('TH', 'ตัวช่วยสร้าง object ให้เมนู Hierarchy ที่ generate ไว้ โดยรวมขั้นตอนสร้างไว้จุดเดียว'),
    'Finds the prefab named': ('TH', 'ค้นหา prefab ตามชื่อ สร้างลง Scene และเลือก object ที่สร้าง; หากไม่พบจะสร้าง GameObject ว่างแทน'),
    'Scans every prefab': ('TH', 'ตรวจ prefab ที่ใช้ CreateHierarchyMenu แล้ว generate เมนูจากชื่อ prefab อัตโนมัติ; โค้ดเมนูอยู่ภายใต้ UNITY_EDITOR'),
    'Returns a dictionary of scriptAssetGUID': ('TH', 'สร้างตาราง GUID ของสคริปต์ไปยัง menu path สำหรับ MonoBehaviour ที่ไม่เป็น abstract และมี CreateHierarchyMenu'),
    'สร้าง VNChoiceButton ใน Scene': ('EN', 'Creates a VNChoiceButton scene object with Button, Image and TMP label; the user can save it as a prefab.'),
    'สร้าง VNChoicePanel ใน Scene': ('EN', 'Creates a VNChoicePanel scene object with a child ButtonContainer.'),
    'Marks a MonoBehaviour class': ('TH', 'กำหนด menu path ให้ MonoBehaviour เพื่อ generate เมนูสร้าง prefab ทุกตัวที่มี component นี้ โดยใช้ชื่อ prefab ที่ค้นพบ'),
    'Path after': ('TH', 'ส่วนของ menu path ที่ต่อจาก GameObject/ เช่น KogetsuLibrary/Core/Manager'),
    'Messaging contract shared': ('TH', 'สัญญาการส่งเหตุการณ์ร่วมกันระหว่างผู้ส่งและผู้รับ เพื่อลดการผูกกับ implementation'),
    'Pool API for consumers': ('TH', 'สัญญาดึงและคืน object ให้ผู้เรียกใช้ pool โดยไม่ผูกกับ manager ตัวใดตัวหนึ่ง'),
    'True when both Context and Stats': ('TH', 'เป็น true เมื่อ Context และ Stats ไม่เป็น null จึงอ่านข้อมูลสถิติผ่าน context ได้'),
    'Current move speed from StatsController': ('TH', 'ความเร็วเคลื่อนที่ปัจจุบันที่อ่านจาก StatsController ผ่าน context'),
    'Jump force from StatsController': ('TH', 'แรงกระโดดที่อ่านจาก StatsController ผ่าน context'),
    'Sets <see cref="IMoveContext.CurrentDirection"': ('TH', 'ตั้ง CurrentDirection และจำทิศล่าสุดที่ไม่เป็นศูนย์ใน LastFacingDirection เพื่อให้การหันภาพยังอ้างอิงทิศเดิมเมื่อหยุด'),
    'Set full 2D velocity': ('TH', 'กำหนดความเร็ว Rigidbody2D ทั้งแกน X และ Y'),
    'Set X-axis velocity only': ('TH', 'กำหนดเฉพาะความเร็วแกน X โดยเก็บค่า Y เดิมไว้เพื่อคงแรงโน้มถ่วงและการกระโดด'),
    'Zero out Rigidbody2D velocity': ('TH', 'ตั้งความเร็ว Rigidbody2D เป็นศูนย์ทุกแกน'),
    'Full keyboard/gamepad input pipeline': ('TH', 'ประมวลผล input ตามลำดับ ClampMagnitude(1), TransformInput และ SnapDirection แล้วคืนทิศสุดท้ายให้การเคลื่อนที่และการหันภาพ'),
    'Partial input pipeline': ('TH', 'จำกัดขนาด input แล้ว TransformInput โดยไม่ snap เพื่อใช้ทิศดิบกับความเร็วและ snap แยกเฉพาะทิศภาพ'),
    '2D move ability รองรับ': ('EN', 'Supports free/top-down movement and side-view movement; side-view preserves vertical velocity and adds coyote time, input buffering and variable jump height when jumping is enabled.'),
    'Top-down: velocity follows': ('TH', 'โหมด top-down ใช้ทิศก่อน snap กับความเร็วเพื่อให้เดินทแยงลื่น และใช้ทิศที่ snap แล้วกับภาพหรือ flip'),
    'Side-view: X-only velocity': ('TH', 'โหมด side-view เปลี่ยนความเร็วเฉพาะ X เพื่อคงแรงโน้มถ่วง และ snap ทั้งทิศความเร็วกับทิศภาพ'),
    'Click-to-move ability สำหรับ': ('EN', 'Moves a 2D character toward a clicked destination, stops within StoppingDistance or on SolidLayer, and updates display/facing direction.'),
    '2D Move Controller': ('EN', 'Hosts BaseMoveAbility2D implementations with observer input, optional rolling/jumping, ground checks and sprite or cutout rig configuration.'),
    'true เมื่อยังกระโดดได้': ('EN', 'Indicates whether another jump is allowed; check before applying jump force.'),
    'เรียกหลัง apply jump force': ('EN', 'Records a consumed jump after jump force is applied.'),
    'reset ตัวนับ': ('EN', 'Resets the jump counter when the grounded state becomes true.'),
    'Transform raw input before': ('TH', 'แปลงทิศ input ดิบก่อนกำหนดความเร็ว เช่น การเดินทแยงในโหมด perspective'),
    'Implemented by controllers': ('TH', 'สัญญาสำหรับ controller ที่กลับไป state เคลื่อนที่เริ่มต้นได้ เพื่อให้ ability เช่น Roll คืนการควบคุมเมื่อจบ'),
    'คลาส State สำหรับเล่น Animation': ('EN', 'Plays a temporary animation state and returns to the previous state when playback finishes.'),
    'ห่อ ObjectPool&lt;ElectricBullet&gt;': ('EN', 'Wraps the library ObjectPool for ElectricBullet; assign BulletPrefab and configure the capacity for the expected firing load.'),
    'กระสุนเดี่ยว (pellet)': ('EN', 'Moves a pellet with Transform.Translate and returns it to its pool after reaching the range or hitting Enemy, Ground or Wall.'),
    'วาด debug range': ('EN', 'Draws flashlight and shooting cones plus a dashed bullet guide in the Scene view; DrawAlways selects always-visible versus selected-only rendering.'),
    'broadcast ทุกครั้งที่ energy': ('EN', 'Broadcasts energy changes for consumers such as the energy HUD.'),
    'Map action type: Value': ('EN', 'Reads a Value/Vector2 input action and sends screen position whenever the mouse moves.'),
    'Map action type: Button': ('EN', 'Forwards a Button action, conventionally R, to request a scene restart.'),
    'ล้าง subscriber ทั้งหมด': ('EN', 'Clears all subscribers on scene unload to avoid callbacks to missing scene objects.'),
    'พลังงานร่วมระหว่างปืน': ('EN', 'Shares an energy reserve between gun spending and per-frame flashlight drain.'),
    'ใช้พลังงาน — คืน false': ('EN', 'Spends the requested energy only when enough remains; returns false without spending on failure.'),
    'ดูดพลังงานต่อเนื่อง': ('EN', 'Drains continuous energy use and clamps the reserve without a failure result.'),
    'ย้อมสี SpriteRenderer': ('EN', 'Tints SpriteRenderers in a cutout rig for hurt/death effects; renderers can be discovered in children or assigned explicitly.'),
    'เรียก hurt flash': ('EN', 'Triggers a hurt flash from an external caller, for example a debug action.'),
    'เรียก death effect': ('EN', 'Triggers the death effect from an external caller.'),
    'Roll/Dodge ability สำหรับ': ('EN', 'Implements a 2D roll with an entry impulse and fixed-update duration; cooldown ticks independently and IReturnableMovement restores default control.'),
    'Called every frame by MoveController2D': ('TH', 'ถูกเรียกทุกเฟรมโดย MoveController2D.Update ไม่ว่า ability ใดเป็น state ปัจจุบัน'),
    'Scene navigation API': ('TH', 'สัญญาคำสั่งเปลี่ยน Scene โดยให้ implementation รับผิดชอบ transition และการรับ event'),
    'Called by Unity when any serialized field': ('TH', 'Unity เรียกเมื่อแก้ serialized field ใน Inspector; ระหว่าง Play จะ clamp ค่าปัจจุบันให้ตรงช่วง min/max ใหม่'),
    'จำนวน Object ที่ว่างอยู่ใน Pool': ('EN', 'Number of entries currently waiting in the inactive queue.'),
    'ขีดจำกัดสูงสุดมาตรฐาน': ('EN', 'Standard pool capacity, before percentage overflow is applied.'),
    'คอนสตรักเตอร์เริ่มต้นระบบ Pool': ('EN', 'Initializes prefab, standard capacity, overflow percentage and parent container for this pool.'),
    'ดึง Object ออกมาใช้งาน': ('EN', 'Dequeues an inactive object or creates one within the hard limit, then tracks it as active.'),
    'ส่งคืน Object กลับเข้า Pool': ('EN', 'Removes a non-null item from active tracking and enqueues it for reuse.'),
    'เรียกคืน Object ทั้งหมดที่กำลัง Active': ('EN', 'Deactivates tracked live objects, enqueues them for reuse, then clears active tracking.'),
    'ทำลาย Object ทิ้งถาวร': ('EN', 'Removes an item from active tracking, decreases the created count and destroys its GameObject.'),
}


def signature(source):
    source = re.sub(r'(?m)^[ \t]*#(?:end)?region[^\n]*', '', source)
    return [m.group() for m in TOKEN.finditer(source)
            if not m.group().isspace() and not m.group().lstrip().startswith(
                ('//', '/*', '#region', '#endregion'))]


def comment_offsets(source):
    return {m.start() for m in TOKEN.finditer(source) if m.group().startswith('//')}


def section(body, title):
    # Keep code/list blocks intact; normalize prose and para tags around them.
    parts = re.split(r'(<(?:code|list)\b[\s\S]*?</(?:code|list)>)', body)
    result = []
    for part in parts:
        if re.match(r'<(?:code|list)\b', part):
            result.extend(part.strip().splitlines())
            continue
        for paragraph in re.split(r'</?para\s*>', part):
            paragraph = re.sub(r'\s+', ' ', paragraph).strip()
            if not paragraph:
                continue
            for header in HEADERS:
                if re.fullmatch(re.escape(header) + r'\s*:', paragraph, re.I):
                    paragraph = header + ' :'
                    break
            paragraph = re.sub(r'^\((TH|EN)\)\s*:\s*', r'(\1) : ', paragraph)
            if paragraph == title + ' :':
                continue
            if not re.match(r'^\((?:TH|EN)\)', paragraph) and not any(
                    paragraph == header + ' :' for header in HEADERS):
                language = 'TH' if re.search(r'[\u0e00-\u0e7f]', paragraph) else 'EN'
                paragraph = f'({language}) : {paragraph}'
            result.append('<para> ' + paragraph + ' </para>')
    joined = '\n'.join(result)
    if title == 'Summary':
        for key, (language, text) in TRANSLATIONS.items():
            if key in joined and f'({language}) :' not in joined:
                result.insert(0, f'<para> ({language}) : {text} </para>')
                break
    # Keep each language run in TH/EN order, without moving section headings,
    # code examples, lists or XML links to another section.
    ordered = []
    run = []
    for paragraph in result + ['']:
        if re.match(r'<para> \((?:TH|EN)\) : ', paragraph):
            run.append(paragraph)
        else:
            ordered.extend(sorted(run, key=lambda p: '(EN) :' in p))
            run = []
            if paragraph:
                ordered.append(paragraph)
    result = ordered
    return '<para> ' + title + ' : </para>\n' + '\n'.join(result)


def normalize_docs(source):
    offsets = comment_offsets(source)

    def replace(match):
        indent = match[1]
        if match.start() + len(indent) not in offsets:
            return match.group()
        content = '\n'.join(re.sub(r'^[ \t]*/// ?', '', line)
                            for line in match.group().splitlines())
        content = SUMMARY.sub(lambda m: '<summary>\n' + section(m[1], 'Summary') + '\n</summary>', content)
        content = RETURNS.sub(lambda m: '<returns>\n' + section(m[1], 'Return') + '\n</returns>', content)
        return '\n'.join(indent + '///' + (' ' + line if line else '') for line in content.splitlines())
    return DOC_BLOCK.sub(replace, source)


def fold_comments(source, pattern, label):
    offsets = comment_offsets(source)

    def replace(match):
        indent = match[1]
        if match.start() + len(indent) not in offsets:
            return match.group()
        previous = source[:match.start()].rstrip().split('\n')[-1].strip()
        if previous.startswith('#region Documentation'):
            return match.group()
        return (indent + '#region Documentation - ' + label + '\n' + match.group()
                + '\n' + indent + '#endregion')
    return pattern.sub(replace, source)


def validate_docs(source, path):
    offsets = comment_offsets(source)
    for match in DOC_BLOCK.finditer(source):
        if match.start() + len(match[1]) not in offsets:
            continue
        content = '\n'.join(re.sub(r'^[ \t]*/// ?', '', line)
                            for line in match.group().splitlines())
        if '<summary>' not in content:
            continue
        try:
            ET.fromstring('<documentation>' + content + '</documentation>')
        except ET.ParseError as error:
            line = source.count('\n', 0, match.start()) + 1
            raise RuntimeError(f'Invalid XML documentation: {path}:{line}: {error}') from error


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    pending = []
    for path in sorted(ROOT.rglob('*.cs')):
        if 'Third Party Library' in path.parts or path.name.endswith('.g.cs'):
            continue
        raw = path.read_bytes()
        bom = raw.startswith(b'\xef\xbb\xbf')
        original = raw.decode('utf-8-sig').replace('\r\n', '\n')
        updated = normalize_docs(original)
        updated = fold_comments(updated, DOC_BLOCK, 'XML')
        updated = fold_comments(updated, NOTE_BLOCK, 'Notes')
        if signature(original) != signature(updated):
            raise RuntimeError(f'C# token change detected: {path}')
        validate_docs(updated, path)
        if updated != original:
            pending.append(str(path.relative_to(ROOT)))
            if not args.check:
                newline = '\r\n' if b'\r\n' in raw else '\n'
                payload = updated.replace('\n', newline).encode('utf-8')
                path.write_bytes((b'\xef\xbb\xbf' if bom else b'') + payload)
    print(f'{len(pending)} files ' + ('need formatting' if args.check else 'formatted'))
    print('C# executable token comparison: passed')
    if args.check and pending:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
