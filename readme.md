# PayEx payment provider #

The PayEx payment provider for EPiServer Commerce supports the following payment methods: 

- [Credit Card](http://www.payexpim.com/payment-methods/credit-cards/)
- [PayPal](http://www.payexpim.com/payment-methods/paypal/)
- [Gift Cards](http://www.payexpim.com/payment-methods/gift-cards-generic-cards/)
- [Invoice Ledger](http://www.payexpim.com/payment-methods/invoice/)
- [Invoice 2.0](http://www.payexpim.com/payment-methods/payex-faktura-2-0/)
- [Direct Bank Debit](http://www.payexpim.com/payment-methods/direct-bank-debit/)
- [PayEx Part Payment](http://www.payexpim.com/payment-methods/payex-part-payment/)

## Prerequisites ##

TODO: Agreement with PayEx? 

- EPiServer.CMS version 7.6.3 or higher
- EPiServer.Commerce version 7.6.1 or higher
- .NET Framework 4.5 or higher

## Installing the PayEx payment provider ##

1. Download the EPiServer.Business.Commerce.Payment.PayEx package from the [EPiServer NuGet feed](https://nuget.episerver.com/en/Feed/). Add the package to your **Web project**. 

2. Download the EPiServer.Business.Commerce.Payment.PayEx.CommerceManager package from the [EPiServer NuGet feed](https://nuget.episerver.com/en/Feed/). Add the package to your **Commerce Manager project**.

3. Login to EPiServer Admin, click on the *Config* tab and click on the *Plug-in Manager* under *Tool Settings*. Click on the *EPiServer.Business.Commerce.Payment.PayEx* plugin and fill in the following settings: 

**Merchants PayEx account number**: The merchants PayEx account number given to you by PayEx.

**PayEx Encryption Key**: The encryption key generated in PayEx Admin

4. TODO:Create PayExPayment meta klasse

## Implementering the payment methods ##

TODO :

## Extending the payment provider ##

### Generating order numbers ###

The payment provider requires an order number in order to begin initialization of the payment. This means that the payment provider will generate an order number for you, but you have the ability to override this functionality to make order number sequences appropriate for your project. 

The default order number generation looks like this: 

    public string Generate(Cart cart)
	{
        string num = new Random().Next(1000, 9999).ToString();
        return string.Format("{0}{1}", cart.OrderGroupId, num);
    }

An example order number generated with this method is *3259375*.

If you want the order numbers to look differently for your project, you can implement your own *IOrderNumberGenerator*. In this example, all order numbers are prefixed by the number 5, an example order number generated with the method below is *51011521*:

	using System;
	using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
	using Mediachase.Commerce.Orders;

	namespace PayExProviderDemo
	{
    	public class OrderNumberGenerator : IOrderNumberGenerator
    	{
        	public string Generate(Cart cart)
        	{
            	string num = new Random().Next(100, 999).ToString();
            	return string.Format("5{0}{1}", cart.OrderGroupId, num);
        	}
    	}
	}

**Important: PayEx requires order numbers to be numeric. If you generate a non-numeric order number, payment initialization will fail.**

After implementing your own *IOrderNumberGenerator*, it will have to be injected into your dependency injection container to override the default implementation (examples below use StructureMap): 

	x.For<IOrderNumberGenerator>().Use<OrderNumberGenerator>();

Remember that your implementation of *IOrderNumberGenerator* will have to be injected into **both your web project and your Commerce Manager project**. If you don't already have a dependency resolver module for your Commerce Manager project, you can use the following *InitializableModule*:

	using EPiServer.Framework;
	using EPiServer.Framework.Initialization;
	using EPiServer.ServiceLocation;
	using Mediachase.Commerce.Initialization;
	using StructureMap;

	namespace PayExProviderDemo.Initializers
	{
    	[ModuleDependency(typeof(CommerceInitialization))]
    	[InitializableModule]
    	public class DependencyResolverModule : IConfigurableModule
    	{
        	private IContainer container;

        	public void ConfigureContainer(ServiceConfigurationContext context)
        	{
            	container = context.Container;

            	container.Configure(
                	x =>
                	{
                    	x.For<IOrderNumberGenerator>().Use<OrderNumberGenerator>();
                	});
        	}

        	public void Initialize(InitializationEngine context)
        	{
        	}

        	public void Preload(string[] parameters)
        	{
        	}

        	public void Uninitialize(InitializationEngine context)
        	{
        	}
    	}
	}

### Specifying the *additionalValues* parameter

During payment initialization, PayEx gives you the option of passing in *additionalValues* as a parameter. The *additionalValues* parameter is used for several things, for example enabling the payment menu, enabling responsive design or passing in invoice data. Take a look at the [PayEx documentation for the additionalValues parameter](http://www.payexpim.com/technical-reference/pxorder/initialize8/) to see all the options available.

#### ... if it is a plain string

If the value you wish to pass along with the *additionalValues* parameter is a plain string (such as "PAYMENTMENU=TRUE"), you can specify it in the parameters tab for the payment method in Commerce Manager: 

1. In the Commerce Manager, click on Administration -> Order System -> Payments
2. Click on the language folder you wish to view
3. Click on the PayEx payment method you wish to add a parameter to
4. Go to the *Parameters* tab and enter the AdditionalValue

TODO: C:\Users\karoline.klever\Dropbox\Jobb\PayEx\Screenshots\AdditionalValues.PNG

#### ... if it is a dynamic value

If the value you wish to pass along the *additionalValues* parameter is a dynamic value such as the invoice due date or invoice customer number, you cannot specify this using the previous method. This is because both the due date and the customer ID should be generated based on the purchase being made, in other words: It will vary from purchase to purchase.

In order to specify dynamic values you can implement your own *IAdditionalValuesFormatter*. In the example below we're assuming that the [Invoice Ledger](http://www.payexpim.com/payment-methods/invoice/) payment method is being used, which enables us to specify the customer ID and invoice due date that is six days from now:

	using EPiServer.Business.Commerce.Payment.PayEx;
	using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
	using System;
	using System.Text;
	
	namespace PayExProviderDemo.Commerce.Core.Payment.PayEx
	{
	    public class AdditionalValuesFormatter : IAdditionalValuesFormatter
	    {
	        public string Format(PayExPayment payExPayment)
	        {
	            StringBuilder stringBuilder = new StringBuilder();
	            stringBuilder.AppendFormat("INVOICE_CUSTOMERID={0}", payExPayment.CustomerId);
	
	            DateTime sixDaysForward = payExPayment.Created.AddDays(6);
	            stringBuilder.AppendFormat("&INVOICE_DUEDATE={0}", new DateTime(sixDaysForward.Year, sixDaysForward.Month, sixDaysForward.Day).ToString("yyyy-MM-dd"));
	            return stringBuilder.ToString();
	        }
	    }
	}

