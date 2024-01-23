using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace th_launcher_wbf
{
    public partial class MainWindow : Window
    {
        private MusicHandler m;
        private List<string> gameDirs;
        private List<string> tasofroDirs;
        private List<string> spinoffDirs;
        private List<string> fanGameDirs;
        private JsonObject songInfo;
        private Button playPause;
        private bool isPlaying;

        private const int BUTTONS_PER_ROW = 4;
        private const int BUTTON_MARGIN = 12;

        public MainWindow()
        {
            gameDirs = GetGameDirectories("dirs.txt");
            fanGameDirs = GetGameDirectories("fangames.txt");
            tasofroDirs = GetGameDirectories("tasofro.txt");
            spinoffDirs = GetGameDirectories("spinoffs.txt");
            songInfo = GetSongNames();
            isPlaying = false;

            InitializeComponent();
            SetupWindow();
        }

        private void SetupWindow()
        {
            Width = 1330;
            Height = 700;
            // Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/images/bg.jpg"))); // background color = #293243
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#293243"));
            ResizeMode = ResizeMode.NoResize;

            TabControl tabControl = new TabControl();
            tabControl.Background = Brushes.Transparent;

            TabItem shmupsTab = CreateShmupsTab();
            tabControl.Items.Add(shmupsTab);

            TabItem tasofroTab = CreateGameTab("TasoFro", "pack://application:,,,/images/tasofro/", 7, tasofroDirs);
            tabControl.Items.Add(tasofroTab);

            TabItem spinoffsTab = CreateGameTab("Spin-Offs", "pack://application:,,,/images/spinoffs/", 6, spinoffDirs);
            tabControl.Items.Add(spinoffsTab);

            TabItem fanGamesTab = CreateGameTab("Fan Games", "pack://application:,,,/images/fangames/", 2, fanGameDirs);
            tabControl.Items.Add(fanGamesTab);

            TabItem musicRoomTab = CreateMusicRoomTab();
            tabControl.Items.Add(musicRoomTab);

            Grid.SetRow(tabControl, 0);
            Grid.SetColumn(tabControl, 0);

            Grid grid = new Grid();
            grid.Children.Add(tabControl);

            Content = grid;
        }

        private TabItem CreateShmupsTab()
        {
            TabItem shmupsTab = new TabItem();
            shmupsTab.Header = "Shmups";
            shmupsTab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4f6082"));
            const int NUMBER_OF_WINDOWS_GAMES = 14;

            ScrollViewer scrollViewer = new ScrollViewer();
            Grid shmupsGrid = new Grid();
            scrollViewer.Content = shmupsGrid;
            shmupsGrid.Background = Brushes.Transparent;
            SetGridDefinitions(ref shmupsGrid, NUMBER_OF_WINDOWS_GAMES);

            for (int i = 0; i < NUMBER_OF_WINDOWS_GAMES; i++)
            {
                int gameIndex = i + 6;
                string gamePath = gameDirs[i] + ((gameIndex < 6) ? $"/th0{gameIndex}.exe" : $"/th{gameIndex}.exe");
                Button gameButton = CreateGameButton($"pack://application:,,,/images/th{gameIndex}cover.jpg", gamePath);

                int row = i / BUTTONS_PER_ROW;
                int column = i % BUTTONS_PER_ROW;

                Grid.SetRow(gameButton, row);
                Grid.SetColumn(gameButton, column);

                Thickness margin = new Thickness(BUTTON_MARGIN);
                gameButton.Margin = margin;

                shmupsGrid.Children.Add(gameButton);
            }

            shmupsTab.Content = scrollViewer;
            return shmupsTab;
        }

        private TabItem CreateGameTab(string header, string imageFilePath, int numberOfGames, List<string> dirs)
        {
            TabItem tab = new TabItem();
            tab.Header = header;
            tab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4f6082"));

            ScrollViewer scrollViewer = new ScrollViewer();
            Grid g = new Grid();
            scrollViewer.Content = g;
            g.Background = Brushes.Transparent;
            SetGridDefinitions(ref g, numberOfGames);

            for (int i = 0; i < numberOfGames; i++)
            {
                Button b = CreateGameButton(imageFilePath + (i + 1) + ".jpg", dirs[i]); //"pack://application:,,,/images/fangames/"

                int row = i / BUTTONS_PER_ROW;
                int column = i % BUTTONS_PER_ROW;

                Grid.SetRow(b, row);
                Grid.SetColumn(b, column);

                Thickness margin = new Thickness(BUTTON_MARGIN);
                b.Margin = margin;

                g.Children.Add(b);
            }
            tab.Content = g;

            return tab;
        }

        private TabItem CreateMusicRoomTab()
        {
            TabItem musicRoomTab = new TabItem();
            DockPanel d = new DockPanel();

            ScrollViewer scrollViewer = new ScrollViewer();
            Grid songButtonGrid = new Grid();
            scrollViewer.Content = songButtonGrid;
            musicRoomTab.Header = "Music Room";

            // Define text that indicates the music that is being played
            TextBlock text = new TextBlock();
            text.Text = "Welcome to the Music Room!";
            text.FontSize = 18;
            text.FontWeight = FontWeights.Bold;
            text.Foreground = Brushes.WhiteSmoke;

            songButtonGrid = CreateMusicButtons(songButtonGrid, text, 10);

            // Combobox logic
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 350;
            // Style comboBoxStyle = new Style(typeof(ComboBox));
            // comboBoxStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.WhiteSmoke));
            // comboBox.Style = comboBoxStyle;

            foreach (var elem in songInfo)
            {
                comboBox.Items.Add(elem.Key);
            }
            comboBox.SelectedIndex = 4; // Default to MoF for the time being

            // Play / Pause Button styling and logic
            playPause = new Button();
            playPause.IsEnabled = false;
            playPause.Content = "⏵";
            playPause.Style = SetButtonStyle();
            playPause.Foreground = Brushes.WhiteSmoke;
            playPause.FontSize = 45;
            playPause.FontWeight = FontWeights.Bold;
            playPause.Height = 75;
            playPause.Width = 75;
            SetButtonCursorLogic(ref playPause);
            playPause.Click += (s, e) =>
            {
                if (isPlaying)
                {
                    m.StopTrack();
                    isPlaying = false;
                    playPause.Content = "⏵";
                }
                else
                {
                    m.ResumeTrack();
                    isPlaying = true;
                    playPause.Content = "II";
                }
            };

            Button stop = new Button();
            stop.Content = "■";
            stop.Style = SetButtonStyle();
            stop.Foreground = Brushes.WhiteSmoke;
            stop.FontSize = 45;
            stop.FontWeight = FontWeights.Bold;
            stop.Height = 75;
            stop.Width = 75;
            SetButtonCursorLogic(ref stop);
            stop.Click += (s, e) =>
            {
                if (isPlaying)
                {
                    isPlaying = false;
                    playPause.Content = "⏵";
                    playPause.IsEnabled = false;
                    m.StopTrack();
                }
            };


            // Recreating the music buttons
            comboBox.SelectionChanged += (s, e) =>
            {
                int index = comboBox.SelectedIndex;
                songButtonGrid.Children.Clear();
                songButtonGrid.RowDefinitions.Clear();
                songButtonGrid.ColumnDefinitions.Clear();

                songButtonGrid = CreateMusicButtons(songButtonGrid, text, index + 6);
            };

            // TODO: Style the controls better...
            DockPanel controls = new DockPanel();
            DockPanel.SetDock(text, Dock.Top);
            controls.Children.Add(text);
            DockPanel.SetDock(comboBox, Dock.Bottom);
            controls.Children.Add(comboBox);
            controls.Children.Add(playPause);
            controls.Children.Add(stop);

            DockPanel.SetDock(scrollViewer, Dock.Left);
            d.Children.Add(scrollViewer);
            DockPanel.SetDock(controls, Dock.Right);
            d.Children.Add(controls);

            musicRoomTab.Content = d;

            return musicRoomTab;
            
        }

        private Grid CreateMusicButtons(Grid g, TextBlock t, int gameId)
        {
            JsonArray? songNames = songInfo.ElementAt(gameId - 6).Value as JsonArray;
            int index = 0;

            foreach (var song in songNames)
            {
                // Grid definitions
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

                // Button styling logic
                Button b = new Button();
                b.Height = 50;
                b.Width = 550;
                b.Foreground = Brushes.WhiteSmoke;
                b.Style = SetButtonStyle();
                b.Content = song;
                SetButtonCursorLogic(ref b);

                int songIndex = index;
                b.Click += (s, e) =>
                {
                    t.Text = "Now Playing: " + song;
                    m?.StopTrack();
                    if (!playPause.IsEnabled)
                        playPause.IsEnabled = true;
                    isPlaying = true;
                    playPause.Content = "II";
                    MusicInfo i = new MusicInfo(
                        new FileInfo(gameDirs[gameId - 6] + "/thbgm.dat"),
                        new FileInfo(gameDirs[gameId - 6] + "/thbgm.fmt"),
                        songIndex
                    );

                    Task.Run(() => {
                        m = new MusicHandler(i, song.ToString().Contains("(Trance)"));
                        m.PlayTrack();
                    });
                };

                Grid.SetRow(b, index);
                Grid.SetColumn(b, 0);
                index++;

                g.Children.Add(b);
            }

            return g;
        }

        private Button CreateGameButton(string imageResourcePath, string executable)
        {
            Button gameButton = new Button();
            const int BUTTON_SIZE = 300;

            gameButton.Width = BUTTON_SIZE;
            gameButton.Height = BUTTON_SIZE;

            gameButton.Background = Brushes.Transparent;
            gameButton.BorderBrush = Brushes.Transparent;

            gameButton.Content = new Image
            {
                Source = new BitmapImage(new Uri(imageResourcePath))
            };

            // Cursor Logic
            gameButton.MouseMove += (s, e) =>
            {
                Cursor = Cursors.Hand;
            };
            gameButton.MouseLeave += (s, e) =>
            {
                Cursor = Cursors.Arrow;
            };

            // Start the game
            gameButton.Click += (s, e) =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(executable);
                Process.Start(startInfo);
            };

            return gameButton;
        }

        private List<string> GetGameDirectories(string fileName)
        { 
            List<string> dirs = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                    dirs.Add(reader.ReadLine());
            }
            return dirs;
        }

        private JsonObject GetSongNames()
        {
            JsonArray songNames = null;
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("songNamesEN.json"));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) 
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonString = reader.ReadToEnd();
                songInfo = JsonSerializer.Deserialize<JsonObject>(jsonString);
            }
            return songInfo;
        }

        private Style SetButtonStyle()
        {
            Style buttonStyle = new Style(typeof(Button));

            buttonStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.SlateGray)); // Default background color

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;
            buttonStyle.Setters.Add(new Setter(TemplateProperty, template));

            // Create a trigger for MouseOver
            Trigger mouseOverTrigger = new Trigger { Property = IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.SteelBlue)); // Hover background color

            // Add the trigger to the Style
            buttonStyle.Triggers.Add(mouseOverTrigger);

            return buttonStyle;
        }

        private void SetGridDefinitions(ref Grid g, int numberOfGames)
        {
            int numberOfRows = (int)Math.Ceiling((double)numberOfGames / BUTTONS_PER_ROW);

            for (int i = 0; i < numberOfRows; i++)
            {
                g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            }

            for (int i = 0; i < BUTTONS_PER_ROW; i++)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            }
        }
        private void SetButtonCursorLogic(ref Button b)
        {
            b.MouseMove += (s, e) =>
            {
                Cursor = Cursors.Hand;
            };
            b.MouseLeave += (s, e) =>
            {
                Cursor = Cursors.Arrow;
            };
        }
    }
}
