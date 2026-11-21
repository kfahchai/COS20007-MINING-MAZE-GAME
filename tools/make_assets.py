"""Generates every image and sound used by Mining-Maze.

All art is original pixel art drawn from code (ASCII sprites + procedural
textures), so the game has no third-party image or audio assets.

    python tools/make_assets.py
"""
import math
import os
import random
import struct
import wave

from PIL import Image, ImageDraw

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Resources")
IMG = os.path.join(ROOT, "images")
SND = os.path.join(ROOT, "sounds")
os.makedirs(IMG, exist_ok=True)
os.makedirs(SND, exist_ok=True)


def hexc(h, a=255):
    h = h.lstrip("#")
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


# ----------------------------------------------------------------------------
# ASCII sprite renderer
# ----------------------------------------------------------------------------
def sprite(rows, palette, size, scale):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    for y, row in enumerate(rows):
        assert len(row) <= size, (row, len(row))
        for x, ch in enumerate(row):
            if ch != ".":
                px[x, y] = palette[ch]
    return img.resize((size * scale, size * scale), Image.NEAREST)


def save(img, name):
    img.save(os.path.join(IMG, name))


OUT = hexc("#1b1b1b")

# --- Player: an original miner with a yellow hard hat and head lamp ----------
MINER = [
    "........KKKKK.......",
    "......KKYYYYYKK.....",
    ".....KYYYYYYYYLK....",
    ".....KYYYYYYYYLLK...",
    "....KyyyyyyyyyyyK...",
    ".....KHHSSSSSSSK....",
    ".....KHSSSSSESSK....",
    ".....KHSSSSSSSSSK...",
    "......KSSSSssSSK....",
    ".......KKSSSSKK.....",
    ".....KRRKBBBBKRRK...",
    "....KRRRBBBBBBRRRK..",
    "....KRRKBBbbBBKRRK..",
    "....KGGKBBBBBBKGGK..",
    ".....KKKBBBBBBKKK...",
    ".......KBBBKBBBK....",
    ".......KBBK.KBBK....",
    "......KWWWK.KWWWK...",
    "......KWWWWK.KWWWWK.",
    ".......KKKK...KKKK..",
]
MINER_PAL = {
    "K": OUT, "Y": hexc("#f2c418"), "y": hexc("#c99a0c"), "L": hexc("#fff7a8"),
    "S": hexc("#f1c27d"), "s": hexc("#d39b5d"), "E": OUT, "H": hexc("#5b3a1e"),
    "B": hexc("#2f5fbf"), "b": hexc("#1f3f86"), "R": hexc("#d4552f"),
    "G": hexc("#7a4a24"), "W": hexc("#4a2b14"),
}
player = sprite(MINER, MINER_PAL, 20, 2)
save(player, "player_right.png")
save(player.transpose(Image.FLIP_LEFT_RIGHT), "player_left.png")
save(sprite(MINER, MINER_PAL, 20, 4), "player_big.png")

# --- Ghost monster -----------------------------------------------------------
GHOST = [
    "......KKKKKKK.......",
    "....KKWWWWWWWKK.....",
    "...KWWWWWWWWWWWK....",
    "..KWWWWWWWWWWWWWK...",
    "..KWWKKWWWWKKWWWK...",
    ".KWWWKKWWWWKKWWWWK..",
    ".KWWWWWWWWWWWWWWWK..",
    ".KWWWWWWKKKWWWWWWK..",
    ".KWWWWWKggKWWWWWWK..",
    ".KWWWWWWKKWWWWWWWK..",
    ".KWWWWWWWWWWWWWWWK..",
    ".KWWWWWWWWWWWWWgWK..",
    ".KWWWWWWWWWWWWWgWK..",
    ".KWWWWWWWWWWWWggWK..",
    ".KWWWWWWWWWWWWWWWK..",
    ".KWWWWWWWWWWWWWWWK..",
    ".KWWKWWWWKWWWWKWWK..",
    ".KWK.KWWK.KWWK.KWK..",
    "..K...KK...KK...K...",
]
GHOST_PAL = {"K": hexc("#2a2a3a"), "W": hexc("#f4f6ff", 235), "g": hexc("#c9cde0", 235)}
save(sprite(GHOST, GHOST_PAL, 20, 2), "ghost.png")
save(sprite(GHOST, GHOST_PAL, 20, 4), "ghost_big.png")

# --- Backpack (inventory) icon ----------------------------------------------
PACK = [
    "",
    "......KKKKKK........",
    ".....K......K.......",
    "....KKKKKKKKKK......",
    "...KPPPPPPPPPPK.....",
    "..KPPPPPPPPPPPPK....",
    "..KPPKKKKKKKKPPK....",
    "..KPPKppppppKPPK....",
    "..KPPKppppppKPPK....",
    "..KPPKKKKKKKKPPK....",
    "..KPPPPPPPPPPPPK....",
    "..KPPPPPPYPPPPPK....",
    "..KPPPPPPPPPPPPK....",
    "..KPPPPPPPPPPPPK....",
    "...KKKKKKKKKKKK.....",
]
save(sprite(PACK, {"K": OUT, "P": hexc("#3e8e7e"), "p": hexc("#2c6b5f"), "Y": hexc("#f2c418")}, 20, 2), "backpack.png")

# --- 16x16 item icons (drawn at 2x = 32 px) ------------------------------------
ICONS = {
    "coal": ([
        "", "",
        ".....KKKK.......",
        "....KccccKK.....",
        "...KccCcccccK...",
        "..KcccccCccccK..",
        "..KcCccccccccK..",
        ".KccccccCcccccK.",
        ".KcccCcccccCccK.",
        ".KcccccccccccK..",
        "..KcccCcccccK...",
        "...KKccccccK....",
        ".....KKKKKK.....",
    ], {"K": hexc("#0d0d0d"), "c": hexc("#2b2b2b"), "C": hexc("#5a5a5a")}),
    "iron": None, "gold": None,
    "diamond": ([
        "", "",
        "....KKKKKKKK....",
        "...KbBBwBBBbK...",
        "..KbBBwBBBBBbK..",
        ".KKKKKKKKKKKKKK.",
        ".KbbBBBBBBBBbbK.",
        "..KbbBBBBBBbbK..",
        "...KbbBBBBbbK...",
        "....KbbBBbbK....",
        ".....KbBBbK.....",
        "......KbbK......",
        ".......KK.......",
    ], {"K": hexc("#0b3c4d"), "b": hexc("#1fa7c9"), "B": hexc("#6fe3f5"), "w": hexc("#ffffff")}),
    "burger": ([
        "", "",
        ".....KKKKKK.....",
        "...KKbbbbbbKK...",
        "..KbbybbbbybbK..",
        "..KbbbbbbybbbK..",
        "..KKKKKKKKKKKK..",
        "..KggggggggggK..",
        "..KyyyyyyyyyyK..",
        "..KmmmmmmmmmmK..",
        "..KKKKKKKKKKKK..",
        "..KbbbbbbbbbbK..",
        "...KKKKKKKKKK...",
    ], {"K": OUT, "b": hexc("#e0a040"), "y": hexc("#ffd23f"), "g": hexc("#5fbf3f"), "m": hexc("#7a3b1c")}),
    "pizza": ([
        "",
        "..KKKKKKKKKKKK..",
        "..KccccccccccK..",
        "..KyyyyyyyyyyK..",
        "...KyyrryyyyK...",
        "...KyyrryyrrK...",
        "....KyyyyyrrK...",
        "....KyyyyyyK....",
        ".....KyrryK.....",
        ".....KyrryK.....",
        "......KyyK......",
        "......KyyK......",
        ".......KK.......",
    ], {"K": OUT, "c": hexc("#d08a3a"), "y": hexc("#ffd75e"), "r": hexc("#d23b2a")}),
    "soda": ([
        "",
        "......KKKK......",
        ".....KssssK.....",
        "....KKKKKKKK....",
        "....KrrrrrrK....",
        "....KrwwrrrK....",
        "....KrwrrrrK....",
        "....KrrwwrrK....",
        "....KrrrrwrK....",
        "....KrwwwrrK....",
        "....KrrrrrrK....",
        "....KrrrrrrK....",
        "....KKKKKKKK....",
        ".....KssssK.....",
        "......KKKK......",
    ], {"K": OUT, "s": hexc("#c8ccd4"), "r": hexc("#2f7fd8"), "w": hexc("#ffffff")}),
    "bomb": ([
        "..........Y.....",
        ".........YoY....",
        "........KfY.....",
        "........Kf......",
        ".......KK.......",
        ".....KKKKKK.....",
        "....KkkkkkkK....",
        "...KkwkkkkkkK...",
        "...KkwkkkkkkK...",
        "...KkkkkkkkkK...",
        "...KkkkkkkkkK...",
        "...KkkkkkkkkK...",
        "....KkkkkkkK....",
        ".....KKKKKK.....",
    ], {"K": hexc("#000000"), "k": hexc("#2e2e3a"), "w": hexc("#8a8aa0"), "f": hexc("#b88a5a"),
        "Y": hexc("#ffd23f"), "o": hexc("#ff7a1a")}),
}
INGOT = [
    "", "", "", "",
    "......KKKKKKKK..",
    ".....KwwwwwwwK..",
    "....KwwwwwwwKdK.",
    "...KKKKKKKKKddK.",
    "...KmmmmmmmmKdK.",
    "...KmmmmmmmmKK..",
    "...KKKKKKKKKK...",
]
ICONS["iron"] = (INGOT, {"K": OUT, "w": hexc("#eeeeee"), "m": hexc("#b8b8b8"), "d": hexc("#8a8a8a")})
ICONS["gold"] = (INGOT, {"K": OUT, "w": hexc("#fff08a"), "m": hexc("#f2c418"), "d": hexc("#c99a0c")})
for name, (rows, pal) in ICONS.items():
    save(sprite(rows, pal, 16, 2), "item_%s.png" % name)

# Pickaxes: same shape, head colour depends on the tier
PICK = [
    "",
    "....XXXXXX......",
    "..XXxxxxxXXX....",
    ".XXX....KhXXX...",
    ".XX....KhK.XXX..",
    ".X....KhK...XX..",
    "......KhK....X..",
    ".....KhK........",
    "....KhK.........",
    "...KhK..........",
    "..KhK...........",
    ".KhK............",
    ".KK.............",
]
for tier, head, light in [("iron", "#9aa0a8", "#d9dde2"), ("gold", "#e0b415", "#fff08a"),
                          ("diamond", "#25b8d8", "#a8f2ff")]:
    pal = {"K": OUT, "h": hexc("#8a5a2b"), "X": hexc(head), "x": hexc(light)}
    save(sprite(PICK, pal, 16, 2), "item_pickaxe_%s.png" % tier)

# ----------------------------------------------------------------------------
# Procedural 20x20 tiles (drawn at 2x = 40 px)
# ----------------------------------------------------------------------------
def noise_tile(base, dark, light, seed, density=0.18):
    rnd = random.Random(seed)
    img = Image.new("RGBA", (20, 20), hexc(base))
    px = img.load()
    for y in range(20):
        for x in range(20):
            r = rnd.random()
            if r < density / 2:
                px[x, y] = hexc(dark)
            elif r < density:
                px[x, y] = hexc(light)
    return img


def cobble(base, edge, light, seed):
    rnd = random.Random(seed)
    img = Image.new("RGBA", (20, 20), hexc(edge))
    d = ImageDraw.Draw(img)
    for (x, y, w, h) in [(0, 0, 9, 6), (10, 0, 10, 7), (0, 7, 6, 6), (7, 8, 7, 5), (15, 8, 5, 6),
                         (0, 14, 10, 6), (11, 14, 9, 6)]:
        d.rectangle([x + 1, y + 1, x + w - 1, y + h - 1], fill=hexc(base))
        d.line([x + 1, y + 1, x + w - 2, y + 1], fill=hexc(light))
    px = img.load()
    for _ in range(14):
        x, y = rnd.randrange(20), rnd.randrange(20)
        if px[x, y] == hexc(base):
            px[x, y] = hexc(light)
    return img


def up2(img):
    return img.resize((40, 40), Image.NEAREST)


save(up2(noise_tile("#c8875a", "#a0663f", "#dba377", 1)), "tile_dirt.png")
save(up2(cobble("#8fa3c0", "#5f7290", "#b4c4dc", 2)), "tile_wall.png")


def ore(colour, light, seed):
    rnd = random.Random(seed)
    img = cobble("#9a9a9a", "#6e6e6e", "#bdbdbd", seed)
    px = img.load()
    for cx, cy in [(4, 4), (13, 3), (8, 10), (15, 12), (4, 15), (11, 16)]:
        cx += rnd.randrange(-1, 2)
        cy += rnd.randrange(-1, 2)
        for dx, dy in [(0, 0), (1, 0), (0, 1), (1, 1)]:
            px[min(19, cx + dx), min(19, cy + dy)] = hexc(colour)
        px[cx, cy] = hexc(light)
    return up2(img)


save(ore("#1e1e1e", "#4a4a4a", 11), "tile_coal.png")
save(ore("#d8b89a", "#f3e2d0", 12), "tile_iron.png")
save(ore("#f2c418", "#fff08a", 13), "tile_gold.png")
save(ore("#2fd0ef", "#b8f6ff", 14), "tile_diamond.png")

# ----------------------------------------------------------------------------
# Backgrounds
# ----------------------------------------------------------------------------
def menu_background():
    W, H, S = 250, 150, 4
    img = Image.new("RGBA", (W, H))
    d = ImageDraw.Draw(img)
    top, bottom = (71, 168, 239), (150, 214, 255)
    for y in range(110):
        t = y / 110
        d.line([0, y, W, y], fill=tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)) + (255,))

    def cloud(x, y, s):
        for (dx, dy, r) in [(0, 4, 5), (6, 0, 7), (14, 2, 6), (20, 5, 4)]:
            d.ellipse([x + dx * s / 6, y + dy * s / 6, x + (dx + 2 * r) * s / 6, y + (dy + 2 * r) * s / 6],
                      fill=(245, 251, 255, 255))
        d.rectangle([x + 2, y + 12 * s / 6, x + 26 * s / 6, y + 14 * s / 6], fill=(205, 230, 248, 255))
    for (x, y, s) in [(18, 22, 7), (66, 10, 5), (150, 18, 8), (205, 34, 5), (100, 60, 4)]:
        cloud(x, y, s)
    for bx in [25, 140, 200]:
        d.ellipse([bx, 96, bx + 24, 116], fill=(46, 130, 46, 255))
        d.ellipse([bx + 6, 92, bx + 20, 106], fill=(66, 160, 60, 255))
    d.rectangle([0, 108, W, 116], fill=(108, 194, 74, 255))
    rnd = random.Random(5)
    for x in range(0, W, 3):
        d.polygon([(x, 116), (x + 3, 116), (x + 1.5, 119 + rnd.randrange(2))], fill=(108, 194, 74, 255))
    d.rectangle([0, 118, W, 134], fill=(138, 75, 43, 255))
    px = img.load()
    for y in range(118, 134):
        for x in range(W):
            r = rnd.random()
            if r < 0.12:
                px[x, y] = (110, 58, 32, 255)
            elif r < 0.18:
                px[x, y] = (165, 98, 60, 255)
    d.rectangle([0, 135, W, H], fill=(0, 0, 0, 255))
    return img.resize((W * S, H * S), Image.NEAREST)


save(menu_background(), "bg_menu.png")


def wood_board(w, h):
    img = Image.new("RGBA", (w // 2, h // 2), hexc("#b07a45"))
    d = ImageDraw.Draw(img)
    rnd = random.Random(8)
    for y in range(0, h // 2, 8):
        d.line([0, y, w, y], fill=hexc("#7a4f28"))
        for _ in range(6):
            x = rnd.randrange(w // 2)
            d.line([x, y + 3, x + rnd.randrange(4, 12), y + 3], fill=hexc("#9a6638"))
    d.rectangle([0, 0, w // 2 - 1, h // 2 - 1], outline=hexc("#5a3818"))
    return img.resize((w, h), Image.NEAREST)


save(wood_board(240, 150), "panel_wood.png")


def parchment(w, h):
    img = Image.new("RGBA", (w // 2, h // 2), hexc("#f3d9a4"))
    px = img.load()
    rnd = random.Random(9)
    for y in range(h // 2):
        for x in range(w // 2):
            edge = min(x, y, w // 2 - 1 - x, h // 2 - 1 - y)
            if edge < 3 or rnd.random() < 0.05:
                px[x, y] = hexc("#d9b77a") if edge >= 1 else hexc("#b8925a")
    return img.resize((w, h), Image.NEAREST)


save(parchment(220, 250), "panel_parchment.png")

# ----------------------------------------------------------------------------
# Sounds: tiny chiptune effects + a looping theme (8-bit square/noise waves)
# ----------------------------------------------------------------------------
RATE = 22050


def write_wav(name, samples):
    with wave.open(os.path.join(SND, name), "w") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s)) * 12000)) for s in samples))


def square(freq, dur, vol=0.6, duty=0.5, slide=0.0):
    out = []
    n = int(RATE * dur)
    phase = 0.0
    for i in range(n):
        f = freq + slide * i / n
        phase = (phase + f / RATE) % 1.0
        env = 1.0 - i / n
        out.append(vol * env * (1 if phase < duty else -1))
    return out


def noise(dur, vol=0.5, seed=1):
    rnd = random.Random(seed)
    n = int(RATE * dur)
    return [vol * (1 - i / n) * (rnd.random() * 2 - 1) for i in range(n)]


write_wav("mine.wav", noise(0.07, 0.6, 1))
write_wav("break.wav", noise(0.18, 0.7, 2) + square(110, 0.05, 0.3))
write_wav("collect.wav", square(880, 0.06, 0.5) + square(1320, 0.10, 0.5))
write_wav("buy.wav", square(660, 0.05) + square(990, 0.05) + square(1320, 0.12))
write_wav("click.wav", square(520, 0.04, 0.4))
write_wav("hurt.wav", square(300, 0.25, 0.6, 0.3, -200))
write_wav("steal.wav", square(900, 0.25, 0.5, 0.25, -700))
write_wav("bomb.wav", noise(0.45, 0.9, 3))
write_wav("eat.wav", square(400, 0.06) + square(500, 0.06) + square(650, 0.1))
write_wav("win.wav", sum([square(f, 0.14) for f in [523, 659, 784, 1046]], []) + square(1046, 0.35))
write_wav("lose.wav", sum([square(f, 0.18) for f in [392, 330, 262]], []) + square(196, 0.45))
write_wav("ghost.wav", square(220, 0.6, 0.4, 0.5, 120))

NOTES = {"C4": 262, "D4": 294, "E4": 330, "F4": 349, "G4": 392, "A4": 440, "B4": 494,
         "C5": 523, "D5": 587, "E5": 659, "G5": 784, "-": 0}
MELODY = ("E4 G4 C5 G4 A4 G4 E4 - D4 E4 G4 E4 D4 C4 D4 - "
          "E4 G4 C5 D5 E5 D5 C5 - A4 G4 E4 G4 D4 E4 C4 -").split()
BASS = "C4 C4 A4 A4 F4 F4 G4 G4".split()
music = []
beat = 0.2
for i, note in enumerate(MELODY):
    lead = square(NOTES[note], beat, 0.28, 0.25) if NOTES[note] else [0.0] * int(RATE * beat)
    b = square(NOTES[BASS[(i // 4) % len(BASS)]] / 2, beat, 0.18, 0.5)
    music += [lead[k] + b[k] for k in range(len(lead))]
write_wav("theme.wav", music * 2)
print("assets written to", ROOT)
