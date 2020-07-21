using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.IO;

using Tesseract;


namespace ProjectCarsSpeed
{/* load in video, crop out the dashboard image */
    class Program
    {
        static void Main(string[] args)
        {
            /* initialize Tesseract object */
            TesseractEngine ocr = new TesseractEngine("./tessdata", "eng", EngineMode.Default);

            /* declare variables for Tesseract */
            string speedStr;
            int speed = 0;
            int preSpeed = 0; // previous speed
            Pix pixImage;
            Page page;

            /* declare variables for video input */
            string videoName = Directory.GetCurrentDirectory() + "/ProjectCARS_short.mp4";
            VideoCapture capture = new VideoCapture(videoName);
            double frameCount = 0;
            double totalFrame = capture.GetCaptureProperty(CapProp.FrameCount); // 影片中的影格總數;
            Bitmap BitmapFrame;
            Rectangle cropArea = new Rectangle(1545, 865, 60, 38);
            ImageProcess imageProcess = new ImageProcess();

            /* declare variables for testing*/
            int wrong = 0;
            DateTime beforDT;
            DateTime afterDT;
            TimeSpan ts;
            KalmanFilter filter = new KalmanFilter(1, 1, 0.05, 1, 0.1, speed);


            /* check if video file exist */
            if (!File.Exists(videoName))
            {
                throw new FileNotFoundException(videoName);
            }
            Console.WriteLine("Start\n");
            

            /* get each frame from the video */
            while (frameCount < totalFrame)
            {
                /* calculate total process time */
                beforDT = System.DateTime.Now;

                /* Crop area
                 * x:1545px  y:865px  boxsize: 60x38*/
                // Mat pFrame = Crop_frame(capture.QueryFrame(), cropArea);
                BitmapFrame = Crop_frame(capture.QueryFrame(), cropArea).ToBitmap();
                frameCount += 1;


                /* image processing */
                imageProcess.ToBlackWhite(BitmapFrame); // grayscale(black and white)
                // BitmapFrame = imageProcess.NegativePicture(BitmapFrame); //turn into negative image
                BitmapFrame = imageProcess.ResizeImage(BitmapFrame, 120, 76); // enlarge image(x2)


                /* use Tesseract to recognize number */
                try
                {
                    pixImage = PixConverter.ToPix(BitmapFrame); //unable to work at Tesseract 3.3.0
                    page = ocr.Process(pixImage);
                    speedStr = page.GetText(); // 識別後的內容
                    // Console.WriteLine("String result: " + speedStr);
                    page.Dispose();

                    /* Parse str to int */
                    bool isParsable = Int32.TryParse(speedStr, out speed);
                    if (!isParsable)
                    {
                        Console.WriteLine("Could not be parsed.");
                        wrong += 1;
                        speed = preSpeed; // 如果偵測不到速度，使用前一個速度值
                    }

                    /* 防止負數或過大數字出現 */
                    if (speed < 0 || speed > 300)
                        speed = preSpeed;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error message: " + ex.Message);
                }
                Console.WriteLine("  -Current speed: " + speed + " mph");
                preSpeed = speed;

                /* Filtering(denoise) */
                speed = (int)filter.Output(speed);
                Console.WriteLine("  -Smoothed speed: " + speed + " mph");

                /* calculate total process time */
                afterDT = System.DateTime.Now;
                ts = afterDT.Subtract(beforDT);
                Console.WriteLine("總共花費" + ts.TotalMilliseconds + "ms.\n");
            }
            
            Console.WriteLine("無法辨識數量: " + wrong + " 全部frame數量: " + totalFrame);
            Console.WriteLine("辨識率: " + (double)(totalFrame - wrong) / totalFrame + "\n");
        }


        /* Crop out a square area */
        static Mat Crop_frame(Mat input, Rectangle crop_region)
        {
            Image<Bgr, Byte> buffer_im = input.ToImage<Bgr, Byte>();
            buffer_im.ROI = crop_region;
            Image<Bgr, Byte>  cropped_im = buffer_im.Copy();

            return cropped_im.Mat;
        }
    }
}
