﻿using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows;
using System.IO.Ports;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using System.IO;
using swf = System.Windows.Forms;
using sd = System.Drawing;
using Newtonsoft.Json;
using System.Reflection;


namespace COTTask_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string serialPortIO8_name;

        public string file_saved;
        public string audioFile_Correct, audioFile_Error;

        public bool showCloseCircle;

        private List<Window> openedWins = new List<Window>();
        presentation taskPresentWin;

        // Strings stoing the Colors
        public string targetFillColorStr, targetOutlineColorStr;
        public string BKWaitTrialColorStr, BKReadyColorStr, BKTargetShownColorStr;
        public string CorrFillColorStr, CorrOutlineColorStr, ErrorFillColorStr, ErrorOutlineColorStr, ErrorCrossingColorStr;

        // Time Related Variables
        public float[] tRange_ReadyTimeS;
        public float tMax_ReactionTimeS, tMax_ReachTimeS, t_VisfeedbackShowS, t_InterTrialS, t_JuicerCorrectGivenS;


        
        // Target Related Variables
        public int targetNoOfPositions;
        public float targetDiaCM;
        public int targetDiaPixal;
        public List<int[]> optPostions_OCenter_List;

        public bool hasStartedPresention = false;


        // Touch Screen Rectangle
        sd.Rectangle Rect_touchScreen;

        public MainWindow()
        {
            InitializeComponent();


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.WorkingArea;
            this.Top = Rect_showMainScreen.Top;
            this.Left = Rect_showMainScreen.Left;

            // Get the touch Screen Rectangle
            Rect_touchScreen = Utility.Detect_PrimaryScreen_Rect(); 


            // locate serial Port Name
            serialPortIO8_name = SerialPortIO8.Locate_serialPortIO8();
            if (String.Equals(serialPortIO8_name,""))
            {
                btn_start.IsEnabled = false;
                btn_comReconnect.Visibility = Visibility.Visible;
                btn_comReconnect.IsEnabled = true;
                textblock_comState.Visibility = Visibility.Visible;

                run_comState.Text = "Can't Find the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.Orange);
                run_comState.Foreground = new SolidColorBrush(Colors.Red);
                run_instruction.Text = "Please connect it correctly and reCheck!";
                run_instruction.Background = new SolidColorBrush(Colors.Orange);
                run_instruction.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                btn_comReconnect.Visibility = Visibility.Hidden;
                btn_comReconnect.IsEnabled = false;
                textblock_comState.Visibility = Visibility.Hidden;
            }

            // Load Default Config File
            LoadConfigFile("");
            targetDiaPixal = Utility.cm2pixal(targetDiaCM);

            if (textBox_NHPName.Text != "" && serialPortIO8_name != null)
            {
                btn_start.IsEnabled = true;
                btn_stop.IsEnabled = false;
            }
            else
            {
                btn_start.IsEnabled = false;
                btn_stop.IsEnabled = false;
            }
        }

        private void Btn_comReconnect_Click(object sender, RoutedEventArgs e)
        {
            serialPortIO8_name = SerialPortIO8.Locate_serialPortIO8();
            if (String.Equals(serialPortIO8_name, ""))
            {
                run_comState.Text = "Can't Find the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.Orange);
                run_comState.Foreground = new SolidColorBrush(Colors.Red);
                run_instruction.Text = "Please connect it correctly and reCheck!";
                run_instruction.Background = new SolidColorBrush(Colors.Orange);
                run_instruction.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                btn_comReconnect.Visibility = Visibility.Hidden;
                btn_comReconnect.IsEnabled = false;
                run_comState.Text = "Found the COM Port for DLP-IO8!";
                run_comState.Background = new SolidColorBrush(Colors.White);
                run_comState.Foreground = new SolidColorBrush(Colors.Green);
                run_instruction.Text = "Can start trials now";
                run_instruction.Background = new SolidColorBrush(Colors.White);
                run_instruction.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (textBox_NHPName.Text != "" && serialPortIO8_name != null)
            {
                btn_start.IsEnabled = true;
            }

        }

        private void TextBox_NHPName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textBox_NHPName.Text != "" && serialPortIO8_name != null)
            {
                btn_start.IsEnabled = true;
            }
        }

        private void btnTestTouchpadJuicer_Click(object sender, RoutedEventArgs e)
        {
            TestStartpadJuicerWin Win_TestStartpadJuicer = new TestStartpadJuicerWin(this)
            {

                // Set Owner
                Owner = this
            };

            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.WorkingArea;
            Win_TestStartpadJuicer.Top = Rect_showMainScreen.Top;
            Win_TestStartpadJuicer.Left = Rect_showMainScreen.Left;

            Win_TestStartpadJuicer.Show();
        }

        private void saveInputParameters()
        {
            DateTime time_now = DateTime.Now;

            // if saved_folder not exist, created!
            if (Directory.Exists(textBox_savedFolder.Text) == false)
            {
                Directory.CreateDirectory(textBox_savedFolder.Text);
            }

            string filename_saved = textBox_NHPName.Text + time_now.ToString("-yyyyMMdd-HHmmss") + ".txt";
            file_saved = System.IO.Path.Combine(textBox_savedFolder.Text, filename_saved);

            using (StreamWriter file = new StreamWriter(file_saved))
            {
                file.WriteLine("Date: " + time_now.ToString("MM/dd/yyyy hh:mm:ss tt"));
                file.WriteLine("NHP Name: " + textBox_NHPName.Text);
                file.WriteLine("\n");


                file.WriteLine(String.Format("{0, -40}:  {1}", "Screen Resolution(pixal)", Rect_touchScreen.Width.ToString() + "x" + Rect_touchScreen.Height.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "CM to Pixal Ratio", Utility.ratioCM2Pixal.ToString()));


                file.WriteLine("\n\nInput Parameters:");

                // Save Target Settings
                file.WriteLine("\nTarget Settings:");
                file.WriteLine(String.Format("{0, -40}:  {1}", "Target Diameter (CM)", targetDiaCM.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "No of Targets", targetNoOfPositions.ToString()));
                file.WriteLine("Center Coordinates of Each Target (Pixal, (0,0) in Screen Center):");
                for (int i = 0; i < optPostions_OCenter_List.Count; i++)
                {
                    int[] position = optPostions_OCenter_List[i];
                    file.WriteLine(String.Format("{0, -40}:{1}, {2}", "Postion " + (i+1).ToString(), position[0], position[1]));
                }

                // Save Time Settings
                file.WriteLine("\nTime Settings:");
                file.WriteLine(String.Format("{0, -40}:  [{1} {2}]", "Ready Interface Show Time Range (s)", tRange_ReadyTimeS[0].ToString(), tRange_ReadyTimeS[1].ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Max Reaction Time (s)", tMax_ReactionTimeS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Max Reach Time (s)", tMax_ReachTimeS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Inter-Trial Time (s)", t_InterTrialS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Visual Feedback Time (s)", t_VisfeedbackShowS.ToString()));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Juicer Given Time for Correct (s)", t_JuicerCorrectGivenS.ToString()));

                // Save Color Settings
                file.WriteLine("\nColor Settings:");
                file.WriteLine(String.Format("{0, -40}:  {1}", "Target Fill Color", targetFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Wait Start Background", BKWaitTrialColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Ready Background", BKReadyColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Target Shown Background", BKTargetShownColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Correct Fill", CorrFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Correct Outline", CorrOutlineColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Error Fill", ErrorFillColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Error Outline", ErrorOutlineColorStr));
                file.WriteLine(String.Format("{0, -40}:  {1}", "Error Crossing", ErrorCrossingColorStr));

            }
        }


        private void MenuItem_NewWindow(object sender, RoutedEventArgs e)
        {
            Window1 testWind = new Window1(this);
            testWind.Show();

        }

        private void menuSaveConf_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDlg = new Microsoft.Win32.SaveFileDialog
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".json",
                Filter = "Json Files|*.json",
                FileName = "config"
            };

            Nullable<bool> result = saveFileDlg.ShowDialog();
            if (result == true)
            {
                // Open document 
                string saveConfigFile = saveFileDlg.FileName;
                SaveConfigFile(saveConfigFile);
            }

        }

        private void menuLoadConf_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".json",
                Filter = "Json Files|*.json"
            };


            // Get the selected file name and display in a TextBox 
            if (openFileDlg.ShowDialog() == true)
            {
                // Open document 
                string configFile = openFileDlg.FileName;
                LoadConfigFile(configFile);
            }
        }

        private void Btn_SelectSavefolder_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog folderBrowser = new Microsoft.Win32.OpenFileDialog
            {

                // Set validate names and check file exists to false otherwise windows will
                // not let you select "Folder Selection."
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Save Folder Selection. "
            };

            Nullable<bool> result = folderBrowser.ShowDialog();

            if (result == true)
            {
                textBox_savedFolder.Text = System.IO.Path.GetDirectoryName(folderBrowser.FileName);
            }
        }

        private void Btn_Select_AudioFile_Correct_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".wav",
                Filter = "Audio Files|*.wav",
                Title = "Selecting an Audio for Correcting "
            };

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                textBox_audioFile_Correct.Text = openFileDlg.FileName;
            }
        }

        private void Btn_Select_AudioFile_Error_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".wav",
                Filter = "Audio Files|*.wav",
                Title = "Selecting an Audio for Error "
            };


            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                textBox_audioFile_Error.Text = openFileDlg.FileName;
            }
        }

        private void MenuItem_SetupTime(object sender, RoutedEventArgs e)
        {
            SetupTimeWin Win_SetupTime = new SetupTimeWin(this);

            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.WorkingArea;
            Win_SetupTime.Top = Rect_showMainScreen.Top;
            Win_SetupTime.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupTime.Owner = this;

            Win_SetupTime.Show();
        }

        private void MenuItem_SetupColors(object sender, RoutedEventArgs e)
        {
            SetupColorsWin Win_SetupColors = new SetupColorsWin(this);


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.WorkingArea;
            Win_SetupColors.Top = Rect_showMainScreen.Top;
            Win_SetupColors.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupColors.Owner = this;

            Win_SetupColors.Show();


        }

        private void MenuItem_SetupTarget(object sender, RoutedEventArgs e)
        {
            SetupTargetsWin Win_SetupTarget = new SetupTargetsWin(this);


            // Get the first not Primary Screen 
            swf.Screen showMainScreen = Utility.Detect_oneNonPrimaryScreen();
            // Show the  MainWindow on the Touch Screen
            sd.Rectangle Rect_showMainScreen = showMainScreen.WorkingArea;
            Win_SetupTarget.Top = Rect_showMainScreen.Top;
            Win_SetupTarget.Left = Rect_showMainScreen.Left;

            // Set Owner
            Win_SetupTarget.Owner = this;

            Win_SetupTarget.Show();
        }



        private void LoadConfigFile(string configFile)
        {/*Load Config File .json 
            configFile == '': load the default Config File
            */

            // Read the Config. File
            string jsonStr;
            if (String.IsNullOrEmpty(configFile))
            {
                configFile = @"..\\..\\Resources\\ConfigFiles\\defaultConfig.json";

            }

            using (StreamReader r = new StreamReader(configFile))
            {
                jsonStr = r.ReadToEnd();
            }

                      
            dynamic config = JsonConvert.DeserializeObject(jsonStr);
            
            /* ---- Config into the Interface ---- */
            textBox_NHPName.Text = config["NHP Name"];

            textBox_savedFolder.Text = config["saved folder"];
            if (String.Compare(textBox_savedFolder.Text, "default", true) == 0)
            {
                textBox_savedFolder.Text = @"C:\\COTTaskSave";
            }

            textBox_audioFile_Correct.Text = config["audioFile_Correct"];
            textBox_audioFile_Error.Text = config["audioFile_Error"];



            // Times Sections
            var configTime = config["Times"];
            tRange_ReadyTimeS = new float[] {float.Parse((string)configTime["Ready Show Time Range"][0]), float.Parse((string)configTime["Ready Show Time Range"][1])};
            tMax_ReactionTimeS = float.Parse((string)configTime["Max Reach Time"]);
            tMax_ReachTimeS = float.Parse((string)configTime["Max Reaction Time"]);
            t_VisfeedbackShowS = float.Parse((string)configTime["Visual Feedback Show Time"]);
            t_InterTrialS = float.Parse((string)configTime["Inter Trials Time"]);
            t_JuicerCorrectGivenS = float.Parse((string)configTime["Juice Correct Given Time"]);

            // Color Sections
            var configColors = config["Colors"];
            targetFillColorStr = configColors["Target Fill Color"];
            BKWaitTrialColorStr = configColors["Wait Start Background"];
            BKReadyColorStr = configColors["Ready Background"];
            BKTargetShownColorStr = configColors["Target Shown Background"];
            CorrFillColorStr = configColors["Correct Fill"];
            CorrOutlineColorStr = configColors["Correct Outline"];
            ErrorFillColorStr = configColors["Error Fill"];
            ErrorOutlineColorStr = configColors["Error Outline"];
            ErrorCrossingColorStr = configColors["Error Crossing"];


            // Target Sections
            var configTarget = config["Target"];
            targetDiaCM = int.Parse((string)configTarget["Target Diameter (cm)"]);
            targetNoOfPositions = int.Parse((string)configTarget["Target No of Positions"]);
            optPostions_OCenter_List = new List<int[]>();
            dynamic tmp = configTarget["optPostions_OCenter_List"];
            foreach (var xyPos in tmp)
            {
                    int a = int.Parse((string)xyPos[0]);
                    int b = int.Parse((string)xyPos[1]);
                    optPostions_OCenter_List.Add(new int[] { a,  b});
            }
        }


        private void SaveConfigFile(string configFile)
        {/*Load Config File .json 
            configFile == '': load the default Config File
            */

            // config Times
            ConfigTimes configTimes = new ConfigTimes();
            configTimes.tRange_ReadyTime = tRange_ReadyTimeS;
            configTimes.tMax_ReactionTime = tMax_ReactionTimeS;
            configTimes.tMax_ReachTime = tMax_ReachTimeS;
            configTimes.t_InterTrial = t_InterTrialS;
            configTimes.t_VisfeedbackShow = t_VisfeedbackShowS;
            configTimes.t_JuicerCorrectGiven = t_JuicerCorrectGivenS;


            // config Target
            ConfigTarget configTarget = new ConfigTarget();
            configTarget.targetDiaCM = targetDiaCM;
            configTarget.targetNoOfPositions = targetNoOfPositions;
            configTarget.optPostions_OCenter_List = optPostions_OCenter_List;


            // config Colors
            ConfigColors configColors = new ConfigColors();
            configColors.targetFillColorStr = targetFillColorStr;
            configColors.BKWaitTrialColorStr = BKWaitTrialColorStr;
            configColors.BKReadyColorStr = BKReadyColorStr;
            configColors.BKTargetShownColorStr = BKTargetShownColorStr;
            configColors.CorrFillColorStr = CorrFillColorStr;
            configColors.CorrOutlineColorStr = CorrOutlineColorStr;
            configColors.ErrorFillColorStr = ErrorFillColorStr;
            configColors.ErrorOutlineColorStr = ErrorOutlineColorStr;
            configColors.ErrorCrossingColorStr = ErrorCrossingColorStr;



            // Combine all configs
            Config config = new Config();
            config.saved_folder = textBox_savedFolder.Text;
            config.NHPName = textBox_NHPName.Text;
            config.configTimes = configTimes;
            config.configTarget = configTarget;
            config.configColors = configColors;
            config.audioFile_Correct = textBox_audioFile_Correct.Text;
            config.audioFile_Error = textBox_audioFile_Error.Text;

            // Write to Json file
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFile, json);
        }

        private void DisabledSetParameters()
        {
            textBox_NHPName.IsEnabled = false;
            btn_SelectSavefolder.IsEnabled = false;
            btn_Select_AudioFile_Correct.IsEnabled = false;
            btn_Select_AudioFile_Error.IsEnabled = false;

            menu_File.IsEnabled = false;
            menu_Settings.IsEnabled = false;
            menu_Tools.IsEnabled = false;
        }

        private void EnabledSetParameters()
        {
            textBox_NHPName.IsEnabled = true;
            btn_SelectSavefolder.IsEnabled = true;
            btn_Select_AudioFile_Correct.IsEnabled = true;
            btn_Select_AudioFile_Error.IsEnabled = true;

            menu_File.IsEnabled = true;
            menu_Settings.IsEnabled = true;
            menu_Tools.IsEnabled = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // save all the Input parameters
            saveInputParameters();

            // btn_Start and btn_stop
            btn_start.IsEnabled = false;
            btn_stop.IsEnabled = true;

            DisabledSetParameters();


            // Create Presentation Instance only at the First Start Click
            if (hasStartedPresention == false)
            {
                taskPresentWin = new presentation(this)
                {
                    Top = Rect_touchScreen.Top,
                    Left = Rect_touchScreen.Left,

                    Name = "childWin_Task",
                    Owner = this
                };

                hasStartedPresention = true;
            }   

            // Start the Task
            taskPresentWin.Show();
            taskPresentWin.Present_Start();
        }

        private void Btn_stop_Click(object sender, RoutedEventArgs e)
        {
            if (taskPresentWin != null)
            {
                taskPresentWin.Present_Stop();
                taskPresentWin.Hide();
            }
            // btn_Start and btn_stop
            btn_start.IsEnabled = true;
            btn_stop.IsEnabled = false;
            EnabledSetParameters();
        }

        private void MenuItem_showCloseCircle(object sender, RoutedEventArgs e)
        {
            showCloseCircle = true;
        }

        private void MenuItem_noShowCloseCircle(object sender, RoutedEventArgs e)
        {
            showCloseCircle = false;
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (taskPresentWin != null)
            {
                taskPresentWin.Present_Stop();
            }
        }
    }
}