using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        static void Main() => MainAsync().GetAwaiter().GetResult();

        public static int LastMinute;
        public static int LastWeek;

        static async Task MainAsync()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;

            await Log(new LogMessage(LogSeverity.Info, "BossBot", "Başlatılıyor..."));
            CalculateThisWeeksDates();
            if (RuntimeCalculatedDates.Any(x =>
                x.Key > DateTime.Now && x.Key - DateTime.Now <= TimeSpan.FromMinutes(35)))
            {
                await Login();
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
            var timer = new Timer {AutoReset = true, Enabled = true, Interval = 1000};
            Check(true);
            timer.Elapsed += (a, b) => Check();

            await Log(new LogMessage(LogSeverity.Info, "BossBot", "Başlatıldı."));

            await Task.Delay(-1);
        }

        private static void Check(bool first = false)
        {
            if (LastMinute == DateTime.Now.Minute)
            {
                return;
            }

            if (!first && DateTime.Now.Minute % 5 != 0)
            {
                Log(new LogMessage(LogSeverity.Info, "BossBot", "Bir dakika geçti."));
            }

            LastMinute = DateTime.Now.Minute;
            // hafta her değiştiğinde haftanın saatlerini yeniden hesapla
            if (CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek,
                    System.DayOfWeek.Monday) != LastWeek)
            {
                if (!first)
                {
                    Log(new LogMessage(LogSeverity.Info, "BossBot", "Bir hafta geçti. Saatler yeniden hesaplanıyor."));
                    CalculateThisWeeksDates();
                }

                LastWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now,
                    CalendarWeekRule.FirstFullWeek, System.DayOfWeek.Monday);
            }

            if (DateTime.Now.Minute % 5 == 0)
            {
                if (!first)
                {
                    Log(new LogMessage(LogSeverity.Info, "BossBot", "5 dakika geçti. Liste kontrol ediliyor."));
                }

                CheckForTime();
            }
        }

        private static async Task Login()
        {
            if (Client.ConnectionState == ConnectionState.Connecting ||
                Client.ConnectionState == ConnectionState.Connected)
            {
                return;
            }

            await Client.LoginAsync(TokenType.Bot, Constants.TOKEN);
            await Client.StartAsync();
        }

        private static async Task Logout()
        {
            if (Client.ConnectionState == ConnectionState.Disconnecting ||
                Client.ConnectionState == ConnectionState.Disconnected)
            {
                await Task.Delay(1000);
                return;
            }

            await Client.LogoutAsync();
            await Client.StopAsync();
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private static async void CheckForTime()
        {
            foreach (var moment in RuntimeCalculatedDates)
            {
                if (moment.Key.AddMinutes(-35).DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await Log(new LogMessage(LogSeverity.Info, "BossBot", "Bir ana 35 dakika kalmış. " +
                                                                          "Discord hesabına giriş yapılıyor."));
                    await Login();
                }

                if (moment.Key.AddMinutes(-30).DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await NotifyUsers($"{moment.Value} 30 dakika sonra doğacak! <@&469922791768981515>");
                }

                if (moment.Key.AddMinutes(-15).DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await NotifyUsers($"{moment.Value} 15 dakika sonra doğacak! <@&469922791768981515>");
                }

                if (moment.Key.AddMinutes(-10).DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await NotifyUsers($"{moment.Value} 10 dakika sonra doğacak! <@&469922791768981515>");
                }

                if (moment.Key.AddMinutes(-5).DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await NotifyUsers($"{moment.Value} 5 dakika sonra doğacak! <@&469922791768981515>");
                }

                if (moment.Key.DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await NotifyUsers($"{moment.Value} doğdu! <@&469922791768981515>");
                }

                if (moment.Key.AddMinutes(5).DatesEqualsIgnoreSeconds(DateTime.Now))
                {
                    await Log(new LogMessage(LogSeverity.Info, "BossBot", "Bir anın üzerinden 5 dakika geçti. " +
                                                                          "Discord hesabından çıkış yapılıyor."));
                    await Logout();
                }
            }
        }

        private static async Task NotifyUsers(string message)
        {
            await Log(new LogMessage(LogSeverity.Info, "BossBot", $"\"{message}\" mesajı gönderiliyor..."));
            await Client.GetGuild(ChannelId).GetTextChannel(ChannelId).SendMessageAsync(message);
            await Log(new LogMessage(LogSeverity.Info, "BossBot", "Mesaj gönderildi."));
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
            var diff = dt.GetDayOfWeek();
            return dt.AddDays(-1 * (int)diff).Date;
        }

        public static bool DatesEqualsIgnoreSeconds(this DateTime d1, DateTime d2)
        {
            return d1.Year == d2.Year &&
                   d1.Month == d2.Month &&
                   d1.Day == d2.Day &&
                   d1.Hour == d2.Hour &&
                   d1.Minute == d2.Minute;
        }

        public static DayOfWeek GetDayOfWeek(this DateTime dt)
        {
            return dt.DayOfWeek == System.DayOfWeek.Sunday ? DayOfWeek.Sunday : (DayOfWeek)(dt.DayOfWeek - 1);
        }
    }

    public enum DayOfWeek
    {
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6
    }
}
