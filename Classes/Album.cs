﻿using Music_thing.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public class Album : INotifyPropertyChanged
    {
        public string name { get; set; }

        public string artist { get; set; }

        public string key { get; set; }

        public int year { get; set; }

        //public BitmapImage albumart;
        //public Windows.UI.Xaml.Media.ImageSource albumart200;

        //public Windows.UI.Xaml.Media.ImageSource albumart250;

        //public Windows.UI.Xaml.Media.ImageSource albumart100;

        // public BitmapIcon icon;

        //Should only be in memory for the album list.
        [JsonIgnore]
        public ImageSource albumart;

        public event PropertyChangedEventHandler PropertyChanged;

        public string albumartsongid { get; set; }

        public List<string> Songids { get; set; }
        = new List<string>();

        public ObservableCollection<Song> ObserveSongs(ConcurrentDictionary<string, Song> SongDict)
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            foreach (string songid in Songids)
            {
                Song song = SongDict[songid];
                song.isFlavour = false; //MB REMOVE
                Songs.Add(song);
            }

            return Songs;
        }

        /*public ObservableCollection<Song> ObserveSongsForFlavour()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            foreach (int songid in Songids)
            {
                Song song = SongListStorage.SongDict[songid];
                song.isFlavour = true;
                Songs.Add(song);
            }

            return Songs;
        }*/

        public List<string> AddSong(string songid, ConcurrentDictionary<string, Song> SongDict)
        {
           // if (Songids.Count != 0) //fix
           // {
                SetAlbumArt(songid, SongDict);
           // }
            Songids.Add(songid);
            OrderByTrack(SongDict);
            return Songids;
        }

        public string GetStringYear()
        {
            if (year == 0)
            {
                return "N/A";
            }
            return year.ToString();
        }

        //Function to sort songs by track number.
        public void OrderByTrack(ConcurrentDictionary<string, Song> SongDict)
        {
            //Need to add disk number support
            //Songids = Songids.OrderBy(x => SongListStorage.SongDict[x].TrackNumber) as List<int>;

            Songids.Sort((x, y) => SongDict[x].TrackNumber - SongDict[y].TrackNumber);
        }

        public async void SetAlbumArt(string songid, ConcurrentDictionary<string, Song> SongDict)
        {
            Song song = SongDict[songid];
            if (albumartsongid == null || SongDict[songid].TrackNumber == 1)
            {
                var file = await song.GetFile();
                var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView);
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    albumartsongid = songid;
                }
            }
        }

        public async Task<ImageSource> GetAlbumArt(int size, ConcurrentDictionary<string, Song> SongDict)
        {
            if (albumartsongid != null)
            {
                return await SongDict[albumartsongid].GetArt(size);
            }
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/Album.png"));
            bitmapImage.DecodePixelHeight = size;
            bitmapImage.DecodePixelWidth = size;
            return bitmapImage;
        }

        [JsonIgnore]
        public ImageSource Albumart
        {
            get
            {
                return this.albumart;
            }
            set
            {
                NotifyPropertyChanged();
                if (value != this.albumart)
                {
                    this.albumart = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            // Your UI update code goes here!
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        /* public async void SetAlbumArt(string songid)
            {
                Song song = SongListStorage.SongDict[songid];
                if (albumart200 == null || SongListStorage.SongDict[songid].TrackNumber == 1)
                {
                    var file = await song.GetFile();
                    var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 200);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        //ImageControl.Source = bitmapImage;

                        bitmapImage.DecodePixelHeight = 200;
                        bitmapImage.DecodePixelWidth = 200;
                        albumart200 = bitmapImage;
                    }

                    thumbnail = await (await song.GetFile()).GetThumbnailAsync(ThumbnailMode.MusicView, 250);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        //ImageControl.Source = bitmapImage;

                        bitmapImage.DecodePixelHeight = 250;
                        bitmapImage.DecodePixelWidth = 250;
                        albumart250 = bitmapImage;
                    }

                    thumbnail = await (await song.GetFile()).GetThumbnailAsync(ThumbnailMode.MusicView, 100);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        //ImageControl.Source = bitmapImage;

                        bitmapImage.DecodePixelHeight = 100;
                        bitmapImage.DecodePixelWidth = 100;
                        albumart100 = bitmapImage;
                    }


                    //using (var )
                }
            }*/









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
                AlbumArt = bitmapImage;
            }
            else
            {
                AlbumArt = null;
            }

            //}
        }*/

    }

}