using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using kursach.objects;
using System.Diagnostics;

namespace kursach
{
    public class GridManager
    {
        private Random rnd = new Random();
        private int gridSize;
        private Button[,] buttons;
        private IObject[,] objects;
        private int buttonSize = 100;
        Controller controller;

        public GridManager(int gridSize, Button[,] buttons, IObject[,] objects, Controller controller)
        {
            this.gridSize = gridSize;
            this.buttons = buttons;
            this.objects = objects;
            this.controller = controller;
        }

        public void CreateGameGrid(Grid mapGrid)
        {
            for (int i = 0; i < gridSize; i++)
            {
                mapGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(buttonSize)
                });

                mapGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(buttonSize)
                });
            }

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Color buttonColor = (row + col) % 2 == 0 ? Colors.CadetBlue : Colors.PeachPuff;
                    Button gameButton = new Button
                    {
                        Background = new SolidColorBrush(buttonColor),
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0),
                        DataContext = new Vector2(row, col),
                    };

                    gameButton.Click += (sender, e) => GridClick(sender, e, row, col);

                    Grid.SetRow(gameButton, row);
                    Grid.SetColumn(gameButton, col);
                    mapGrid.Children.Add(gameButton);

                    buttons[row, col] = gameButton;
                }
            }
        }

        private void GridClick(object sender, RoutedEventArgs e, int clickedRow, int clickedCol)
        {
        }

        public void FillGrid()
        {
            var ObjectTypes = Enum.GetValues(typeof(ObjectType));

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    List<IObject> matchList = controller.GetMatchList(i, j);

                    if (objects[i, j] == null || objects[i, j].Type == ObjectType.Empty)
                    {
                        ObjectType randomType;
                        do
                        {
                            randomType = (ObjectType)ObjectTypes.GetValue(rnd.Next(1, ObjectTypes.Length));
                            objects[i, j] = new GameObject(randomType, new Vector2(i, j), controller);
                            matchList = controller.GetMatchList(i, j);
                        } while (CheckForMatches(matchList));
                    }
                }
            }
        }

        private bool CheckForMatches(List<IObject> matchList)
        {
            return matchList != null && matchList.Count >= 3;
        }
    }
}
