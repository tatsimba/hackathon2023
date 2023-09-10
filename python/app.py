import logging
from flask import Flask, request, jsonify
from PIL import Image
from transformers import SegformerImageProcessor, AutoModelForSemanticSegmentation
import matplotlib.pyplot as plt
import torch.nn as nn
from flask_compress import Compress

app = Flask(__name__)
app.config["COMPRESS_REGISTER"] = True  # disable default compression of all eligible requests
app.config["COMPRESS_ALGORITHM"] = 'gzip'  # 	Supported compression algorithms
compress = Compress()
compress.init_app(app)

processor = SegformerImageProcessor.from_pretrained("mattmdjaga/segformer_b2_clothes")
model = AutoModelForSemanticSegmentation.from_pretrained("mattmdjaga/segformer_b2_clothes")

def main():
    logger.info(f"main starting up for app name {__name__}")
    logger.info("main finished starting up")

def getLogger():
    print("### initializing logger")
    logging.basicConfig(level = logging.DEBUG, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
    logger = logging.getLogger(__name__)
    logger.setLevel(logging.DEBUG)
    ch = logging.StreamHandler()
    formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
    ch.setFormatter(formatter)
    logger.addHandler(ch)

    logger.info("finished init logger")
    return logger


@app.route('/upload', methods=['POST'])
@compress.compressed()
def upload():
  if 'myImage' not in request.files:
    return 'No file uploaded', 400
  file = request.files['myImage']
  # file.save('im-received.jpg')
  image = Image.open(file.stream)

  pred_seg = get_segmentation_array(image)

  return jsonify({'msg': 'Image uploaded successfully', 'imageSegmentationLabels': pred_seg.tolist()})

# Create segmentation array
# Image object should be of type PIL.Image
# Returns torch.Tensor
# 0 - Background
# 1 - Hat
# 2 - Hair
# 3 - Sunglasses
# 4 - Upper-clothes
# 5 - Skirt
# 6 - Pants
# 7 - Dress
# 8 - Belt
# 9 - Left-shoe
# 10 - Right-shoe
# 11 - Face
# 12 - Left-leg
# 13 - Right-leg
# 14 - Left-arm
# 15 - Right-arm
# 16 - Bag
# 17 - Scarf
def get_segmentation_array(img):
  inputs = processor(images=img, return_tensors="pt")
  outputs = model(**inputs)
  logits = outputs.logits.cpu()
  upsampled_logits = nn.functional.interpolate(
    logits,
    size=img.size[::-1],
    mode="bilinear",
    align_corners=False,
  )

  pred_seg = upsampled_logits.argmax(dim=1)[0]
  print(type(pred_seg))
  return pred_seg.numpy()


logger = getLogger()
main()

if __name__ == '__main__':
  try:
    logger.info('starting app.run()')
    # print("### starting app.run()")
    app.run(debug=True)
    # print("### completed app.run()")
    logger.info('completed app.run()')
  except Exception as e:
    # print("### failed to start up: " + + getattr(e, 'message', repr(e)))
    logger.error("failed to start up: " + + getattr(e, 'message', repr(e)))