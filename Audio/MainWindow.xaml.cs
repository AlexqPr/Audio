using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Audio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool forplay;
        public string selected;
        string[] files = new string[] { };
        string[] b = new string[] { };
        bool flag = true;
        bool flag2 = true;
        
        public MainWindow()
        {
            InitializeComponent();
            volume.Maximum = 1;
        }

        private void OpenBut_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok)
            {
                var filenames = Directory.GetFiles(dialog.FileName, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f));//достаем файлы без пути
                

                foreach (var item in filenames)//запихиваем в массив новые элементы
                {
                    b = b.Append(item).ToArray();
                }

                b = b.Where(e => Path.GetExtension(e) == ".wav" || Path.GetExtension(e) == ".mp3").ToArray();





                files = Directory.GetFiles(dialog.FileName);
                files = files.Where(e => Path.GetExtension(e) == ".wav" || Path.GetExtension(e) == ".mp3").ToArray(); //можно добавтить еще расшиирений но я не знаю каких
                b = b.OrderBy(f => f).ToArray();//сортировка
                files = files.OrderBy(f => f).ToArray();//сортировка
                Music.ItemsSource = b;
                Music.SelectedIndex = 0;
                Play(files[Music.SelectedIndex]);
            }
            
        }

        private void Music_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected = Music.Items[Music.SelectedIndex].ToString();
            //MessageBox.Show(selected);
            //MessageBox.Show(files.Length.ToString());
            Play(files[Music.SelectedIndex]);
        }

        private void Play(string song)
        {
            var potok = new Thread(Timer);
            potok.Start();
            media.Source = new Uri(song);
            media.Play();
            forplay = true;
            media.Volume = 0.7;
            volume.Value = media.Volume;
            

        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           if(forplay == true)
            {
                media.Pause();
                forplay = false;
            }
           else
            {
                media.Play();
                forplay = true;
            }
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if (Music.SelectedIndex + 2  > files.Length)
            {
                Music.SelectedIndex = 0;
                Play(files[Music.SelectedIndex]);
            }
            else
            {
                Music.SelectedIndex = Music.SelectedIndex + 1;
                Play(files[Music.SelectedIndex]);
            }


        }

        private void early_Click(object sender, RoutedEventArgs e)
        {
            if(Music.SelectedIndex - 1  < 0)
            {
                Play(files[Music.SelectedIndex]);
               
            }
            else
            {
                Music.SelectedIndex = Music.SelectedIndex - 1;
                Play(files[Music.SelectedIndex]);
               
            }
           
            
        }

        private void audioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Position = new TimeSpan(Convert.ToInt64(audioSlider.Value));
          
        }
        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            audioSlider.Maximum = media.NaturalDuration.TimeSpan.Ticks;
            //MessageBox.Show(media.NaturalDuration.TimeSpan.Ticks.ToString());
            //MessageBox.Show(duration.TotalMinutes.ToString());
        }

        private void replay_Click(object sender, RoutedEventArgs e)
        {
            flag = !flag;
           
        }

        private void Timer()
        {
            while(true)
            {
                try
                {
                    this.Dispatcher.Invoke(() => { 
                        audioSlider.Value = Convert.ToDouble(media.Position.Ticks);
                        start.Content = Math.Round(media.Position.TotalMinutes,2);
                        end.Content = Math.Round(TimeSpan.FromTicks(media.NaturalDuration.TimeSpan.Ticks - media.Position.Ticks).TotalMinutes,2);
                        if (flag == false)
                        {
                            if (audioSlider.Value == Convert.ToDouble(media.NaturalDuration.TimeSpan.Ticks))
                            {
                                media.Position = TimeSpan.Zero;//повтор музла
                                media.Play();
                            }
                        }
                        else if (flag == true && audioSlider.Value == Convert.ToDouble(media.NaturalDuration.TimeSpan.Ticks))//переключение на след когда закончился
                        {
                            if (Music.SelectedIndex + 2 > files.Length)
                            {
                                Music.SelectedIndex = 0;
                                Play(files[Music.SelectedIndex]);
                            }
                            else
                            {
                                Music.SelectedIndex = Music.SelectedIndex + 1;
                                Play(files[Music.SelectedIndex]);
                            }
                        }
                    });
                    Thread.Sleep(100);
                    
                }
                catch
                {
                    
                  //так по рофлу просто ошибку выдавал на всякий оставлю
                }
                
            }

            
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)//слайдер громкости
        {
            media.Volume = volume.Value;
        }

        private void random_Click(object sender, RoutedEventArgs e)
        {
            if(flag2  == false)
            {
                b = b.OrderBy(f => f).ToArray();//сортировка
                files = files.OrderBy(f => f).ToArray();//сортировка
                Music.Items.Refresh();
                Music.ItemsSource = b;
                Play(files[Music.SelectedIndex]);
            }
            else
            {
                var rand = new Random();
                for (int i = b.Length - 1; i >= 0; i--)//рандомно перемешиваем
                {
                    int j = rand.Next(i);
                    var temp = b[i];
                    var temp2 = files[i];
                    b[i] = b[j];
                    files[i] = files[j];
                    b[j] = temp;
                    files[j] = temp2;
                }
                Music.ItemsSource =  b;
                Music.Items.Refresh();
                Play(files[Music.SelectedIndex]);
            }
            flag2 = !flag2;
        }
    }
}
