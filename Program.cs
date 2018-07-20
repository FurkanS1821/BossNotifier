using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

namespace BossBot
{
    public static class Program
    {
        public static ulong ChannelId = 337145835621974017;
        public static Dictionary<(DayOfWeek, string), string> Times = new Dictionary<(DayOfWeek, string), string>
        {
            [(DayOfWeek.Monday, "01:00")] = "Karanda",
            [(DayOfWeek.Monday, "11:00")] = "Nouver",
            [(DayOfWeek.Monday, "16:00")] = "Kutum/Kzarka",
            [(DayOfWeek.Monday, "20:00")] = "Nouver",
            [(DayOfWeek.Monday, "23:15")] = "Kzarka",
            [(DayOfWeek.Tuesday, "01:00")] = "Kutum",
            [(DayOfWeek.Tuesday, "11:00")] = "Kzarka",
            [(DayOfWeek.Tuesday, "16:00")] = "Nouver/Kutum",
            [(DayOfWeek.Tuesday, "20:00")] = "Kzarka/Karanda",
            [(DayOfWeek.Tuesday, "23:15")] = "Nouver",
            [(DayOfWeek.Wednesday, "01:00")] = "Kzarka",
            [(DayOfWeek.Wednesday, "11:00")] = "Kutum",
            [(DayOfWeek.Wednesday, "16:00")] = "Kzarka/Nouver",
            [(DayOfWeek.Wednesday, "20:00")] = "Nouver/Kutum",
            [(DayOfWeek.Wednesday, "23:15")] = "Karanda",
            [(DayOfWeek.Thursday, "01:00")] = "Karanda",
            [(DayOfWeek.Thursday, "11:00")] = "Nouver",
            [(DayOfWeek.Thursday, "16:00")] = "Kutum/Kzarka",
            [(DayOfWeek.Thursday, "20:00")] = "Nouver/Kutum",
            [(DayOfWeek.Thursday, "23:15")] = "Karanda/Kzarka",
            [(DayOfWeek.Friday, "01:00")] = "Karanda",
            [(DayOfWeek.Friday, "11:00")] = "Nouver",
            [(DayOfWeek.Friday, "16:00")] = "Kutum/Kzarka",
            [(DayOfWeek.Friday, "20:00")] = "Nouver/Kzarka",
            [(DayOfWeek.Friday, "23:15")] = "Kutum",
            [(DayOfWeek.Saturday, "01:00")] = "Kzarka",
            [(DayOfWeek.Saturday, "11:00")] = "Kutum/Kzarka",
            [(DayOfWeek.Saturday, "16:00")] = "Nouver/Kutum",
            [(DayOfWeek.Sunday, "00:15")] = "Karanda/Nouver",
            [(DayOfWeek.Sunday, "01:00")] = "Nouver",
            [(DayOfWeek.Sunday, "11:00")] = "Kutum",
            [(DayOfWeek.Sunday, "16:00")] = "Kzarka/Karanda",
            [(DayOfWeek.Sunday, "20:00")] = "Nouver/Karanda",
            [(DayOfWeek.Sunday, "23:15")] = "Kutum/Kzarka",
        };

        public static Dictionary<DateTime, string> RuntimeCalculatedDates = new Dictionary<DateTime, string>();

        public static DiscordSocketClient Client;

        static async Task Main(string[] args)
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;

            string token = ""; // TODO
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            CalculateThisWeeksDates();
            var timer = new Timer {AutoReset = true, Enabled = true, Interval = 60000};
            var lastDay = DateTime.Now.Day;
            timer.Elapsed += delegate
            {
                // Her 5 dakikada bir atılacak mesaj var mı diye bak
                if (DateTime.Now.Minute % 5 == 0)
                {
                    CheckForTime();
                }

                // gün her değiştiğinde haftanın saatlerini yeniden hesapla
                if (DateTime.Now.Day != lastDay)
                {
                    CalculateThisWeeksDates();
                    lastDay = DateTime.Now.Day;
                }
            };

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private static void CheckForTime()
        {
            foreach (var moment in RuntimeCalculatedDates)
            {
                // TODO
                // nasıl yapacam lan ben bunu aq
            }
        }

        private static async Task NotifyUsers(string message)
        {
            await Client.GetGuild(ChannelId).GetTextChannel(ChannelId).SendMessageAsync(message);
        }

        private static void CalculateThisWeeksDates()
        {
            RuntimeCalculatedDates.Clear();

            var startOfWeek = DateTime.Now.StartOfWeek();
            foreach (var moment in Times)
            {
                var date = startOfWeek;
                date = date.AddDays(moment.Key.Item1 - DayOfWeek.Monday).Date;
                var time = moment.Key.Item2.Split(':');
                date = date.AddHours(int.Parse(time[0]));
                date = date.AddMinutes(int.Parse(time[1]));

                RuntimeCalculatedDates.Add(date, moment.Value);
            }
        }

        public static DateTime StartOfWeek(this DateTime dt)
        {
            var diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
