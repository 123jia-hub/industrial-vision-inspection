import cv2
import numpy as np

def detect(path):
    """ 输入图片路径，返回缺陷值 """
    img = cv2.imread(path)
    if img is None:
        print("读取失败：", path)
        return []

    # 1.灰度化
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    # 2.高斯模糊，压掉噪声
    blur = cv2.GaussianBlur(gray, (5, 5), 0)

    # 3.阈值：亮于127变白，暗于127变黑 -> 工件变成大白块
    _, wp_bin = cv2.threshold(blur, 127, 255, cv2.THRESH_BINARY)

    # 4.找最大轮廓 = 工件本体
    contours, _ = cv2.findContours(wp_bin, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    if not contours:
        return []
    workpiece = max(contours, key=cv2.contourArea)

    # 5.生成工件蒙版（工件区域内255， 其余0）
    mask = np.zeros_like(gray)
    cv2.drawContours(mask, [workpiece], -1, 255, -1)

    # 6.收边：蒙版往里缩，去掉工件边缘的细线干扰
    kernel = np.ones((3, 3), np.uint8)
    mask = cv2.erode(mask, kernel, iterations=2)

    # 7.反向阈值：暗的东西变白（背景和缺陷都变白）
    _, dark = cv2.threshold(blur, 127, 255, cv2.THRESH_BINARY_INV)

    # 8.只保留工件内部的暗块 = 缺陷
    defects_bin = cv2.bitwise_and(dark, mask)

    # 9.开运算：先腐蚀再膨胀，抹掉零星噪点
    defects_bin = cv2.morphologyEx(defects_bin, cv2.MORPH_OPEN, kernel, iterations=1)

    # 10.找缺陷轮廓 + 面积过滤
    cnts, _ = cv2.findContours(defects_bin, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    results = []
    for c in cnts:
        area = cv2.contourArea(c)
        if area < 30:
            continue
        x, y, w, h = cv2.boundingRect(c)
        cx, cy = x + w // 2, y + h // 2
        typ = "scratch" if (w / h > 3 or h / w > 3) else "stain"
        results.append((cx, cy, typ, w, h))

    # 11.画框 + 存结果图
    out = img.copy()
    for i, (cx, cy, typ, w, h) in enumerate(results, 1):
        cv2.rectangle(out, (cx - w // 2, cy - h // 2), (cx + w // 2, cy + h // 2), (0, 0, 255), 2)
        cv2.putText(out, f"#{i}, {typ}", (cx - 30, cy - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
    cv2.imwrite(path.replace(".jpg", "_result.jpg"), out)

    return results

for name in ["samples/ok.jpg", "samples/scratch1.jpg", "samples/multi.jpg"]:
    r = detect(name)
    print(f"{name:22s} -> 缺陷 {len(r)} 个 {r}")


