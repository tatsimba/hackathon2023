import logging
from flask import Flask, request, jsonify, render_template
from PIL import Image
from transformers import SegformerImageProcessor, AutoModelForSemanticSegmentation
from transformers import BlipProcessor, BlipForConditionalGeneration
import torch.nn as nn
from flask_compress import Compress
from flask_cors import CORS
import numpy as np
import concurrent.futures

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
     'imageSegmentationLabels': pred_seg.tolist(),
     'boxes': boxes
  })


@app.route('/captions', methods=['POST'])
def captions():
  if 'image' not in request.files:
    return 'No file uploaded', 400
  file = request.files['image']
  # file.save('im-received.jpg')
  image = Image.open(file.stream)

  pred_seg = get_segmentation_array(image)
  boxes = get_clothing_boxes(pred_seg)
  captions = get_captions_in_parallel(boxes, image)

  return jsonify({
     'boxes': boxes,
     'captions': captions
  })


@app.route('/segmentation', methods=['POST'])
@compress.compressed()
def segmentation():
  if 'image' not in request.files:
    return 'No file uploaded', 400
  file = request.files['image']
  # file.save('im-received.jpg')
  image = Image.open(file.stream)

  pred_seg = get_segmentation_array(image)
  boxes = get_clothing_boxes(pred_seg)

  return jsonify({
     'boxes': boxes,
     'imageSegmentationLabels': pred_seg.tolist()
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
    left_shoe_box = get_bbox_for_label(seg_matrix_numpy, [left_shoe_label], 'left_shoe')
    right_shoe_box = get_bbox_for_label(seg_matrix_numpy, [right_shoe_label], 'right_shoe')
    upper_clothing_box = get_bbox_for_label(seg_matrix_numpy, [dress_label, shirt_label], 'upper_clothing')

    if(dress_box is not None and shirt_box is not None):
        dress_area = dress_box['w'] * dress_box['h']
        shirt_area = shirt_box['w'] * shirt_box['h']
        if(dress_area > shirt_area):
            shirt_box = None        
        else:
            dress_box = None

    return list(filter(lambda x: x is not None, 
        [shirt_box, pants_box, dress_box, hat_box, belt_box, shoes_box, upper_clothing_box, left_shoe_box, right_shoe_box]))

def first(iterable, condition = lambda x: True):
    items = list(filter(condition, iterable))
    return items[0] if len(items) > 0 else None

caption_processor = BlipProcessor.from_pretrained("Salesforce/blip-image-captioning-large")
caption_model = BlipForConditionalGeneration.from_pretrained("Salesforce/blip-image-captioning-large")

def get_caption(cropped_image, prompt, preprocessed_image=None):
    if(cropped_image is None):
        return None
    inputs = caption_processor(cropped_image, prompt, return_tensors="pt") if preprocessed_image is None else preprocessed_image
    out = caption_model.generate(**inputs)
    text = caption_processor.decode(out[0], skip_special_tokens=True)
    return text

def get_cropped_image(image, boxes, label, show=False):
    box = first(boxes, lambda x: x['label'] == label)
    if(box is None):
        return None
    cropped_image = image.crop((box['x'], box['y'], box['x'] + box['w'], box['y'] + box['h']))
    if(show):
        plt.imshow(cropped_image)
        plt.show()
    return cropped_image


def get_captions_in_parallel(boxes, image, show=False):
    if(show):
        plt.imshow(image)
        plt.show()
    
    cropped_shoes = get_cropped_image(image, boxes, 'shoes', show)
    cropped_upper_clothing = get_cropped_image(image, boxes, 'upper_clothing', show)
    cropped_pants = get_cropped_image(image, boxes, 'pants', show)
    cropped_hat = get_cropped_image(image, boxes, 'hat', show)
    cropped_belt = get_cropped_image(image, boxes, 'belt', show)
    
    # Define a list of caption tasks to run in parallel
    caption_tasks = [
        (cropped_shoes, 'The shoes colors are '),
        (cropped_shoes, 'The shoes are '),
        (cropped_upper_clothing, 'The upper clothing color is '),
        (cropped_upper_clothing, 'The upper clothing is '),
        (cropped_pants, 'The pants color is '),
        (cropped_pants, 'The pants are '),
        (cropped_hat, 'The hat color is '),
        (cropped_hat, 'The hat is '),
        (cropped_belt, 'The belt color is '),
        (cropped_belt, 'The belt is '),
        (image, 'The person is wearing '),
        (image, 'The person is '),
    ]

    captions = []

    # Use ThreadPoolExecutor to run caption tasks in parallel
    with concurrent.futures.ThreadPoolExecutor() as executor:
        caption_results = [executor.submit(get_caption, cropped_image, prompt) for cropped_image, prompt in caption_tasks]
    
    # Retrieve the results from the caption tasks
    for result in concurrent.futures.as_completed(caption_results):
        caption = result.result()
        if caption is not None:
            captions.append(caption)

    return captions


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