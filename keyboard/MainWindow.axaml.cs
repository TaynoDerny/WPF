using System;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace keyboard
{
    public partial class MainWindow : Window
    {
        // --- Состояние клавиатуры ---
        private bool _shiftPressed = false;
        private bool _capsLockToggled = false;

        // --- Игровое состояние ---
        private string _targetText = "";
        private int _currentIndex = 0;
        private int _fails = 0;
        private int _correctChars = 0;
        private DateTime _startTime;
        private bool _isRunning = false;
        private StringBuilder _userInput = new StringBuilder(); // Храним реальный ввод

        public MainWindow()
        {
            InitializeComponent();
            UpdateStats();
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            _isRunning = true;
            _currentIndex = 0;
            _fails = 0;
            _correctChars = 0;
            _startTime = DateTime.Now;
            _userInput.Clear();

            int length = (int)DifficultySlider.Value;
            bool caseSensitive = CaseSensitiveCheckBox.IsChecked == true;
            _targetText = GenerateRandomString(length, caseSensitive);

            TargetText.Text = _targetText;
            UserInputDisplay.Text = "";
            UpdateStats();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        // --- Обработка физической клавиатуры (KeyDown) ---
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Обновляем состояние модификаторов
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _shiftPressed = true;
            else if (e.Key == Key.CapsLock)
                _capsLockToggled = !_capsLockToggled;

            if (!_isRunning) return;

            string key = MapKeyToString(e.Key);
            if (key == null) return;

            HighlightKey(e.Key, true);

            if (key.Length > 0)
            {
                ProcessCharacter(key[0]);
            }
        }

        // --- Сброс подсветки и Shift при отпускании клавиш ---
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _shiftPressed = false;

            HighlightKey(e.Key, false);
        }

        // --- Обработка кликов мышкой по виртуальным кнопкам ---
        private void VirtualKey_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning || sender is not Button button) return;

            string content = button.Content?.ToString() ?? "";
            char? charToProcess = null;

            // Обработка специальных кнопок
            switch (content)
            {
                case "Space":
                    charToProcess = ' ';
                    break;
                case "Enter":
                    return;
                case "Backspace":
                    if (_userInput.Length > 0 && _currentIndex > 0)
                    {
                        _userInput.Remove(_userInput.Length - 1, 1);
                        _currentIndex--;
                        UserInputDisplay.Text = _userInput.ToString();
                    }
                    return;
                case "Shift": // Если кнопка Shift активна
                    _shiftPressed = !_shiftPressed; // Переключаем состояние
                    return;
                case "Caps": // Если кнопка CapsLock активна
                    _capsLockToggled = !_capsLockToggled;
                    return;
                case "Tab":
                    return;
                default:
                    // Обычные символы (буквы, цифры, знаки препинания)
                    if (!string.IsNullOrEmpty(content) && content.Length == 1)
                    {
                        // Определяем, нужно ли преобразовывать в верхний регистр
                        bool upper = _shiftPressed ^ _capsLockToggled;
                        
                        // Если это буква, учитываем регистр
                        if (char.IsLetter(content[0]))
                        {
                            charToProcess = upper ? char.ToUpper(content[0]) : char.ToLower(content[0]);
                        }
                        else
                        {
                            // Для цифр и знаков: если Shift нажат, могут меняться символы
                            // В этой простой реализации оставляем как есть.
                            charToProcess = content[0];
                        }
                    }
                    break;
            }

            if (charToProcess.HasValue)
            {
                ProcessCharacter(charToProcess.Value);
                HighlightKeyByString(content, true);
            }
        }

        // --- Основная логика обработки одного введённого символа ---
        private void ProcessCharacter(char inputChar)
        {
            // Добавляем введённый символ в историю
            _userInput.Append(inputChar);
            UserInputDisplay.Text = _userInput.ToString();

            if (_currentIndex < _targetText.Length)
            {
                char expected = _targetText[_currentIndex];
                bool caseSensitive = CaseSensitiveCheckBox.IsChecked == true;

                // Сравнение с учётом регистра
                bool isMatch = caseSensitive ? (inputChar == expected) : (char.ToLower(inputChar) == char.ToLower(expected));

                if (isMatch)
                {
                    _correctChars++;
                }
                else
                {
                    _fails++;
                }

                _currentIndex++;

                UpdateStats();

                if (_currentIndex == _targetText.Length)
                {
                    TargetText.Text = "Готово! Нажмите Start для новой попытки.";
                    _isRunning = false;
                    StartButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                }
            }
        }

        // --- Обновление статистики ---
        private void UpdateStats()
        {
            var elapsed = (DateTime.Now - _startTime).TotalMinutes;
            double speed = elapsed > 0 ? _correctChars / elapsed : 0;

            SpeedText.Text = $"Speed: {speed:F0} chars/min";
            FailsText.Text = $"Fails: {_fails}";
        }

        // --- Подсветка по Key ---
        private void HighlightKey(Key key, bool highlight)
        {
            string keyName = key.ToString();
            HighlightKeyByString(keyName, highlight);
        }

        // --- Подсветка по имени кнопки ---
        private void HighlightKeyByString(string keyName, bool highlight)
        {
            var button = this.FindControl<Button>(keyName);
            if (button != null)
            {
                button.Background = highlight ? Brushes.Yellow : Brushes.LightGray;
            }
        }

        // --- Преобразование физической клавиши в строку (с учётом регистра!) ---
        private string MapKeyToString(Key key)
        {
            // Цифры (0-9)
            if (key >= Key.D0 && key <= Key.D9)
            {
                int digit = (int)key - (int)Key.D0;
                // Если нажат Shift, возвращаем символы !@#$%^&*()
                if (_shiftPressed)
                {
                    string[] shiftDigits = { ")", "!", "@", "#", "$", "%", "^", "&", "*", "(" };
                    return shiftDigits[digit];
                }
                return digit.ToString();
            }

            // Буквы (A-Z)
            if (key >= Key.A && key <= Key.Z)
            {
                char letter = (char)('A' + (key - Key.A));
                // Определяем регистр: Shift XOR CapsLock
                bool upper = _shiftPressed ^ _capsLockToggled;
                return upper ? letter.ToString() : char.ToLower(letter).ToString();
            }

            // Остальные символы
            switch (key)
            {
                case Key.OemMinus: return _shiftPressed ? "_" : "-";
                case Key.OemPlus: return _shiftPressed ? "+" : "=";
                case Key.OemOpenBrackets: return _shiftPressed ? "{" : "[";
                case Key.OemCloseBrackets: return _shiftPressed ? "}" : "]";
                case Key.OemBackslash: return _shiftPressed ? "|" : "\\";
                case Key.OemSemicolon: return _shiftPressed ? ":" : ";";
                case Key.OemQuotes: return _shiftPressed ? "\"" : "'";
                case Key.OemComma: return _shiftPressed ? "<" : ",";
                case Key.OemPeriod: return _shiftPressed ? ">" : ".";
                case Key.OemQuestion: return _shiftPressed ? "?" : "/";
                case Key.Space: return " ";
                case Key.Enter: return "\n";
                default: return null;
            }
        }

        // --- Генерация случайной строки ---
        private string GenerateRandomString(int length, bool caseSensitive)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789`-=[]\\;',./ ";
            var sb = new StringBuilder();
            var rand = new Random();
            for (int i = 0; i < length; i++)
            {
                char c = chars[rand.Next(chars.Length)];
                if (caseSensitive && rand.Next(2) == 0)
                    c = char.ToUpper(c);
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}