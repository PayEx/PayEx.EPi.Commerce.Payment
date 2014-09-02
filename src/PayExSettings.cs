using System.Web.UI.WebControls;
using Epinova.PayExProvider.Contracts;
using EPiServer.Events.Clients;
using EPiServer.PlugIn;
using System;

namespace Epinova.PayExProvider
{
    [GuiPlugIn(Area = PlugInArea.None, DisplayName = "Epinova.PayExProvider Settings")]
    public class PayExSettings : IPayExSettings
    {
        internal static Guid BroadcastSettingsChangedEventId = new Guid("75714741-2ec2-4317-8110-1b1b63818602");
        private static PayExSettings _instance;

        [PlugInProperty(Description = "PayEx Account Number", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public long AccountNumber { get; set; }

        [PlugInProperty(Description = "PayEx Encryption Key", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string EncryptionKey { get; set; }

        [PlugInProperty(Description = "PayEx Purchase Operation (ex: SALE/AUTHORIZATION)", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PurchaseOperation { get; set; }

        [PlugInProperty(Description = "Title for ordernote created after authorization", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string AuthorizationNoteTitle { get; set; }

        [PlugInProperty(Description = "Message prefix for ordernote created after authorization", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string AuthorizationNoteMessage { get; set; }

        [PlugInProperty(Description = "Title for ordernote created after capture", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string CaptureNoteTitle { get; set; }

        [PlugInProperty(Description = "Message prefix for ordernote created after capture", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string CaptureNoteMessage { get; set; }

        [PlugInProperty(Description = "Payment Cancel URL", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PaymentCancelUrl { get; set; }

        [PlugInProperty(Description = "Payment Return URL", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PaymentReturnUrl { get; set; }

        [PlugInProperty(Description = "User Agent format", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string UserAgentFormat { get; set; }

        private PayExSettings()
        {
            PlugInSettings.SettingsChanged += BroadcastToAllServers;

            Event broadcastSettingsChangedEvent = Event.Get(BroadcastSettingsChangedEventId);
            broadcastSettingsChangedEvent.Raised += (sender, e) => { _instance = null; };
        }

        public void BroadcastToAllServers(object sender, EventArgs e)
        {
            Event settingsChangedEvent = Event.Get(BroadcastSettingsChangedEventId);
            settingsChangedEvent.Raise(BroadcastSettingsChangedEventId, null);
        }

        public static PayExSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PayExSettings();
                    PlugInSettings.AutoPopulate(_instance);
                }
                return _instance;
            }
        }
    }
}
