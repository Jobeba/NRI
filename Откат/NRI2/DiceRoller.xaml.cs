using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NRI
{


    /// <summary>
    /// Логика взаимодействия для DiceRoller.xaml
    /// </summary>
    public partial class DiceRoller : Window
    {
        public DiceRoller()
        {
            InitializeComponent();
        }
        private void RollDice_D6_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiceCountTextBox_D6.Text, out int diceCount) && diceCount > 0)
            {
                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                DiceItemsControl_D6.Items.Clear();

                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 7); // Случайное число от 1 до 6
                    results.Add(dieValue);
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };

                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D6.Items.Add(dieTextBlock);
                }

                ResultsTextBlock_D6.Text = $"Сумма: {sum}";
                ResultsTextBlock_D6_summ.Text = $"Значения: {string.Join(", ", results)}";
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void AnimateDieRoll(TextBlock dieTextBlock)
        {
            // Пример простой анимации - изменение размера
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.5,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                AutoReverse = true
            };
            ScaleTransform scaleTransform = new ScaleTransform(1.0, 1.0);
            dieTextBlock.RenderTransform = scaleTransform;
            DoubleAnimation scaleXAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.5, // Или любое другое значение
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };

            DoubleAnimation scaleYAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.5, // Или любое другое значение
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);

        }


        private void RollDice_D20_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiceCountTextBox_D20.Text, out int diceCount) && diceCount > 0)
            {
                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                DiceItemsControl_D20.Items.Clear();

                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 21); // Случайное число от 1 до 6
                    results.Add(dieValue);
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };

                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D20.Items.Add(dieTextBlock);
                }

                ResultsTextBlock_D20.Text = $"Сумма: {sum}";
                ResultsTextBlock_D20_summ.Text = $"Значения: {string.Join(", ", results)}";
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow dice = new MainWindow();
            dice.Show();
            this.Hide();
        }

        private void Exist_click(object sender, RoutedEventArgs e)
        {
            Registration dice = new Registration();
            dice.Show();
            this.Close();
        }

        private void RollDice_D4_Click(object sender, RoutedEventArgs e)
        {

            if (int.TryParse(DiceCountTextBox_D4.Text, out int diceCount) && diceCount > 0)
            {
                DiceItemsControl_D4.Items.Clear();

                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 5); // Случайное число от 1 до 6
                    results.Add(dieValue);
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };

                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D4.Items.Add(dieTextBlock);
                }

                ResultsTextBlock_D4.Text = $"Сумма: {sum}";
                ResultsTextBlock_D4_summ.Text = $"Значения: {string.Join(", ", results)}";
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void RollDice_D8_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiceCountTextBox_D8.Text, out int diceCount) && diceCount > 0)
            {
                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                DiceItemsControl_D8.Items.Clear();

                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 9); // Случайное число от 1 до 8
                    results.Add(dieValue);
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };

                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D8.Items.Add(dieTextBlock);
                }

                ResultsTextBlock_D8.Text = $"Сумма: {sum}";
                ResultsTextBlock_D8_summ.Text = $"Значения: {string.Join(", ", results)}";
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void RollDice_D10_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiceCountTextBox_D10.Text, out int diceCount) && diceCount > 0)
            {
                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                DiceItemsControl_D10.Items.Clear();

                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 11); // Случайное число от 1 до 10
                    results.Add(dieValue);
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };

                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D10.Items.Add(dieTextBlock);
                }

                ResultsTextBlock_D10.Text = $"Сумма: {sum}";
                ResultsTextBlock_D10_summ.Text = $"Значения: {string.Join(", ", results)}";
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void RollDice_D12_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiceCountTextBox_D12.Text, out int diceCount) && diceCount > 0)
            {
                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                DiceItemsControl_D12.Items.Clear();

                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 13); // Случайное число от 1 до 12
                    results.Add(dieValue);
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };

                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D12.Items.Add(dieTextBlock);
                }

                ResultsTextBlock_D12.Text = $"Сумма: {sum}";
                ResultsTextBlock_D12_Summ.Text = $"Значения: {string.Join(", ", results)}";
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void RollDice_D100_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DiceCountTextBox_D100.Text, out int diceCount) && diceCount > 0)
            {
                var random = new Random();
                var results = new List<int>();
                int sum = 0;

                DiceItemsControl_D100.Items.Clear();

                
                for (int i = 0; i < diceCount; i++)
                {
                    int dieValue = random.Next(1, 101); // Случайное число от 1 до 100
                    results.Add(dieValue);
                    
                    sum += dieValue;

                    // создаем визуальный элемент для кубика
                    TextBlock dieTextBlock = new TextBlock
                    {
                        Text = dieValue.ToString(),
                        FontSize = 24,
                        Width = 50,
                        Height = 50,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(2)
                    };


                    AnimateDieRoll(dieTextBlock); // добавляем анимацию

                    DiceItemsControl_D100.Items.Add(dieTextBlock);
                    
                }
                ResultsTextBlock_D100.Text = $"Сумма: {sum},";
                ResultsTextBlock_D100_summ.Text = $"Значения: {string.Join(", ", results)}";
                
            }
            else
            {
                MessageBox.Show("Введите корректное количество кубиков.");
            }
        }

        private void Clear_Result_Text_Block_D4(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D4.Text = "";
            }
        }

        private void Clear_Result_Text_Block_D6(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D6.Text = "";
            }
        }

        private void Clear_Result_Text_Block_D8(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D8.Text = "";
            }
        }

        private void Clear_Result_Text_Block_D10(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D10.Text = "";
            }
        }

        private void Clear_Result_Text_Block_D12(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D12.Text = "";

            }
        }

        private void Clear_Result_Text_Block_D20(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D20.Text = "";

            }
        }

        private void Clear_Result_Text_Block_D100(object sender, RoutedEventArgs e)
        {
            {
                ResultsTextBlock_D100.Text = "";

            }
        }

    }
}




