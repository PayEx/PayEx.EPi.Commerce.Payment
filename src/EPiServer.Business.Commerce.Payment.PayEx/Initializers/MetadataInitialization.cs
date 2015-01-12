using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Initialization;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using System.Collections.Generic;

namespace EPiServer.Business.Commerce.Payment.PayEx.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(CommerceInitialization))]
    public class MetadataInitialization : IInitializableModule
    {
        private class MetadataInfo
        {
            public string MetadataNamespace { get; set; }
            public string MetaFieldName { get; set; }
            public MetaDataType Type { get; set; }
            public int Length { get; set; }
            public bool AllowNulls { get; set; }
            public bool CultureSpecific { get; set; }
            public string ClassName { get; set; }

            public MetadataInfo(string metadataNamespace, string metaFieldName, MetaDataType type, int length, bool allowNulls, bool cultureSpecific, string className)
            {
                MetadataNamespace = metadataNamespace;
                MetaFieldName = metaFieldName;
                Type = type;
                Length = length;
                AllowNulls = allowNulls;
                CultureSpecific = cultureSpecific;
                ClassName = className;
            }
        }

        public void Initialize(InitializationEngine context)
        {
            var metadataInfo = new List<MetadataInfo>
            {
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.LineItem.VatAmount, MetaDataType.ShortString, 255, true, false, Constants.Metadata.LineItem.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.LineItem.VatPercentage, MetaDataType.ShortString, 255, true, false, Constants.Metadata.LineItem.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.OrderNumber, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.PayExOrderRef, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.Description, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.ProductNumber, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.ClientIpAddress, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.CancelUrl, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.ReturnUrl, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.Order, Constants.Metadata.Payment.CustomerId, MetaDataType.ShortString, 255, true, false, Constants.Metadata.Payment.ClassName),
                new MetadataInfo(Constants.Metadata.Namespace.OrderGroup, Constants.Metadata.OrderForm.PaymentMethodCode, MetaDataType.ShortString, 255, true, false, Constants.Metadata.OrderForm.ClassName),
            };

            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            CreateMetaClass(mdContext, Constants.Metadata.OrderFormPayment.ClassName, Constants.Metadata.Namespace.User, Constants.Metadata.Payment.ClassName,
                Constants.Metadata.Payment.TableName, string.Empty);

            foreach (var item in metadataInfo)
            {
                var metaField = CreateMetaField(mdContext, item.MetadataNamespace, item.MetaFieldName, item.Type, item.Length, item.AllowNulls, item.CultureSpecific);
                JoinField(mdContext, metaField, item.ClassName);
            }
        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }

        private void CreateMetaClass(MetaDataContext mdContext, string parentName, string metaDataNamespace, string name, string tableName, string description)
        {
            MetaClass parentClass = MetaClass.Load(mdContext, parentName);
            if (MetaClass.Load(mdContext, name) == null)
                MetaClass.Create(mdContext, metaDataNamespace, name, name, tableName, parentClass, false, description);
        }

        private MetaField CreateMetaField(MetaDataContext mdContext, string metaDataNamespace, string name, MetaDataType type, int length, bool allowNulls, bool cultureSpecific)
        {
            var f = MetaField.Load(mdContext, name) ??
                    MetaField.Create(mdContext, metaDataNamespace, name, name, string.Empty, type, length, allowNulls, cultureSpecific, false, false);
            return f;
        }

        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }

        private static bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }
    }
}
