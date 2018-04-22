using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OpenairLineupExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "https://www.openairsg.ch/line-up/";
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var node = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'portfolio-block')]");

            var artistList = new List<Artist>();

            foreach(var child in node.ChildNodes)
            {
                var artistId = child.GetAttributeValue("id", "");
                var name = "";
                var timeAndPlace = "";

                var link = child.FirstChild.FirstChild;
                foreach(var linkChildren in link.ChildNodes)
                {
                    if(linkChildren.GetAttributeValue("class", "") == "name")
                    {
                        name = HtmlEntity.DeEntitize(linkChildren.FirstChild.InnerText);
                    }
                    if (linkChildren.GetAttributeValue("class", "") == "time-n-place")
                    {
                        timeAndPlace = HtmlEntity.DeEntitize(linkChildren.FirstChild.InnerText);
                    }
                }
                var timePlace = timeAndPlace.Split(", ");
                var day = timePlace[0];
                var time = timePlace[1];
                var dateTime = DateTime.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture);
                if(dateTime.Hour < 8)
                {
                    day = day.Replace("Samstag", "Sonntag")
                        .Replace("Freitag", "Samstag")
                        .Replace("Donnerstag", "Freitag");
                }

                var artist = new Artist()
                {
                    ArtistId = artistId,
                    Name = name,
                    Day = day,
                    Time = dateTime,
                    Place = timePlace[2]
                };
                artistList.Add(artist);
            }
            var artistsSorted = artistList.OrderBy(a => a.Day).ThenBy(a => a.Time);
            var csvText = "";
            foreach(var artist in artistsSorted)
            {
                csvText += artist.Day + ";" + artist.Time.ToShortTimeString() + ";" + artist.Place + ";" + artist.Name + "\n";
            }
            File.WriteAllText("openairsg-lineup.csv", csvText);
            Console.Write(csvText);
            Console.ReadLine();
        }
    }
}
