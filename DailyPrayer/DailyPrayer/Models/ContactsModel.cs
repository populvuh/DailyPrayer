using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DailyPrayer.Models
{
    public class MobileContact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string EmailId { get; set; }
        public string PhotoId { get; set; }

        public string Name
        {
            get => $"{FirstName} {LastName}";
        }
        /*public string Contact_Picture
        {
            get
            {
                return Device.OnPlatform("default_human_pic@2x", "ic_action_default_human_pic.png", "Images/default_human_pic.png");
            }
        }*/
    }
}
