using OpenTK;
using Sakshar.resources;
using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Sakshar
{

    public partial class MainScreen : Form
    {
        static bool teachingMode = true;
        Thread projectorThread;
        static CultureInfo CurrentUICulture;
        object launcherLock = new object();
        LeapControl Leap;
        MouseControl mouseControl;
        int LeapCalibrationStep = -1;
        int MouseCalibrationStep = -1;

        public MainScreen()
        {
            InitializeComponent();
            disableWindowResize();
        }

        void disableWindowResize()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        void MainScreen_Load(object sender, EventArgs e)
        {
            SetLanguage("en"); //Set english as the default language
            SetDefaultValues();
        }

        /// <summary>
        /// Set default values to languague and game mode combo box
        /// </summary>
        void SetDefaultValues()
        {
            langComboBox.SelectedIndex = 0;
            gameModeComboBox.SelectedIndex = 0;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            lock (launcherLock)
            {
                if (projectorThread == null)
                {
                    // Game is not running, launch it in a new thread
                    // remove disconnect sensor from calibration and check it
                    projectorThread = new Thread(() =>
                    {
                        DisplayDevice projectorScreen = DisplayDevice.GetDisplay(DisplayIndex.First);
                        if (projectorScreen == null)
                        {
                            // ONLY for debugging purpose
                            projectorScreen = DisplayDevice.GetDisplay(DisplayIndex.Default);
                            MessageBox.Show("Secondary display Device not found. Make sure the calibration is connected in 'Extended' mode. Application will run on debug mode.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }


                        projectorThread.CurrentUICulture = CurrentUICulture;

                        if (teachingMode)
                        {
                            using (var projector = new TeachingMode(projectorScreen))
                                projector.Run(60);
                        }
                        else
                        {
                            using (var projector = new TestingMode(projectorScreen))
                                projector.Run(60);
                        }

                        projectorThread = null;
                    });

                    projectorThread.SetApartmentState(ApartmentState.STA);
                    projectorThread.Start();
                }
                else
                    MessageBox.Show("The game is already running. Please exit the game to re run it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SetLanguage(string Culture)
        {
            CurrentUICulture = CultureInfo.GetCultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = CurrentUICulture;
            SetUIChanges();
        }

        void SetUIChanges()
        {
            var rm = new ResourceManager(typeof(MainScreen));

            startButton.Text = rm.GetString("startButton.Text");
            chooseLangLabel.Text = rm.GetString("chooseLangLabel.Text");
        }

        
        

        private void LanguageChanged(object sender, EventArgs e)
        {
            if (langComboBox.SelectedIndex == 1)
                SetLanguage("hi");

            else
                SetLanguage("en");
        }

        private void ModeChanged(object sender, EventArgs e)
        {
            teachingMode = gameModeComboBox.SelectedIndex == 0 ? true : false;
        }

        private void CalibrateLeapMotion_Click(object sender, EventArgs e)
        {
            if (LeapCalibrationStep != -1)
                Leap.GetLeapPoint();
            switch (LeapCalibrationStep)
            {
                case -1:
                    Leap = new LeapControl();
                    CalibrateLeapMotion.Text = "Set Origin";
                    ++LeapCalibrationStep;
                    break;
                case 0:
                    if (Leap.SetOrigin())
                    {
                        ++LeapCalibrationStep;
                        CalibrateLeapMotion.Text = "Set X-Axis";
                    }
                    break;
                case 1:
                    if (Leap.SetAxis1())
                    {
                        ++LeapCalibrationStep;
                        CalibrateLeapMotion.Text = "Set Edge";
                    }
                    break;
                case 2:
                    if (Leap.SetAxis2andCalculateScreenAxes())
                    {
                        CoordinateStatus.WidthRatio = (float)400 / Leap.GetOriginToXDistance();
                        CoordinateStatus.HeightRatio = (float)300 / Leap.GetOriginToYDistance();
                        ++LeapCalibrationStep;
                        CalibrateLeapMotion.Text = "Begin tracking";
                    }
                    break;
                case 3:
                    Leap.InitLeapDataFetch();
                    ++LeapCalibrationStep;
                    CalibrateLeapMotion.Text = "Stop tracking";
                    break;
                case 4:
                    Leap.exitDataFetching = true;
                    Leap.Disconnect();
                    CalibrateLeapMotion.Text = "Calibrate Leap Motion";
                    LeapCalibrationStep = -1;
                    break;
            }
        }

        private void CalibrateMouse_Click(object sender, EventArgs e)
        {

            switch (MouseCalibrationStep)
            {
                case -1:
                    mouseControl = new MouseControl();
                    mouseControl.InitMouseDataFetch();
                    CalibrateMouse.Text = "Stop Tracking";
                    ++MouseCalibrationStep;
                    break;
                case 0:
                    mouseControl.exitMouseDataFetching = true;
                    CalibrateMouse.Text = "Turn on Tracking";
                    MouseCalibrationStep = -1;
                    break;
            }
        }
    }
}
