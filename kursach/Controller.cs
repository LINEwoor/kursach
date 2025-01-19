using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using kursach.objects;

namespace kursach
{
    public class Controller
    {
        Random rnd = new Random();
        int gridSize;
        GridManager gameMap;
        Button[,] buttons;
        IObject[,] objects;
        IObject selectedObject;
        IObject lastSelectObject;
        Button selectedButton;
        int selectedRow;
        int selectedCol;
        Canvas buttonCanvas;
        Canvas imagesCanvas;
        int buttonSize = 100;
        int points;
        bool isPlaying = false;
        ProgressBar progressBar;
        public double PGperSec = 5;
        public bool isFreeze { get; set; }
        private DispatcherTimer freezeTimer;


        public Controller(int gridsize, Canvas ButtonCanvas, Canvas ImageCanvas, ProgressBar ProgressBar)
        {

            gridSize = gridsize;
            gameMap = new GridManager(gridsize,buttons,objects,this);
            buttonCanvas = ButtonCanvas;
            progressBar = ProgressBar;
            objects = new IObject[gridSize, gridSize];
            buttons = new Button[gridSize, gridSize];
            imagesCanvas = ImageCanvas;

            freezeTimer = new DispatcherTimer();
            freezeTimer.Interval = TimeSpan.FromSeconds(1);
            freezeTimer.Tick += FreezeTimer_Tick;
        }

        public void DestroyVLine(Vector2 bombPosition)
        {
            int row = (int)bombPosition.Y;

            for (int i = 0; i < gridSize; i++)
            {
                IObject obj = objects[i, row];
                if (obj != null && obj.Type != ObjectType.Empty)
                {
                    obj.Destroy();
                }
            }

            Visualization();

            Task.Delay(200).ContinueWith(_ => ShiftObjectsDown());
        }

        public void DestroyHLine(Vector2 bombPosition)
        {
            int column = (int)bombPosition.X;

            for (int i = 0; i < gridSize; i++)
            {
                IObject obj = objects[column, i];
                if (obj != null && obj.Type != ObjectType.Empty)
                {
                    obj.Destroy();
                }
            }

            Visualization();

            Task.Delay(200).ContinueWith(_ => ShiftObjectsDown());
        }

        public void Freeze(double duration)
        {
            isFreeze = true;
            freezeTimer.Start();
            Task.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(_ =>
            {
                isFreeze = false;
                freezeTimer.Stop();
            });
        }

        private void FreezeTimer_Tick(object sender, EventArgs e)
        {
        }
        public void UpdateObjectPositions()
        {
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (objects[row, col] != null)
                    {
                        objects[row, col].Position = new Vector2(row, col);
                    }
                }
            }
        }


        public void GameStatus(bool status)
        {
            isPlaying = status;
        }

        public IObject[,] GetObjects()
        {
            return objects;
        }

        public bool GetGameStatus() { return isPlaying; }

        public int getPoints() { return points; }
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

                    gameButton.Click += GridClick;

                    Grid.SetRow(gameButton, row);
                    Grid.SetColumn(gameButton, col);
                    mapGrid.Children.Add(gameButton);

                    buttons[row, col] = gameButton;

                }
            }
        }

        private void SwapObjects(int row1, int col1, int row2, int col2)
        {
            var temp = objects[row1, col1];
            objects[row1, col1] = objects[row2, col2];
            objects[row2, col2] = temp;

            objects[row1, col1].Position = new Vector2(row1, col1);
            objects[row2, col2].Position = new Vector2(row2, col2);

            Visualization();

            var firstList = GetMatchList(row1, col1);
            var secondList = GetMatchList(row2, col2);

            bool firstMatch = CheckForMatches(firstList);
            bool secondMatch = CheckForMatches(secondList);

            if (!firstMatch && !secondMatch)
            {
                SwapObjectsTemp(row1, col1, row2, col2);
            }
            else
            {
                if (firstMatch) DestroyMatches(firstList, firstList.Count);
                if (secondMatch) DestroyMatches(secondList, secondList.Count);
                ShiftObjectsDown();
            }
        }

        private void GridClick(object sender, RoutedEventArgs e)
        {
            Visualization();
            Button clickedButton = sender as Button;
            int clickedRow = Grid.GetRow(clickedButton);
            int clickedCol = Grid.GetColumn(clickedButton);

            IObject clickedObject = objects[clickedRow, clickedCol];
            if (selectedObject == null)
            {
                selectedButton = clickedButton;
                selectedObject = clickedObject;
                lastSelectObject = clickedObject;

                selectedRow = clickedRow;
                selectedCol = clickedCol;
                Select();
            }
            else
            {
                int x1 = Grid.GetRow(selectedButton);
                int y1 = Grid.GetColumn(selectedButton);
                int x2 = Grid.GetRow(clickedButton);
                int y2 = Grid.GetColumn(clickedButton);

                UnSelected();
                lastSelectObject = clickedObject;
                selectedObject = null;

                if ((x1 == x2 && Math.Abs(y1 - y2) == 1) || (y1 == y2 && Math.Abs(x1 - x2) == 1))
                {
                    if (CanMatch(x1, y1, x2, y2) && GetGameStatus())
                    {
                        SwapObjects(x1, y1, x2, y2);
                    }
                    
                }
            }
        }
        private void SwapObjectsTemp(int row1, int col1, int row2, int col2)
        {
            IObject temp = objects[row1, col1];
            objects[row1, col1] = objects[row2, col2];
            objects[row2, col2] = temp;
        }
        public void FillGrid()
        {
            var ObjectTypes = Enum.GetValues(typeof(ObjectType));

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    List<IObject> matchList = GetMatchList(i, j);

                    if (objects[i, j] == null || objects[i, j].Type == ObjectType.Empty)
                    {
                        ObjectType randomType;
                        do
                        {
                            randomType = (ObjectType)ObjectTypes.GetValue(rnd.Next(1, ObjectTypes.Length));
                            objects[i, j] = new GameObject(randomType, new Vector2(i, j), this);
                            matchList = GetMatchList(i, j);
                        } while (CheckForMatches(matchList));
                    }
                }
            }
            if (!GetGameStatus()) GameStatus(true);
            Visualization();
        }

        public void Visualization()
        {
            UpdateObjectPositions();

            imagesCanvas.Children.Clear();
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (objects[i, j] == null) continue;
                    var image = new Image() { Source = objects[i, j].SetSprite(), Width = 100, IsHitTestVisible = false };
                    Canvas.SetTop(image, buttonSize * i);
                    Canvas.SetLeft(image, buttonSize * j);
                    imagesCanvas.Children.Add(image);
                }
            }
        }
        private bool CanMatch(int row1, int col1, int row2, int col2)
        {
            SwapObjectsTemp(row1, col1, row2, col2);

            bool hasMatch = CheckForMatches(GetMatchList(row1, col1)) || CheckForMatches(GetMatchList(row2, col2));

            SwapObjectsTemp(row1, col1, row2, col2);
            return hasMatch;
        }
        public List<IObject> GetMatchList(int x, int y)
        {
            var verticalLine = new List<IObject>();
            var horizontalLine = new List<IObject>();

            IObject currentObject = objects[x, y];

            if (currentObject == null) return new List<IObject>();

            for (int i = x - 1; i >= 0; i--) // вверх
            {
                if (objects[i, y] != null && objects[i, y].Type == currentObject.Type)
                {
                    verticalLine.Add(objects[i, y]);
                }
                else break;
            }

            for (int i = x + 1; i < gridSize; i++) // вниз
            {
                if (objects[i, y] != null && objects[i, y].Type == currentObject.Type)
                {
                    verticalLine.Add(objects[i, y]);
                }
                else break;
            }

            for (int i = y - 1; i >= 0; i--) // влево
            {
                if (objects[x, i] != null && objects[x, i].Type == currentObject.Type)
                {
                    horizontalLine.Add(objects[x, i]);
                }
                else break;
            }

            for (int i = y + 1; i < gridSize; i++) // вправо
            {
                if (objects[x, i] != null && objects[x, i].Type == currentObject.Type)
                {
                    horizontalLine.Add(objects[x, i]);
                }
                else break;
            }

            if (horizontalLine.Count < 2) horizontalLine.Clear();
            if (verticalLine.Count < 2) verticalLine.Clear();

            List<IObject> matchList = new List<IObject>();

            matchList.Add(currentObject);

            matchList.AddRange(horizontalLine);
            matchList.AddRange(verticalLine);
            return matchList;
        }

        private bool CheckForMatches(List<IObject> matchList)
        {
            return matchList != null && matchList.Count >= 3;
        }

        private void DestroyMatches(List<IObject> matchList, int matchCount)
        {
            if (matchList == null) return;

            bool bombIsInserted = false;

            foreach (IObject obj in matchList)
            {
                if (matchCount > 3 && !bombIsInserted)
                {
                    bombIsInserted = true;
                    Vector2 bombPosition = new Vector2((int)obj.Position.X, (int)obj.Position.Y);

                    if (obj.Type == ObjectType.Blue)
                    {
                        objects[(int)obj.Position.X, (int)obj.Position.Y] =
                            new FreezeBomb(obj.Type, bombPosition, this);
                    }
                    else if (obj.Type == ObjectType.Yellow)
                    {
                        objects[(int)obj.Position.X, (int)obj.Position.Y] =
                            new HBomb(obj.Type, bombPosition, this);
                    }
                    else if(obj.Type == ObjectType.Red)
                    {
                        objects[(int)obj.Position.X, (int)obj.Position.Y] =
                            new VBomb(obj.Type, bombPosition, this);
                    }
                    else if (obj.Type == ObjectType.Green)
                    {
                        objects[(int)obj.Position.X, (int)obj.Position.Y] =
                            new HBomb(obj.Type, bombPosition, this);
                    }
                    else if (obj.Type == ObjectType.Brown)
                    {
                        objects[(int)obj.Position.X, (int)obj.Position.Y] =
                            new VBomb(obj.Type, bombPosition, this);
                    }
                }

                if (obj != null && obj.Type != ObjectType.Empty)
                {
                    obj.Destroy();
                }
            }

            Visualization();

            Task.Delay(200).ContinueWith(_ => ShiftObjectsDown());
        }


        public List<List<IObject>> MatchAll()
        {
            bool isMatched = false;
            List<List<IObject>> matches = new List<List<IObject>>();
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    List<IObject> matchList = GetMatchList(i, j);
                    if (CheckForMatches(matchList))
                    {
                        matches.Add(matchList);
                    }
                }
            }
            return matches;
        }

        private void Select()
        {
            selectedButton.Background = new SolidColorBrush(Colors.Red);
            Trace.WriteLine(selectedObject.Position);
            Trace.WriteLine(selectedObject.Type.ToString());

        }

        private void UnSelected()
        {
            Color color = ((selectedRow + selectedCol) % 2 == 0) ? Colors.CadetBlue : Colors.PeachPuff;
            selectedButton.Background = new SolidColorBrush(color);
        }

        public void AddScore(int value)
        {
            if (isPlaying)
            {
                points += value;
                progressBar.Value += value * 0.1;
            }
        }
        private async Task ShiftObjectsDown()
        {
            for (int row = gridSize - 1; row >= 0; row--)
            {
                for (int col = gridSize - 1; col >= 0; col--)
                {
                    if (objects[row, col] != null && objects[row, col].Type == ObjectType.Empty)
                    {
                        for (int r = row - 1; r >= 0; r--)
                        {
                            if (objects[r, col] != null && objects[r, col].Type != ObjectType.Empty)
                            {
                                GameStatus(false);
                                var temp = objects[row, col];
                                objects[row, col] = objects[r, col];
                                objects[row,col].Position = objects[r, col].Position;
                                objects[r, col] = temp;
                                objects[r, col].Type = ObjectType.Empty;
                                Visualization();
                                await Task.Delay(200);
                                break;
                            }
                        }
                    }
                }
            }
            GameStatus(true);
            List<List<IObject>> matches = MatchAll();
            if (matches != null && matches.Count > 0)
            {
                foreach (var matchList in matches)
                {
                    int matchCount = CheckForMatches(matchList) ? matchList.Count : 0;
                    if (matchCount > 0)
                    {
                        DestroyMatches(matchList, matchCount);
                    }
                }
            }
            FillGrid();
        }

    }
}
