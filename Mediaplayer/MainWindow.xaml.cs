using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Media_Player
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Allow dragging and sliding wherever mouse is pressed on slider for seeker bar
            seekerBar.ApplyTemplate();
            Thumb thumbSeeker = (seekerBar.Template.FindName("PART_Track", seekerBar) as Track).Thumb;
            thumbSeeker.MouseEnter += new MouseEventHandler(thumb_MouseEnter);

            // Allow dragging and sliding wherever mouse is pressed on slider for volume bar
            volumeAdjust.ApplyTemplate();
            Thumb thumbVolume = (volumeAdjust.Template.FindName("PART_Track", volumeAdjust) as Track).Thumb;
            thumbVolume.MouseEnter += new MouseEventHandler(thumb_MouseEnter);
        }

        private void thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            // I think this sends another mouse down event instantly to allow click and drag from anywhere on the slider track
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
                args.RoutedEvent = MouseLeftButtonDownEvent;

                (sender as Thumb).RaiseEvent(args);
            }
        }

        private DispatcherTimer globalTimer = new DispatcherTimer();

        private void selectMediaClick(object sender, RoutedEventArgs e)
        {
            // Select file to play
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) { return; }

            // Set volume
            mediaPlayer.Volume = volumeAdjust.Value;

            // Set button to play
            playMedia.Visibility = Visibility.Hidden;
            pauseMedia.Visibility = Visibility.Visible;

            // Set media element source and begin playing
            mediaPlayer.Source = new Uri(ofd.FileName);
            mediaPlayer.Play();

            // Begin global timer
            globalTimer.Interval = TimeSpan.FromMilliseconds(1);
            globalTimer.Tick += globalTimerTick;
            globalTimer.Start();
        }

        public void globalTimerTick(object sender, EventArgs e)
        {
            // If the media event has not fired yet, return
            if (!mediaPlayer.NaturalDuration.HasTimeSpan) { return; }

            // If the media has finished playing
            if (mediaPlayer.Position.TotalMilliseconds == mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds)
            {
                pauseMedia.Visibility = Visibility.Hidden;
                playMedia.Visibility = Visibility.Visible;
            }

            // Settings seekbar maximum to total milliseconds, allowing us to seek to any point using the sliders value
            seekerBar.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            seekerBar.Value = mediaPlayer.Position.TotalMilliseconds;

            // Update current duration clock and total duration of media
            currentDuration.Text = mediaPlayer.Position.ToString("mm\\:ss");
            totalDuration.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString("mm\\:ss");
        }

        private void pauseMediaClick(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            globalTimer.Stop();

            // Swap play/pause button visibility
            pauseMedia.Visibility = Visibility.Hidden;
            playMedia.Visibility = Visibility.Visible;
        }

        private void playMediaClick(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Position.TotalMilliseconds == mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds)
            {
                mediaPlayer.Position = TimeSpan.FromMilliseconds(0);
            }

            mediaPlayer.Play();
            globalTimer.Start();

            // Swap play/pause button visibility
            playMedia.Visibility = Visibility.Hidden;
            pauseMedia.Visibility = Visibility.Visible;
        }

        private void seekerBarDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            globalTimer.Stop();
        }

        private void seekerBarDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            globalTimer.Start();

            // Set the position of the media player to where the user drags the slider thumb
            mediaPlayer.Position = TimeSpan.FromMilliseconds(seekerBar.Value);
        }

        private void volumeAdjustValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Set volume of media element when adjusting volume slider
            mediaPlayer.Volume = volumeAdjust.Value;
        }

        private void ControlsMouseEnter(object sender, MouseEventArgs e)
        {
            // Set the controls to visible when mouse enters application
            //controlsGrid.Visibility = Visibility.Visible;
        }

        private void ControlsMouseLeave(object sender, MouseEventArgs e)
        {
            // Set the controls to hidden when mouse leaves application
            //controlsGrid.Visibility = Visibility.Hidden;
        }
    }
}