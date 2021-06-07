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
model1 = load_model('1-model-clean-lr-100p.h5')                     # load trained model 1
classes1 = {0: 'Clean', 1: 'lead or rhythm'}                                                                    # load classes for model 1

model2 = load_model('1-model-lead-rhythm-92.5p.h5')                 # load trained model 2
classes2 = {0: 'Lead', 1: 'Rhythm'}                                                                             # load classes for model 2

model3 = load_model('eq1.h5')                                       # load trained model 3
classes3 = {0: '0-10-0 or 5-5-5', 1: '10-0-0 or 0-0-10'}                                                        # load classes for model 3

model4 = load_model('eq2.h5')                                       # load trained model 4
classes4 = {0: '0-0-10', 1: '10-0-0'}                                                                           # load classes for model 4

model5 = load_model('eq3.h5')                                       # load trained model 5
classes5 = {0: '0-10-0', 1: '5-5-5'}                                                                            # load classes for model 5

# classify files


def classify(filepath, model, classes, current_pred):

    img = image.load_img(
        filepath,
        target_size = (res[0], res[1])
    )

    X = image.img_to_array(img)
    X = np.expand_dims(X, axis=0)
    images = np.vstack([X])
    prediction = int(model.predict(images))

    return classes[prediction]

def get_eq(pred):
    if (pred == '0-10-0 or 5-5-5'):
        f_pred = classify(filepath, model5, classes5, current_pred)
    elif (pred == '10-0-0 or 0-0-10'):
        f_pred = classify(filepath, model4, classes4, current_pred)
    
    return f_pred

current_pred = ""

pred = classify(filepath, model1, classes1, current_pred)

if (pred == 'Clean'):
    current_pred = current_pred + "Clean "
    pred = classify(filepath, model3, classes3, current_pred)
    current_pred = current_pred + get_eq(pred)
elif (pred == 'lead or rhythm'):
    pred = classify(filepath, model2, classes2, current_pred)
    if (pred == 'Lead'):
        current_pred = current_pred + "Lead "
        pred = classify(filepath, model3, classes3, current_pred)
        current_pred = current_pred + get_eq(pred)
    elif (pred == 'Rhythm'):
        current_pred = current_pred + "Rhythm "
        pred = classify(filepath, model3, classes3, current_pred)
        current_pred = current_pred + get_eq(pred)


print(current_pred)