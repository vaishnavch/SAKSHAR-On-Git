using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sakshar
{
    class Configuration
    {
        string fileName;
        Dictionary<string, string> dictionary;

        public Configuration(string fileName)
        {
            this.fileName = fileName;
            dictionary = readFile();
        }

        private Dictionary<string, string> readFile()
        {
            var data = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(fileName))
            {
                string[] parts = line.Split('=');
                data.Add(parts[0], string.Join("=", parts.Skip(1).ToArray()));
            }
            return data;
        }

        public string GetValue(string key)
        {
            return dictionary[key];
        }

        public void Save()
        {
            if (!File.Exists(fileName))
                File.Create(fileName);

            StreamWriter file = new StreamWriter(fileName);

            foreach (String prop in dictionary.Keys.ToArray())
                file.WriteLine(prop + "=" + dictionary[prop]);

            file.Close();
        }

        public void Set(String field, string value)
        {
            if (!dictionary.ContainsKey(field))
                dictionary.Add(field, value.ToString());
            else
                dictionary[field] = value.ToString();
        }
    }
}
