# imports
import sys, os
import numpy as np
import matplotlib.pyplot as plt 
from glob import glob
import librosa
import librosa.display
import IPython.display as ipd # needed to preview audio file
import warnings
warnings.filterwarnings("ignore")

# Get arguments
if len(sys.argv) > 2:
    filepath = sys.argv[1]
    savepath = sys.argv[2]
else:
    print("No file found")
    quit()

# load audio in librosa
y, sr = librosa.load(filepath)

# Mel-Spectrogram, NO axis and legend
S = librosa.feature.melspectrogram(y=y, sr=sr)
fig, ax = plt.subplots(figsize=(15,7.5))
S_dB = librosa.power_to_db(S, ref=np.max)
img = librosa.display.specshow(S_dB, sr=sr)

# below line isolates the filename if needed
#filename = filepath.split('\\')[len(filepath.split('\\'))-1][:-4]

fig.savefig('temp-classify.png', transparent=True)
plt.close(fig)

# Mel-Spectogram, axis and legend
S = librosa.feature.melspectrogram(y=y, sr=sr, n_mels=128, fmax=8000)
fig, ax = plt.subplots(figsize=(12,5))
S_dB = librosa.power_to_db(S, ref=np.max)
img = librosa.display.specshow(S_dB, x_axis='time', y_axis='mel', sr=sr, fmax=8000, ax=ax)
cbar = fig.colorbar(img, ax=ax, format='%+2.0f dB')
cbar.ax.tick_params(labelsize=18)
ax.set_xlabel("Time", fontsize=20)
ax.set_ylabel("Hz", fontsize=20)
plt.xticks(fontsize=18)
plt.yticks(fontsize=18)
plt.tight_layout()

fig.savefig('temp-display.png', transparent=True)
plt.close(fig)

# console output
print('SUCCESS')