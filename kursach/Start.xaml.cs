using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using static System.Net.Mime.MediaTypeNames;

namespace kursach
{
    public partial class Start : Window
    {
        public Start()
        {
            InitializeComponent();
            LoadFromJson();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void LoadFromJson()
        {
            string filePath = Environment.CurrentDirectory + "\\scores.json";

            if (File.Exists(filePath))
            {
                string data = File.ReadAllText(filePath);
                var records = JsonSerializer.Deserialize<List<Record>>(data);

                Leaderboard.ItemsSource = records;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory + "\\scores.json";


            List<Record> records = new List<Record>();

            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
            }

            records.Add(new Record
            {
                Name = "   ",
                Score = 0,
            });

            string resetRecords = JsonSerializer.Serialize(records);

            File.WriteAllText(filePath, resetRecords);

            Leaderboard.ItemsSource = records;
        }
    }
}
