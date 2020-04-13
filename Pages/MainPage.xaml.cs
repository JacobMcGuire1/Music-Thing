﻿using Microsoft.Graphics.Canvas.Effects;
using Music_thing.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Music_thing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        

        public int currentid;

        String currentpage;

        public MainPage()
        {
            this.InitializeComponent();

            currentid = 0;

            //BAck stuff
            //KeyboardAccelerator GoBack = new KeyboardAccelerator();
            //GoBack.Key = VirtualKey.GoBack;
            //GoBack.Invoked += BackInvoked;
            //KeyboardAccelerator AltLeft = new KeyboardAccelerator();
            //AltLeft.Key = VirtualKey.Left;
            // AltLeft.Invoked += BackInvoked;
            ///    //  KeyboardAccelerator t = new KeyboardAccelerator();
            //   t.Key = VirtualKey.Back;
            //  t.Invoked += BackInvoked;
            //   this.KeyboardAccelerators.Add(GoBack);
            //  this.KeyboardAccelerators.Add(AltLeft);
            //  this.KeyboardAccelerators.Add(t);
            // ALT routes here
            //   AltLeft.Modifiers = VirtualKeyModifiers.Menu;



            //SongListStorage.GetSongList(); //Should change/remove this.
            //Database.GetSongs(); //Temp
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);
            try
            {
                VolumeSlider.Value = (double)Windows.Storage.ApplicationData.Current.LocalSettings.Values["chosenVol"] * 100;
            }
            catch { VolumeSlider.Value = 30; }


            //SongListStorage.FindArtists();

            mediaPlayerElement.SetMediaPlayer(Media.Instance.mediaPlayer);
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ShowUnpinnedFlavours"))
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowUnpinnedFlavours"] = true;
            }
            ShowUnpinnedCheckBox.IsChecked = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowUnpinnedFlavours"];

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await LoadPinnedFlavours();
        }

        //Returns the instance of the media instance to allow information from it to be accessed.
        public Media MediaProxy { get { return Media.Instance; } }

        public void NotificationMessage(String Message)
        {
            progressbar.Visibility = Visibility.Collapsed;
            progressbartextblock.Visibility = Visibility.Visible;
            progressbartextblock.Text = Message;
        }

        public void DisplayLoading(int songsloaded, int totalfiles, int filesscanned, bool complete)
        {
            progressbartextblock.Visibility = Visibility.Visible;
            if (!complete)
            {
                progressbartextblock.Text = "Scanning for music. Found " + songsloaded.ToString() + " songs so far.";
                progressbar.Visibility = Visibility.Visible;
                if (totalfiles == 0)
                {
                    progressbar.IsIndeterminate = true;
                }
                else
                {
                    progressbar.IsIndeterminate = false;
                    progressbar.Value = filesscanned;
                    progressbar.Maximum = totalfiles;
                }
                
            }
            else
            {
                progressbartextblock.Text = "Loaded " + songsloaded + " songs.";
                progressbar.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdatePlaylistName(long playlistid)
        {
            var newname = SongListStorage.PlaylistDict[playlistid].Name;
            foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {

                if (menuitem.Name.Equals("Flavour"))
                {
                    var tag = (long)menuitem.Tag;
                    if (tag == playlistid)
                    {
                        var stack1 = (StackPanel)((NavigationViewItem)menuitem).Content;
                        var innerstack = (StackPanel)stack1.Children[1];
                        var nametb = (TextBlock)innerstack.Children[0];
                        nametb.Text = newname;
                    }
                }
            }
        }

        //Loads the pinned flavours and playlists into the list on the left of the screen.

        public async Task LoadPinnedFlavours()
        {
            List<long> inlist = new List<long>();
            bool showunpinnedflavours = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowUnpinnedFlavours"];
            foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {

                if (menuitem.Name.Equals("Flavour"))
                {
                    var tag = (long)menuitem.Tag;
                    inlist.Add(tag);
                }
            }
            var playlists = SongListStorage.PlaylistDict.Values;
            //Loops through the playlists to load the necessary ones.
            foreach(Playlist playlist in playlists)
            {
                if ((playlist.pinned || showunpinnedflavours) && !inlist.Contains(playlist.PlaylistID))
                {
                    await LoadFlavour(playlist);
                }
            }
            for (int i = NavView.MenuItems.Count - 1; i >= 0; i--)
            {
                NavigationViewItemBase menuitem = (NavigationViewItemBase)NavView.MenuItems[i];
                if (menuitem.Name.Equals("Flavour"))
                {
                    var key = (long)menuitem.Tag;
                    if (!SongListStorage.PlaylistDict.ContainsKey(key) || (!SongListStorage.PlaylistDict[key].pinned && !showunpinnedflavours))
                    {
                        NavView.MenuItems.Remove(menuitem);
                        //menuitem.Visibility = Visibility.Collapsed;
                    }
                }
            }
          

            /*newlist.Add((long)(test as NavigationViewItem).Tag);
            for (int i = NavView.MenuItems.Count - 1; i >= 0; i--)
            {
                NavigationViewItemBase menuitem = (NavigationViewItemBase)NavView.MenuItems[i];
                if (menuitem.Name.Equals("Flavour") && (NavigationViewItem)menuitem != test)
                {
                    NavView.MenuItems.Remove(menuitem);
                }
            }*/
            //var orderedlist = NavView.MenuItems.OrderBy(x => ((Playlist)x).Name); //get rid of
            //NavView.MenuItems.orde
            //artist.Albums.Sort((x, y) => SongListStorage.AlbumDict[y].Year.CompareTo(SongListStorage.AlbumDict[x].Year));

            /*foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {
                if (menuitem.Name.Equals("Flavour"))
                {
                    var key = (long)menuitem.Tag;
                    var tempname = SongListStorage.PlaylistDict[key].Name;
                    if (!SongListStorage.PlaylistDict.ContainsKey(key) || (!SongListStorage.PlaylistDict[key].pinned && !SongListStorage.ShowUnpinnedFlavours))
                    {
                        NavView.MenuItems.Remove(menuitem);
                        //menuitem.Visibility = Visibility.Collapsed;
                    }
                }
            }*/
        }

        private async Task LoadFlavour(Playlist playlist)
        {
            NavigationViewItem navigationViewItem = new NavigationViewItem();
            var stackpanelouter = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            TextBlock nametb = new TextBlock
            {
                Text = playlist.Name
            };
            //nametb.FontWeight = FontWeights.Bold; 

            string desctext = playlist.isflavour ? playlist.Artist + " - " + playlist.albumname : "Playlist";
            TextBlock artisttb = new TextBlock
            {
                Text = desctext,
                FontWeight = FontWeights.ExtraLight,
                FontSize = 12
            };

            stackpanel.Children.Add(nametb);
            stackpanel.Children.Add(artisttb);

            //IMAGESTUFF

            Image icon = new Image();
            try
            {
                if (playlist.isflavour)
                {
                    icon.Source = await SongListStorage.AlbumDict[playlist.albumkey].GetAlbumArt(15, SongListStorage.SongDict);
                }
                else
                {
                    var art = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"));
                    art.DecodePixelHeight = 15;
                    art.DecodePixelWidth = 15;
                    icon.Source = art;
                }
                
            }
            catch { }
            icon.Width = 20;
            icon.Height = 20;
            icon.Margin = new Thickness(0, 0, 14, 0);

            stackpanelouter.Children.Add(icon);
            stackpanelouter.Children.Add(stackpanel);

            navigationViewItem.Content = stackpanelouter;
            navigationViewItem.Name = "Flavour";
            navigationViewItem.AllowDrop = true;
            navigationViewItem.Drop += NavigationViewItem_Drop;
            navigationViewItem.DragOver += NavigationViewItem_DragOver;

            navigationViewItem.Tag = playlist.PlaylistID;

            var flyout = new MenuFlyout();

            var flyoutRename = new MenuFlyoutItem() { Text = "Rename", Tag = playlist.PlaylistID };
            var flyoutPlay = new MenuFlyoutItem() { Text = "Play", Tag = playlist.PlaylistID };
            var flyoutAddToNowPLaying = new MenuFlyoutItem() { Text = "Add To Now Playing", Tag = playlist.PlaylistID };
            var flyoutDelete = new MenuFlyoutItem() { Text = "Delete", Tag = playlist.PlaylistID };

            var flyoutUnpin = new MenuFlyoutItem() { Text = "Unpin", Tag = playlist.PlaylistID };
            if (!playlist.pinned) flyoutUnpin.Text = "Pin";

            flyoutRename.Click += FlyoutRename_Click;
            flyoutAddToNowPLaying.Click += FlyoutAddToNowPLaying_Click;
            flyoutPlay.Click += FlyoutPlay_Click;
            flyoutDelete.Click += FlyoutDelete_Click;
            flyoutUnpin.Click += FlyoutUnpin_Click;

            flyout.Items.Add(flyoutPlay);
            flyout.Items.Add(flyoutAddToNowPLaying);
            if (playlist.isflavour) flyout.Items.Add(flyoutUnpin);
            flyout.Items.Add(flyoutRename);
            flyout.Items.Add(flyoutDelete);
            navigationViewItem.ContextFlyout = flyout;

            NavView.MenuItems.Add(navigationViewItem);
        }

        private void FlyoutUnpin_Click(object sender, RoutedEventArgs e)
        {
            var playlistid = (long)(((MenuFlyoutItem)sender).Tag);
            var playlist = SongListStorage.PlaylistDict[playlistid];
            if (playlist.pinned)
            {
                ((MenuFlyoutItem)sender).Text = "Pin";
                playlist.pinned = false;
            }
            else
            {
                ((MenuFlyoutItem)sender).Text = "Unpin";
                playlist.pinned = true;
            }
            
        }

        private async void FlyoutDelete_Click(object sender, RoutedEventArgs e)
        {
            var playlistid = (long)(((MenuFlyoutItem)sender).Tag);
            var playlist = SongListStorage.PlaylistDict[playlistid];

            if (playlist.isflavour) SongListStorage.AlbumDict[playlist.albumkey].RemoveFlavour(playlistid);
            await playlist.DeleteFile();
            SongListStorage.PlaylistDict.Remove(playlistid, out playlist);

            await App.GetForCurrentView().LoadPinnedFlavours();
        }

        private async void FlyoutPlay_Click(object sender, RoutedEventArgs e)
        {
            var playlistid = (long)(((MenuFlyoutItem)sender).Tag);
            await SongListStorage.PlaylistDict[playlistid].Play();
        }

        private async void FlyoutAddToNowPLaying_Click(object sender, RoutedEventArgs e)
        {
            var playlistid = (long)(((MenuFlyoutItem)sender).Tag);
            await SongListStorage.PlaylistDict[playlistid].AddToPlaylist();
        }

        private async void FlyoutRename_Click(object sender, RoutedEventArgs e)
        {
            var playlistid = (long)(((MenuFlyoutItem)sender).Tag);
            await SongListStorage.PlaylistDict[playlistid].Rename();
        }

        private void NavigationViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.Text)) ? DataPackageOperation.Copy : DataPackageOperation.None;
        }

        //Adds a song to a flavour/playlist if it is dragged onto it.
        private async void NavigationViewItem_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var def = e.GetDeferral();

                var send = (NavigationViewItem)sender;
                var playlistid = (long)send.Tag;
                Playlist flavour = SongListStorage.PlaylistDict[playlistid];

                var s = await e.DataView.GetTextAsync();
                var ids = s.Split(Environment.NewLine);
                if (ids.Length > 0)
                {
                    foreach (string id in ids)
                    {
                        flavour.AddSong(id);
                    }
                }
                e.AcceptedOperation = DataPackageOperation.Copy;
                //await SongListStorage.SaveFlavours();
                await flavour.SavePlaylistFile(false);
                App.GetForCurrentView().NotificationMessage("Added song(s) to playlist '" + SongListStorage.PlaylistDict[playlistid].Name + "'.");
                def.Complete();
            }
        }

        //Loads a page when it is navigated to.
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                if (item.Name.Equals("Flavour"))
                {
                    HandleFlavourClick(item);
                }
                /*else if (item.Name.Equals("ButtonHolder"))
                {
                    //args.
                    sender.SelectedItem = null; //doesnt work
                    //sender.sel
                }*/
                else
                {
                    switch (item.Tag.ToString())
                    {
                        case "songs":
                            ContentFrame.Navigate(typeof(SongList));
                            //NavView.Header = "Songs";
                            break;
                        case "artists":
                            ContentFrame.Navigate(typeof(ArtistList));
                            //NavView.Header = "Artists";
                            break;
                        case "albums":
                            ContentFrame.Navigate(typeof(AlbumList));
                            //NavView.Header = "Albums";
                            break;
                        case "nowPlaying":
                            ContentFrame.Navigate(typeof(NowPlaying));
                            //NavView.Header = "Now Playing";
                            break;
                        case "recentlyPlayed":
                            ContentFrame.Navigate(typeof(RecentlyPlayed));
                            //NavView.Header = "Recently Played";
                            break;
                    }
                }

                
            }
        }

        //Loads the flavour/playlist when it's clicked on.
        private void HandleFlavourClick(NavigationViewItem item)
        {
            var id = (long)item.Tag;
            //ContentFrame.Navigate(typeof(AlbumPage), dict["albumkey"]);
            ContentFrame.Navigate(typeof(AlbumPage), id);
        }

        public enum MediaState
        {
            Stopped, Playing, Paused
        }

        //MAKE THIS ONLY HAPPEN WHEN THE SELECTION DOESN'T CHANGE
        //Reloads the current page when it is clicked on.
        private void NavView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (currentpage)
            {
                case "songs":
                    ContentFrame.Navigate(typeof(SongList));
                    //NavView.Header = "Songs";
                    break;
                case "artists":
                    ContentFrame.Navigate(typeof(ArtistList));
                    //NavView.Header = "Artists";
                    break;
                case "albums":
                    ContentFrame.Navigate(typeof(AlbumList));
                    //NavView.Header = "Albums";
                    break;
                case "nowPlaying":
                    ContentFrame.Navigate(typeof(NowPlaying));
                    //NavView.Header = "Now Playing";
                    break;
                case "recentlyPlayed":
                    ContentFrame.Navigate(typeof(RecentlyPlayed));
                    //NavView.Header = "Recently Played";
                    break;
            }
        }

        //Shows song suggestions in the searchbox based on what has been typed.
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = SongListStorage.SearchSongs(SearchBox.Text);
                var textresults = new List<String>();
                foreach(Song song in results)
                {
                    textresults.Add(song.Title);
                }
                SearchBox.ItemsSource = textresults;
            }
        }
        
        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender,
                         KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            //if (NavView.IsPaneOpen &&
            //    (NavView.DisplayMode == muxc.NavigationViewDisplayMode.Compact ||
            //     NavView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal))
            //    return false;

            ContentFrame.GoBack();
            return true;
        }

        private bool On_ForwardRequested()
        {
            if (!ContentFrame.CanGoForward)
                return false;
            ContentFrame.GoForward();
            return true;
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            progressbar.Visibility = Visibility.Collapsed;
            progressbartextblock.Visibility = Visibility.Collapsed;
        }

        private async void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playlistname = "";
            bool err = false;
            while (playlistname == "")
            {
                playlistname = "New Playlist";
                ContentDialog nameFlavourDialog = new ContentDialog()
                {
                    Title = "Name your playlist",
                    CloseButtonText = "Ok"
                };
                TextBox textBox = new TextBox()
                {

                };
                if (!err)
                {
                    nameFlavourDialog.Content = textBox;
                }
                else
                {
                    var stackpanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical
                    };
                    TextBlock errormsgtext = new TextBlock()
                    {
                        Text = "Error: Please try another name."
                    };
                    stackpanel.Children.Add(textBox);
                    stackpanel.Children.Add(errormsgtext);
                    nameFlavourDialog.Content = stackpanel;
                }
                await nameFlavourDialog.ShowAsync();

                playlistname = textBox.Text;
                err = true;
            }

            //Flavour flavour = new Flavour()
            Playlist playlist = new Playlist(playlistname);
            await playlist.SavePlaylistFile(true);
            SongListStorage.PlaylistDict.TryAdd(playlist.PlaylistID, playlist);

            await App.GetForCurrentView().LoadPinnedFlavours(); //Because the flavour is pinned by default the list is updated in the UI.
            //await SongListStorage.SaveFlavours();
        }

        

        private void SongNameTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var songfile = SongListStorage.GetCurrentSong();
            ContentFrame.Navigate(typeof(AlbumPage), songfile.AlbumKey);
        }

        private void CurrentArtistsTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var songfile = SongListStorage.GetCurrentSong();
            //Navigates to the artist if it exists, otherwise the albumartist.
            if (SongListStorage.ArtistDict.ContainsKey(songfile.ArtistKey))
            {
                ContentFrame.Navigate(typeof(ArtistPage), songfile.ArtistKey);
            }
            else
            {
                if (SongListStorage.ArtistDict.ContainsKey(songfile.AlbumArtist))
                {
                    ContentFrame.Navigate(typeof(ArtistPage), songfile.AlbumArtist);
                }
            }
        }

        private void AlbumArtImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var songfile = SongListStorage.GetCurrentSong();
            ContentFrame.Navigate(typeof(AlbumPage), songfile.AlbumKey);
        }

        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            
        }

        private void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            if (ptr.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                Windows.UI.Input.PointerPoint ptrPt = e.GetCurrentPoint(this);
                if (ptrPt.Properties.IsXButton1Pressed)
                {
                    //On_BackRequested();
                }
            }
        }

        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Input.Pointer ptr = e.Pointer;
            if (ptr.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                Windows.UI.Input.PointerPoint ptrPt = e.GetCurrentPoint(this);
                if (ptrPt.Properties.IsXButton1Pressed)
                {
                    On_BackRequested();
                }
                else if(ptrPt.Properties.IsXButton2Pressed)
                {
                    On_ForwardRequested();
                }
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            var k = e.NewSize.Width;
            SmallScreenBeams(k);
            /*if (k < 1000.0)
            {
                //SmallScreenBeams(true);
                var coldef = new ColumnDefinition()
                {
                    Width = new GridLength(0, GridUnitType.Star)
                };
                BottomGrid.ColumnDefinitions[2] = coldef;
            }
            else
            {
                //SmallScreenBeams(false);
                var coldef = new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };
                BottomGrid.ColumnDefinitions[2] = coldef;
            }*/
        }

        private void SmallScreenBeams(double size)
        {
            SmolGrid.Children.Clear();
            BottomGrid.Children.Clear();
            VolumeNowPlayingGrid.Children.Clear();
            //SongTextStackPanel.Children.Remove(mediaPlayerElement);
            //SongInfoStackPanel.Children.Remove(VolumeStack);
            if (size < 800.0)
            {
                Grid.SetColumn(VolumeStack, 1);
                VolumeNowPlayingGrid.Children.Add(SongInfoStackPanel);
                VolumeNowPlayingGrid.Children.Add(VolumeStack);
                //VolumeStack.HorizontalAlignment = HorizontalAlignment.Right;
                ProgressStackPanel.Visibility = Visibility.Collapsed;
                //SongNameTextBlock.Margin = new Thickness(0, 0, 0, 0);
                //SongTextStackPanel.Children.Add(mediaPlayerElement);
                mediaPlayerElement.MaxWidth = 400;
                mediaPlayerElement.TransportControls.IsCompact = true;

                SmolGrid.Children.Add(VolumeNowPlayingGrid);
                SmolGrid.Children.Add(mediaPlayerElement);
                SmolGrid.Visibility = Visibility.Visible;
                BottomGrid.Visibility = Visibility.Collapsed;
            }
            else if(size < 1200.0)
            {
                Grid.SetColumn(VolumeStack, 2);
                ProgressStackPanel.Visibility = Visibility.Collapsed;
                VolumeStack.Visibility = Visibility.Visible;
                //SongNameTextBlock.Margin = new Thickness(0, 25, 0, 0);
                mediaPlayerElement.MaxWidth = 321312;
                mediaPlayerElement.TransportControls.IsCompact = false;
                BottomGrid.Children.Add(SongInfoStackPanel);
                BottomGrid.Children.Add(mediaPlayerElement);
                //BottomGrid.Children.Add(ProgressStackPanel);
                BottomGrid.Children.Add(VolumeStack);
                SmolGrid.Visibility = Visibility.Collapsed;
                BottomGrid.Visibility = Visibility.Visible;
                var coldef = new ColumnDefinition()
                {
                    Width = new GridLength(0, GridUnitType.Star)
                };
                BottomGrid.ColumnDefinitions[3] = coldef;
            }
            else
            {
                Grid.SetColumn(VolumeStack, 2);
                ProgressStackPanel.Visibility = Visibility.Visible;
                var coldef = new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };
                BottomGrid.ColumnDefinitions[3] = coldef;
                //SongNameTextBlock.Margin = new Thickness(0, 25, 0, 0);
                mediaPlayerElement.MaxWidth = 321312;
                mediaPlayerElement.TransportControls.IsCompact = false;
                BottomGrid.Children.Add(SongInfoStackPanel);
                BottomGrid.Children.Add(mediaPlayerElement);
                BottomGrid.Children.Add(VolumeStack);
                BottomGrid.Children.Add(ProgressStackPanel);
                SmolGrid.Visibility = Visibility.Collapsed;
                BottomGrid.Visibility = Visibility.Visible;
            }
            
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var volume = e.NewValue / 100;
            Media.Instance.chosenVol = volume;
            Media.Instance.ChangeVolume();
            VolumeLevel.Text = e.NewValue.ToString();
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["chosenvol"] = Media.Instance.chosenVol;
            if (Media.Instance.chosenVol == 0)
            {
                MuteButton.Content = "\xE74F";
            }
            else
            {
                MuteButton.Content = "\xE767";
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Media.Instance.Muted)
            {
                MuteButton.Content = "\xE74F";
            }
            else
            {
                MuteButton.Content = "\xE767";
            }
            Media.Instance.ToggleMute();
        }

        private async void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool showunpinnedflavours = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowUnpinnedFlavours"];
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowUnpinnedFlavours"] = !showunpinnedflavours;
            await App.GetForCurrentView().LoadPinnedFlavours();
        }

        private void ButtonHolder_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
