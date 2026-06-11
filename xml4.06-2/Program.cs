using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Console;

namespace DiagonalLinux_Program
{
    class Program
    {   // Linux  Linux Linux
        static void Main(string[] args)
        {
            try
            {
                // Запускаем xrandr и читаем вывод
                Process p = new Process();
                p.StartInfo.FileName = "xrandr";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Regex regex = new Regex(@"(\d+)mm x (\d+)mm");
                Match match = regex.Match(output);

                if (match.Success)
                {
                    int widthMm = int.Parse(match.Groups[1].Value);
                    int heightMm = int.Parse(match.Groups[2].Value);

                    double diagonalMm = Math.Sqrt(widthMm * widthMm + heightMm * heightMm);
                    double diagonalInches = diagonalMm / 25.4;

                    WriteLine($"Ширина: {widthMm} мм");
                    WriteLine($"Высота: {heightMm} мм");
                    WriteLine($"Диагональ: {diagonalInches:F2} дюймов");
                    WriteLine($"Примерный размер: {Math.Round(diagonalInches)} дюймов");
                }
                else
                {
                    WriteLine("Не удалось найти физические размеры в выводе xrandr. Убедитесь, что утилита xrandr установлена.");
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка: {ex.Message}");
            }
            
            WriteLine("\nНажмите любую клавишу для выхода...");
            ReadKey();
        }
    }
}