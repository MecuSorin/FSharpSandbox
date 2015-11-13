using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSL_Sprache
{
    class Mobil
    {
        public double AngleInRadians { get; set; }
        public double Momentum { get; set; }
        public double LastMove { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        private Point toPosition()
        {
            return new Point((int)X, (int)Y);
        }

        public IEnumerable<Point> PerformCommands(IEnumerable<Command> commands)
        {
            foreach (var command in commands)   
            {
                var forward = command as CommandForward;
                if (null != forward)
                {
                    yield return toPosition();
                    PerformCommandForward(forward.Distance);
                    yield return toPosition();
                    continue;
                }
                var turn = command as CommandTurn;
                if (null != turn)
                {
                    AngleInRadians += turn.AngleInDegrees * (Math.PI / 180d);
                    continue;
                }
                var momentum = command as CommandMomentum;
                if (null != momentum)
                {
                    Momentum = momentum.Ponder;
                    continue;
                }
                var go = command as CommandGo;
                if (null != go)
                {
                    yield return toPosition();
                    PerformCommandForward(LastMove * Momentum);
                    yield return toPosition();
                    continue;
                }
                var repeat = command as CommandRepeat;
                if (null != repeat)
                {
                    for (var i = 0; i < repeat.NumberOfTimes; i++)
                        foreach (var subc in PerformCommands(repeat.CommandsToRepeat))
                            yield return subc;
                    continue;
                }
                throw new ArgumentException("Command provided si not supported yet");
            }
        }

        private void PerformCommandForward(double distance)
        {
            X += distance * Math.Cos(AngleInRadians);
            Y += distance * Math.Sin(AngleInRadians);
            LastMove = distance;
        }
    }
}