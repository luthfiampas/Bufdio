using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BufdioAvalonia.Views
{
    public class InputDialog : Window
    {
        private readonly TextBox _inputTextBox;
        private readonly TextBlock _descriptionTextBlock;

        public InputDialog()
        {
            AvaloniaXamlLoader.Load(this);
            
            _inputTextBox = this.Find<TextBox>("InputTextBox");
            _descriptionTextBlock = this.Find<TextBlock>("DescriptionTextBlock");
            
            Opened += OnWindowOpened;
            KeyDown += OnWindowKeyDown;
            _inputTextBox.KeyDown += OnInputTextBoxKeyDown;
            this.Find<Button>("OkButton").Click += OnOkButtonClick;
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close(null);
            }
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            _inputTextBox.Focus();
        }
        
        private void OnInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Close(_inputTextBox.Text);
            }
            else if (e.Key == Key.Escape)
            {
                Close(null);
            }
        }
        
        public void SetParameters(string title = "", string description = "")
        {
            Title = title;
            _descriptionTextBlock.Text = description;
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Close(_inputTextBox.Text);
        }
    }
}
