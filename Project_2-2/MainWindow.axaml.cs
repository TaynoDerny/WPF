
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Project_2_2
{
    public partial class MainWindow : Window
    {
        private string _currentNumber = "0";
        private string _history = "";
        private double _result = 0;
        private string _lastOperator = "";
        private bool _isOperatorPressed = false;
        private bool _isResultDisplayed = false;

        public string CurrentNumberText
        {
            get => _currentNumber;
            set
            {
                _currentNumber = value;
                // Используем INotifyPropertyChanged, но для простоты обновляем через код
                // Лучше реализовать через VM, но здесь сделаем так:
                var textBox = this.FindControl<TextBox>("CurrentNumberTextBox"); 
                // *УПРОЩЕНИЕ*: Т.к. привязка у нас есть, нам нужно уведомить UI об изменении.
                // В Avalonia проще всего использовать RaisePropertyChanged, если наследовать ViewModelBase.
                // Для чистого кода без VM используем прямую установку значения или шаблон MVVM.
                // В данном примере для краткости будем использовать прямую установку значения в TextBox.
            }
        }

        public string HistoryText
        {
            get => _history;
            set => _history = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            // Начальное состояние
            UpdateUI("0", ""); 
        }

        private void UpdateUI(string number, string history)
        {
            var tbNumber = this.FindControl<TextBox>("CurrentNumberTextBox");
            var tbHistory = this.FindControl<TextBox>("HistoryTextBox");
            if (tbNumber != null) tbNumber.Text = number;
            if (tbHistory != null) tbHistory.Text = history;
        }

        private void OnDigit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string digit = btn.Content?.ToString() ?? "";
                if (digit == "" || digit.Length > 1) return;

                // Если после оператора или после "=" мы вводим новое число, то сбрасываем текущее
                if (_isOperatorPressed || _isResultDisplayed)
                {
                    _currentNumber = "";
                    _isOperatorPressed = false;
                    _isResultDisplayed = false;
                }

                // Проверка на ведущий ноль: если текущее число "0" и не содержит точку, то заменяем его на новое число
                if (_currentNumber == "0" && digit != ".")
                {
                    _currentNumber = digit;
                }
                else
                {
                    // Запрещаем вводить цифры, если число начинается с 0 и нет точки (кроме 0.xxx)
                    if (_currentNumber.StartsWith("0") && !_currentNumber.Contains(".") && digit != ".")
                    {
                        // Ничего не делаем, остаётся 0
                        return;
                    }
                    
                    _currentNumber += digit;
                }
                UpdateUI(_currentNumber, _history);
            }
        }

        private void OnDecimalPoint(object sender, RoutedEventArgs e)
        {
            // Если после оператора, сбрасываем
            if (_isOperatorPressed || _isResultDisplayed)
            {
                _currentNumber = "0.";
                _isOperatorPressed = false;
                _isResultDisplayed = false;
                UpdateUI(_currentNumber, _history);
                return;
            }

            // Если текущее число уже содержит точку, ничего не делаем
            if (_currentNumber.Contains("."))
                return;

            // Если текущее число пустое, ставим "0."
            if (string.IsNullOrEmpty(_currentNumber))
                _currentNumber = "0."; 
            else
                _currentNumber += ".";
                
            UpdateUI(_currentNumber, _history);
        }

        private void OnOperator(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string op = btn.Content?.ToString() ?? "";

                // Если уже нажата операция, выполняем предыдущую, если есть
                if (_isOperatorPressed && !_isResultDisplayed)
                {
                    CalculateCurrentOperation();
                }

                // Сохраняем текущее число как результат
                if (double.TryParse(_currentNumber, out double num))
                {
                    _result = num;
                    _lastOperator = op;
                    _history = $"{_result} {op}";
                    _isOperatorPressed = true;
                    _isResultDisplayed = false;
                    UpdateUI(_currentNumber, _history);
                }
            }
        }

        private void OnEquals(object sender, RoutedEventArgs e)
        {
            if (_lastOperator != "")
            {
                CalculateCurrentOperation();
                _lastOperator = "";
                _history = "";
                _isResultDisplayed = true;
                UpdateUI(_currentNumber, _history);
            }
        }

        private void CalculateCurrentOperation()
        {
            if (!double.TryParse(_currentNumber, out double secondNum))
                return;

            switch (_lastOperator)
            {
                case "+":
                    _result += secondNum;
                    break;
                case "-":
                    _result -= secondNum;
                    break;
                case "*":
                    _result *= secondNum;
                    break;
                case "/":
                    if (secondNum != 0)
                        _result /= secondNum;
                    else
                    {
                        // Обработка деления на ноль
                        _result = 0;
                        _currentNumber = "Error";
                        _history = "";
                        UpdateUI(_currentNumber, _history);
                        return;
                    }
                    break;
                default:
                    break;
            }

            _currentNumber = _result.ToString();
            _isOperatorPressed = true; // Чтобы следующий ввод цифры начал новое число
            _isResultDisplayed = true;
        }

        private void OnClearEntry(object sender, RoutedEventArgs e)
        {
            _currentNumber = "0";
            if (!_isResultDisplayed)
            {
                // Не сбрасываем историю, только текущее число
            }
            UpdateUI(_currentNumber, _history);
        }

        private void OnClearAll(object sender, RoutedEventArgs e)
        {
            _currentNumber = "0";
            _history = "";
            _result = 0;
            _lastOperator = "";
            _isOperatorPressed = false;
            _isResultDisplayed = false;
            UpdateUI(_currentNumber, _history);
        }

        private void OnBackspace(object sender, RoutedEventArgs e)
        {
            // Нельзя удалять, если после оператора или результата
            if (_isOperatorPressed || _isResultDisplayed) return;

            if (_currentNumber.Length > 1)
            {
                _currentNumber = _currentNumber.Substring(0, _currentNumber.Length - 1);
            }
            else
            {
                _currentNumber = "0";
            }
            UpdateUI(_currentNumber, _history);
        }
    }
}