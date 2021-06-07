# AI Neural Network to classify Guitar Distortions

This project aimed to find out if a Convolutional Neural Network (CNN) could be trained to classify audio files, in this case guitar distortions.

1440 audio files were turned into [mel-spectrograms](https://librosa.org/doc/latest/_images/librosa-feature-melspectrogram-1.png) (visual representations of an audio signal, with time on the x-axis, frequencies on the y-axis and the intensifty of the frequency in dB represented by the color/z-axis).

The CNN was implemeted using the keras API and tensorflow as backend, and the multiple models trained to classify the files were all wrapped in a C# GUI.

Two apporaches were taken:
* One multi-class model (with 12 classes) 
* Multiple binary models (2 classes each)

It has been found that multiple binary models performed better, with an average accuracy of 98% while the multi-class model obtained 81% accuracy. This, however, is hindered by the rather small collection of audio files that I had to produce myself. I believe that in the future, a multi-class algorithm should perform better if more data becomes available and would also allow a developer to include more classes.
