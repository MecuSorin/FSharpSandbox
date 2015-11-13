using Sprache;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace DSL_Sprache
{
    public  class HtmlGenerator
    {
        const int CANVAS_HALF_SIZE = 1000;
        const string path = "pseudoFractal.html";

        public static string GenerateHtml(string commands)
        {
            var errorMessage = string.Empty;
            var htmlLines = new StringBuilder();
            var result = Parser.Get.TryParse(commands);
            if (!result.WasSuccessful)
                errorMessage = result.Message;
            else
            {
                var points = new Mobil() { X = CANVAS_HALF_SIZE, Y = CANVAS_HALF_SIZE }.PerformCommands(result.Value);
                points.ToObservable().Buffer(2).Subscribe(t =>
                    htmlLines.AppendFormat("<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" stroke=\"black\" />\n",
                        t[0].X, t[0].Y, t[1].X, t[1].Y));
            }

            var html = new StringBuilder("<html>< body >< h1 > C# snail!</h1>");
            html.AppendFormat("{0}<svg width=\"{1}\" height=\"{1}\">{2}</svg></body></html>", errorMessage, 2*CANVAS_HALF_SIZE, htmlLines.ToString());
            return html.ToString();
        }

        public static void Test(string commands)
        {
            File.WriteAllText(path, GenerateHtml(commands));
            System.Diagnostics.Process.Start(path);
        }
    }
}
