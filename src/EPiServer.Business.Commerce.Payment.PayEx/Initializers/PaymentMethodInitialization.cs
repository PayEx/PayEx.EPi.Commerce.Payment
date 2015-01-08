using System;
using System.Collections.Generic;
using System.Linq;
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
            // Add disable setting

            // Create list of payex payment method objects to create, send them to DoWork
            DoWork();
        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new System.NotImplementedException();
        }

        public void Preload(string[] parameters)
        {
            throw new System.NotImplementedException();
        }

        private void DoWork()
        {
         
            IList<LanguageBranch> enabledSiteLanguages = GetEnabledSiteLanguages();
            foreach (var enabledSiteLanguage in enabledSiteLanguages)
            {
                List<PaymentMethodDto.PaymentMethodRow> paymentMethodsForLanguage =
                    GetPaymentMethodsForLanguage(enabledSiteLanguage.LanguageID);

            }
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

        public void Test()
        {
            PaymentMethodDto paymentMethod = new PaymentMethodDto();
            PaymentMethodDto.PaymentMethodRow paymentMethodRow = paymentMethod.PaymentMethod.NewPaymentMethodRow();
            paymentMethodRow[paymentMethod.PaymentMethod.PaymentMethodIdColumn] = (object)Guid.NewGuid();
            paymentMethodRow[paymentMethod.PaymentMethod.ApplicationIdColumn] = (object)AppContext.Current.ApplicationId;
            paymentMethodRow[paymentMethod.PaymentMethod.NameColumn] = "Test";
            paymentMethodRow[paymentMethod.PaymentMethod.DescriptionColumn] = "Test";
            paymentMethodRow[paymentMethod.PaymentMethod.LanguageIdColumn] = "no";
            paymentMethodRow[paymentMethod.PaymentMethod.SystemKeywordColumn] = "Test";
            paymentMethodRow[paymentMethod.PaymentMethod.IsActiveColumn] = (false);
            paymentMethodRow[paymentMethod.PaymentMethod.IsDefaultColumn] = (false);
            paymentMethodRow[paymentMethod.PaymentMethod.ClassNameColumn] = "EPiServer.Business.Commerce.Payment.PayEx.PayExPaymentGateway, EPiServer.Business.Commerce.Payment.PayEx";
            paymentMethodRow[paymentMethod.PaymentMethod.PaymentImplementationClassNameColumn] = "EPiServer.Business.Commerce.Payment.PayEx.PayExPayment, EPiServer.Business.Commerce.Payment.PayEx";
            paymentMethodRow[paymentMethod.PaymentMethod.SupportsRecurringColumn] = false;
            paymentMethodRow[paymentMethod.PaymentMethod.OrderingColumn] = 1000;
            paymentMethodRow[paymentMethod.PaymentMethod.CreatedColumn] = FrameworkContext.Current.CurrentDateTime;
            paymentMethodRow[paymentMethod.PaymentMethod.ModifiedColumn] = FrameworkContext.Current.CurrentDateTime;
            paymentMethod.PaymentMethod.Rows.Add(paymentMethodRow);
            PaymentManager.SavePayment(paymentMethod);
        }
    }
}
