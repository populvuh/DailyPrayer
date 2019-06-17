using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Android.Content;
using Android.Provider;

using DailyPrayer.Models;

using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

using Xamarin.Forms;

using Debug = System.Diagnostics.Debug;

[assembly: Dependency(typeof(DailyPrayer.Droid.Services.ContactsService))]
namespace DailyPrayer.Droid.Services
{
    public class ContactsService : DailyPrayer.Services.IContactsService
    {
        string _Tag = "ContactsService";

        private static ObservableCollection<MobileContact> _contacts;

        public ContactsService()
        {
        }

        public async Task<bool> GetPermissionAsync()
        {
            Debug.WriteLine($"{_Tag}.GetPermissionAsync()");

            bool OK = false;
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Contacts))
                    {
                        Debug.WriteLine($"{_Tag}.GetPermission() waiting");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Contacts);

                    if (results.ContainsKey(Permission.Contacts))
                        status = results[Permission.Contacts];
                }

                OK = (status == PermissionStatus.Granted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{_Tag}.GetPermission() failed: {ex.Message}");
            }

            return OK;
        }

        public List<MobileContact> FindContacts(string searchString)
        {
            var ResultContacts = new List<MobileContact>();

            foreach (var currentContact in _contacts)
            {
                // Running a basic String Contains() search through all the 
                // fields in each Contact in the list for the given search string
                if ((currentContact.FirstName != null && currentContact.FirstName.ToLower().Contains(searchString.ToLower())) ||
                    (currentContact.LastName != null && currentContact.LastName.ToLower().Contains(searchString.ToLower())) ||
                    (currentContact.EmailId != null && currentContact.EmailId.ToLower().Contains(searchString.ToLower())))
                {
                    ResultContacts.Add(currentContact);
                }
            }

            return ResultContacts;
        }

        public async Task<IEnumerable<MobileContact>> GetAllContactsAsync()
        {
            Debug.WriteLine($"{_Tag}.GetAllContactsAsync()");
            ObservableCollection<MobileContact> mobileContacts = new ObservableCollection<MobileContact>();

            try
            {
                bool result = await GetPermissionAsync();
                if (!result)
                {
                    Debug.WriteLine("Sorry! Permission was denied by user or manifest !");
                    return _contacts;
                }

                var uri = ContactsContract.Contacts.ContentUri;
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id,
                                        ContactsContract.Contacts.InterfaceConsts.DisplayName };

                var loader = new CursorLoader(CrossCurrentActivity.Current.AppContext, uri, projection, null, null, null);
                var cursor = (Android.Database.ICursor)loader.LoadInBackground();

                if (cursor.MoveToFirst())
                {
                    do
                    {
                        string id = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                        string name = cursor.GetString(cursor.GetColumnIndex(projection[1]));
                        //id = cursor.GetString(cursor.GetColumnIndex(BaseColumns.Id));

                        // load email address
                        loader = new CursorLoader(CrossCurrentActivity.Current.AppContext, ContactsContract.CommonDataKinds.Email.ContentUri, null,
                                                    ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + " = " + id, null, null);
                        Android.Database.ICursor nestedCursor = (Android.Database.ICursor)loader.LoadInBackground();
                        if (nestedCursor.MoveToFirst())
                        {
                            string[] words = name.Split(' ');
                            do
                            {
                                mobileContacts.Add(new MobileContact()
                                {
                                    DisplayName = name,
                                    FirstName = words[0],
                                    LastName = (words.Length > 1) ? words[1] : "",
                                    EmailId = nestedCursor.GetString(
                                        nestedCursor.GetColumnIndex(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data)),
                                });
                            } while (nestedCursor.MoveToNext());
                        }

                        /****load phones
                        loader = new CursorLoader(CrossCurrentActivity.Current.AppContext, ContactsContract.CommonDataKinds.Phone.ContentUri, null,
                            ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = " + id, null, null);
                        nestedCursor = (Android.Database.ICursor)loader.LoadInBackground();

                        if (nestedCursor.MoveToFirst())
                        {
                            do
                            {
                                mobileContacts.Add(new MobileContact()
                                {
                                    DisplayName = name,
                                    FirstName = words[0],
                                    LastName = (words.Length > 1) ? words[1] : "",
                                    EmailId = nestedCursor.GetString(
                                        nestedCursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number)),
                                });
                            } while (nestedCursor.MoveToNext());
                        }*/

                    } while (cursor.MoveToNext());
                }
            }
            catch (Exception ex)
            {
                //something wrong with one contact, may be display name is completely empty, decide what to do
                Debug.WriteLine($"{_Tag}.GetAllContacts() error: {ex.Message}\n{ex.InnerException}");
            }

            _contacts = new ObservableCollection<MobileContact>(
                (from c in mobileContacts orderby c.FirstName select c).ToList());

            return _contacts;
        }
    }
}
