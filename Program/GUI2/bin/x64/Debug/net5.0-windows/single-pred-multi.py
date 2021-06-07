import sys
import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2' 
import numpy as np
from keras.models import load_model
from keras.preprocessing import image

if len(sys.argv) > 1:
    filepath = sys.argv[1]
else:
    print("No file found")
    quit()

res = [135, 270]                                                                                                # default res used to train previous algorithms
model = load_model('D:\\Misc\\Napier\\Honours-Project\\zpython\\12-classes.h5')                                # load trained model
classes = {0: 'clean-0-0-10', 1: 'clean-0-10-0', 2: 'clean-10-0-0', 3: 'clean-5-5-5', 4: 'lead-0-0-10', 5: 'lead-0-10-0', 6: 'lead-10-0-0', 7: 'lead-5-5-5', 8: 'rhythm-0-0-10', 9: 'rhythm-0-10-0', 10: 'rhythm-10-0-0', 11: 'rhythm-5-5-5'}


# classify files

def classify(filepath, model, classes):

    img = image.load_img(
        filepath,
        target_size = (res[0], res[1])
    )

    X = image.img_to_array(img)
    X = np.expand_dims(X, axis=0)
    images = np.vstack([X])
    
    predictions = model.predict(images)[0]

    for j in range(len(predictions)):
        if predictions[j] != 0:
            prediction = j
            break

    return classes[prediction]

pred = classify(filepath, model, classes)
#print("File '" + str(filepath.split('\\')[len(filepath.split('\\'))-1]) + "' predicted as " + str(pred))
print(pred)