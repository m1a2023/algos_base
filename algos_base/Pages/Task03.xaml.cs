using System;
using System.Collections.Generic;
using System.Diagnostics;  // Для измерения времени
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace algos_base
{
    public partial class Task03 : Page
    {
        private string _filePath;
        private Stopwatch _stopwatch = new Stopwatch();

        public Task03()
        {
            InitializeComponent();
        }

        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Browse button clicked. Opening file dialog...\n");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                LogTextBox.AppendText($"File selected: {_filePath}\n");
            }
            else
            {
                LogTextBox.AppendText("No file selected.\n");
            }
        }

        private async void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Start Sorting button clicked.\n");

            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                LogTextBox.AppendText("Error: No file selected.\n");
                return;
            }

            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedMethod))
            {
                MessageBox.Show("Please select sorting method.");
                LogTextBox.AppendText("Error: Sorting method not selected.\n");
                return;
            }

            LogTextBox.AppendText($"Sorting method selected: {selectedMethod}\n");

            try
            {
                // Стартуем измерение времени
                _stopwatch.Start();

                LogTextBox.AppendText($"Starting sorting using {selectedMethod}...\n");

                List<string> words = new List<string>(File.ReadLines(_filePath)
                    .SelectMany(line => line.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(word => word.ToLower())));

                // Безопасное обновление UI
                Dispatcher.Invoke(() => LogTextBox.AppendText($"File loaded. Number of words: {words.Count}\n"));

                switch (selectedMethod)
                {
                    case "QuickSort":
                        await Task.Run(() => QuickSort(words, 0, words.Count - 1)); // Вызов QuickSort в фоновом потоке
                        Dispatcher.Invoke(() => LogTextBox.AppendText("Using QuickSort sorting method.\n"));
                        break;

                    case "RadixSort":
                        await Task.Run(() => RadixSort(words)); // Вызов RadixSort в фоновом потоке
                        Dispatcher.Invoke(() => LogTextBox.AppendText("Using RadixSort sorting method.\n"));
                        break;

                    default:
                        Dispatcher.Invoke(() => LogTextBox.AppendText("Unknown sorting method.\n"));
                        return;
                }

                // Остановка времени после завершения сортировки
                _stopwatch.Stop();
                Dispatcher.Invoke(() => LogTextBox.AppendText($"Sorting completed in {_stopwatch.Elapsed.TotalMilliseconds} ms.\n"));
                Dispatcher.Invoke(() => TimeTakenTextBlock.Text = $"Время выполнения: {_stopwatch.Elapsed.TotalMilliseconds} секунд");
                _stopwatch.Reset();

                // Подсчет слов
                await Task.Run(() => CountWords(words)); // Вызов CountWords в фоновом потоке

                Dispatcher.Invoke(() => LogTextBox.AppendText("Sorting and counting completed.\n"));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogTextBox.AppendText($"Error during sorting: {ex.Message}\n"));
            }
        }
        private List<string> LoadWordsFromFile(string filePath)
        {
            return new List<string>(File.ReadLines(filePath)
                .SelectMany(line => line.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => word.ToLower())));
        }
        private async Task QuickSort(List<string> words, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(words, low, high);
                await Task.Run(() => QuickSort(words, low, pivotIndex - 1)); // Вызов QuickSort в фоновом потоке
                await Task.Run(() => QuickSort(words, pivotIndex + 1, high)); // Вызов QuickSort в фоновом потоке
            }
        }

        private int Partition(List<string> words, int low, int high)
        {
            string pivot = words[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (string.Compare(words[j], pivot) <= 0)
                {
                    i++;
                    Swap(words, i, j);
                }
            }

            Swap(words, i + 1, high);
            return i + 1;
        }
        private void Swap(List<string> words, int i, int j)
        {
            string temp = words[i];
            words[i] = words[j];
            words[j] = temp;
        }

        private async Task RadixSort(List<string> words)
        {
            LogTextBox.AppendText("Starting Radix Sort...\n");

            int maxLength = words.Max(w => w.Length);

            for (int digitPosition = maxLength - 1; digitPosition >= 0; digitPosition--)
            {
                List<string>[] buckets = new List<string>[26];
                for (int i = 0; i < 26; i++)
                    buckets[i] = new List<string>();

                foreach (var word in words)
                {
                    int charIndex = digitPosition < word.Length ? word[word.Length - digitPosition - 1] - 'a' : -1;
                    if (charIndex >= 0)
                    {
                        buckets[charIndex].Add(word);
                    }
                    else
                    {
                        buckets[25].Add(word);
                    }
                }

                words.Clear();
                foreach (var bucket in buckets)
                {
                    words.AddRange(bucket);
                }

                // Безопасное обновление UI
                Dispatcher.Invoke(() => LogTextBox.AppendText($"After processing digit position {digitPosition}, words are sorted.\n"));
            }

            LogTextBox.AppendText("Radix Sort completed.\n");
        }

        private async Task CountWords(List<string> words)
        {
            Dictionary<string, int> wordCounts = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (wordCounts.ContainsKey(word))
                {
                    wordCounts[word]++;
                }
                else
                {
                    wordCounts[word] = 1;
                }
            }

            int uniqueWords = wordCounts.Count;
            int totalWords = words.Count;

            // Обновление интерфейса в UI-потоке
            Dispatcher.Invoke(() =>
            {
                TotalWordsTextBlock.Text = $"Общее количество слов: {totalWords}";
                UniqueWordsTextBlock.Text = $"Количество уникальных слов: {uniqueWords}";
            });

            // Вывод всех слов и их количества в лог в фоновом потоке
            await Task.Run(() =>
            {
                StringBuilder logOutput = new StringBuilder();

                foreach (var wordCount in wordCounts)
                {
                    logOutput.AppendLine($"{wordCount.Key}: {wordCount.Value}");
                }

                // Обновление LogTextBox в главном потоке
                Dispatcher.Invoke(() =>
                {
                    LogTextBox.AppendText(logOutput.ToString());
                });
            });

            // Вывод в лог завершен
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText("\nCounting words finished.\n");
            });
        }
    }
}
