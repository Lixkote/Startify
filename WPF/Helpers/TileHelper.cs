using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WPF.Helpers
{
    internal class TileHelper
    {
        public void CreateAndLoadTileDirectory()
        {
            string folderPath = Environment.GetEnvironmentVariable("programdata") + @"\Startify\Tiles";

            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Tile folder created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating tiles folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Tiles folder already exists.");
            }
        }
    }
}
