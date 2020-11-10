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
    class TeachingMode : Game
    {
        public TeachingMode(DisplayDevice device)
            : base(device)
        {
            Sequential = true; // Alphabet should come sequentially
            ItemsPerAlphabet = 1; //We have 1 image for each alphabet
        }

        protected override void CalculatePoints(int slideIndex, string slideAns)
        {
            if (fruitsTextures[slideIndex].label.Equals(slideAns))
            {
                Msg = Language.correctans;
                drawTexture(correctanswerimg, startX1 + 190, startY2 + 215, 50, 50);
                TotalPoints += ItemsPerSlide;
                GotoNextSlide = true;
            }

            else
            {
                Msg = Language.incorrectans;
                TotalPoints--;
                GotoNextSlide = false;
            }
            DisplayDialogbox = true;
        }
    }
}
