import logging
from flask import Flask, request, jsonify, render_template
from PIL import Image
from transformers import SegformerImageProcessor, AutoModelForSemanticSegmentation
import torch.nn as nn
from flask_compress import Compress
from flask_cors import CORS
import numpy as np

app = Flask(__name__)
app.config["COMPRESS_REGISTER"] = True  # disable default compression of all eligible requests
app.config["COMPRESS_ALGORITHM"] = 'gzip'  # 	Supported compression algorithms
compress = Compress()
compress.init_app(app)
CORS(app)

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


@app.route('/')
def index():
   print('Request for index page received')
   return render_template('index.html')

@app.route('/upload', methods=['POST'])
@compress.compressed()
def upload():
  if 'image' not in request.files:
    return 'No file uploaded', 400
  file = request.files['image']
  # file.save('im-received.jpg')
  image = Image.open(file.stream)

  pred_seg = get_segmentation_array(image)
  boxes = get_clothing_boxes(pred_seg)

  return jsonify({
     'msg': 'Image uploaded successfully',
    #  'imageSegmentationLabels': pred_seg.tolist(),
     'boxes': boxes
  })

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

# Given a matrix of segmentation values, return the bounding box of the shirt
# Returns a tuple of (upper_x, upper_y, width, height)
def get_bbox_for_label(seg_matrix_numpy, labels, label_name):
    mask = seg_matrix_numpy == labels[0]
    for label in labels[1:]:
        mask = mask | (seg_matrix_numpy == label)
    indices = np.argwhere(mask)
    if(len(indices) == 0):
        return None
    x_min = np.min(indices[:, 1]).item()
    x_max = np.max(indices[:, 1]).item()
    y_min = np.min(indices[:, 0]).item()
    y_max = np.max(indices[:, 0]).item()
    width = abs(x_max - x_min)
    height = abs(y_max - y_min)
    return {
        'label': label_name,
        'x': x_min,
        'y': y_min,
        'w': width,
        'h': height
    }

def get_clothing_boxes(seg_matrix_numpy):
    shirt_label = 4
    pants_label = 6
    dress_label = 7
    hat_label = 1
    belt_label = 8
    left_shoe_label = 9
    right_shoe_label = 10
    shirt_box = get_bbox_for_label(seg_matrix_numpy, [shirt_label], 'shirt')
    pants_box = get_bbox_for_label(seg_matrix_numpy, [pants_label], 'pants')
    dress_box = get_bbox_for_label(seg_matrix_numpy, [dress_label], 'dress')
    hat_box = get_bbox_for_label(seg_matrix_numpy, [hat_label], 'hat')
    belt_box = get_bbox_for_label(seg_matrix_numpy, [belt_label], 'belt')
    shoes_box = get_bbox_for_label(seg_matrix_numpy, [left_shoe_label, right_shoe_label], 'shoes')
    upper_clothing_box = get_bbox_for_label(seg_matrix_numpy, [dress_label, shirt_label], 'upper_clothing')

    if(dress_box is not None and shirt_box is not None):
        dress_area = dress_box['w'] * dress_box['h']
        shirt_area = shirt_box['w'] * shirt_box['h']
        if(dress_area > shirt_area):
            shirt_box = None        
        else:
            dress_box = None

    return list(filter(lambda x: x is not None, [shirt_box, pants_box, dress_box, hat_box, belt_box, shoes_box, upper_clothing_box]))


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