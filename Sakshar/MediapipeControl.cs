using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sakshar
{
    class MediapipeControl
    {
        Thread DataThread;

        public MediapipeControl()
        {

        }

        public void Disconnect()
        {

        }

        public void InitMediapipeDataFetch()
        {
            DataThread = new Thread(UpdatePoint);
            DataThread.IsBackground = true;
            DataThread.Start();
        }

        void UpdatePoint()                              //everything goes here
        {

        }


    }
}
