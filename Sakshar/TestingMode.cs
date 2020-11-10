using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Text;

namespace Sakshar
{
    class TestingMode : Game
    {
        public TestingMode(DisplayDevice device)
            : base(device)
        {
            Sequential = false; // Alphabet should come randonly
            ItemsPerAlphabet = 4; //We have 4 image for each alphabet
        }

        protected override void CalculatePoints(int slideIndex, string slideAns)
        {
            if (fruitsTextures[slideIndex].label.Equals(slideAns))
            {
                Msg = Language.correctans;
                TotalPoints += ItemsPerSlide;
            }

            else
            {
                Msg = Language.incorrectans;
                TotalPoints--;
            }

            GotoNextSlide = true;
            DisplayDialogbox = true;
        }
    }
}
