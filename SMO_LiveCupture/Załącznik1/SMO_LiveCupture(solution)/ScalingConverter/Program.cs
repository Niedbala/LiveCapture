using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalingConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var directoryPath = @"C:\Roboczy\toconvert";
            var files = Directory.GetFiles(directoryPath).ToList();

            files.ForEach(f =>
            {
                var unconvertedLines = File.ReadAllLines(f);
               
                var newFileName = f.Replace(".txt", ".tsv");

               var convertedLines = unconvertedLines.Select(ul =>
                {
                    var splitted = ul.Split('\t').ToList();
                    var last = splitted.Last();
                    splitted.Remove(last);

                    var convertedCells = splitted.Select(c => Convert.ToInt32(c, 16).ToString()).ToList();
                    convertedCells.Add(last);
                    var convertedLine = string.Join("\t", convertedCells.ToArray());
                    return convertedLine;
                }).ToList();

                using (StreamWriter sr = new StreamWriter(newFileName,false))
                {
                  convertedLines.ForEach(cl=>sr.WriteLine(cl));
                };
            });
        }
    }
}
