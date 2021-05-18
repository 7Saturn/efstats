using System;
using System.Collections.Generic; // Lists
using System.Text.RegularExpressions; // Regex.Replace
namespace EfStats {
    public static class Csv {
        public static string CsvDelimited(string value) {
            if (value == null) return "";

            string inch = '"'.ToString();
            if (   value.Contains(inch)
                || value.Contains(';')
                || value.Contains('\n')
                || value.Contains(' ')) {
                value.Replace(inch, inch + inch);
                value = inch + value + inch;
            }
            return value;
        }

        public static string CsvLine(List<string> list) {
            string line = "";
            foreach(string s in list) {
                string sCsv = CsvDelimited(s);
                if (line.Equals("")) {
                    line = sCsv;
                }
                else {
                    line += ";" + sCsv;
                }
            }
            Match hit = Regex.Match(line, ";$");
            while (hit.Success) { // Make this as lean as possible by removing the last columns if they are empty anyways.
                line = Regex.Replace(line, ";$", "");
                hit = Regex.Match(line, ";$");
            }
            return line;
        }
    }
}
