import os
import cv2
import numpy as np
import matplotlib.pyplot as plt
import tensorflow as tf
from tensorflow.keras.preprocessing.image import ImageDataGenerator


mnist = tf.keras.datasets.mnist
(x_train, y_train), (x_test, y_test) = mnist.load_data()


x_train = x_train.astype('float32') / 255.0
x_test = x_test.astype('float32') / 255.0


x_train = x_train.reshape(-1, 28, 28, 1)
x_test = x_test.reshape(-1, 28, 28, 1)


datagen = ImageDataGenerator(
    rotation_range=10,
    zoom_range=0.1,
    width_shift_range=0.1,
    height_shift_range=0.1
)

model = tf.keras.models.Sequential([
    tf.keras.layers.Conv2D(32, (3, 3), activation='relu', input_shape=(28, 28, 1)),
    tf.keras.layers.MaxPooling2D(2, 2),
    tf.keras.layers.Conv2D(64, (3, 3), activation='relu'),
    tf.keras.layers.MaxPooling2D(2, 2),
    tf.keras.layers.Conv2D(64, (3, 3), activation='relu'),
    tf.keras.layers.Flatten(),
    tf.keras.layers.Dense(64, activation='relu'),
    tf.keras.layers.Dropout(0.5),
    tf.keras.layers.Dense(10, activation='softmax')
])

model.compile(
    optimizer='adam',
    loss='sparse_categorical_crossentropy',
    metrics=['accuracy']
)

print("Training improved model...")
model.fit(
    datagen.flow(x_train, y_train, batch_size=32),
    epochs=10,
    validation_data=(x_test, y_test),
    verbose=1
)

model.save('handwritten.keras')


model = tf.keras.models.load_model('handwritten.keras')


loss, accuracy = model.evaluate(x_test, y_test, verbose=0)
print(f"Test Loss: {loss:.4f}")
print(f"Test Accuracy: {accuracy:.4f}")

def preprocess_image(image_path):
    """
    Preprocess custom digit images to match MNIST format
    """

    img = cv2.imread(image_path, cv2.IMREAD_GRAYSCALE)
    
    if img is None:
        return None
    
    if img.shape != (28, 28):
        img = cv2.resize(img, (28, 28))    

    if np.mean(img) > 127: 
        img = 255 - img  

    img = img.astype('float32') / 255.0
    
    img = img.reshape(1, 28, 28, 1)
    
    return img


print("\nCounting handwritten digit predictions in 'digits' folder:")
print("-" * 40)


digit_counts = [0] * 10
image_number = 0
num_files = 0

while os.path.isfile(f"digits/{image_number:05d}.jpg"):
    try:
        processed_img = preprocess_image(f"digits/{image_number:05d}.jpg")
        if processed_img is not None:
            prediction = model.predict(processed_img, verbose=0)
            predicted_digit = int(np.argmax(prediction))
            digit_counts[predicted_digit] += 1
            num_files += 1

    except Exception as e:
        pass  
    image_number += 1

print(f"Processed {num_files} digit images.")
print(f"Digit counts: {digit_counts}")



