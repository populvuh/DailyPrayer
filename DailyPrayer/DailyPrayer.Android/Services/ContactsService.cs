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
        //private readonly Android.Provider.Contacts.AddressBook _book;

        private static ObservableCollection<MobileContact> _contacts;

        public ContactsService()
        {
            //_book = new Xamarin.Contacts.AddressBook(Android.App.Application.Context);
            //_book = new Xamarin.Contacts.AddressBook(Forms.Context.ApplicationContext);
        }

        public async Task<bool> GetPermissionAsync()
        {
            Debug.WriteLine($"{_Tag}.GetPermission()");

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

                    Debug.WriteLine($"{_Tag}.GetPermission() - before hang");
                    //status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Contacts))[Permission.Contacts];
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Contacts);

                    if (results.ContainsKey(Permission.Contacts))
                        status = results[Permission.Contacts];
                    Debug.WriteLine($"{_Tag}.GetPermission() - after hang\n");
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
            Debug.WriteLine($"{_Tag}.FindContacts()");
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


    /*void FillContacts()
    {
        var uri = ContactsContract.Contacts.ContentUri;

        string[] projection = {
            ContactsContract.Contacts.InterfaceConsts.Id,
            ContactsContract.Contacts.InterfaceConsts.DisplayName,
            ContactsContract.Contacts.InterfaceConsts.PhotoId,
            ContactsContract.CommonDataKinds.Email.Address
        };

        // ManagedQuery is deprecated in Honeycomb (3.0, API11)
        //var cursor = activity.ManagedQuery (uri, projection, null, null, null);

        // ContentResolver requires you to close the query yourself
        //var cursor = activity.ContentResolver.Query(uri, projection, null, null, null);

        // CursorLoader introduced in Honeycomb (3.0, API11)
        var loader = new CursorLoader(Android.App.Application.Context, uri, projection, null, null, null);
        var cursor = (ICursor)loader.LoadInBackground();

        List<Contact> contactList = new List<Contact>();

        if (cursor.MoveToFirst())
        {
            do
            {
                string emailAddress = cursor.GetString(cursor.GetColumnIndex(projection[3]));
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    List<Email> emails = new List<Email>();
                    Email email = new Email();
                    email.Address = emailAddress;
                    emails.Add(email);
                    contactList.Add(new Contact
                    {
                        //Id = cursor.GetLong(cursor.GetColumnIndex(projection[0])),
                        DisplayName = cursor.GetString(cursor.GetColumnIndex(projection[1])),
                        //PhotoId = cursor.GetString(cursor.GetColumnIndex(projection[2]))
                        Emails = emails
                    });
                }
            } while (cursor.MoveToNext());
        }

        foreach (Contact contact in contactList)
        {
            Console.WriteLine($"{contact.DisplayName}, {contact.Emails.FirstOrDefault<Email>().Address}");
        }
    }*/

    //const int permissioncode = 25;
    /*PermissionStatus GetPermission2()
    {

        var context = Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity ?? Android.App.Application.Context;
        if (context == null)
        {
            Debug.WriteLine("Unable to detect current Activity or App Context. Please ensure Plugin.CurrentActivity is installed in your Android project and your Application class is registering with Application.IActivityLifecycleCallbacks.");
            return PermissionStatus.Unknown;
        }

        var activity = Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity;
        if (activity == null)
        {
            Debug.WriteLine("Unable to detect current Activity. Please ensure Plugin.CurrentActivity is installed in your Android project and your Application class is registering with Application.IActivityLifecycleCallbacks.");
            //foreach (var permission in permissions)
            //{
            //    if (results.ContainsKey(permission))
            //        continue;
            //    results.Add(permission, PermissionStatus.Unknown);
            //}
            //return results;
        }

        if (ContextCompat.CheckSelfPermission(context,
                Manifest.Permission.ReadContacts) == Android.Content.PM.Permission.Denied)
            //return Task.FromResult(PermissionStatus.Denied);
            return PermissionStatus.Denied;
        {

            // Should we show an explanation?
                if (ActivityCompat.ShouldShowRequestPermissionRationale(activity,
                Manifest.Permission.ReadContacts))
            {

                // Show an explanation to the user *asynchronously* -- don't block
                // this thread waiting for the user's response! After the user
                // sees the explanation, try again to request the permission.

            }
            else
            {

                // No explanation needed, we can request the permission.
                ActivityCompat.RequestPermissions(activity,
                    new String[] { Manifest.Permission.ReadContacts },
                        permissioncode);
            }
            // MY_PERMISSIONS_REQUEST_READ_CONTACTS is an
            // app-defined int constant. The callback method gets the
            // result of the request.
        }
    }*/
}
