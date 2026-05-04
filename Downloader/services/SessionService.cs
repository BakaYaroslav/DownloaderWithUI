using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Downloader.services
{
    internal class SessionService
    {
        private static readonly string SessionFile = Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
       "Downloader", "session.json");


        public static SessionData Load()
        {
            if (!File.Exists(SessionFile))
                return new SessionData();


            var json = File.ReadAllText(SessionFile);
            return JsonSerializer.Deserialize<SessionData>(json) ?? new SessionData(); // превращает json строку в объект SessionData
        }

        public static void Save(SessionData data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true // для удобства чтения, добавляет отступы и переносы строк в JSON


            });
            Directory.CreateDirectory(Path.GetDirectoryName(SessionFile)!);
            File.WriteAllText(SessionFile, json);
        }

        public static void AddAccount(string login)
        {
            var data = Load();
            if (!data.Saved.Contains(login))  // не дублируем аккаунты
                data.Saved.Add(login);
            data.Active = login;
            Save(data);
        }

        public static void SwitchAccount(string login)
        {
            var data = Load();
            data.Active = login;
            Save(data);
        }

        public static bool RemoveAccount(string login)
        {
            var data = Load();
            data.Saved.Remove(login);
            if (data.Active == login) // если удаляем активный аккаунт, переключаем на другой
                data.Active = data.Saved.FirstOrDefault() ?? ""; // если нет других аккаунтов, оставляем пустым
            Save(data);

            return data.Saved.Count == 0;
        }
    }
}

    
