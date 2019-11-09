﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public class Song
    {
        public string id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AlbumArtist { get; set; }
        public string Album { get; set; }
        public TimeSpan Duration { get; set; }
        public uint Year { get; set; }
        public int TrackNumber { get; set; }
        public string DiscNumber { get; set; }


        public bool isFlavour { get; set; }

        public uint Bitrate { get; set; }
        public StorageFile File { get; set; }

        //public BitmapImage AlbumArt { get; set; }

        public override bool Equals(object obj)
        {
            if (this.id == (obj as Song).id) return true;
            return false;
        }

        public String SongToJSON()
        {
            String JSON = "{";
            return JSON;
        }

        public string GetPath()
        {
            return File.Path;
        }

        public string GetDuration()
        {
            //Double dur = Duration.TotalSeconds / 100;
            //dur.ToString("N2");
            //return dur.ToString("N2").Replace('.', ':');
            return Duration.ToString(@"mm\:ss");
        }

        public Visibility CheckIfFlavour()
        {
            if (isFlavour)
            {
                return 0;
            }
            else
            {
                return (Visibility)1;
            }
        }

        /*public async void SetAlbumArt()
        {
            var thumbnail = await File.GetThumbnailAsync(ThumbnailMode.MusicView, 300);
            //var result = task.WaitAndUnwrapException();
            //using (StorageItemThumbnail thumbnail = File.GetThumbnailAsync(ThumbnailMode.MusicView, 300).Wait() )
            //{
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    //ImageControl.Source = bitmapImage;
                    AlbumArt =  bitmapImage;
                }
                else
                {
                    AlbumArt =  null;
                }
              
            //}
        }*/

    }

    
}
