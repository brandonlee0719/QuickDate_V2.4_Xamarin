using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS; 
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Core.Content;
using Bumptech.Glide;
using AT.Markushi.UI;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using QuickDate.Activities.Base;
using QuickDate.Helpers.CacheLoaders;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Fonts;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace QuickDate.Activities.MyProfile
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditProfileActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.IInputCallback
    {
        #region Variables Basic

        private ImageView ImageView1, ImageView2, ImageView3, ImageView4, ImageView5, ImageView6;
        private ImageView IconAboutEdit, IconProfileInfoEdit, IconInterestEdit, IconLooksEdit, IconPersonalityInfoEdit, IconLifestyleEdit, IconFavoriteEdit;
        private CircleButton BtnBoostImage1, BtnBoostImage2, BtnBoostImage3, BtnBoostImage4, BtnBoostImage5, BtnBoostImage6;
        private TextView PlayIcon1, PlayIcon2, PlayIcon3, PlayIcon4, PlayIcon5, PlayIcon6;
        private TextView CountPercent, TxtAbout, TxtName, TxtGender, TxtBirthday, TxtLocation, TxtLanguage, TxtRelationship, TxtWork, TxtEducation , TxtSeeMoreMedia;
        private TextView TxtInterest, TxtEthnicity, TxtBody, TxtHeight, TxtHair, TxtCharacter, TxtChildren, TxtFriends, TxtPets, TxtLiveWith, TxtCar;
        private TextView TxtReligion, TxtSmoke, TxtDrink, TxtTravel, TxtMusic, TxtDish, TxtSong, TxtHobby, TxtCity, TxtSport, TxtBook, TxtMovie, TxtColor, TxtTvShow;
        private ProgressBar ProgressBar;
        private int NumImage;
        private string TypeDialog = "", ImageType = "", IdImage1 = "", IdImage2 = "", IdImage3 = "", IdImage4 = "", IdImage5 = "", IdImage6 = "";
        private UserInfoObject DataUser;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.EditMyProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GetMyInfoData();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {



                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageView1 = FindViewById<ImageView>(Resource.Id.ImageView1);
                ImageView2 = FindViewById<ImageView>(Resource.Id.ImageView2);
                ImageView3 = FindViewById<ImageView>(Resource.Id.ImageView3);
                ImageView4 = FindViewById<ImageView>(Resource.Id.ImageView4);
                ImageView5 = FindViewById<ImageView>(Resource.Id.ImageView5);
                ImageView6 = FindViewById<ImageView>(Resource.Id.ImageView6);

                BtnBoostImage1 = FindViewById<CircleButton>(Resource.Id.BoostButton1);
                BtnBoostImage2 = FindViewById<CircleButton>(Resource.Id.BoostButton2);
                BtnBoostImage3 = FindViewById<CircleButton>(Resource.Id.BoostButton3);
                BtnBoostImage4 = FindViewById<CircleButton>(Resource.Id.BoostButton4);
                BtnBoostImage5 = FindViewById<CircleButton>(Resource.Id.BoostButton5);
                BtnBoostImage6 = FindViewById<CircleButton>(Resource.Id.BoostButton6);

                PlayIcon1 = FindViewById<TextView>(Resource.Id.playIcon1);
                PlayIcon2 = FindViewById<TextView>(Resource.Id.playIcon2);
                PlayIcon3 = FindViewById<TextView>(Resource.Id.playIcon3);
                PlayIcon4 = FindViewById<TextView>(Resource.Id.playIcon4);
                PlayIcon5 = FindViewById<TextView>(Resource.Id.playIcon5);
                PlayIcon6 = FindViewById<TextView>(Resource.Id.playIcon6);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PlayIcon1, FontAwesomeIcon.PlayCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PlayIcon2, FontAwesomeIcon.PlayCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PlayIcon3, FontAwesomeIcon.PlayCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PlayIcon4, FontAwesomeIcon.PlayCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PlayIcon5, FontAwesomeIcon.PlayCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PlayIcon6, FontAwesomeIcon.PlayCircle);

                CountPercent = FindViewById<TextView>(Resource.Id.countPercent);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                IconAboutEdit = FindViewById<ImageView>(Resource.Id.iconAboutEdit);
                TxtAbout = FindViewById<TextView>(Resource.Id.AboutTextview);

                TxtSeeMoreMedia = FindViewById<TextView>(Resource.Id.SeeMoreMedia);

                IconProfileInfoEdit = FindViewById<ImageView>(Resource.Id.iconProfileInfoEdit); 
                TxtName = FindViewById<TextView>(Resource.Id.NameTextView);
                TxtGender = FindViewById<TextView>(Resource.Id.GenderTextView);
                TxtBirthday = FindViewById<TextView>(Resource.Id.BirthdayTextView);
                TxtLocation = FindViewById<TextView>(Resource.Id.LocationTextView);
                TxtLanguage = FindViewById<TextView>(Resource.Id.preferred_language_valueTextView);
                TxtRelationship = FindViewById<TextView>(Resource.Id.relationship_status_valueTextView);
                TxtWork = FindViewById<TextView>(Resource.Id.work_status_valueTextView);
                TxtEducation = FindViewById<TextView>(Resource.Id.education_level_valueTextView);

                IconInterestEdit = FindViewById<ImageView>(Resource.Id.iconInterestEdit);
                TxtInterest = FindViewById<TextView>(Resource.Id.interestTextview);

                IconLooksEdit = FindViewById<ImageView>(Resource.Id.iconLooksEdit);
                TxtEthnicity = FindViewById<TextView>(Resource.Id.EthnicityText);
                TxtBody = FindViewById<TextView>(Resource.Id.Body_Type_Value);
                TxtHeight = FindViewById<TextView>(Resource.Id.height_value);
                TxtHair = FindViewById<TextView>(Resource.Id.hair_color_value);

                IconPersonalityInfoEdit = FindViewById<ImageView>(Resource.Id.iconPersonalityinfoEdit);
                TxtCharacter = FindViewById<TextView>(Resource.Id.CharacterText);
                TxtChildren = FindViewById<TextView>(Resource.Id.ChildrenText);
                TxtFriends = FindViewById<TextView>(Resource.Id.FriendsText);
                TxtPets = FindViewById<TextView>(Resource.Id.PetsText);

                IconLifestyleEdit = FindViewById<ImageView>(Resource.Id.iconLifestyleEdit);
                TxtLiveWith = FindViewById<TextView>(Resource.Id.livewithText);
                TxtCar = FindViewById<TextView>(Resource.Id.CarText);
                TxtReligion = FindViewById<TextView>(Resource.Id.ReligionText);
                TxtSmoke = FindViewById<TextView>(Resource.Id.SmokeText);
                TxtDrink = FindViewById<TextView>(Resource.Id.DrinkText);
                TxtTravel = FindViewById<TextView>(Resource.Id.TravelText);

                IconFavoriteEdit = FindViewById<ImageView>(Resource.Id.iconFavouritesEdit);
                TxtMusic = FindViewById<TextView>(Resource.Id.MusicGenreText);
                TxtDish = FindViewById<TextView>(Resource.Id.DishTextView);
                TxtSong = FindViewById<TextView>(Resource.Id.SongTextView);
                TxtHobby = FindViewById<TextView>(Resource.Id.HobbyTextView);
                TxtCity = FindViewById<TextView>(Resource.Id.CityTextView);
                TxtSport = FindViewById<TextView>(Resource.Id.SportTextView);
                TxtBook = FindViewById<TextView>(Resource.Id.BookTextView);
                TxtMovie = FindViewById<TextView>(Resource.Id.MovieTextView);
                TxtColor = FindViewById<TextView>(Resource.Id.ColorTextView);
                TxtTvShow = FindViewById<TextView>(Resource.Id.TVShowTextView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_MyProfile);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ImageView1.Click += ImageView1OnClick;
                    ImageView2.Click += ImageView2OnClick;
                    ImageView3.Click += ImageView3OnClick;
                    ImageView4.Click += ImageView4OnClick;
                    ImageView5.Click += ImageView5OnClick;
                    ImageView6.Click += ImageView6OnClick;
                    BtnBoostImage1.Click += BtnBoostImage1OnClick;
                    BtnBoostImage2.Click += BtnBoostImage2OnClick;
                    BtnBoostImage3.Click += BtnBoostImage3OnClick;
                    BtnBoostImage4.Click += BtnBoostImage4OnClick;
                    BtnBoostImage5.Click += BtnBoostImage5OnClick;
                    BtnBoostImage6.Click += BtnBoostImage6OnClick;
                    IconAboutEdit.Click += IconAboutEditOnClick;
                    IconProfileInfoEdit.Click += IconProfileInfoEditOnClick;
                    IconInterestEdit.Click += IconInterestEditOnClick;
                    IconLooksEdit.Click += IconLooksEditOnClick;
                    IconPersonalityInfoEdit.Click += IconPersonalityInfoEditOnClick;
                    IconLifestyleEdit.Click += IconLifestyleEditOnClick;
                    IconFavoriteEdit.Click += IconFavoriteEditOnClick;
                    TxtSeeMoreMedia.Click += TxtSeeMoreMediaOnClick;
                }
                else
                {
                    ImageView1.Click -= ImageView1OnClick;
                    ImageView2.Click -= ImageView2OnClick;
                    ImageView3.Click -= ImageView3OnClick;
                    ImageView4.Click -= ImageView4OnClick;
                    ImageView5.Click -= ImageView5OnClick;
                    ImageView6.Click -= ImageView6OnClick;
                    BtnBoostImage1.Click -= BtnBoostImage1OnClick;
                    BtnBoostImage2.Click -= BtnBoostImage2OnClick;
                    BtnBoostImage3.Click -= BtnBoostImage3OnClick;
                    BtnBoostImage4.Click -= BtnBoostImage4OnClick;
                    BtnBoostImage5.Click -= BtnBoostImage5OnClick;
                    BtnBoostImage6.Click -= BtnBoostImage6OnClick;
                    IconAboutEdit.Click -= IconAboutEditOnClick;
                    IconProfileInfoEdit.Click -= IconProfileInfoEditOnClick;
                    IconInterestEdit.Click -= IconInterestEditOnClick;
                    IconLooksEdit.Click -= IconLooksEditOnClick;
                    IconPersonalityInfoEdit.Click -= IconPersonalityInfoEditOnClick;
                    IconLifestyleEdit.Click -= IconLifestyleEditOnClick;
                    IconFavoriteEdit.Click -= IconFavoriteEditOnClick;
                    TxtSeeMoreMedia.Click -= TxtSeeMoreMediaOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //See All media (image and video)
        private void TxtSeeMoreMediaOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(this, typeof(AllMediaActivity)), 2314);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //#Add Or Change Image 
        private void ImageView6OnClick(object sender, EventArgs e)
        {
            try
            {
                NumImage = 6;
                OpenDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }

        private void ImageView5OnClick(object sender, EventArgs e)
        {
            try
            {
                NumImage = 5;
                OpenDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        private void ImageView4OnClick(object sender, EventArgs e)
        {
            try
            {
                NumImage = 4;
                OpenDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }

        private void ImageView3OnClick(object sender, EventArgs e)
        {
            try
            {
                NumImage = 3;
                OpenDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        private void ImageView2OnClick(object sender, EventArgs e)
        {
            try
            {
                NumImage = 2;
                OpenDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        private void ImageView1OnClick(object sender, EventArgs e)
        {
            try
            {
                NumImage = 1;
                OpenDialog();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        //#Edit info
        private void IconFavoriteEditOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(FavoriteEditActivity));
                StartActivityForResult(intent, 3040);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconLifestyleEditOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(LifeStyleEditActivity));
                StartActivityForResult(intent, 3030);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconPersonalityInfoEditOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(PersonalityInfoEditActivity));
                StartActivityForResult(intent, 3020);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconLooksEditOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(LooksEditActivity));
                StartActivityForResult(intent, 3010);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconInterestEditOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Interest";
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(GetString(Resource.String.Lbl_Interest));
                dialog.Input(GetString(Resource.String.Lbl_EnterTextInterest), TxtInterest.Text, false, this);
                dialog.InputType(InputTypes.TextFlagImeMultiLine);
                dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(new MyMaterialDialog());
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Edit profile info
        private void IconProfileInfoEditOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(ProfileInfoEditActivity));
                StartActivityForResult(intent, 3000);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //About
        private void IconAboutEditOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "About";

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(Resource.String.Lbl_About);
                dialog.Input(GetString(Resource.String.Lbl_AddWordsAbout), TxtAbout.Text, false, this);
                dialog.InputType(InputTypes.TextFlagImeMultiLine);
                dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(new MyMaterialDialog());
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //#Reset image
        private void BtnBoostImage6OnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                GlideImageLoader.LoadImage(this, "ImagePlacholder", ImageView6, ImageStyle.CenterCrop, ImagePlaceholders.Drawable); 
                DeletePhotoFromUtils(IdImage6);
                IdImage6 = "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnBoostImage5OnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                GlideImageLoader.LoadImage(this, "ImagePlacholder", ImageView5, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);  
                DeletePhotoFromUtils(IdImage5); 
                IdImage5 = "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnBoostImage4OnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                GlideImageLoader.LoadImage(this, "ImagePlacholder", ImageView4, ImageStyle.CenterCrop, ImagePlaceholders.Drawable); 
                DeletePhotoFromUtils(IdImage4);
                IdImage4 = "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnBoostImage3OnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                GlideImageLoader.LoadImage(this, "ImagePlacholder", ImageView3, ImageStyle.CenterCrop, ImagePlaceholders.Drawable); 
                DeletePhotoFromUtils(IdImage3);
                IdImage3 = "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnBoostImage2OnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                GlideImageLoader.LoadImage(this, "ImagePlacholder", ImageView2, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);  
                DeletePhotoFromUtils(IdImage2);
                IdImage2 = "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnBoostImage1OnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                GlideImageLoader.LoadImage(this, "ImagePlacholder", ImageView1, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                DeletePhotoFromUtils(IdImage1);
                IdImage1 = "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    // Add image using camera
                    case 503 when resultCode == Result.Ok:
                    {
                        if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Image")
                                {
                                    SendFile(filepath, filepath, "Image");
                                }
                            }
                        }
                        else
                        {
                            if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                            {
                                SendFile(IntentController.CurrentPhotoPath, IntentController.CurrentPhotoPath, "Image");
                            }
                        }

                        break;
                    }
                    // Add video 
                    case 501 when resultCode == Result.Ok:
                    {
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                        if (filepath != null)
                        {
                            var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                            if (type == "Video")
                            { 
                                var fileName = filepath.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();

                                var path = Methods.Path.FolderDcimVideo + "/" + fileNameWithoutExtension + ".png";

                                var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimVideo, fileNameWithoutExtension + ".png");
                                if (videoPlaceHolderImage == "File Dont Exists")
                                {
                                    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                    if (bitmapImage != null)
                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimVideo);
                                    else
                                    {
                                        File file2 = new File(filepath);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                        Glide.With(this)
                                            .AsBitmap()
                                            .Load(photoUri) // or URI/path
                                            .Into(new MySimpleTarget(filepath));  //image view to set thumbnail to

                                        await Task.Delay(500);
                                    }
                                }

                                SendFile(filepath, path, "Video");
                            }
                        }

                        break;
                    }
                    // Add video Camera 
                    case 513 when resultCode == Result.Ok:
                    {
                        if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentVideoPath) != "File Dont Exists" && Build.VERSION.SdkInt <= BuildVersionCodes.OMr1)
                        {
                            var fileName = IntentController.CurrentVideoPath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var path = Methods.Path.FolderDcimVideo + "/" + fileNameWithoutExtension + ".png";

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimVideo, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                if (bitmapImage != null)
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimVideo);
                                else
                                {
                                    File file2 = new File(IntentController.CurrentVideoPath);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                    Glide.With(this)
                                        .AsBitmap()
                                        .Load(photoUri) // or URI/path
                                        .Into(new MySimpleTarget(IntentController.CurrentVideoPath));  //image view to set thumbnail to

                                    await Task.Delay(500);
                                }
                            }
                            SendFile(IntentController.CurrentVideoPath, path, "Video");
                        }
                        else
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Video")
                                {
                                    var fileName = filepath.Split('/').Last();
                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var path = Methods.Path.FolderDcimVideo + "/" + fileNameWithoutExtension + ".png";

                                    var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimVideo, fileNameWithoutExtension + ".png");
                                    if (videoPlaceHolderImage == "File Dont Exists")
                                    {
                                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                        if (bitmapImage != null)
                                            Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimVideo);
                                        else
                                        {
                                            File file2 = new File(filepath);
                                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                            Glide.With(this)
                                                .AsBitmap()
                                                .Load(photoUri) // or URI/path
                                                .Into(new MySimpleTarget(filepath));  //image view to set thumbnail to

                                            await Task.Delay(500);
                                        }
                                    }
                                    SendFile(filepath, path, "Video");
                                }
                            }
                        }

                        break;
                    }
                    case CropImage.CropImageActivityRequestCode when resultCode == Result.Ok:
                    {
                        var result = CropImage.GetActivityResult(data);
                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;
                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                SendFile(resultPathImage, resultPathImage, "Image");
                            }
                        }

                        break;
                    }
                    case 3000:
                    case 3010:
                    case 3020:
                    case 3030:
                    case 3040:
                    case 2314 when resultCode == Result.Ok:
                        GetMyInfoData();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (ImageType)
                        {
                            case "Image": //requestCode >> 500 => Image Gallery
                                OpenDialogGallery("Image");
                                break;
                            case "VideoGallery":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "VideoCamera":
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            case "Camera":
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                                break;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        private async void SendFile(string path, string thumb, string type)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                var infoObject = ListUtils.MyUserInfo?.FirstOrDefault();

                var time = Methods.GetTimestamp(DateTime.Now);
                infoObject?.Mediafiles.Insert(0, new MediaFile
                {
                    Id = time,
                    Full = path,
                    Avater = thumb,
                    UrlFile = path,
                });

                switch (NumImage)
                {
                    case 1:
                        GlideImageLoader.LoadImage(this, thumb, ImageView1, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //DeletePhotoFromUtils(IdImage1);

                        if (type == "Video")
                            PlayIcon1.Visibility = ViewStates.Visible;

                        break;
                    case 2:
                        GlideImageLoader.LoadImage(this, thumb, ImageView2, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //DeletePhotoFromUtils(IdImage2);

                        if (type == "Video")
                            PlayIcon2.Visibility = ViewStates.Visible;

                        break;
                    case 3:
                        GlideImageLoader.LoadImage(this, thumb, ImageView3, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //DeletePhotoFromUtils(IdImage3);

                        if (type == "Video")
                            PlayIcon3.Visibility = ViewStates.Visible;

                        break;
                    case 4:
                        GlideImageLoader.LoadImage(this, thumb, ImageView4, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //DeletePhotoFromUtils(IdImage4);

                        if (type == "Video")
                            PlayIcon4.Visibility = ViewStates.Visible;

                        break;
                    case 5:
                        GlideImageLoader.LoadImage(this, thumb, ImageView5, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //DeletePhotoFromUtils(IdImage5);

                        if (type == "Video")
                            PlayIcon5.Visibility = ViewStates.Visible;

                        break;
                    case 6:
                        GlideImageLoader.LoadImage(this, thumb, ImageView6, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //DeletePhotoFromUtils(IdImage6);

                        if (type == "Video")
                            PlayIcon6.Visibility = ViewStates.Visible;

                        break;
                }
                  
                switch (type)
                {
                    case "Image":
                    {
                        //sent api 
                        var (apiStatus, respond) = await RequestsAsync.Users.UploadMediaFileUserAsync(path);
                        if (apiStatus == 200)
                        {
                            if (respond is UploadVideoFileObject resultImage)
                            { 
                                infoObject?.Mediafiles.Insert(0, new MediaFile
                                {
                                    Avater = path,
                                    Full = path,
                                    Id = resultImage.Data.Id
                                });

                                var item = infoObject?.Mediafiles?.FirstOrDefault(a => a.Id == time);
                                if (item != null)
                                {
                                    item.Id = resultImage.Data.Id;
                                }

                                var reviewMediaFiles = ListUtils.SettingsSiteList?.ReviewMediaFiles;
                                if (reviewMediaFiles == "1") //Uploaded successfully, file will be reviewed
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_UploadedSuccessfullyWithReviewed), ToastLength.Long)?.Show();

                                AndHUD.Shared.Dismiss(this);
                            }
                        }
                        else
                        {
                            //Methods.DisplayReportResult(this, respond);
                            //Show a Error image with a message
                            AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_Error), MaskType.Clear, TimeSpan.FromSeconds(2));
                        }

                        break;
                    }
                    case "Video":
                    {
                        //sent api 
                        var (apiStatus, respond) = await RequestsAsync.Users.UploadVideoFileUserAsync(path, thumb);
                        if (apiStatus == 200)
                        {
                            if (respond is UploadVideoFileObject result)
                            {
                                var newPath = result.Data.VideoFile;
                                if (!newPath.Contains(InitializeQuickDate.WebsiteUrl))
                                {
                                    newPath = InitializeQuickDate.WebsiteUrl + result.Data.VideoFile;
                                }

                                var newPathAvatar = result.Data.VideoFile;
                                if (!newPathAvatar.Contains(InitializeQuickDate.WebsiteUrl))
                                {
                                    newPathAvatar = InitializeQuickDate.WebsiteUrl + result.Data.File;
                                }
                             
                                infoObject?.Mediafiles.Insert(0, new MediaFile
                                {
                                    Full = newPathAvatar,
                                    VideoFile = newPath,
                                    Id = result.Data.Id,
                                    IsVideo = "1"
                                });

                                var item = infoObject?.Mediafiles?.FirstOrDefault(a => a.Id == time);
                                if (item != null)
                                {
                                    item.Full = newPathAvatar;
                                    item.VideoFile = newPath;
                                    item.Id = result.Data.Id;
                                }

                                var reviewMediaFiles = ListUtils.SettingsSiteList?.ReviewMediaFiles;
                                if (reviewMediaFiles == "1") //Uploaded successfully, file will be reviewed
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_UploadedSuccessfullyWithReviewed), ToastLength.Long)?.Show();

                                AndHUD.Shared.Dismiss(this);
                            }
                        }
                        else
                        {
                            //Methods.DisplayReportResult(this, respond);
                            //Show a Error image with a message
                            AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_Error), MaskType.Clear, TimeSpan.FromSeconds(2));
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DeletePhotoFromUtils(string imageId)
        {
            try
            { 
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                   
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(GetText(Resource.String.Lbl_Warning));
                dialog.Content(GetText(Resource.String.Lbl_AskDeleteFile));
                dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(imageId))
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.DeleteMediaFileUserAsync(imageId) });

                            var infoObject = ListUtils.MyUserInfo?.FirstOrDefault();
                            var dataImage = infoObject?.Mediafiles?.FirstOrDefault(file => file.Id == imageId);
                            if (dataImage != null)
                            {
                                infoObject.Mediafiles?.Remove(dataImage);

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdate_DataMyInfo(infoObject);
                                
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfo.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                    
                }

                DataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                if (DataUser != null)
                {
                    if (DataUser.Mediafiles?.Count > 0)
                    {
                        for (int i = 0; i < DataUser.Mediafiles.Count; i++)
                        {
                            try
                            {
                                switch (i)
                                {
                                    case 0:
                                        GlideImageLoader.LoadImage(this, DataUser.Mediafiles[i]?.Full, ImageView1, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        IdImage1 = DataUser.Mediafiles[i]?.Id;
                                   
                                        if (!string.IsNullOrEmpty(DataUser.Mediafiles[i].VideoFile) || DataUser.Mediafiles[i].IsVideo == "1")
                                            PlayIcon1.Visibility = ViewStates.Visible;
                                        break;
                                    case 1:
                                        GlideImageLoader.LoadImage(this, DataUser.Mediafiles[i]?.Full, ImageView2, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        IdImage2 = DataUser.Mediafiles[i]?.Id;
                                    
                                        if (!string.IsNullOrEmpty(DataUser.Mediafiles[i].VideoFile) || DataUser.Mediafiles[i].IsVideo == "1")
                                            PlayIcon2.Visibility = ViewStates.Visible;
                                        break;
                                    case 2:
                                        GlideImageLoader.LoadImage(this, DataUser.Mediafiles[i]?.Full, ImageView3, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        IdImage3 = DataUser.Mediafiles[i]?.Id;

                                        if (!string.IsNullOrEmpty(DataUser.Mediafiles[i].VideoFile) || DataUser.Mediafiles[i].IsVideo == "1")
                                            PlayIcon3.Visibility = ViewStates.Visible;
                                        break;
                                    case 3:
                                        GlideImageLoader.LoadImage(this, DataUser.Mediafiles[i]?.Full, ImageView4, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        IdImage4 = DataUser.Mediafiles[i]?.Id;

                                        if (!string.IsNullOrEmpty(DataUser.Mediafiles[i].VideoFile) || DataUser.Mediafiles[i].IsVideo == "1")
                                            PlayIcon4.Visibility = ViewStates.Visible;
                                        break;
                                    case 4:
                                        GlideImageLoader.LoadImage(this, DataUser.Mediafiles[i]?.Full, ImageView5, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        IdImage5 = DataUser.Mediafiles[i]?.Id;

                                        if (!string.IsNullOrEmpty(DataUser.Mediafiles[i].VideoFile) || DataUser.Mediafiles[i].IsVideo == "1")
                                            PlayIcon5.Visibility = ViewStates.Visible;
                                        break;
                                    case 5:
                                        GlideImageLoader.LoadImage(this, DataUser.Mediafiles[i]?.Full, ImageView6, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                        IdImage6 = DataUser.Mediafiles[i]?.Id;

                                        if (!string.IsNullOrEmpty(DataUser.Mediafiles[i].VideoFile) || DataUser.Mediafiles[i].IsVideo == "1")
                                            PlayIcon6.Visibility = ViewStates.Visible;
                                        break;
                                } 
                            }
                            catch (Exception exName)
                            {
                                Methods.DisplayReportResultTrack(exName);
                            }
                        }
                        TxtSeeMoreMedia.Visibility = DataUser.Mediafiles?.Count > 5 ? ViewStates.Visible : ViewStates.Gone;
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        ProgressBar.SetProgress(Convert.ToInt32(DataUser.ProfileCompletion), true);
                    }
                    else
                    {
                        try
                        {
                            // For API < 24 
                            ProgressBar.Progress = Convert.ToInt32(DataUser.ProfileCompletion);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }

                    CountPercent.Text = DataUser.ProfileCompletion + "%";

                    TxtAbout.Text = Methods.FunString.StringNullRemover(Methods.FunString.DecodeString(DataUser.About));

                    if (!string.IsNullOrEmpty(DataUser.FirstName) && !string.IsNullOrEmpty(DataUser.LastName))
                        TxtName.Text = DataUser.FirstName + " " + DataUser.LastName;
                    else
                        TxtName.Text = QuickDateTools.GetNameFinal(DataUser);

                    TxtGender.Text = QuickDateTools.GetGender(Convert.ToInt32(DataUser.Gender));
                    TxtBirthday.Text = DataUser.Birthday;

                    if (Methods.FunString.StringNullRemover(DataUser.Location) != "-----")
                    {
                        TxtLocation.Text = DataUser.Location;
                    }

                    if (Methods.FunString.StringNullRemover(DataUser.Language) != "-----")
                    {
                        TxtLanguage.Text = DataUser.Language;
                    }

                    string relationship = QuickDateTools.GetRelationship(DataUser.Relationship);
                    if (Methods.FunString.StringNullRemover(relationship) != "-----")
                    {
                        TxtRelationship.Text = relationship;
                    }

                    string work = QuickDateTools.GetWorkStatus(DataUser.WorkStatus);
                    if (Methods.FunString.StringNullRemover(work) != "-----")
                    {
                        TxtWork.Text = work;
                    }

                    string education = QuickDateTools.GetEducation(DataUser.Education);
                    if (Methods.FunString.StringNullRemover(education) != "-----")
                    {
                        TxtEducation.Text = education;
                    }

                    if (Methods.FunString.StringNullRemover(DataUser.Interest) != "-----")
                    {
                        TxtInterest.Text = DataUser.Interest.Remove(DataUser.Interest.Length - 1, 1);
                    }

                    string ethnicity = QuickDateTools.GetEthnicity(DataUser.Ethnicity);
                    if (Methods.FunString.StringNullRemover(ethnicity) != "-----")
                    {
                        TxtEthnicity.Text = ethnicity;
                    }

                    string body = QuickDateTools.GetBody(DataUser.Body);
                    if (Methods.FunString.StringNullRemover(body) != "-----")
                    {
                        TxtBody.Text = body;
                    }

                    TxtHeight.Text = DataUser.Height + " cm";

                    string hairColor = QuickDateTools.GetHairColor(DataUser.HairColor);
                    if (Methods.FunString.StringNullRemover(hairColor) != "-----")
                    {
                        TxtHair.Text = hairColor;
                    }

                    string character = QuickDateTools.GetCharacter(DataUser.Character);
                    if (Methods.FunString.StringNullRemover(character) != "-----")
                    {
                        TxtCharacter.Text = character;
                    }

                    string children = QuickDateTools.GetChildren(DataUser.Children);
                    if (Methods.FunString.StringNullRemover(children) != "-----")
                    {
                        TxtChildren.Text = children;
                    }

                    string friends = QuickDateTools.GetFriends(DataUser.Friends);
                    if (Methods.FunString.StringNullRemover(friends) != "-----")
                    {
                        TxtFriends.Text = friends;
                    }

                    string pets = QuickDateTools.GetPets(DataUser.Pets);
                    if (Methods.FunString.StringNullRemover(pets) != "-----")
                    {
                        TxtPets.Text = pets;
                    }

                    string liveWith = QuickDateTools.GetLiveWith(DataUser.LiveWith);
                    if (Methods.FunString.StringNullRemover(liveWith) != "-----")
                    {
                        TxtLiveWith.Text = liveWith;
                    }

                    string car = QuickDateTools.GetCar(DataUser.Car);
                    if (Methods.FunString.StringNullRemover(car) != "-----")
                    {
                        TxtCar.Text = car;
                    }

                    string religion = QuickDateTools.GetReligion(DataUser.Religion);
                    if (Methods.FunString.StringNullRemover(religion) != "-----")
                    {
                        TxtReligion.Text = religion;
                    }

                    string smoke = QuickDateTools.GetSmoke(DataUser.Smoke);
                    if (Methods.FunString.StringNullRemover(smoke) != "-----")
                    {
                        TxtSmoke.Text = smoke;
                    }

                    string drink = QuickDateTools.GetDrink(DataUser.Drink);
                    if (Methods.FunString.StringNullRemover(drink) != "-----")
                    {
                        TxtDrink.Text = drink;
                    }

                    string travel = QuickDateTools.GetTravel(DataUser.Travel);
                    if (Methods.FunString.StringNullRemover(travel) != "-----")
                    {
                        TxtTravel.Text = travel;
                    }

                    TxtMusic.Text = Methods.FunString.StringNullRemover(DataUser.Music);
                    TxtDish.Text = Methods.FunString.StringNullRemover(DataUser.Dish);
                    TxtSong.Text = Methods.FunString.StringNullRemover(DataUser.Song);
                    TxtHobby.Text = Methods.FunString.StringNullRemover(DataUser.Hobby);
                    TxtCity.Text = Methods.FunString.StringNullRemover(DataUser.City);
                    TxtSport.Text = Methods.FunString.StringNullRemover(DataUser.Sport);
                    TxtBook.Text = Methods.FunString.StringNullRemover(DataUser.Book);
                    TxtMovie.Text = Methods.FunString.StringNullRemover(DataUser.Movie);
                    TxtColor.Text = Methods.FunString.StringNullRemover(DataUser.Colour);
                    TxtTvShow.Text = Methods.FunString.StringNullRemover(DataUser.Tv);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialog()
        {
            try
            { 
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    return;
                }
                 
                var naxPhotoPerUser = ListUtils.SettingsSiteList?.MaxPhotoPerUser;
                var isPro = DataUser?.IsPro ?? "0";
                if (isPro == "0" && DataUser?.Mediafiles?.Count == Convert.ToInt32(naxPhotoPerUser))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorLimitOfMediaUploads), ToastLength.Short)?.Show();
                    return;
                }

                var arrayAdapter = new List<string>
                { 
                    GetString(Resource.String.Lbl_ImageGallery),
                    GetString(Resource.String.Lbl_TakeImageFromCamera),
                    GetString(Resource.String.Lbl_VideoGallery),
                    GetString(Resource.String.Lbl_RecordVideoFromCamera),
                };

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                dialogList.Title(GetText(Resource.String.Lbl_ChooseTheFileType)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogGallery(string type)
        {
            try
            {
                ImageType = type;

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog
          
        public async void OnInput(MaterialDialog dialog, string itemString)
        {
            try
            {
                var strName = itemString;
                if (!string.IsNullOrEmpty(strName))
                {
                    if (itemString.Length <= 0) return;

                    switch (TypeDialog)
                    {
                        case "About" when Methods.CheckConnectivity():
                        {
                            TxtAbout.Text = strName;

                            var dictionary = new Dictionary<string, string>
                            {
                                {"about", strName},
                            };

                            var (apiStatus, respond) = await RequestsAsync.Users.UpdateProfileAsync(dictionary);
                            if (apiStatus == 200)
                            {
                                if (respond is UpdateProfileObject result)
                                {
                                    Console.WriteLine(result.Message);
                                    var local = ListUtils.MyUserInfo?.FirstOrDefault();
                                    if (local != null)
                                    {
                                        local.About = strName;

                                        SqLiteDatabase database = new SqLiteDatabase();
                                        database.InsertOrUpdate_DataMyInfo(local);
                                        
                                    }
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_SuccessfullyUpdated), ToastLength.Short)?.Show();
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                        case "About":
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            break;
                        case "Interest" when Methods.CheckConnectivity():
                        {
                            TxtInterest.Text = strName;

                            var dictionary = new Dictionary<string, string>
                            {
                                {"interest", strName},
                            };

                            var (apiStatus, respond) = await RequestsAsync.Users.UpdateProfileAsync(dictionary);
                            if (apiStatus == 200)
                            {
                                if (respond is UpdateProfileObject result)
                                {
                                    Console.WriteLine(result.Message);
                                    var local = ListUtils.MyUserInfo?.FirstOrDefault();
                                    if (local != null)
                                    {
                                        local.Interest = strName;

                                        SqLiteDatabase database = new SqLiteDatabase();
                                        database.InsertOrUpdate_DataMyInfo(local);
                                        
                                    }
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_SuccessfullyUpdated), ToastLength.Short)?.Show();
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                        case "Interest":
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                var txt = itemString;
                if (txt == GetString(Resource.String.Lbl_ImageGallery))
                {
                    OpenDialogGallery("Image");
                }
                else if (txt == GetString(Resource.String.Lbl_TakeImageFromCamera))
                {
                    ImageType = "Camera";
                    // Check if we're running on Android 5.0 or higher 
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (txt == GetString(Resource.String.Lbl_VideoGallery))
                {
                    ImageType = "VideoGallery";
                    // Check if we're running on Android 5.0 or higher 
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (txt == GetString(Resource.String.Lbl_RecordVideoFromCamera))
                {
                    ImageType = "VideoCamera";
                    // Check if we're running on Android 5.0 or higher 
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 513 => video Camera
                        new IntentController(this).OpenIntentVideoCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 513 => video Camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}