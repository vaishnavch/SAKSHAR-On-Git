using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sakshar
{
    class Point//replicated
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    class LeapControl : IDisposable
    {
        int r;
        int ss;
        double alpha;
        bool disposed;
        int screenWidth;
        int screenHeight;
        int radialDeviation;

        UrgCtrl.UrgCtrl sensor;

        // Alpha is angle in degree in clock wise order
        // ss, r, screenWidth and screenHeight is in mm
        public LeapControl(int comPort, int baudRate, int ss, int r, int screenWidth, int screenHeight, int radialDeviation, double alpha = 0D)//replicated in LeapControl
        {
            this.alpha = convertToRadian(alpha);

            this.r = r;
            this.ss = ss;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.radialDeviation = radialDeviation;

            this.sensor = new UrgCtrl.UrgCtrl();

            if (!connectSensor(comPort, baudRate))
                Console.WriteLine("Unable to connect to sensor at port COM{0} using baud rate {1}", comPort, baudRate);
        }

        public LeapControl(int comPort, int baudRate)//replicated in LeapControl
        {
            this.sensor = new UrgCtrl.UrgCtrl();

            if (!connectSensor(comPort, baudRate))
                Console.WriteLine("Unable to connect to sensor at port COM{0} using baud rate {1}", comPort, baudRate);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                disconnectSensor();

            disposed = true;
        }

        void disconnectSensor()//replicated in LeapControl
        {
            sensor.Disconnect();
        }

        bool connectSensor(int comPort, int baudRate)
        {
            try
            {
                return sensor.Connect(comPort, baudRate);
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        // void saveDataInFileForDebugging()
        //{
        //    List<Point> points = getPointsOLD();

        //    string lines = string.Empty;
        //    foreach (Point point in points)
        //        lines = lines + "(" + point.X + "," + point.Y + ")\n";

        //    System.IO.StreamWriter file = new System.IO.StreamWriter("O:/sample-data/continious/test4" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt");
        //    file.WriteLine(lines);
        //    file.Close();

        //}

        //Point calculateCenter(List<Point> points)
        //{
        //    int x = 0;
        //    int y = 0;
        //    int totalPoints = points.Count;

        //    if (totalPoints > 0)
        //    {
        //        for (int i = 0; i < totalPoints; i++)
        //        {
        //            x += points[i].X;
        //            y += points[i].Y;
        //        }

        //        return new Point(x / totalPoints, y / totalPoints);
        //    }

        //    // If the screen doesn't contains any points, return a point which is outside the screen
        //    else
        //    {
        //        int sum = ss + r;
        //        return new Point(sum + screenWidth, sum + screenHeight);
        //    }
        //}

        //public Point getAbsolutePoint()
        //{
        //    int minLength = sensor.MinDistance;
        //    int maxLength = sensor.MaxDistance;

        //    int[] data = new int[sensor.MaxBufferSize];
        //    int noOfPoints = 0;

        //    try
        //    {
        //        noOfPoints = sensor.Capture(data);
        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }

        //    int x = 0;
        //    int y = 0;
        //    int totalPoints = 0;

        //    for (int index = 0; index < noOfPoints; index++)
        //    {
        //        int currentLength = data[index];

        //        // Neglect the out of range values
        //        if (currentLength > minLength && currentLength < maxLength)
        //        {
        //            double angleRadian = sensor.Index2Radian(index);

        //            // Front of sensor is facing to positive X
        //            int sensorX = (int)(currentLength * Math.Cos(angleRadian));
        //            int sensorY = (int)(currentLength * Math.Sin(angleRadian));

        //            x += -sensorY;
        //            y += sensorX;

        //            totalPoints++;
        //        }
        //    }

        //    return new Point(x / totalPoints, y / totalPoints);
        //}

        public Point getPoint()//doubt
        {
            int minLength = sensor.MinDistance;
            int maxLength = sensor.MaxDistance;

            int[] data = new int[sensor.MaxBufferSize];
            int noOfPoints = 0;

            try
            {
                noOfPoints = sensor.Capture(data);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

            int x = 0;
            int y = 0;
            int totalPoints = 0;

            for (int index = 0; index < noOfPoints; index++)
            {
                int currentLength = data[index] + radialDeviation; // add deviation here

                // Neglect the out of range values
                if (currentLength > minLength && currentLength < maxLength)
                {
                    double angleRadian = sensor.Index2Radian(index) - alpha; // substract angle here

                    // Front of sensor is facing to positive X
                    int sensorX = (int)(currentLength * Math.Cos(angleRadian));
                    int sensorY = (int)(currentLength * Math.Sin(angleRadian));

                    int xBar = r - sensorY; // xBar is in mm with respect to screen coordinate system
                    int yBar = sensorX - ss; // yBar is in mm with respect to screen coordinate system

                    //Ignore the outide points
                    if (isInsideScreen(xBar, yBar))
                    {
                        x += xBar;
                        y += yBar;
                        totalPoints++;
                    }
                }
            }

            if (totalPoints > 0)
                return new Point(x / totalPoints, y / totalPoints);

            int outside = ss + r;
            return new Point(outside + screenWidth, outside + screenHeight);
        }

        bool isInsideScreen(int x, int y)
        {
            return x > 0 && x < screenWidth && y > 0 && y < screenHeight;
        }

        double convertToRadian(double degree)
        {
            return (degree * Math.PI) / 180;
        }

        //double covertToDegree(double radian)
        //{
        //    return (radian * 180) / Math.PI;
        //}

        //string getSensorInfo()
        //{
        //    string[] versionInfos = sensor.GetVersionInformation();
        //    return string.Join(System.Environment.NewLine, versionInfos);
        //}

        //bool IsSensorConnected()
        //{
        //    return sensor.IsConnected();
        //}

        ~LeapControl()
        {
            Dispose(false);
        }
    }
}
