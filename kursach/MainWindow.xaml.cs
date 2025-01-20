using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Text.Json;
using System;
using System.IO;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;
using System.Net.Http.Json;

namespace kursach
{
    public partial class MainWindow : Window
    {
        int gridSize = 8;
        Controller controller;
        private DispatcherTimer timer;
        private TimeSpan time;
        private string name;

        public MainWindow(string name)
        {
            InitializeComponent();
            controller = new Controller(gridSize, ButtonCanvas, ImageCanvas, progressBar);
            controller.CreateGameGrid(GridLayout);
            controller.FillGrid();
            Height = (gridSize + 1) * 100 + 40;
            Width = gridSize * 100 + 15;

            this.name = name;

            time = TimeSpan.Zero;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            controller.Visualization();
            time = time.Add(timer.Interval);
            Score.Content = $"Score: {controller.getPoints()}";
            if (controller.GetGameStatus() && !controller.isFreeze)
            {
                progressBar.Value -= controller.PGperSec / 10;
                progressBar.Foreground = new SolidColorBrush(Colors.Green);
            }
            if (controller.isFreeze)
            {
                progressBar.Foreground = new SolidColorBrush(Colors.Blue);
            }
            if (progressBar.Value <= 0)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            SaveToJson();
            timer.Stop();
            Start startWindow = new Start();
            startWindow.Show();
            Close();
        }

        private void SaveToJson()
        {
            string filePath = System.IO.Directory.GetCurrentDirectory() + "\\scores.json";

            List<Record> records = new List<Record>();

            if (File.Exists(filePath))
            {
                
                string jsonContent = File.ReadAllText(filePath);

                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    records = JsonSerializer.Deserialize<List<Record>>(jsonContent) ?? new List<Record>();
                }
            }

            records.Add(new Record
            {
                Name = name,
                Score = (int)controller.getPoints()
            });

            string updatedRecords = JsonSerializer.Serialize(records);

            File.WriteAllText(filePath, updatedRecords);
        }

    }
}