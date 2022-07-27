using StreamThreads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static StreamThreads.StreamExtensions;

namespace StreamThreadsWPFSample
{
    internal class Example
    {
        private MainWindow _owner;
        public StreamState? State;

        public Example(MainWindow owner)
        {
            _owner = owner;
        }

        public IEnumerable<StreamState> StartState()
        {
            double x = _owner.ActualWidth / 2;
            double y = _owner.ActualHeight / 2;

            yield return MovingBox(40, 5, 0, _owner.ActualWidth / 2, _owner.ActualHeight / 2).Background();

            yield return WaitForever;
        }

        private IEnumerable<StreamState> MovingBox(int maxspawns, int maxlevel, int level, double x, double y)
        {
            var aw = _owner.ActualWidth;
            var ah = _owner.ActualHeight;
            var bw = 20; // box width
            var bh = 20; // box height

            if (level >= maxlevel) yield break;

            var rnd = new Random();

            Shape box;
            switch (rnd.Next(0, 2))
            {
                case 0: box = new Rectangle(); break;
                case 1: box = new Ellipse(); break;
                case 2: box = new Polygon() { }; break;
                case 3: box = new Line(); break;
                default: box = new Rectangle(); break;
            };

            box.Width = bw;
            box.Height = bh;

            //box.Stroke = new SolidColorBrush(Colors.Black);
            //box.StrokeThickness = 10;
            box.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255)));
            // Set Canvas position    
            // Add Rectangle to Canvas    
            _owner.scene.Children.Add(box);

            double dx = 0.1;
            double dy = 0.1;

            while (true)
            {
                dx += (rnd.NextDouble() - 0.5) * 5.5;
                dy += (rnd.NextDouble() - 0.5) * 5.5;
                if (x < 0) { dx = Math.Abs(dx); x = 0; }
                if (y < 0) { dy = Math.Abs(dy); y = 0; }
                if (x > aw - bw) { dx = -Math.Abs(dx); x = 0; x = aw - bw; }
                if (y > ah - bh) { dy = -Math.Abs(dy); y = ah - bh; }

                x += dx;
                y += dy;
                Canvas.SetLeft(box, x);
                Canvas.SetTop(box, y);

                if (rnd.NextDouble() > 0.99)
                {
                    if (maxspawns > 0)
                    {
                        maxspawns--;
                        yield return MovingBox(maxspawns + 1, maxlevel, level + 1, x, y).Background();
                    }
                }

                yield return OK;

            }
        }
    }
}
