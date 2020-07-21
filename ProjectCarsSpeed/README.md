# Project Cars 2 speed extraction
### Emgu.CV.runtime.windows(4.3.0)
Get video frames and crop image  
### Tesseract_463(3.3.0.1)
Recognize number from dashboard image
### ImageProcess.cs
Three function: turn into negative picture, turn into black and white image, resize image
### KalmanFilter
An algorithm that provides estimates of some unknown variables given the measurements observed over time  

---
## Message meaning:  
* Current speed: speed recognized by Tesseract   
* Smoothed speed: speed estimated with Kalman Filter  
* Could not be parsed: the string that Tesseract produced is not a number  
* Empty page!!: Tesseract couldn't recognize any letter  

---
## Testing video:
* ProjectCARS_short1.mp4
* ProjectCARS_short2.mp4
* ProjectCARS_short3.mp4  

Change testing video at:
```c#
string videoName = Directory.GetCurrentDirectory() + "/ProjectCARS_short.mp4";
```