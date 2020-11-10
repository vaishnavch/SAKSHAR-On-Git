using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Leap;
using System.Windows.Forms;

namespace Sakshar.resources
{
    class LeapControl
    {
        private Controller controller;
        Frame frame;
        Hand hand;
        Vector Leap = new Vector();
        Vector Screen = new Vector();
        Vector _Origin = new Vector();
        Vector _Axis2 = new Vector();
        Vector Axis2;
        Vector AxisX;
        Vector AxisZ;
        float[,] _orientationScreen = new float[3, 3];
        float[,] _orientationScreenTransposed = new float[3, 3];
        float[,] _transformationInverse = new float[4, 4];

        public bool exitDataFetching = false;


        public LeapControl()
        {
            controller = new Controller();
            //controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
        }

        public void Disconnect()
        {
            if (controller.IsConnected)
                controller.StopConnection();
        }

       

        public bool SetOrigin()
        {
            _Origin.x = Leap.x;
            _Origin.y = Leap.y;
            _Origin.z = Leap.z;
            if (_Origin.z == 0)
                return false;
            return true;
        }

        public bool SetAxis1()
        {
            float _Axis1X = Leap.x;
            float _Axis1Y = Leap.y;
            float _Axis1Z = Leap.z;
            AxisX = new Vector(_Axis1X - _Origin.x, _Axis1Y - _Origin.y, _Axis1Z - _Origin.z);
            if (_Axis1Z == 0)
                return false;
            return true;
        }

        public bool SetAxis2andCalculateScreenAxes()
        {
            _Axis2.x = Leap.x;
            _Axis2.y = Leap.y;
            _Axis2.z = Leap.z;
            Axis2 = new Vector(_Axis2.x - _Origin.x, _Axis2.y - _Origin.y, _Axis2.y - _Origin.z);
            if (_Axis2.z == 0)
                return false;

            AxisZ = AxisX.Cross(Axis2);
            Vector UnitAxisZ = AxisZ.Normalized;
            Vector UnitAxisX = AxisX.Normalized;
            
            Vector UnitAxisY = -UnitAxisX.Cross(UnitAxisZ);
            _orientationScreen[0, 0] = UnitAxisX.x;
            _orientationScreen[1, 0] = UnitAxisY.x;
            _orientationScreen[2, 0] = UnitAxisZ.x;

            _orientationScreen[0, 1] = UnitAxisX.y;
            _orientationScreen[1, 1] = UnitAxisY.y;
            _orientationScreen[2, 1] = UnitAxisZ.y;

            _orientationScreen[0, 2] = UnitAxisX.z;
            _orientationScreen[1, 2] = UnitAxisY.z;
            _orientationScreen[2, 2] = UnitAxisZ.z;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _orientationScreenTransposed[i, j] = _orientationScreen[j, i];
                }
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _transformationInverse[i, j] = _orientationScreenTransposed[i, j];
                }
            }
            _transformationInverse[3, 0] = -(_orientationScreenTransposed[0, 0] * _Origin.x + _orientationScreenTransposed[1, 0] * _Origin.y + _orientationScreenTransposed[2, 0] * _Origin.z);
            _transformationInverse[3, 1] = -(_orientationScreenTransposed[0, 1] * _Origin.x + _orientationScreenTransposed[1, 1] * _Origin.y + _orientationScreenTransposed[2, 1] * _Origin.z);
            _transformationInverse[3, 2] = -(_orientationScreenTransposed[0, 2] * _Origin.x + _orientationScreenTransposed[1, 2] * _Origin.y + _orientationScreenTransposed[2, 2] * _Origin.z);
            _transformationInverse[0, 3] = _transformationInverse[1, 3] = _transformationInverse[2, 3] = 0;
            _transformationInverse[3, 3] = 1;
            return true;
        }

        public void GetLeapPoint()
        {
            frame = controller.Frame();

            if (frame.Hands.Count > 0)
            {
                hand = frame.Hands[0];
                Leap.x = Convert.ToSingle(Math.Round(hand.PalmPosition.x, 3));
                Leap.y = Convert.ToSingle(Math.Round(hand.PalmPosition.z, 3));
                Leap.z = -Convert.ToSingle(Math.Round(hand.PalmPosition.y, 3));         //negative to make xyz right-hand compliant
            }
        }

        void GetScreenPoint()
        {
            GetLeapPoint();
            Screen.x = _transformationInverse[0, 0] * Leap.x + _transformationInverse[1, 0] * Leap.y + _transformationInverse[2, 0] * Leap.z + _transformationInverse[3, 0];
            Screen.y = _transformationInverse[0, 1] * Leap.x + _transformationInverse[1, 1] * Leap.y + _transformationInverse[2, 1] * Leap.z + _transformationInverse[3, 1];
            Screen.z = _transformationInverse[0, 2] * Leap.x + _transformationInverse[1, 2] * Leap.y + _transformationInverse[2, 2] * Leap.z + _transformationInverse[3, 2];
            CoordinateStatus.X = transformWidthToOrtho(Screen.x);
            CoordinateStatus.Y = transformHeightToOrtho(Screen.y);
            if (Screen.z < Math.Abs(50.0))
                CoordinateStatus.ClickStatus = true;
            else
                CoordinateStatus.ClickStatus = false;
        }

        

        public float GetOriginToXDistance()
        {
            return AxisX.Magnitude;
        }

        public float GetOriginToYDistance()
        {
            return AxisX.DistanceTo(Axis2);
        }

        int transformWidthToOrtho(float pixelX)
        {
            return (int)(400 + (pixelX * CoordinateStatus.WidthRatio));               
        }

        int transformHeightToOrtho(float pixelY)
        {
            return (int)(300 + (-pixelY * CoordinateStatus.HeightRatio));              
        }



         public void InitLeapDataFetch()
        {
            Thread DataThread = new Thread(UpdatePoint);
            DataThread.IsBackground = true;
            DataThread.Start();
        }

        void UpdatePoint()
        {
            while (exitDataFetching == false)
            {
                GetScreenPoint();
            }
        }
    }
}
