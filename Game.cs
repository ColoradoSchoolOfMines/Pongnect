using ACMX.Games.Pongnect;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pongnect
{
    class Game
    {
        public Ball ball { get; private set; }
        public Paddle paddleLeft { get; private set; }
        public Paddle paddleRight { get; private set; }
        public int scoreLeft { get; private set; }
        public int scoreRight {get; private set; }

        private int playerLeftId, playerRightId;

        private KinectSensor sensor;

        public Game(KinectSensor sensor)
        {
            // TODO: Setup kinect callbacks
            new KinectSensorChooser();
            new InteractionStream(null, null);
        }

    }
}
