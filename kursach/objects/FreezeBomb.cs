using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;


namespace kursach.objects
{
    public class FreezeBomb : IObject
    {
        public ObjectType Type { get; set; }
        public Vector2 Position { get; set; }
        public double duration = 5; 

        int points = 20;

        Controller controller;
        public FreezeBomb(ObjectType type, Vector2 position, Controller controller)
        {
            Type = type;
            Position = position;
            this.controller = controller;
        }

        public void Destroy()
        {
            if (Type == ObjectType.Empty)
                return;
            Type = ObjectType.Empty;
            controller.AddScore(points);
            Freeze();
        }

        private void Freeze()
        {
            controller.Freeze(duration);
        }

        public BitmapImage SetSprite()
        {
            Uri uriSource;
            switch (Type)
            {
                case ObjectType.Empty:
                    uriSource = new Uri("img/.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Red:
                    uriSource = new Uri("img/bomb/BRed.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Blue:
                    uriSource = new Uri("img/bomb/BBlue.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Green:
                    uriSource = new Uri("img/bomb/BGreen.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Yellow:
                    uriSource = new Uri("img/bomb/BYellow.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Brown:
                    uriSource = new Uri("img/bomb/BBrown.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                default:
                    uriSource = new Uri("img/bomb/BRed.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
            }
        }
    }
}
