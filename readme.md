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
TODO: Commerce version? 
TODO: EPiServer version?

.NET Framework 4.5 or higher

## Installing the PayEx payment provider ##

1. Download the EPiServer.Business.Commerce.Payment.PayEx package from the [EPiServer NuGet feed](https://nuget.episerver.com/en/Feed/). The package must be added to both your Web project and your Commerce Manager.

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