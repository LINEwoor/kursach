using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace kursach.objects
{
    public class GameObject : IObject
    {
        public ObjectType Type { get; set; }
        public Vector2 Position { get; set; }

        int points = 20;

        Image sprite;
        Controller controller;
        public GameObject(ObjectType type, Vector2 position, Controller controller)
        {
            Type = type;
            Position = position;
            this.controller = controller;
        }

        public async void Destroy()
        {
            if (Type == ObjectType.Empty)
                return;
            Type = ObjectType.Empty;
            controller.AddScore(points);

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
                    uriSource = new Uri("img/base/Red.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Blue:
                    uriSource = new Uri("img/base/Blue.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Green:
                    uriSource = new Uri("img/base/Green.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Yellow:
                    uriSource = new Uri("img/base/yellow.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                case ObjectType.Brown:
                    uriSource = new Uri("img/base/Brown.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
                default:
                    uriSource = new Uri("img/base/Red.png", UriKind.Relative);
                    return new BitmapImage(uriSource);
            }
        }



    }
}
