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

using Sakshar.resources;

namespace Sakshar
{

    #region Button internal class
    internal class Button
    {
        internal int LeftX;
        internal int TopY;
        internal int Width;
        internal int Height;
        internal Color color = Color.Green;
        internal string label;

        // Button state true indicates that Button is pressed
        internal bool state = true;

        public Button(string label, Color color, int leftX, int topY)
        {
            this.label = label;
            this.color = color;
            this.LeftX = leftX;
            this.TopY = topY;
        }

        public Button(string label, int leftX, int topY)
        {
            this.label = label;
            this.LeftX = leftX;
            this.TopY = topY;
        }

        public Button(string label, Color color)
        {
            this.label = label;
            this.color = color;
        }

        public Button(string label)
        {
            this.label = label;
        }

        public void setPressed()
        {
            this.state = false;
        }

        public void setReleased()
        {
            this.state = true;
        }

        public void setXY(int leftX, int topY)
        {
            this.LeftX = leftX;
            this.TopY = topY;
        }
    }
    #endregion

    #region Texture internal class
    internal class Texture
    {
        internal int id;
        internal int leftX;
        internal int topY;
        internal string label = string.Empty;

        public void setParam(int leftX, int topY, string label)
        {
            this.label = label;
            this.leftX = leftX;
            this.topY = topY;
        }

        public Texture(int id, int leftX, int topY, string label)
        {
            this.id = id;
            this.leftX = leftX;
            this.topY = topY;
            this.label = label;
        }
    }
    #endregion

    public static class CoordinateStatus
    {

        public static int X = 0;
        public static int Y = 0;
        public static bool ClickStatus = true;
        public static float HeightRatio = 1;
        public static float WidthRatio = 1;
    }

    abstract class Game : GameWindow
    {
        const int farZ = 1;
        const int topY = 0;
        const int leftX = 0;
        const int nearZ = -1;
        const int rightX = 800;
        const int bottomY = 600;

        public int startX1;
        public int startY1;
        public int startX2;
        public int startY2;
        int dialogBoxTexture;
        int correctdialogBoxTexture;
        int incorrectdialogBoxTexture;
        //Sidd
        int instructions;
        int about;
        int cancel;
        public int startimg;
        public int correctanswerimg;
        bool flip;
        int exitLeapThreadReq;
        double aspectRatio;

        int tempX, tempY;
        bool tempClickStatus;

        DisplayDevice device;
        protected Texture[] fruitsTextures;
        Text.TextWriter textWriter;

        Dictionary<string, int> texturePool;

        object pointLock = new object();

        // An internal clock in milli second which starts from zero
        // A ulong data type can have 18446744073709551615 max value (~584554531 years)
        ulong clock = 0;
        ulong gameTime = 0;

        bool once = true;

        Button okButton;
        Button[] buttons;
        Button exitButton;
        Button replayButton;

        Slide[] slides;

        string label;
        string alphabet;
        string[] alphabets;
        Configuration gameConfig;

        int currentX;
        int currentY;
        int currentScreen;
        int currentBackground;
        int[] backgroundTextures;
        const int fruitEdgeLen = 180;

        protected string Msg;

        protected bool GotoNextSlide;
        protected bool DisplayDialogbox;


        protected int TotalPoints;
        protected const int ItemsPerSlide = 4;

        protected bool Sequential { get; set; }
        protected int ItemsPerAlphabet { get; set; }
        

        Thread DataFetchThread;

        public Game(DisplayDevice device)
            : base(device.Width,
                device.Height,
                GraphicsMode.Default,
                "Projector Window",
                GameWindowFlags.Fullscreen,
                device)
        {
            this.device = device;IsFlipRequired();
            SetScreenParams();

            VSync = VSyncMode.On;
            InitDataFetchThread();
        }

        protected abstract void CalculatePoints(int slideIndex, string slideAns);

        void InitDataFetchThread()
        {
            DataFetchThread = new Thread(FetchPointFromCoordinateStatus);
            DataFetchThread.IsBackground = true;
            DataFetchThread.Start();
        }
       
        void FetchPointFromCoordinateStatus()
        {
            while (exitLeapThreadReq == 0)
            {
                lock (pointLock)
                {
                    tempX = CoordinateStatus.X;
                    tempY = CoordinateStatus.Y;
                    tempClickStatus = CoordinateStatus.ClickStatus;
                }
            }
        }

        void SetAllXY()
        {
            startX1 = getWidth(15);
            startY1 = getHeight(15);
            startX2 = getWidth(65);
            startY2 = getHeight(55);
        }

        void SetScreenParams()
        {
            aspectRatio = (double)device.Height / (double)device.Width;
        }

        void DrawBackground(int texture)
        {
            int top = getHeight(10);
            int bottom = getHeight(90);
            drawTexture(texture, leftX, top, rightX, bottom - top);
        }

        void IsFlipRequired()
        {
            //Flip is not required when the Device is primary
            flip = device.IsPrimary ? false : true;
        }

        void InitTexturePool()
        {
            texturePool = new Dictionary<string, int>();
        }

        int LoadTexture(string image)
        {
            Bitmap bitmap = new Bitmap(image);
            int textureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
            bitmap.Dispose();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return textureId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Background color
            GL.ClearColor(Color.BlueViolet);

            InitTexturePool();
            SetAllXY();
            LoadFont();
            LoadButtons();
            LoadBackgroundImages();
            LoadFruitImages();
        }

        void LoadFont()
        {
            ProjectionProperties ProjectionProperties = new ProjectionProperties(leftX, rightX, bottomY, topY, nearZ, farZ);

            if (Thread.CurrentThread.CurrentUICulture.Name.Equals("hi"))
            {
                alphabets = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "BB", "CC", "DD", "EE", "FF", "GG", "HH", "II", "JJ", "KK", "LL", "MM", "NN", "OO", "PP", "QQ", "RR", "SS" };
                textWriter = new Text.TextWriter(this.ClientSize, ProjectionProperties, 0, 8);
            }
            else
            {
                textWriter = new Text.TextWriter(this.ClientSize, ProjectionProperties);
                alphabets = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            }
        }

        void LoadButtons()
        {
            okButton = new Button(Language.okbuttonlabel);
            dialogBoxTexture = getTextureFromPool("resources/dialogbox/1.jpg");
            correctdialogBoxTexture = getTextureFromPool("resources/dialogbox/correct.png");
            incorrectdialogBoxTexture = getTextureFromPool("resources/dialogbox/incorrect.png");
            //Sidd
            instructions = getTextureFromPool("resources/mainpage/instructions.png");
            about = getTextureFromPool("resources/mainpage/about.png");
            cancel = getTextureFromPool("resources/mainpage/cancel.png");
            startimg = getTextureFromPool("resources/mainpage/start.png");
            correctanswerimg = getTextureFromPool("resources/mainpage/correct.png");

            exitButton = new Button(Language.exitbuttonlabel, Color.Red, getWidth(85), topY);
            replayButton = new Button(Language.replaybuttonlabel, getWidth(45), getHeight(70));

            buttons = new Button[4];
            buttons[0] = new Button(Language.howtoplaybuttonlabel, startX1, startY1);
            buttons[1] = new Button(Language.aboutbuttonlabel, startX2, startY1);
            buttons[2] = new Button(Language.cancelbuttonlabel, startX1, startY2);
            buttons[3] = new Button(Language.startbuttonlabel, startX2, startY2);
        }

        int getTextureFromPool(string img)
        {
            if (texturePool.ContainsKey(img))
                return texturePool[img];

            int id = LoadTexture(img);
            texturePool.Add(img, id);
            return id;
        }

        void GenerateSlides()
        {
            slides = new SlidesGenerator(alphabets, ItemsPerAlphabet, Sequential).getAllSlides();
            fruitsTextures = new Texture[slides.Length * ItemsPerSlide];

            string imgPath = "resources/" + Thread.CurrentThread.CurrentUICulture.Name + "/alphabets/";

            int index = 0;
            foreach (Slide slide in slides)
            {
                string[] chars = slide.getAllItems();

                fruitsTextures[index++] = new Texture(getTextureFromPool(imgPath + gameConfig.GetValue(chars[0]) + ".png"), startX1, startY1, chars[0]);
                fruitsTextures[index++] = new Texture(getTextureFromPool(imgPath + gameConfig.GetValue(chars[1]) + ".png"), startX2, startY1, chars[1]);
                fruitsTextures[index++] = new Texture(getTextureFromPool(imgPath + gameConfig.GetValue(chars[2]) + ".png"), startX1, startY2, chars[2]);
                fruitsTextures[index++] = new Texture(getTextureFromPool(imgPath + gameConfig.GetValue(chars[3]) + ".png"), startX2, startY2, chars[3]);
            }
        }

        void LoadFruitImages()
        {
            gameConfig = new Configuration(Environment.CurrentDirectory + "/resources/" + Thread.CurrentThread.CurrentUICulture.Name + "/" + Thread.CurrentThread.CurrentUICulture.Name + ".conf");
            GenerateSlides();
        }

        void LoadBackgroundImages()
        {
            string[] allFiles = Directory.GetFiles("resources/backgrounds");
            int count = allFiles.Length;

            backgroundTextures = new int[count];
            for (int i = 0; i < count; i++)
                backgroundTextures[i] = getTextureFromPool(allFiles[i]);
        }

        void deleteTexture(Texture[] Textures)
        {
            foreach (Texture texture in Textures)
                GL.DeleteTexture(texture.id);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            Interlocked.Increment(ref exitLeapThreadReq);
            DataFetchThread.Join();

            deleteTexture(fruitsTextures);
            GL.DeleteTextures(backgroundTextures.Length, backgroundTextures);
            GL.DeleteTexture(dialogBoxTexture);
            GL.DeleteTexture(instructions);
            GL.DeleteTexture(about);
            GL.DeleteTexture(cancel);
            GL.DeleteTexture(startimg);
            GL.DeleteTexture(correctanswerimg);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            if (flip)
                GL.Ortho(rightX, leftX, topY, bottomY, farZ, nearZ);
            else
                GL.Ortho(leftX, rightX, bottomY, topY, farZ, nearZ);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            clock += (ulong)(TargetUpdatePeriod * 1000);
            lock (pointLock)
            {
                currentX = tempX;
                currentY = tempY;
            }

            if (DisplayDialogbox && isPointInside(currentX, currentY, okButton.LeftX, okButton.TopY, okButton.Width, okButton.Height) && tempClickStatus == true)
            {
                if (GotoNextSlide)
                {
                    currentScreen++;
                    currentBackground = RandomNoGenerator.getNumber(backgroundTextures.Length);
                }
                DisplayDialogbox = false;
            }

            else if (!DisplayDialogbox && isPointInside(currentX, currentY, exitButton.LeftX, exitButton.TopY, exitButton.Width, exitButton.Height) && tempClickStatus == true)
            {
                this.Exit();
            }
            else if (currentScreen == 0)
            {
                int constant = 10;
                if (!DisplayDialogbox && !DisplayDialogbox && isPointInside(currentX, currentY, startX1, startY1, startX1 + constant, startY1 + constant) && tempClickStatus == true)
                {
                    Msg = Language.howtoplay;
                    DisplayDialogbox = true;
                }

                if (!DisplayDialogbox && !DisplayDialogbox && isPointInside(currentX, currentY, startX2, startY1, startX2 + constant, startY1 + constant) && tempClickStatus == true)
                {
                    Msg = Language.aboutmsg;
                    DisplayDialogbox = true;
                }

                if (!DisplayDialogbox && !DisplayDialogbox && isPointInside(currentX, currentY, startX2, startY2, startX2 + constant, startY2 + constant) && tempClickStatus == true)
                {
                    gameTime = clock;
                    GotoNextSlide = true;
                    DisplayDialogbox = true;
                    Msg = Language.gameisstartingmsg;
                }
            }


            else if (currentScreen <= alphabets.Length)
            {
                string slideAns = slides[currentScreen - 1].GetAnswer();
                label = gameConfig.GetValue(slideAns);
                alphabet = gameConfig.GetValue(Regex.Replace(slideAns, @"\d", string.Empty));

                int start = (currentScreen - 1) * ItemsPerSlide;
                for (int slideIndex = start; slideIndex < start + ItemsPerSlide; slideIndex++)
                {
                    if (!DisplayDialogbox && isPointInside(currentX, currentY, fruitsTextures[slideIndex].leftX, fruitsTextures[slideIndex].topY, fruitEdgeLen, fruitEdgeLen) && tempClickStatus == true)
                    {
                        CalculatePoints(slideIndex, slideAns);
                        return;
                    }
                }
            }

            else if (!DisplayDialogbox && isPointInside(currentX, currentY, replayButton.LeftX, replayButton.TopY, replayButton.Width, replayButton.Height) && tempClickStatus == true)
            {
                once = true;
                gameTime = 0;
                currentScreen = 0;
                currentBackground = 0;
                GotoNextSlide = false;
                GenerateSlides();
            }

            else
            {
                if (once)
                {
                    gameTime = clock - gameTime;
                    once = false;
                }
            }
        }
        void drawDialogBox(Button okButton, string msg, int texture, bool display = false, float lineSpacing = 1.5f)
        {
            if (display)
            {
                int margin = 20;
                int okButtonMargin = 24;

                SizeF msgText = textWriter.MeasureText(msg);
                SizeF okText = textWriter.MeasureText(Language.okbuttonlabel);

                int textWidth = (int)msgText.Width;
                int textHeight = (int)msgText.Height;
                int width = textWidth + 2 * margin;
                int fontHeight = (int)okText.Height;
                int height = fontHeight == textHeight ? 2 * textHeight + 3 * margin : textHeight + fontHeight + 3 * margin;
                int startX = (rightX - width) / 2;
                int startY = (bottomY - height) / 2;
                int fontX = startX + margin;

                drawTexture(texture, startX, startY, width, height);
                drawText(msg, fontX, startY + margin, Brushes.White, FontAlignment.Left);

                okButton.setXY(fontX + textWidth / 2 - (int)((okText.Width + okButtonMargin + margin) / 2), startY + 3 * margin + textHeight); // Y cordinate is randomly picked up :D
                drawButton(okButton, okButtonMargin);
            }
        }

        void drawImage(int texture, int leftX, int topY)
        {
            int startX = leftX;
            int startY = topY;
            drawTexture(texture, startX, startY, fruitEdgeLen, fruitEdgeLen);
        }

       public void drawTexture(int texture, float leftX, float topY, float width, float height)
        {
            GL.PushMatrix();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);

            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(leftX, topY);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(leftX + width, topY);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(leftX + width, topY + height);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(leftX, topY + height);

            GL.End();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
        }

        void drawFruit(Texture texture)
        {
            texture.topY += 0;
            drawTexture(texture.id, texture.leftX, texture.topY, fruitEdgeLen, fruitEdgeLen);
        }

        void drawButton(Button button, int margin = 14, float lineSpacing = 1.0f)
        {
            SizeF size = textWriter.MeasureText(button.label);

            button.Width = (int)(size.Width + 2 * margin);
            button.Height = (int)(size.Height + 2 * margin);

           
            GL.PushMatrix();
            GL.Begin(PrimitiveType.Quads);

            if (button.state)
                GL.Color3(button.color);
            else
                GL.Color3(Color.Red);

            GL.Vertex2(button.LeftX, button.TopY);
            GL.Vertex2(button.LeftX + button.Width, button.TopY);
            GL.Vertex2(button.LeftX + button.Width, button.TopY + button.Height);
            GL.Vertex2(button.LeftX, button.TopY + button.Height);

            GL.End();
            GL.PopMatrix();

            drawText(button.label, button.LeftX + margin + size.Width / 2, button.TopY + margin, Brushes.White);
        }

        void drawText(string Text, float CenterX, float TopY, Brush Brush, FontAlignment FontAlignment = FontAlignment.Center)
        {
            textWriter.Print(Text, new PointF(CenterX, TopY), Brush, FontAlignment);
        }

        void drawPoint(float x, float y)
        {
            GL.PushMatrix();
            GL.PointSize(20);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(x, y);
            GL.End();
            GL.PopMatrix();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            DrawBackground(backgroundTextures[currentBackground]);

            int start = ItemsPerSlide * (currentScreen - 1);
            float floatingFactor = 0;

            if (currentScreen == 0)
            {
                drawTexture(startimg, startX1 + 170 + floatingFactor, startY2 + 215, 50, 50);
                drawText("SAKSHAR welcomes you", getWidth(50), getHeight(2), Brushes.White);
                drawText("Touch '         ' to begin the game", getWidth(50) + floatingFactor, getHeight(92), Brushes.White);
                
                //sidd
                drawTexture(instructions, startX1, startY1, 150, 150);
                drawTexture(about, startX2, startY1, 150, 150);
                drawTexture(startimg, startX2, startY2, 150, 150);
            }

            else if (currentScreen <= alphabets.Length)
            {
                drawText(Language.find + alphabet + Language.forstring + label, getWidth(50) + floatingFactor, getHeight(2), Brushes.White);
                drawText(Language.points + TotalPoints + Language.time + ((clock - gameTime) / 1000) + Language.seconds, getWidth(50), getHeight(92), Brushes.White);

                drawFruit(fruitsTextures[start]);
                drawFruit(fruitsTextures[start + 1]);
                drawFruit(fruitsTextures[start + 2]);
                drawFruit(fruitsTextures[start + 3]);
            }

            else
            {
                drawButton(replayButton);
                drawText(Language.gamefinished, getWidth(50) + floatingFactor, getHeight(40) + floatingFactor, Brushes.Red);
                drawText(Language.points + TotalPoints + Language.totaltime + (gameTime / 1000) + Language.seconds, getWidth(50) + floatingFactor, getHeight(60) + floatingFactor, Brushes.Red);
            }

            if (Msg == Language.correctans)
                dialogBoxTexture = correctdialogBoxTexture;
            if (Msg == Language.incorrectans)
                dialogBoxTexture = incorrectdialogBoxTexture;

            drawDialogBox(okButton, Msg, dialogBoxTexture, DisplayDialogbox);

            drawButton(exitButton);
            drawPoint(currentX, currentY); // Cursor

            SwapBuffers();
        }

        int getWidth(int percentage)
        {
            return (int)(percentage * rightX * 0.01);
        }

        int getHeight(int percentage)
        {
            return (int)(percentage * bottomY * 0.01);
        }

        bool isPointInside(int x, int y, int x1, int y1, int x2, int y2)
        {
            return x >= x1 && x <= x1 + x2 && y >= y1 && y <= y1 + y2;
        }
    }
}
