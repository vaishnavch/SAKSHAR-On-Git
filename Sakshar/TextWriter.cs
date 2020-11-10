using System;
using System.Drawing;
using System.Collections.Generic;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Text
{
    class ProjectionProperties
    {
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }
        public float Far { get; private set; }
        public float Near { get; private set; }

        public ProjectionProperties(float Left, float Right, float Bottom, float Top, float Near, float Far)
        {
            this.Left = Left;
            this.Right = Right;
            this.Bottom = Bottom;
            this.Top = Top;
            this.Near = Near;
            this.Far = Far;
        }
    }

    class BitmapInfo
    {
        public SizeF Size;
        public int Texture;

        public BitmapInfo(SizeF Size, int Texture)
        {
            this.Size = Size;
            this.Texture = Texture;
        }
    }

    public enum FontAlignment
    {
        Left,
        Center,
        Right
    }

    class TextWriter
    {
        readonly Size ClientSize;
        readonly int TopOffset;
        readonly int BottomOffset;
        readonly Brush defaultBrush = Brushes.Black; //This brush is used, if none specified
        readonly ProjectionProperties ProjectionProperties;
        readonly Font defaultFont = new Font(FontFamily.GenericSansSerif, 30, FontStyle.Regular); //This font is used, if none specified

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ClientSize"></param>
        /// <param name="ProjectionProperties"></param>
        public TextWriter(Size ClientSize, ProjectionProperties ProjectionProperties, int TopOffset = 0, int BottomOffset = 0)
        {
            this.TopOffset = TopOffset;
            this.BottomOffset = BottomOffset;
            this.ClientSize = ClientSize;
            this.ProjectionProperties = ProjectionProperties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public SizeF MeasureText(string Text)
        {
            return MeasureText(Text, defaultFont, defaultBrush);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Font"></param>
        /// <returns></returns>
        public SizeF MeasureText(string Text, Font Font)
        {
            return MeasureText(Text, Font, defaultBrush);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Font"></param>
        /// <param name="Brush"></param>
        /// <returns></returns>
        public SizeF MeasureText(string Text, Font Font, Brush Brush)
        {
            SizeF Size = SizeF.Empty;
            GetTextSize(Text, Font, Brush, ref Size);

            Size.Width = GetWidthInOrthProjection((int)Size.Width);
            Size.Height = GetHeightInOrthProjection((int)Size.Height);

            return Size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public BitmapInfo CreateBitmap(string Text)
        {
            return CreateBitmap(Text, defaultFont, defaultBrush);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Brush"></param>
        /// <returns></returns>
        public BitmapInfo CreateBitmap(string Text, Brush Brush)
        {
            return CreateBitmap(Text, defaultFont, Brush);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Font"></param>
        /// <param name="Brush"></param>
        /// <param name="BottomOffset">In Pixel</param>
        /// <returns></returns>
        public BitmapInfo CreateBitmap(string Text, Font Font, Brush Brush)
        {
            SizeF Size = SizeF.Empty;
            Bitmap TempBitmap = GetTextSize(Text, Font, Brush, ref Size, false);
            Bitmap Bitmap = TempBitmap.Clone(new Rectangle(0, 0, (int)Math.Ceiling(Size.Width), BottomOffset + (int)Math.Ceiling(Size.Height)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            TempBitmap.Dispose();

            //Bitmap.Save("O:/temp/11.PNG");
            //TempBitmap.Save(@"O:\rendering\del\" + DateTime.Now.Ticks + ".PNG");

            Size.Width = GetWidthInOrthProjection((int)Size.Width);
            Size.Height = GetHeightInOrthProjection(BottomOffset + (int)Size.Height);

            int TextureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            System.Drawing.Imaging.BitmapData bitmapData = Bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            Bitmap.UnlockBits(bitmapData);
            Bitmap.Dispose();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            return new BitmapInfo(Size, TextureId);
        }

        Bitmap GetTextSize(string Text, Font Font, Brush Brush, ref SizeF Size, bool MeasureOnly = true)
        {
            Bitmap Bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);

            using (Graphics Graphics = Graphics.FromImage(Bitmap))
            {
                Graphics.Clear(Color.Transparent);
                Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                Graphics.DrawString(Text, Font, Brush, 0, TopOffset);
                Size = Graphics.MeasureString(Text, Font);
            }

            if (MeasureOnly)
                Bitmap.Dispose();

            return Bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textureId"></param>
        /// <param name="Location"></param>
        /// <param name="orthoWidth"></param>
        /// <param name="height"></param>
        public void drawBitmap(PointF Location, BitmapInfo BitmapInfo, FontAlignment FontAlignment = FontAlignment.Left)
        {
            GL.PushMatrix();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, BitmapInfo.Texture);
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);

            PointF TopLeft = PointF.Empty;

            switch (FontAlignment)
            {
                case FontAlignment.Left:
                    TopLeft.X = Location.X;
                    TopLeft.Y = Location.Y;
                    break;
                case FontAlignment.Center:
                    TopLeft.X = Location.X - BitmapInfo.Size.Width / 2;
                    TopLeft.Y = Location.Y;
                    break;
                case FontAlignment.Right:
                    TopLeft.X = Location.X - BitmapInfo.Size.Width;
                    TopLeft.Y = Location.Y;
                    break;
            }

            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(TopLeft.X, TopLeft.Y);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(TopLeft.X + BitmapInfo.Size.Width, TopLeft.Y);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(TopLeft.X + BitmapInfo.Size.Width, TopLeft.Y + BitmapInfo.Size.Height);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(TopLeft.X, TopLeft.Y + BitmapInfo.Size.Height);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.DeleteTexture(BitmapInfo.Texture); // Since we are creating bitmap every time.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Location"></param>
        public void Print(string Text, PointF Location, FontAlignment FontAlignment = FontAlignment.Left)
        {
            Print(Text, Location, defaultFont, defaultBrush, FontAlignment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Location"></param>
        /// <param name="Font"></param>
        public void Print(string Text, PointF Location, Font Font, FontAlignment FontAlignment = FontAlignment.Left)
        {
            Print(Text, Location, Font, defaultBrush, FontAlignment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Location"></param>
        /// <param name="Brush"></param>
        public void Print(string Text, PointF Location, Brush Brush, FontAlignment FontAlignment = FontAlignment.Left)
        {
            Print(Text, Location, defaultFont, Brush, FontAlignment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Location"></param>
        /// <param name="Font"></param>
        /// <param name="Brush"></param>
        public void Print(string Text, PointF Location, Font Font, Brush Brush, FontAlignment FontAlignment = FontAlignment.Left)
        {
            BitmapInfo BitmapInfo = CreateBitmap(Text, Font, Brush);
            drawBitmap(Location, BitmapInfo, FontAlignment);
        }

        float GetWidthInOrthProjection(int XPixel)
        {
            float orthoWidth = ProjectionProperties.Right - ProjectionProperties.Left;
            return (XPixel * Math.Abs(orthoWidth)) / ClientSize.Width;
        }

        float GetHeightInOrthProjection(int YPixel)
        {
            float orthoHeight = ProjectionProperties.Top - ProjectionProperties.Bottom;
            return (YPixel * Math.Abs(orthoHeight)) / ClientSize.Height;
        }

        ~TextWriter()
        {
            defaultFont.Dispose();
        }
    }
}
