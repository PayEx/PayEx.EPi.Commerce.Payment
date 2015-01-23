using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Business.Commerce.Payment.PayEx.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(MetadataInitialization))]
    public class PaymentMethodInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (PayExSettings.Instance.DisablePaymentMethodCreation)
                return;

            string paymentGatewayClassname = ConcatenateClassAndAssemblyName(typeof(PayExPaymentGateway));
            string payExPaymentClassName = ConcatenateClassAndAssemblyName(typeof (PayExPayment));
            string extendedPayExPaymentClassName = ConcatenateClassAndAssemblyName(typeof(ExtendedPayExPayment));

            var paymentMethodInfo = new List<PaymentMethodInfo>
            {
                new PaymentMethodInfo(Constants.Payment.CreditCard.SystemKeyword, Constants.Payment.CreditCard.Name, paymentGatewayClassname, payExPaymentClassName, 1000),
                new PaymentMethodInfo(Constants.Payment.InvoiceLedger.SystemKeyword, Constants.Payment.InvoiceLedger.Name, paymentGatewayClassname, payExPaymentClassName, 1100),
                new PaymentMethodInfo(Constants.Payment.Invoice.SystemKeyword, Constants.Payment.Invoice.Name, paymentGatewayClassname, payExPaymentClassName, 1200),
                new PaymentMethodInfo(Constants.Payment.DirectDebit.SystemKeyword, Constants.Payment.DirectDebit.Name, paymentGatewayClassname, payExPaymentClassName, 1300),
                new PaymentMethodInfo(Constants.Payment.PartPayment.SystemKeyword, Constants.Payment.PartPayment.Name, paymentGatewayClassname, extendedPayExPaymentClassName, 1400),
                new PaymentMethodInfo(Constants.Payment.PayPal.SystemKeyword, Constants.Payment.PayPal.Name, paymentGatewayClassname, payExPaymentClassName, 1500),
                new PaymentMethodInfo(Constants.Payment.Giftcard.SystemKeyword, Constants.Payment.Giftcard.Name, paymentGatewayClassname, payExPaymentClassName, 1600),
            };

            CreateForEnabledLanguages(paymentMethodInfo);
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }

        private void CreateForEnabledLanguages(List<PaymentMethodInfo> paymentMethodInfos)
        {
            IList<LanguageBranch> enabledSiteLanguages = GetEnabledSiteLanguages();
            foreach (var enabledSiteLanguage in enabledSiteLanguages)
            {
                List<PaymentMethodDto.PaymentMethodRow> paymentMethodsForLanguage =
                    GetPaymentMethodsForLanguage(enabledSiteLanguage.LanguageID);

                foreach (PaymentMethodInfo paymentMethodInfo in paymentMethodInfos)
                {
                    if (paymentMethodsForLanguage.Exists(p => p.SystemKeyword == paymentMethodInfo.SystemKeyword))
                        continue;

                    try
                    {
                        Guid paymentMethodId = GeneratePaymentMethodId();
                        Create(paymentMethodInfo, paymentMethodId, enabledSiteLanguage.LanguageID);
                    }
                    catch (Exception e)
                    {
                        ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
                        logger.LogError(string.Format("Could not create payment method with system name:{0} for language:{1} during initialization", 
                            paymentMethodInfo.SystemKeyword, enabledSiteLanguage.LanguageID), e);
                    }
                }
            }
        }

        private void Create(PaymentMethodInfo paymentMethodInfo, Guid guid, string languageId)
        {
            PaymentMethodDto paymentMethod = new PaymentMethodDto();
            PaymentMethodDto.PaymentMethodRow paymentMethodRow = paymentMethod.PaymentMethod.NewPaymentMethodRow();
            paymentMethodRow[paymentMethod.PaymentMethod.PaymentMethodIdColumn] = guid;
            paymentMethodRow[paymentMethod.PaymentMethod.ApplicationIdColumn] = AppContext.Current.ApplicationId;
            paymentMethodRow[paymentMethod.PaymentMethod.NameColumn] = paymentMethodInfo.Name;
            paymentMethodRow[paymentMethod.PaymentMethod.DescriptionColumn] = paymentMethodInfo.Description;
            paymentMethodRow[paymentMethod.PaymentMethod.LanguageIdColumn] = languageId;
            paymentMethodRow[paymentMethod.PaymentMethod.SystemKeywordColumn] = paymentMethodInfo.SystemKeyword;
            paymentMethodRow[paymentMethod.PaymentMethod.IsActiveColumn] = false;
            paymentMethodRow[paymentMethod.PaymentMethod.IsDefaultColumn] = false;
            paymentMethodRow[paymentMethod.PaymentMethod.ClassNameColumn] = paymentMethodInfo.ClassName;
            paymentMethodRow[paymentMethod.PaymentMethod.PaymentImplementationClassNameColumn] = paymentMethodInfo.PaymentClassName;
            paymentMethodRow[paymentMethod.PaymentMethod.SupportsRecurringColumn] = false;
            paymentMethodRow[paymentMethod.PaymentMethod.OrderingColumn] = paymentMethodInfo.SortOrder;
            paymentMethodRow[paymentMethod.PaymentMethod.CreatedColumn] = FrameworkContext.Current.CurrentDateTime;
            paymentMethodRow[paymentMethod.PaymentMethod.ModifiedColumn] = FrameworkContext.Current.CurrentDateTime;
            paymentMethod.PaymentMethod.Rows.Add(paymentMethodRow);
            PaymentManager.SavePayment(paymentMethod);
        }

        private List<PaymentMethodDto.PaymentMethodRow> GetPaymentMethodsForLanguage(string languageId)
        {
            return PaymentManager.GetPaymentMethods(languageId, true).PaymentMethod.ToList();
        }

        private IList<LanguageBranch> GetEnabledSiteLanguages()
        {
            ILanguageBranchRepository languageBranchRepository = ServiceLocator.Current.GetInstance<ILanguageBranchRepository>();
            return languageBranchRepository.ListEnabled();
        }

        private Guid GeneratePaymentMethodId()
        {
            Guid id = Guid.NewGuid();
            while (PaymentManager.GetPaymentMethod(id, true).PaymentMethod.Count > 0)
            {
                id = Guid.NewGuid();
            }
            return id;
        }

        /// <summary>
        /// Returns the concatenated classname and assembly name of a given type. Example: "EPiServer.Business.Commerce.Payment.PayEx.PayExPayment, EPiServer.Business.Commerce.Payment.PayEx"
        /// </summary>
        private string ConcatenateClassAndAssemblyName(Type type)
        {
            return string.Format("{0}, {1}", type, type.Assembly.GetName().Name);
        }

        private class PaymentMethodInfo
        {
            public string SystemKeyword { get; set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            public string ClassName { get; private set; }
            public string PaymentClassName { get; private set; }
            public int SortOrder { get; private set; }

            public PaymentMethodInfo(string systemKeyword, string name, string className, string paymentClassName, int sortOrder)
            {
                SystemKeyword = systemKeyword;
                Name = name;
                Description = name;
                ClassName = className;
                PaymentClassName = paymentClassName;
                SortOrder = sortOrder;
            }
        }
    }
}
