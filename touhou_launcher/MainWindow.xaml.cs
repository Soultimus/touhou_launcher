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

namespace touhou_launcher
{
    public partial class MainWindow : Window
    {
        private MusicHandler m;
        private Grid contentView;
        private Button playPause;
        private JsonObject infoObj;
        private JsonObject gameDirs;
        private JsonObject songInfo;
        private bool isPlaying;

        private const int BUTTONS_PER_ROW = 4;
        private const int BUTTON_MARGIN = 12;

        public MainWindow()
        {
            GetJsonFromFile("info.json", ref infoObj);
            gameDirs = infoObj.ElementAt(0).Value as JsonObject;
            GetJsonFromFile("songNamesEN.json", ref songInfo);
            isPlaying = false;

            InitializeComponent();
            SetupWindow();
        }

        private void SetupWindow()
        {
            Width = 1327;
            Height = 712;
            // Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/images/bg.jpg"))); // background color = #293243
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#293243"));
            ResizeMode = ResizeMode.CanMinimize;

            DockPanel mainView = new DockPanel();

            Grid tabGrid = new Grid();
            tabGrid.Height = 25;
            tabGrid.HorizontalAlignment = HorizontalAlignment.Left;
            tabGrid.VerticalAlignment = VerticalAlignment.Top;
            DockPanel.SetDock(tabGrid, Dock.Top);
            mainView.Children.Add(tabGrid);

            contentView = new Grid();
            Border contentBorder = new Border();
            contentBorder.BorderBrush = Brushes.White;
            contentBorder.BorderThickness = new Thickness(0, 1, 0, 0);
            contentBorder.Child = contentView;
            contentView.VerticalAlignment = VerticalAlignment.Top;
            contentView.HorizontalAlignment = HorizontalAlignment.Left;
            mainView.Children.Add(contentBorder);

            List<UIElement> tabs = new List<UIElement>();
            ScrollViewer mainGamesTab = CreateShmupsTab();
            tabs.Add(mainGamesTab);
            tabs.Add(CreatePC98Tab()); // Make a ScrollView too?
            tabs.Add(CreateGameTab("pack://application:,,,/images/tasofro/", infoObj.ElementAt(2).Value as JsonObject));
            tabs.Add(CreateGameTab("pack://application:,,,/images/spinoffs/", infoObj.ElementAt(3).Value as JsonObject));
            tabs.Add(CreateGameTab("pack://application:,,,/images/fangames/", infoObj.ElementAt(4).Value as JsonObject)); // Hide Scroll bar?
            tabs.Add(CreateMusicRoomTab());

            int index = 0;
            foreach (var tab in tabs)
            {
                Button b = new Button();
                b.Content = infoObj.ElementAt(index).Key;
                b.Width = 115;
                b.Style = SetButtonStyle();
                b.Foreground = Brushes.WhiteSmoke;

                b.Click += (s, e) => SwitchTab(tab);
                b.MouseLeave += CustomMouseLeave;
                b.MouseMove += CustomMouseMove;

                Grid.SetColumn(b, index);
                tabGrid.Children.Add(b);
                tabGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                index++;
            }            

            contentView.Children.Add(mainGamesTab); // Default to main games

            TabControl tabControl = new TabControl();
            tabControl.Background = Brushes.Transparent;

            Grid grid = new Grid();
            grid.Children.Add(tabControl);

            Content = mainView;
        }

        private ScrollViewer CreateShmupsTab()
        {
            TabItem tab = new TabItem();
            tab.Header = "Shmups";
            tab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4f6082"));
            tab.Width = 100;

            ScrollViewer scrollViewer = new ScrollViewer();
            Grid shmupsGrid = new Grid();
            scrollViewer.Content = shmupsGrid;
            shmupsGrid.Background = Brushes.Transparent;
            SetGridDefinitions(ref shmupsGrid, gameDirs.Count);

            int index = 0;
            foreach (var path in gameDirs)
            {
                int gameIndex = index + 6;
                string gamePath = path.Value + ((gameIndex < 6) ? $"/th0{gameIndex}.exe" : $"/th{gameIndex}.exe");
                Button gameButton = CreateGameButton($"pack://application:,,,/images/main/th{gameIndex}cover.jpg", gamePath);

                int row = index / BUTTONS_PER_ROW;
                int column = index % BUTTONS_PER_ROW;

                Grid.SetRow(gameButton, row);
                Grid.SetColumn(gameButton, column);

                Thickness margin = new Thickness(BUTTON_MARGIN);
                gameButton.Margin = margin;

                shmupsGrid.Children.Add(gameButton);
                index++;
            }

            return scrollViewer;
        }

        private Grid CreatePC98Tab()
        {
            Grid g = new Grid();
            g.Background = Brushes.Transparent;
            JsonObject paths = infoObj.ElementAt(1).Value as JsonObject;
            int actualGameNumber = paths.Count - 1;
            SetGridDefinitions(ref g, actualGameNumber);

            for (int i = 0; i < actualGameNumber; i++)
            {
                string emulatorPath = paths.ElementAt(5).Value.ToString();
                string flag = "";
                string gamePath = paths.ElementAt(i).Value.ToString();
                if (gamePath.Contains(".hdi")) // Hard Disk Image
                    flag = " -hdd ";
                else if (gamePath.Contains(".d88")) // Disk Image
                    flag = " -fd ";

                Button gameButton = CreateGameButton($"pack://application:,,,/images/pc/{i + 1}.jpg", "\"" + emulatorPath + "\"" + flag + "\"" + gamePath + "\"");
                gameButton.Background = Brushes.White;

                int row = i / BUTTONS_PER_ROW;
                int column = i % BUTTONS_PER_ROW;

                Grid.SetRow(gameButton, row);
                Grid.SetColumn(gameButton, column);

                Thickness margin = new Thickness(BUTTON_MARGIN);
                gameButton.Margin = margin;

                g.Children.Add(gameButton);

            }

            return g;
        }

        private ScrollViewer CreateGameTab(string imageFilePath, JsonObject dirs)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            if (dirs.Count < 4)
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            Grid g = new Grid();
            scrollViewer.Content = g;
            g.Background = Brushes.Transparent;
            SetGridDefinitions(ref g, dirs.Count);

            int index = 0;
            foreach (var pair in dirs)
            {
                Button b = CreateGameButton(imageFilePath + (index + 1) + ".jpg", pair.Value.ToString());

                int row = index / BUTTONS_PER_ROW;
                int column = index % BUTTONS_PER_ROW;

                Grid.SetRow(b, row);
                Grid.SetColumn(b, column);

                Thickness margin = new Thickness(BUTTON_MARGIN);
                b.Margin = margin;

                g.Children.Add(b);
                index++;
            }

            return scrollViewer;
        }

        private DockPanel CreateMusicRoomTab()
        {
            DockPanel d = new DockPanel();

            ScrollViewer scrollViewer = new ScrollViewer();
            Grid songButtonGrid = new Grid();
            scrollViewer.Content = songButtonGrid;

            // Define text that indicates the music that is being played
            TextBlock text = new TextBlock();
            text.Text = "Welcome to the Music Room!";
            text.FontSize = 18;
            text.FontWeight = FontWeights.Bold;
            text.Foreground = Brushes.WhiteSmoke;
            text.HorizontalAlignment = HorizontalAlignment.Center;

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
            playPause.MouseMove += CustomMouseMove;
            playPause.MouseLeave += CustomMouseLeave;
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
            stop.MouseMove += CustomMouseMove;
            stop.MouseLeave += CustomMouseLeave;
            stop.Click += (s, e) =>
            {
                if (isPlaying)
                {
                    isPlaying = false;
                    playPause.Content = "⏵";
                    playPause.IsEnabled = false;
                    text.Text = "Welcome to the Music Room!";
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

            DockPanel controls = new DockPanel();
            controls.Height = 150;
            controls.VerticalAlignment = VerticalAlignment.Top;

            DockPanel.SetDock(text, Dock.Top);
            controls.Children.Add(text);
            DockPanel.SetDock(comboBox, Dock.Bottom);
            controls.Children.Add(comboBox);

            DockPanel buttonDock = new DockPanel();
            buttonDock.Width = 175;
            DockPanel.SetDock(playPause, Dock.Left);
            buttonDock.Children.Add(playPause);
            DockPanel.SetDock(stop, Dock.Right);
            buttonDock.Children.Add(stop);
            controls.Children.Add(buttonDock);

            DockPanel.SetDock(scrollViewer, Dock.Left);
            d.Children.Add(scrollViewer);
            DockPanel.SetDock(controls, Dock.Right);
            d.Children.Add(controls);

            return d;
        }

        private void SwitchTab(UIElement elem)
        {
            if (!contentView.Children.Contains(elem) || contentView.Children[0] != elem)
            {
                contentView.Children.Clear();
                contentView.Children.Add(elem);
            }
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
                b.MouseLeave += CustomMouseLeave;
                b.MouseMove += CustomMouseMove;

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
                        new FileInfo(gameDirs.ElementAt(gameId - 6).Value + "/thbgm.dat"),
                        new FileInfo(gameDirs.ElementAt(gameId - 6).Value + "/thbgm.fmt"),
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
            gameButton.MouseMove += CustomMouseMove;
            gameButton.MouseLeave += CustomMouseLeave;

            // Start the game
            gameButton.Click += (s, e) =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(executable);
                Process.Start(startInfo);
            };

            return gameButton;
        }

        private void GetJsonFromFile(string resourceFilename, ref JsonObject obj)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceFilename));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonString = reader.ReadToEnd();
                obj = JsonSerializer.Deserialize<JsonObject>(jsonString);
            }
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

        private void CustomMouseMove(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void CustomMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
