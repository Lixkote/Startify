using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Windows.UI.Notifications;

namespace WPF.Helpers.Launching
{
    internal class Startup
    {
        public void Initialize()
        {

        }

        public async void ShowNotification()
        {
            var assetFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets"); 
            var imageFile = await assetFolder.GetFileAsync("halal.png");
            // Create a toast notification
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // Set the notification text
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Startify Initialized"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode("Try pressing the start button!"));

            // Set the image for the notification
            var toastImageElement = toastXml.GetElementsByTagName("image")[0] as XmlElement;
            toastImageElement.SetAttribute("src", imageFile.Path);
            toastImageElement.SetAttribute("alt", "Success");

            // Specify the duration of the notification (optional)
            var toastNode = toastXml.SelectSingleNode("/toast");
            var duration = toastXml.CreateAttribute("duration");
            duration.Value = "short"; // or "long" for a longer duration
            toastNode.Attributes.SetNamedItem(duration);

            // Create the toast notification and show it
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("Startify_Rectify11").Show(toast);
        }

        public void ShowErrorNotification(string why)
        {
            // Create a toast notification
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // Set the notification text
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Startify Warning"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode("Startify Encountered an" + why + ", and might not work properly."));

            // Set the image for the notification
            var toastImageElement = toastXml.GetElementsByTagName("image")[0] as XmlElement;
            toastImageElement.SetAttribute("src", "ms-appx:///Assets/error.png"); // Replace with the correct path to your image
            toastImageElement.SetAttribute("alt", "ErrorImage");

            // Specify the duration of the notification (optional)
            var toastNode = toastXml.SelectSingleNode("/toast");
            var duration = toastXml.CreateAttribute("duration");
            duration.Value = "short"; // or "long" for a longer duration
            toastNode.Attributes.SetNamedItem(duration);

            // Create the toast notification and show it
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("Startify_Rectify11").Show(toast);
        }

    }
}
