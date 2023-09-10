from azure.storage.blob import BlobServiceClient
from flask import Flask, request, jsonify
from PIL import Image

app = Flask(__name__)

# Azure Blob Storage connection string
connection_string = "your_connection_string"

@app.route('/upload', methods=['POST'])
def upload():
     
    if 'myImage' not in request.files:
      return 'No file uploaded', 400
    
    file = request.files['myImage']
    file.save('im-received.jpg')
    img = Image.open(file.stream)
    return jsonify({'msg': 'Image uploaded successfully', 'size': [img.width, img.height]})
    
    
    # 
    
    # # Create a BlobServiceClient object using the connection string
    # blob_service_client = BlobServiceClient.from_connection_string(connection_string)
    
    # # Get a reference to the container where you want to upload the image
    # container_client = blob_service_client.get_container_client("your_container_name")
    
    # # Upload the image to Azure Blob Storage
    # blob_client = container_client.upload_blob(name=file.filename, data=file.stream)
    
    

if __name__ == '__main__':
    app.run(debug=True)