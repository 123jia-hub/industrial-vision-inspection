import os
import cv2
import numpy as np
from main import analyze        # 复用线上同一套算法，避免两套逻辑漂移

PART_DIR = "samples/parts"
TOTAL = 20          # 样本总数
PASS_RATE = 0.6     # 合格比例（演示用；真实产线一般 0.95+）

def make_canvas():
    """造一张空工件图：深灰背景 + 亮灰矩形工件"""
    img = np.full((600, 800, 3), 40, dtype=np.uint8)
    cv2.rectangle(img, (150, 120), (650, 480), (230, 230, 230), -1)
    noise = np.random.normal(0, 4, img.shape)
    img = np.clip(img.astype(np.int16) + noise, 0, 255).astype(np.uint8)
    return img

def rand_center(used, min_dist=90):
    """在工件内部随机取一个缺陷中心，和已有缺陷保持间距，避免粘连"""
    while True:
        x = np.random.randint(220, 580)
        y = np.random.randint(190, 410)
        if all((x - ux) ** 2 + (y - uy) ** 2 >= min_dist ** 2 for ux, uy in used):
            used.append((x, y))
            return x, y

def add_defect(img, x, y):
    """随机画一个缺陷：划痕（细长）或污点（圆形）"""
    if np.random.rand() < 0.6:
        length = np.random.randint(60, 111)          # 中心到中心整长
        ang = np.random.uniform(0, np.pi)
        dx = int(length * np.cos(ang) / 2)
        dy = int(length * np.sin(ang) / 2)
        cv2.line(img, (x - dx, y - dy), (x + dx, y + dy),
                 (20, 20, 20), np.random.randint(3, 6))
    else:
        cv2.circle(img, (x, y), np.random.randint(9, 15), (20, 20, 20), -1)

def make_part(is_ok):
    """造一件工件，返回 (图, 预期缺陷数)"""
    img = make_canvas()
    if is_ok:
        return img, 0
    used = []
    n = np.random.randint(1, 5)                      # 1~4 个缺陷
    for _ in range(n):
        x, y = rand_center(used)
        add_defect(img, x, y)
    return img, n

os.makedirs(PART_DIR, exist_ok=True)
np.random.seed(20261108)                             # 固定种子，录 demo 可复现

ok_count = int(round(TOTAL * PASS_RATE))
ok_indices = set(np.random.choice(np.arange(1, TOTAL + 1), ok_count, replace=False))

for i in range(1, TOTAL + 1):
    img, expected = make_part(i in ok_indices)
    path = os.path.join(PART_DIR, "part_%02d.jpg" % i)
    cv2.imwrite(path, img)

    actual = len(analyze(img))                       # 用线上算法复核真值
    flag = "OK" if actual == expected else "!! 不一致"
    print("part_%02d.jpg  预期 %d  实际 %d  %s" % (i, expected, actual, flag))