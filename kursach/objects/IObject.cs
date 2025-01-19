using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace kursach.objects
{
    public interface IObject
    {
        ObjectType Type { get; set; }
        Vector2 Position { get; set; }
        void Destroy();
        BitmapImage SetSprite();
    }
}
