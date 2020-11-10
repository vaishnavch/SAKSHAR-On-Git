namespace Sakshar
{
    partial class MainScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainScreen));
            this.languageGroupBox = new System.Windows.Forms.GroupBox();
            this.gameModeLabel = new System.Windows.Forms.Label();
            this.gameModeComboBox = new System.Windows.Forms.ComboBox();
            this.langComboBox = new System.Windows.Forms.ComboBox();
            this.chooseLangLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.CalibrateLeapMotion = new System.Windows.Forms.Button();
            this.CalibrateMouse = new System.Windows.Forms.Button();
            this.languageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // languageGroupBox
            // 
            this.languageGroupBox.Controls.Add(this.gameModeLabel);
            this.languageGroupBox.Controls.Add(this.gameModeComboBox);
            this.languageGroupBox.Controls.Add(this.langComboBox);
            this.languageGroupBox.Controls.Add(this.chooseLangLabel);
            resources.ApplyResources(this.languageGroupBox, "languageGroupBox");
            this.languageGroupBox.Name = "languageGroupBox";
            this.languageGroupBox.TabStop = false;
            // 
            // gameModeLabel
            // 
            resources.ApplyResources(this.gameModeLabel, "gameModeLabel");
            this.gameModeLabel.Name = "gameModeLabel";
            // 
            // gameModeComboBox
            // 
            this.gameModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gameModeComboBox.FormattingEnabled = true;
            this.gameModeComboBox.Items.AddRange(new object[] {
            resources.GetString("gameModeComboBox.Items"),
            resources.GetString("gameModeComboBox.Items1")});
            resources.ApplyResources(this.gameModeComboBox, "gameModeComboBox");
            this.gameModeComboBox.Name = "gameModeComboBox";
            this.gameModeComboBox.SelectedIndexChanged += new System.EventHandler(this.ModeChanged);
            // 
            // langComboBox
            // 
            this.langComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.langComboBox.FormattingEnabled = true;
            this.langComboBox.Items.AddRange(new object[] {
            resources.GetString("langComboBox.Items"),
            resources.GetString("langComboBox.Items1")});
            resources.ApplyResources(this.langComboBox, "langComboBox");
            this.langComboBox.Name = "langComboBox";
            this.langComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageChanged);
            // 
            // chooseLangLabel
            // 
            resources.ApplyResources(this.chooseLangLabel, "chooseLangLabel");
            this.chooseLangLabel.Name = "chooseLangLabel";
            // 
            // startButton
            // 
            resources.ApplyResources(this.startButton, "startButton");
            this.startButton.Name = "startButton";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // CalibrateLeapMotion
            // 
            resources.ApplyResources(this.CalibrateLeapMotion, "CalibrateLeapMotion");
            this.CalibrateLeapMotion.Name = "CalibrateLeapMotion";
            this.CalibrateLeapMotion.UseVisualStyleBackColor = true;
            this.CalibrateLeapMotion.Click += new System.EventHandler(this.CalibrateLeapMotion_Click);
            // 
            // CalibrateMouse
            // 
            resources.ApplyResources(this.CalibrateMouse, "CalibrateMouse");
            this.CalibrateMouse.Name = "CalibrateMouse";
            this.CalibrateMouse.UseVisualStyleBackColor = true;
            this.CalibrateMouse.Click += new System.EventHandler(this.CalibrateMouse_Click);
            // 
            // MainScreen
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CalibrateMouse);
            this.Controls.Add(this.CalibrateLeapMotion);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.languageGroupBox);
            this.MaximizeBox = false;
            this.Name = "MainScreen";
            this.Load += new System.EventHandler(this.MainScreen_Load);
            this.languageGroupBox.ResumeLayout(false);
            this.languageGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox languageGroupBox;
        private System.Windows.Forms.Label chooseLangLabel;
        private System.Windows.Forms.ComboBox langComboBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.ComboBox gameModeComboBox;
        private System.Windows.Forms.Label gameModeLabel;
        private System.Windows.Forms.Button CalibrateLeapMotion;
        private System.Windows.Forms.Button CalibrateMouse;
    }
}

