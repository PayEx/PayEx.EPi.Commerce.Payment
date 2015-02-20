using System;
using System.Web.UI.WebControls;
using EPiServer.Events.Clients;
using EPiServer.PlugIn;
using PayEx.EPi.Commerce.Payment.Contracts;

namespace PayEx.EPi.Commerce.Payment
{
    [GuiPlugIn(Area = PlugInArea.None, DisplayName = "Settings for the PayEx payment provider")]
    internal class PayExSettings : IPayExSettings
    {
        internal static Guid BroadcastSettingsChangedEventId = new Guid("75714741-2ec2-4317-8110-1b1b63818602");
        private static PayExSettings _instance;

        [PlugInProperty(Description = "Merchants PayEx account number", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public long AccountNumber { get; set; }

        [PlugInProperty(Description = "PayEx Encryption Key", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string EncryptionKey { get; set; }

        [PlugInProperty(Description = "Display individual order lines in PayEx", AdminControl = typeof(CheckBox), AdminControlValue = "Checked")]
        public bool IncludeOrderLines { get; set; }

        [PlugInProperty(Description = "Display customer address information in PayEx", AdminControl = typeof(CheckBox), AdminControlValue = "Checked")]
        public bool IncludeCustomerAddress { get; set; }

        [PlugInProperty(Description = "Disable automatic payment method creation during initialization", AdminControl = typeof(CheckBox), AdminControlValue = "Checked")]
        public bool DisablePaymentMethodCreation { get; set; }

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
