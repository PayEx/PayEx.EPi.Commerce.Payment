using System;
using System.Web.UI.WebControls;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Events.Clients;
using EPiServer.PlugIn;

namespace EPiServer.Business.Commerce.Payment.PayEx
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

        [PlugInProperty(Description = "Payment Cancel URL", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PaymentCancelUrl { get; set; }

        [PlugInProperty(Description = "Payment Return URL", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PaymentReturnUrl { get; set; }

        [PlugInProperty(Description = "User Agent format", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string UserAgentFormat { get; set; }

        [PlugInProperty(Description = "PayEx description format", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string DescriptionFormat { get; set; }

        [PlugInProperty(Description = "PayEx callback IP-address", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string PayExCallbackIpAddress { get; set; }

        [PlugInProperty(Description = "Payment system keyword prefix", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string SystemKeywordPrefix { get; set; }

        [PlugInProperty(Description = "Invoice system keyword", AdminControl = typeof(TextBox), AdminControlValue = "Text")]
        public string InvoiceKeyword { get; set; }

        [PlugInProperty(Description = "Give PayEx information on each individual order line", AdminControl = typeof(CheckBox), AdminControlValue = "Checked")]
        public bool IncludeOrderLines { get; set; }

        [PlugInProperty(Description = "Give PayEx address information for the customer", AdminControl = typeof(CheckBox), AdminControlValue = "Checked")]
        public bool IncludeCustomerAddress { get; set; }

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
