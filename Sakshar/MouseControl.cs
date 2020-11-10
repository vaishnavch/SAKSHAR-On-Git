using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sakshar
{
    class MouseControl
    {
        public bool exitMouseDataFetching = false;
        Thread DataThread;

        public MouseControl()
        {

        }

        public void InitMouseDataFetch()
        {
            DataThread = new Thread(UpdatePoint);
            DataThread.IsBackground = true;
            DataThread.Start();
            CoordinateStatus.HeightRatio = (float)600 / 1080;
            CoordinateStatus.WidthRatio = (float)800 / 1920;
        }

        void UpdatePoint()
        {
            while(exitMouseDataFetching==false)
            {
                CoordinateStatus.X = (int)((float)Cursor.Position.X * CoordinateStatus.WidthRatio);
                CoordinateStatus.Y = (int)((float)Cursor.Position.Y * CoordinateStatus.HeightRatio);
                if (Form.MouseButtons != 0)
                {
                    CoordinateStatus.ClickStatus = true;
                }
                else
                    CoordinateStatus.ClickStatus = false;
            }
        }
    }
}
