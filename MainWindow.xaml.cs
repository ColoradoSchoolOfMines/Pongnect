using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace ACMX.Games.Pongnect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Game game;

        private Timer updateDrawTimer = new Timer();
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void WindowLoaded(Object o, RoutedEventArgs e)
        {
            System.Console.WriteLine("Window Loaded!");

            KinectSensor sensor = null;
            foreach (KinectSensor ks in KinectSensor.KinectSensors)
            {
                if (ks.Status == KinectStatus.Connected)
                {
                    sensor = ks;
                    break;
                }
            }

            // Quit if no kinect
            if (sensor == null)
            {
                System.Console.WriteLine("No Kinect Found");
            }

            game = new Game(sensor, new Rect(0, 0, layoutGrid.RenderSize.Width, layoutGrid.RenderSize.Height));
            updateDrawTimer.Interval = 33; // 33ms
            updateDrawTimer.Tick += updateDraw;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Canvas.Source = this.imageSource;

            // Start the update/draw timer loop
            updateDrawTimer.Start();
        }

        private void updateDraw(object sender, EventArgs e)
        {
            System.Console.WriteLine("Update Draw");
            game.Update();
            using (DrawingContext dc = drawingGroup.Open())
            {
                game.Draw(dc);
            }
        }
    }
}
