using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using DailyPrayer.Models;
using DailyPrayer.Models.PrayerSeason;
using DailyPrayer.Services;

using FreshMvvm;

using Plugin.Messaging;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer
{
    class ContactPageModel : FreshBasePageModel
    {
        private const string _Tag = "ContactPageModel";
        public string MainText
        {
            get { return "Gửi đến: "; }                                                         // send to
        }

        public ICommand EmailContactCommand { protected set; get; }

        private ObservableCollection<MobileContact> _contacts;
        public ObservableCollection<MobileContact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        private IEnumerable<MobileContact> _contactList;
        public IEnumerable<MobileContact> ContactList
        {
            get { return _contactList; }
            set { _contactList = value; }
        }

        public string EmailAddress { set; get; }
        public ICommand EnteredEmailCommand { protected set; get; }
        public ICommand SelectedEmailCommand { protected set; get; }

        //private static bool _requestedPermission = false;

        public ContactPageModel()
        {
            Debug.WriteLine("ContactPageModel.ContactPageModel()");
        }

        public override void Init(object initData)
        {
            base.Init(initData);

            this.EnteredEmailCommand = new Command(() =>
            {
                Debug.WriteLine("EnteredEmailCommand " + EmailAddress);
                SendEmail(EmailAddress);
            });
            this.SelectedEmailCommand = new Command(() =>
            {
                // ignore this
            });

            if (Contacts == null || Contacts.Count == 0)
            {
                Contacts = LoadContacts().Result;
            }
        }

        public async Task<ObservableCollection<MobileContact>> LoadContacts()
        {
            ObservableCollection<MobileContact> mobileContacts = new ObservableCollection<MobileContact>();

            //await ContactsPermission();           // done in PrayerPage
            bool result = await GetPermissionAsync();
            if (!result)
            {
                Debug.WriteLine("Sorry! Permission was denied by user or manifest !");
                return mobileContacts;
            }

            ContactList = await DependencyService.Get<IContactsService>().GetAllContactsAsync();

            foreach (MobileContact contact in ContactList)
            {
                mobileContacts.Add(contact);
            }

            return mobileContacts;
        }

        /// <summary>
        /// This methods is called when the View is appearing
        /// </summary>
        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
        }


        private MobileContact _contactSelected;
        public MobileContact ContactSelected
        {
            get
            {
                return _contactSelected;
            }
            set
            {
                if (_contactSelected != value)
                {
                    _contactSelected = value;

                    SendEmail(_contactSelected.EmailId);

                    _contactSelected = null;                                        // so it unsets the selected item in the listview
                }
            }
        }

        void SendEmail(string address)
        {
            Debug.WriteLine($"Sending email to {address}");
            var emailTask = CrossMessaging.Current.EmailMessenger;
            if (emailTask.CanSendEmailBodyAsHtml)
            {
                // get a pointer to prayer page so we can retrieve the date details, and the html prayer
                IPrayer iPrayer = FreshMvvm.FreshIOC.Container.Resolve<IPrayer>();

                string subject = iPrayer.PrayerDate.Replace(" ;", ";").Replace(PrayerSeason._singleLE, "; ");

                // Construct HTML email (iOS and Android only) to single receiver without attachments, CC, or BCC.
                var email = new EmailMessageBuilder()
                  .To(address)
                  .Subject(subject)
				  //.Body(iPrayer.PrayerHtml)
				  .BodyAsHtml(iPrayer.PrayerHtml)
				  .Build();
                emailTask.SendEmail(email);
            }
            Debug.WriteLine($"Sent    email to {address}");

            EmailAddress = "";
            CoreMethods.PopPageModel();
        }

        /*private async Task ContactsPermission()
        {
            var status = PermissionStatus.Unknown;
            status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            //wait DisplayAlert("Results", status.ToString(), "OK");

            if (status != PermissionStatus.Granted)
            {
                status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Contacts))[Permission.Contacts];
                //await DisplayAlert("Results", status.ToString(), "OK");
            }
            //bool OK = (status == PermissionStatus.Granted);

            //return OK;
        }*/


        public async Task<bool> GetPermissionAsync()
        {
            Debug.WriteLine($"{_Tag}.GetPermission()");

            bool OK = false;
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
                OK = (status == PermissionStatus.Granted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{_Tag}.GetPermission() failed: {ex.Message}");
            }

            return OK;
        }

    }
}
