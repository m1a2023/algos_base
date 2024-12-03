using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace algos_base
{
    public partial class Task03 : Page
    {
        private string _filePath;

        public Task03()
        {
            InitializeComponent();
        }
        
        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        // Handle file browsing
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

        // Handle start sorting
        private void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Start Sorting button clicked.\n");

            // Check if file is selected
            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                LogTextBox.AppendText("Error: No file selected.\n");
                return;
            }

            // Get sorting method
            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedMethod))
            {
                MessageBox.Show("Please select sorting method.");
                LogTextBox.AppendText("Error: Sorting method not selected.\n");
                return;
            }

            // Log selected method
            LogTextBox.AppendText($"Sorting method selected: {selectedMethod}\n");

            try
            {
                LogTextBox.AppendText($"Starting sorting using {selectedMethod}...\n");

                // Read the file
                List<string> words = new List<string>(File.ReadLines(_filePath)
                    .SelectMany(line => line.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(word => word.ToLower())));

                LogTextBox.AppendText($"File loaded. Number of words: {words.Count}\n");

                // Perform sorting
                switch (selectedMethod)
                {
                    case "QuickSort":
                        QuickSort(words, 0, words.Count - 1);
                        LogTextBox.AppendText("Using QuickSort sorting method.\n");
                        break;

                    case "RadixSort":
                        RadixSort(words);
                        LogTextBox.AppendText("Using RadixSort sorting method.\n");
                        break;

                    default:
                        LogTextBox.AppendText("Unknown sorting method.\n");
                        return;
                }

                // Count the words after sorting
                CountWords(words);

                LogTextBox.AppendText("Sorting and counting completed.\n");

            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"Error during sorting: {ex.Message}\n");
            }
        }

        // QuickSort algorithm
        private void QuickSort(List<string> words, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(words, low, high);
                QuickSort(words, low, pivotIndex - 1);
                QuickSort(words, pivotIndex + 1, high);
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

        // RadixSort algorithm
        private void RadixSort(List<string> words)
        {
            LogTextBox.AppendText("Starting Radix Sort...\n");

            // Find the maximum length of words in the list
            int maxLength = words.Max(w => w.Length);

            // Iterate over the length of words from right to left (from least significant digit to most)
            for (int digitPosition = maxLength - 1; digitPosition >= 0; digitPosition--)
            {
                // Create 26 buckets for each character (for lowercase letters only)
                List<string>[] buckets = new List<string>[26];
                for (int i = 0; i < 26; i++)
                    buckets[i] = new List<string>();

                // Sort words into buckets based on the current digit position
                foreach (var word in words)
                {
                    // Handle words shorter than the current digit position
                    int charIndex = digitPosition < word.Length ? word[word.Length - digitPosition - 1] - 'a' : -1;
                    if (charIndex >= 0)
                    {
                        buckets[charIndex].Add(word);
                    }
                    else
                    {
                        buckets[25].Add(word); // Add words that don't have a character at this position to the last bucket (for example, empty or short words)
                    }
                }

                // Collect words from the buckets back into the original list
                words.Clear();
                foreach (var bucket in buckets)
                {
                    words.AddRange(bucket);
                }

                LogTextBox.AppendText($"After processing digit position {digitPosition}, words are sorted: {string.Join(", ", words)}\n");
            }

            LogTextBox.AppendText("Radix Sort completed.\n");
        }

        private void CountingSortByDigit(List<string> words, int exp)
        {
            List<string> output = new List<string>(new string[words.Count]);
            int[] count = new int[10];

            foreach (var word in words)
            {
                int index = (word.Length - exp) >= 0 ? word[word.Length - exp] - 'a' : -1;
                if (index >= 0)
                    count[index]++;
                else
                    count[0]++; // Handle short strings
            }

            for (int i = 1; i < 10; i++)
            {
                count[i] += count[i - 1];
            }

            for (int i = words.Count - 1; i >= 0; i--)
            {
                int index = (words[i].Length - exp) >= 0 ? words[i][words[i].Length - exp] - 'a' : -1;
                if (index >= 0)
                {
                    output[count[index] - 1] = words[i];
                    count[index]--;
                }
                else
                {
                    output[count[0] - 1] = words[i];
                    count[0]--;
                }
            }

            for (int i = 0; i < words.Count; i++)
            {
                words[i] = output[i];
            }
        }

        // Word counting method
        private void CountWords(List<string> words)
        {
            // Dictionary to store word counts
            Dictionary<string, int> wordCounts = new Dictionary<string, int>();

            // Iterate through sorted words and count occurrences
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

            // Display the word counts in the log
            foreach (var wordCount in wordCounts)
            {
                LogTextBox.AppendText($"{wordCount.Key}: {wordCount.Value}\n");
            }
        }
    }
}
