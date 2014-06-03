using System.Web.UI.WebControls;
using Epinova.PayExProvider.Contracts;
using EPiServer.Events.Clients;
using EPiServer.PlugIn;
using System;

namespace Epinova.PayExProvider
{
    [GuiPlugIn(Area = PlugInArea.None, DisplayName = "Epinova.PayExProvider Settings")]
    public class Settings : ISettings
    {
        internal static Guid BroadcastSettingsChangedEventId = new Guid("75714741-2ec2-4317-8110-1b1b63818602");
        private static Settings _instance;

        [PlugInProperty(Description = "PayEx Account Number", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public long AccountNumber { get; set; }

        [PlugInProperty(Description = "PayEx Encryption Key", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string EncryptionKey { get; set; }

        [PlugInProperty(Description = "PayEx Purchase Operation (ex: SALE/AUTHORIZATION)", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PurchaseOperation { get; set; }

        private Settings()
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

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Settings();
                    PlugInSettings.AutoPopulate(_instance);
                }
                return _instance;
            }
        }
    }
}
