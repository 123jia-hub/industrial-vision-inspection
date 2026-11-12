from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
import cv2
import numpy as np
import base64

app = FastAPI(title="视觉检测服务")

# ==== 可调参数：现场调参 ====
THRESH_VALUE = 127      # 明暗分隔阈值
MIN_AREA = 30           # 像素面积小于此值当噪声丢弃
ERODE_ITER = 2          # 工件蒙版内缩次数（去边缘干扰）
ASPECT_RATIO = 3        # 长宽比阈值，超过判为划痕
KERNEL = np.ones((3, 3), np.uint8)

def analyze(img):
    """ 核心检测：输入BGR图，返回缺陷列表[(cx, cy, type, w, h)]"""
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)

    # 1.分离工件
    _, wp_bin = cv2.threshold(blur, THRESH_VALUE, 255, cv2.THRESH_BINARY)
    contours, _ = cv2.findContours(wp_bin, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    if not contours:
        return []
    workpiece = max(contours, key=cv2.contourArea)

    # 2.工件蒙版 + 内缩
    mask = np.zeros_like(gray)
    cv2.drawContours(mask, [workpiece], -1, 255, -1)
    mask = cv2.erode(mask, KERNEL, iterations=ERODE_ITER)

    # 3.在工件内部找暗块 = 缺陷
    _, dark = cv2.threshold(blur, THRESH_VALUE, 255, cv2.THRESH_BINARY_INV)
    defects_bin = cv2.bitwise_and(dark, mask)
    defects_bin = cv2.morphologyEx(defects_bin, cv2.MORPH_OPEN, KERNEL, iterations=1)

    # 4.面积过滤 + 分类
    cnts, _ = cv2.findContours(defects_bin, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    defects = []
    for c in cnts:
        if cv2.contourArea(c) < MIN_AREA:
            continue
        x, y, w, h = cv2.boundingRect(c)
        typ = "scratch" if (w / h > ASPECT_RATIO or h / w > ASPECT_RATIO) else "stain"
        defects.append((x + w // 2, y + h // 2, typ, w, h))
    return defects


@app.post("/detect")
async def detect(image: UploadFile = File(...)):

    '''
        接收图片，返回检测结果 + 标注后的图片（base64）
    '''

    # 读取图片字节并解码
    contents = await image.read()
    nparr = np.frombuffer(contents, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    # 如果不是图片 -> 按 C# 约定的 -1 返回
    if img is None:
        return JSONResponse({
            "IsPassed": False,
            "DefectCount": -1,
            "defects": [{"x": 0, "y": 0, "type": "图片解码失败"}]
        })

    defects = analyze(img)

    out = img.copy()
    for i, (cx, cy, typ, w, h) in enumerate(defects, 1):
        cv2.rectangle(out, (cx - w // 2, cy - h // 2), (cx + w // 2, cy + h // 2), (0, 0, 255), 2)
        cv2.putText(out, f"#{i} {typ}", (cx - 30, cy - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

    # 编码成base64字符串
    _, buf = cv2.imencode('.jpg', out)
    annotated_b64 = base64.b64encode(buf.tobytes()).decode('utf-8')

    result = {
        "IsPassed": len(defects) == 0,
        "DefectCount": len(defects),
        "defects": [{"x": d[0], "y": d[1], "type": d[2]} for d in defects],
        "annotated_image": annotated_b64
    }

    return JSONResponse(result)

@app.get("/health")
def health():
    return {"status": "ok", "message": "视觉服务运行中"}

if __name__ == '__main__':
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)






