using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace algos_base
{
    public partial class Task02 : Page
    {
        private string _filePath;
        private int _delay;

        public Task02()
        {
            InitializeComponent();
            _delay = (int)DelaySlider.Value; // Default value for delay is the slider's value
            DelaySlider.ValueChanged += DelaySlider_ValueChanged; // Event handler for slider value change
        }

        // Handle slider value change (update delay)
        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _delay = (int)e.NewValue;
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
        private async void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Start Sorting button clicked.\n");

            // Check if file is selected
            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                LogTextBox.AppendText("Error: No file selected.\n");
                return;
            }

            // Get sorting method and key attribute
            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content.ToString();
            string keyAttribute = KeyAttributeTextBox.Text;

            if (string.IsNullOrEmpty(selectedMethod) || string.IsNullOrEmpty(keyAttribute))
            {
                MessageBox.Show("Please select sorting method and key attribute.");
                LogTextBox.AppendText("Error: Sorting method or key attribute is not selected.\n");
                return;
            }

            // Log selected method and attribute
            LogTextBox.AppendText($"Sorting method selected: {selectedMethod}\n");
            LogTextBox.AppendText($"Key attribute: {keyAttribute}\n");

            // Add sorting logic here
            try
            {
                LogTextBox.AppendText($"Starting sorting using {selectedMethod} by {keyAttribute}...\n");

                // Read the file
                List<string> lines = new List<string>(File.ReadLines(_filePath));
                LogTextBox.AppendText($"File loaded. Number of records: {lines.Count}\n");

                // Example sorting logic (replace with actual sorting logic)
                switch (selectedMethod)
                {
                    case "Natural Merge":
                        // Log action and sort (replace with actual sorting logic)
                        LogTextBox.AppendText("Using Natural Merge sorting method.\n");
                        await NaturalMergeSort(lines, keyAttribute); // Make async call
                        break;

                    case "Direct Merge":
                        // Log action and sort (replace with actual sorting logic)
                        LogTextBox.AppendText("Using Direct Merge sorting method.\n");
                        await DirectMergeSort(lines, keyAttribute); // Make async call
                        break;

                    case "Heap Sort":
                        // Log action and sort (replace with actual sorting logic)
                        LogTextBox.AppendText("Using Heap Sort sorting method.\n");
                        await HeapSort(lines); // Make async call
                        break;

                    default:
                        LogTextBox.AppendText("Unknown sorting method.\n");
                        return;
                }

                // Log progress
                LogTextBox.AppendText("Sorting completed.\n");

                // Save the sorted file
                string sortedFilePath = Path.Combine(Path.GetDirectoryName(_filePath), "sorted_" + Path.GetFileName(_filePath));
                File.WriteAllLines(sortedFilePath, lines);
                LogTextBox.AppendText($"Sorted file saved at {sortedFilePath}\n");

            }
            catch (Exception ex)
            {
                // Log errors if any
                LogTextBox.AppendText($"Error during sorting: {ex.Message}\n");
            }
        }

        // Example of logging for sorting action: Natural Merge Sort
        private async Task NaturalMergeSort(List<string> lines, string keyAttribute)
        {
            LogTextBox.AppendText("Natural Merge Sort started...\n");

            // Dummy sort logic for logging with delay
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    LogTextBox.AppendText($"Comparing: {lines[i]} and {lines[j]}\n");
                    // Simulate a comparison action
                    if (string.Compare(lines[i], lines[j]) > 0)
                    {
                        LogTextBox.AppendText($"Swapping: {lines[i]} with {lines[j]}\n");
                        // Simulate a swap action
                        string temp = lines[i];
                        lines[i] = lines[j];
                        lines[j] = temp;
                    }

                    // Apply delay between actions
                    await Task.Delay(_delay);
                }
            }

            LogTextBox.AppendText("Natural Merge Sort completed.\n");
        }

        // Example of logging for sorting action: Direct Merge Sort
        private async Task DirectMergeSort(List<string> lines, string keyAttribute)
        {
            LogTextBox.AppendText("Direct Merge Sort started...\n");

            // Dummy sort logic for logging with delay
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    LogTextBox.AppendText($"Comparing: {lines[i]} and {lines[j]}\n");
                    // Simulate a comparison action
                    if (string.Compare(lines[i], lines[j]) > 0)
                    {
                        LogTextBox.AppendText($"Swapping: {lines[i]} with {lines[j]}\n");
                        // Simulate a swap action
                        string temp = lines[i];
                        lines[i] = lines[j];
                        lines[j] = temp;
                    }

                    // Apply delay between actions
                    await Task.Delay(_delay);
                }
            }

            LogTextBox.AppendText("Direct Merge Sort completed.\n");
        }

        // Example of logging for sorting action: Heap Sort
        private async Task HeapSort(List<string> lines)
        {
            LogTextBox.AppendText("Heap Sort started...\n");

            // Dummy sort logic for logging with delay
            for (int i = 0; i < lines.Count; i++)
            {
                LogTextBox.AppendText($"Processing: {lines[i]}\n");
                // Simulate action
                await Task.Delay(_delay);
            }

            LogTextBox.AppendText("Heap Sort completed.\n");
        }
    }
}
